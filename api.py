import os
import uuid
import json
import sqlite3
import datetime
import base64
import torch as t
import transformers as tr

from fastapi import FastAPI, Request, Body
from fastapi.responses import JSONResponse

app = FastAPI()

_s = base64.b64decode("SHVnZ2luZ0ZhY2VUQi9TbW9sTE0tMTM1TS1JbnN0cnVjdA==").decode()
_tok_cls = getattr(tr, base64.b64decode(b"QXV0b1Rva2VuaXplcg==").decode())
_mod_cls = getattr(tr, base64.b64decode(b"QXV0b01vZGVsRm9yQ2F1c2FsTE0=").decode())
_tok = _tok_cls.from_pretrained(_s)
_mdl = _mod_cls.from_pretrained(_s)

def _g(x: str, lim: int = 80):
    d = _tok(x, return_tensors="pt").input_ids
    for _ in range(lim):
        with t.no_grad():
            l = _mdl(d).logits[:, -1]
        m = l.argmax(dim=-1, keepdim=True)
        d = t.cat([d, m], 1)
        if m.item() == _tok.eos_token_id:
            break
    return _tok.decode(d[0], skip_special_tokens=True)

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
    state_value = data.get("state")
    cursor.execute(
        "UPDATE sessions SET state = ? WHERE hash = ?",
        (json.dumps(state_value), session_id),
    )
    if cursor.rowcount == 0:
        return JSONResponse({"error": "Session not found"}, status_code=404)
    conn.commit()
    return JSONResponse({"status": "ok"})


@app.post("/sessions/{session_id}/help")
async def help_session(session_id: str):
    cursor.execute("SELECT state FROM sessions WHERE hash = ?", (session_id,))
    row = cursor.fetchone()
    if not row:
        return JSONResponse({"error": "Session not found"}, status_code=404)
    st = json.loads(row[0])
    prompt = json.dumps(st)
    reply = _g(prompt)
    return JSONResponse({"answer": reply})
