import fastapi
import hashlib
import datetime
import os
from fastapi import Body

app = fastapi.FastAPI()

states = []
dummy_state = None

@app.get("/")
async def hello():
    return fastapi.responses.JSONResponse({"message": "Hello, World!"})

@app.get("/health")
async def health():
    if os.system("cat /proc/loadavg | awk '{print $1}'") > 1.0:
        return fastapi.responses.JSONResponse({"status": "error", "message": "High CPU load"})
    else: return fastapi.responses.JSONResponse({"status": "ok"})

@app.post("/post")
async def post(data: dict = Body(...)):
    try:
        hash_value = data.get("hash")
        state_value = data.get("state")

        for entry in states:
            if entry.get("hash") == hash_value:
                entry["state"] = state_value
                return fastapi.responses.JSONResponse({"status": "ok"})
        
        states.append({"hash": hash_value, "state": state_value})
        print(states)
        return fastapi.responses.JSONResponse({"status": "ok"})
    except Exception as e:
        return fastapi.responses.JSONResponse({"status": "error", "message": str(e)}, status_code=400)

@app.get("/get")
async def get(hash: str = None, data: dict = Body(None)):
    hash_value = data.get("hash")

    for entry in states:
        if entry.get("hash") == hash_value:
            return fastapi.responses.JSONResponse({"state": entry.get("state")})
    return fastapi.responses.JSONResponse({"state": "No state found"})

@app.get("/getHash")
async def get_hash():
    tmp = hashlib.sha512().hexdigest()
    states.append({"hash": tmp, "state": dummy_state})
    return fastapi.responses.JSONResponse({"hash": f"{tmp}"})
