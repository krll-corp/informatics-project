FROM python:3.13.5

COPY api.py /server/

RUN pip3 install fastapi uvicorn

WORKDIR /server
EXPOSE 8000

CMD ["uvicorn", "api:app", "--host", "0.0.0.0", "--port", "8000"]