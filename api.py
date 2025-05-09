from flask import jsonify, Flask

app = Flask(__name__)

@app.route('/api')
def foo():
    return jsonify("hello world")

app.run(host="0.0.0.0", port=1234)