FROM python:3.13.3

COPY api.py /server/

RUN pip3 install flask

WORKDIR /server

CMD ["python", "api.py"]