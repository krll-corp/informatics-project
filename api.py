import os
import uuid
import json
import sqlite3
import datetime

from fastapi import FastAPI, Request, Body
from fastapi.responses import JSONResponse

app = FastAPI()

DB_PATH = os.environ.get("DB_PATH", "sessions.db")
conn = sqlite3.connect(DB_PATH, check_same_thread=False)
cursor = conn.cursor()
cursor.execute(
    """
    CREATE TABLE IF NOT EXISTS sessions (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        hash TEXT UNIQUE,
        state TEXT,
        created_at TIMESTAMP
    )
    """
)
conn.commit()

def merge_dict(base: dict, updates: dict) -> None:
    """Recursively merge updates into base."""
    for key, value in updates.items():
        if isinstance(value, dict) and isinstance(base.get(key), dict):
            merge_dict(base[key], value)
        else:
            base[key] = value

def cleanup_sessions():
    cutoff = datetime.datetime.now() - datetime.timedelta(weeks=1) #datetime.timezone.utc
    cursor.execute("DELETE FROM sessions WHERE created_at <= ?", (cutoff,))
    conn.commit()


@app.middleware("http")
async def cleanup_middleware(request: Request, call_next):
    cleanup_sessions()
    response = await call_next(request)
    return response


@app.get("/")
async def hello():
    return JSONResponse({"message": "Hello, World!"})


@app.get("/health")
async def health():
    health_status = {"status": "ok", "checks": {}}
    
    # CPU
    try:
        load = os.getloadavg()[0]
        health_status["checks"]["cpu_load"] = {
            "value": load,
            "status": "error" if load > 1.0 else "ok",
            "threshold": 1.0
        }
        if load > 1.0:
            health_status["status"] = "error"
    except Exception as e:
        health_status["checks"]["cpu_load"] = {"status": "error", "error": str(e)}
        health_status["status"] = "error"
    
    # DB
    try:
        cursor.execute("SELECT 1")
        cursor.fetchone()
        health_status["checks"]["database"] = {"status": "ok"}
    except Exception as e:
        health_status["checks"]["database"] = {"status": "error", "error": str(e)}
        health_status["status"] = "error"
    
    # DISK
    try:
        disk_usage = os.statvfs('.')
        free_space_percent = (disk_usage.f_bavail * disk_usage.f_frsize) / (disk_usage.f_blocks * disk_usage.f_frsize) * 100
        health_status["checks"]["disk_space"] = {
            "free_percent": round(free_space_percent, 2),
            "status": "error" if free_space_percent < 10 else "ok",
            "threshold": 10
        }
        if free_space_percent < 10:
            health_status["status"] = "error"
    except Exception as e:
        health_status["checks"]["disk_space"] = {"status": "error", "error": str(e)}
        health_status["status"] = "error"
    
    # SESSION COUNT
    try:
        cursor.execute("SELECT COUNT(*) FROM sessions")
        session_count = cursor.fetchone()[0]
        health_status["checks"]["session_count"] = {
            "count": session_count,
            "status": "ok"
        }
    except Exception as e:
        health_status["checks"]["session_count"] = {"status": "error", "error": str(e)}
        health_status["status"] = "error"
    
    status_code = 500 if health_status["status"] == "error" else 200
    return JSONResponse(health_status, status_code=status_code)


@app.post("/sessions")
async def create_session():
    session_id = uuid.uuid4().hex
    created_at = datetime.datetime.now()
    cursor.execute(
        "INSERT INTO sessions (hash, state, created_at) VALUES (?, ?, ?)",
        (session_id, json.dumps(None), created_at),
    )
    conn.commit()
    return JSONResponse({"hash": session_id})


@app.get("/sessions/{session_id}")
async def read_session(session_id: str):
    cursor.execute("SELECT state FROM sessions WHERE hash = ?", (session_id,))
    row = cursor.fetchone()
    if row:
        return JSONResponse({"state": json.loads(row[0])})
    return JSONResponse({"error": "Session not found"}, status_code=404)


@app.post("/sessions/{session_id}")
async def update_session(session_id: str, data: dict = Body(...)):
    """Merge the provided values into the existing game state."""
    cursor.execute("SELECT state FROM sessions WHERE hash = ?", (session_id,))
    row = cursor.fetchone()
    if not row:
        return JSONResponse({"error": "Session not found"}, status_code=404)

    current_state = json.loads(row[0]) if row[0] else {}
    if not isinstance(current_state, dict):
        current_state = {}

    # Clients may POST either the raw state object or {"state": {...}} for
    # backward compatibility. The data is always merged into the existing state.
    patch = data.get("state", data)

    if isinstance(patch, dict):
        merge_dict(current_state, patch)
        new_state = current_state
    else:
        new_state = patch

    cursor.execute(
        "UPDATE sessions SET state = ? WHERE hash = ?",
        (json.dumps(new_state), session_id),
    )
    conn.commit()
    return JSONResponse({"status": "ok"})
