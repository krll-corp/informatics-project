import fastapi
import hashlib
import datetime
import os

app = fastapi.FastAPI()

states=dict()

@app.get("/")
async def hello():
    return fastapi.responses.JSONResponse({"message": "Hello, World!"})

@app.get("/health")
async def health():
    if os.system("cat /proc/loadavg | awk '{print $1}'") > 1.0:
        return fastapi.responses.JSONResponse({"status": "error", "message": "High CPU load"})
    else: return fastapi.responses.JSONResponse({"status": "ok"})

@app.post("/post")
async def post(data: dict):
    try:
        states[data.get("hash")] = data.get("state")
        return fastapi.responses.JSONResponse({"status": "ok"})
    except Exception as e:
        return fastapi.responses.JSONResponse({"status": "error", "message": str(e)}, status_code=400)

@app.get("/get")
async def get(hash: str = None):
    return fastapi.responses.JSONResponse({"state": states.get(hash, "No state found")})

@app.get("/getHash")
async def get_hash():
    tmp = hashlib.sha512().hexdigest()
    states[tmp] = {None: None}
    return fastapi.responses.JSONResponse({"hash": f"{tmp}"})
