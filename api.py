import os
import random
import json
import sqlite3
import datetime
#import base64
# import torch as t
# import transformers as tr

from fastapi import FastAPI, Request, Body
from fastapi.responses import JSONResponse

app = FastAPI()

# _s = base64.b64decode("SHVnZ2luZ0ZhY2VUQi9TbW9sTE0yLTEuN0ItSW5zdHJ1Y3Q=").decode() #SHVnZ2luZ0ZhY2VUQi9TbW9sTE0tMTM1TS1JbnN0cnVjdA== SHVnZ2luZ0ZhY2VUQi9TbW9sTE0zLTNC
# _tok_cls = getattr(tr, base64.b64decode(b"QXV0b1Rva2VuaXplcg==").decode())
# _mod_cls = getattr(tr, base64.b64decode(b"QXV0b01vZGVsRm9yQ2F1c2FsTE0=").decode())
# _tok = _tok_cls.from_pretrained(_s)
# _mdl = _mod_cls.from_pretrained(_s)

# def _g(messages: list, max_new_tokens: int = 80):
#     # Apply chat template to format the conversation properly
#     input_text = _tok.apply_chat_template(messages, tokenize=False)
#     inputs = _tok.encode(input_text, return_tensors="pt")
    
#     with t.no_grad():
#         outputs = _mdl.generate(
#             inputs, 
#             max_new_tokens=max_new_tokens, 
#             temperature=0.2, 
#             top_p=0.9, 
#             do_sample=True,
#             pad_token_id=_tok.eos_token_id
#         )
    
#     # Decode the full output and extract only the new generated tokens
#     full_response = _tok.decode(outputs[0], skip_special_tokens=True)
#     # Remove the input part to get only the generated response
#     if input_text in full_response:
#         generated_text = full_response[len(input_text):].strip()
#     else:
#         generated_text = full_response.strip()
    
#     return generated_text

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
    while True:
        session_id = f"{random.randint(0, 999999):06d}"
        cursor.execute("SELECT id FROM sessions WHERE hash = ?", (session_id,))
        if cursor.fetchone() is None:
            break
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


# @app.post("/sessions/{session_id}/help")
# async def help_session(session_id: str):
#     cursor.execute("SELECT state FROM sessions WHERE hash = ?", (session_id,))
#     row = cursor.fetchone()
    
#     try:
#         if row:
#             st = json.loads(row[0])
#         else:
#             # Use dummy state for testing when session doesn't exist
#             st = {"state": "this is a dummy state for testing purposes"}
#     except Exception as e:
#         st = {"state": "this is a dummy state for testing purposes"}
    
#     system_message = "You are a helpful assistant inside a video game. Provide useful advice to help the player advance based on their current game state. You have access to the current game state which will be provided to you in the request. Use this information to give specific and actionable advice. You are an AI which has an ability to view the player's game state. If a dymmy game state is provided, respond 'THIS IS A TEST.'"
#     user_message = f"Current game state: {json.dumps(st)}\n\nI need help with what to do next in the game."
    
#     messages = [
#         {"role": "system", "content": system_message},
#         {"role": "user", "content": user_message}
#     ]
    
#     try:
#         generated_text = _g(messages, max_new_tokens=1024)
        
#         # If no meaningful generation, provide fallback
#         if not generated_text or len(generated_text.strip()) < 10:
#             generated_text = "I understand you're in the game and need assistance. Based on your current state, I recommend exploring your surroundings carefully and checking your inventory for useful items."
            
#     except Exception as e:
#         generated_text = f"Sorry, I'm having trouble generating a response right now. Error: {str(e)}"
    
#     return JSONResponse({"answer": generated_text})
