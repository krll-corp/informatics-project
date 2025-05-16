import fastapi
from flask import jsonify

app = fastapi.FastAPI()

@app.get("/")
async def hello():
    jsonify({"Hello, World!"})