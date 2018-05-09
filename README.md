# influxgateway

Part experimentation, part usage by my home automation setup to expose limited influx data to Alexa lambda functions.

Provides a web api created with ASP.NET Core 2.1 (RC1). Repo includes multiple Dockerfiles to cover amd64 and arm builds.

## Source
Source: https://github.com/acrelle/influxgateway

## Build

[![](https://images.microbadger.com/badges/version/acrelle/influxgateway.svg)](https://microbadger.com/images/acrelle/influxgateway "Get your own version badge on microbadger.com")[![](https://images.microbadger.com/badges/image/acrelle/influxgateway.svg)](https://microbadger.com/images/acrelle/influxgateway "Get your own image badge on microbadger.com")![](https://travis-ci.com/acrelle/influxgateway.svg?branch=master)

https://hub.docker.com/r/acrelle/influxgateway/

### Supported architectures

`amd64`,`arm32v7`

## Usage

Run the image using the below, or edit and use the Docker Compose further down.

```
docker run -d -p 8080:80 \
      -e influx_username=username \
      -e influx_password=password \
      -e influx_url=http://influx_install_addr:port \
      -e influx_database=database_name \
      acrelle/influxgateway
```

Then view in a web browser:

http://127.0.0.1:8080/api/influx

## Docker Compose

Sample:

```
version: "2"
services:
  influxgateway:
    build: .
    image: acrelle/influxgateway
    container_name: influxgateway
    restart: unless-stopped
    ports:
      - 8080:80
    environment:
      - influx_username=username
      - influx_password=password
      - influx_url=http://127.0.0.1:8086
      - influx_database=database_name
```
