import fastapi
import os

app = fastapi.FastAPI()

states=[]

@app.get("/")
async def hello():
    return fastapi.responses.JSONResponse({"message": "Hello, World!"})

@app.get("/health")
async def health():
    if os.system("cat /proc/loadavg | awk '{print $1}'") > 1.0:
        return fastapi.responses.JSONResponse({"status": "error", "message": "High CPU load"})
    else: return fastapi.responses.JSONResponse({"status": "ok"})

message_buffer = ""

@app.post("/post")
async def post(data: dict):
    global message_buffer
    message_buffer = data.get("message", "No message provided")
    return fastapi.responses.JSONResponse({"status": "ok"})

@app.get("/get")
async def get():
    return fastapi.responses.JSONResponse({"message": message_buffer})

@app.get("/getHash")
async def get_hash():
    import hashlib
    return fastapi.responses.JSONResponse({"hash": f"{hashlib.sha256().hexdigest()}"})
