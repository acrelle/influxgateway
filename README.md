# influxgateway

Part experimentation, part usage by my home automation setup to expose limited influx data to Alexa lambda functions.

Provides a web api created with ASP.NET Core 2.1 (RC1). Repo includes a Dockerfile that produces an arm32v7 build that is published on Dockerhub.

To target a different architecture: update the Dockerfile and rebuild.

## Source
Source: https://github.com/acrelle/influxgateway

## Build
[![](https://images.microbadger.com/badges/version/acrelle/rpi-influxgateway.svg)](https://microbadger.com/images/acrelle/rpi-influxgateway "Get your own version badge on microbadger.com")[![](https://images.microbadger.com/badges/image/acrelle/rpi-influxgateway.svg)](https://microbadger.com/images/acrelle/rpi-influxgateway "Get your own image badge on microbadger.com")

https://hub.docker.com/r/acrelle/rpi-influxgateway/

## Usage

Edit docker-compose and change the environment variables to match your influx installation.

Run the Docker using the below Docker Compose on a Raspberry Pi, e.g.

```
docker-compose pull
docker-compose up -d
```

Then view in a web browser:

http://url:port/api/influx

## Docker Compose

Sample:

```
version: "2"
services:
  influxgateway:
    build: .
    image: acrelle/rpi-influxgateway
    container_name: influxgateway
    restart:  unless-stopped
    ports:
      - 8080:80
    environment:
      - influx_username=username
      - influx_password=password
      - influx_url=http://127.0.0.1:8086
      - influx_database=database_name
```
