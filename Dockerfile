FROM python:3.13.3

COPY api.py /server/

RUN pip3 install fastapi uvicorn flask
RUN openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes -subj "/CN=localhost"

COPY cert.pem key.pem /server/
WORKDIR /server
EXPOSE 8000

CMD ["uvicorn", "api:app", "--host", "0.0.0.0", "--port", "8000", "--ssl-keyfile", "key.pem", "--ssl-certfile", "cert.pem"]