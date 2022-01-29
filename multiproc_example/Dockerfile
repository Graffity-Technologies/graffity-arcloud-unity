FROM python:3.8.12-slim-buster

LABEL vendor="GRAFFITY TECHNOLOGIES CO., LTD."

RUN apt-get update -y

COPY . /app

WORKDIR /app/

RUN pip install --upgrade pip && \
  pip install grpcio grpcio-tools numpy awscli

# EXPOSE 50051

CMD ["python", "server_m.py"]