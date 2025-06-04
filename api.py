import os
import uuid
import json
import sqlite3
import datetime

from fastapi import FastAPI, Request, Body
from typing import Optional

from transformers import AutoModelForCausalLM, AutoTokenizer, pipeline
import torch
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

# AI model configuration
MODEL_NAME = os.environ.get("MODEL_NAME", "HuggingFaceTB/SmolLM-135M")
SYSTEM_PROMPT = os.environ.get("SYSTEM_PROMPT", "")
_ai_pipeline: Optional[pipeline] = None


def get_ai_pipeline() -> pipeline:
    """Load and cache the text generation pipeline."""
    global _ai_pipeline
    if _ai_pipeline is None:
        tokenizer = AutoTokenizer.from_pretrained(
            MODEL_NAME, local_files_only=True
        )
        model = AutoModelForCausalLM.from_pretrained(
            MODEL_NAME, local_files_only=True
        )
        device = 0 if torch.cuda.is_available() else -1
        _ai_pipeline = pipeline(
            "text-generation", model=model, tokenizer=tokenizer, device=device
        )
    return _ai_pipeline


def cleanup_sessions():
    cutoff = datetime.datetime.now() - datetime.timedelta(
        weeks=1
    )  # datetime.timezone.utc
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
            "threshold": 1.0,
        }
        if load > 1.0:
            health_status["status"] = "error"
    except Exception as e:
        health_status["checks"]["cpu_load"] = {
            "status": "error",
            "error": str(e),
        }
        health_status["status"] = "error"

    # DB
    try:
        cursor.execute("SELECT 1")
        cursor.fetchone()
        health_status["checks"]["database"] = {"status": "ok"}
    except Exception as e:
        health_status["checks"]["database"] = {
            "status": "error",
            "error": str(e),
        }
        health_status["status"] = "error"

    # DISK
    try:
        disk_usage = os.statvfs(".")
        free_space_percent = (
            (disk_usage.f_bavail * disk_usage.f_frsize)
            / (disk_usage.f_blocks * disk_usage.f_frsize)
            * 100
        )
        health_status["checks"]["disk_space"] = {
            "free_percent": round(free_space_percent, 2),
            "status": "error" if free_space_percent < 10 else "ok",
            "threshold": 10,
        }
        if free_space_percent < 10:
            health_status["status"] = "error"
    except Exception as e:
        health_status["checks"]["disk_space"] = {
            "status": "error",
            "error": str(e),
        }
        health_status["status"] = "error"

    # SESSION COUNT
    try:
        cursor.execute("SELECT COUNT(*) FROM sessions")
        session_count = cursor.fetchone()[0]
        health_status["checks"]["session_count"] = {
            "count": session_count,
            "status": "ok",
        }
    except Exception as e:
        health_status["checks"]["session_count"] = {
            "status": "error",
            "error": str(e),
        }
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


@app.post("/sessions/{session_id}/ai")
async def ai_help(session_id: str, data: dict = Body(...)):
    """Return an AI generated answer based on the session state."""
    prompt = data.get("prompt")
    if not prompt:
        return JSONResponse({"error": "Missing prompt"}, status_code=400)

    cursor.execute("SELECT state FROM sessions WHERE hash = ?", (session_id,))
    row = cursor.fetchone()
    if row is None:
        return JSONResponse({"error": "Session not found"}, status_code=404)

    session_state = json.loads(row[0]) if row[0] else {}

    generator = get_ai_pipeline()
    state_text = json.dumps(session_state)
    full_prompt = f"{SYSTEM_PROMPT}\nGame state:\n{state_text}\nUser question: {prompt}\nAssistant:"
    result = generator(full_prompt, max_new_tokens=150)
    generated = result[0]["generated_text"][len(full_prompt) :].strip()

    return JSONResponse({"answer": generated})
