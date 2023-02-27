# 🐍 Snake assignment analyser

![image](https://user-images.githubusercontent.com/42969112/221485863-78f74ae4-bd52-48cd-a953-5e6ffd131aef.png)

## Introduction

This is a web application built with Asp .Net Core 7, Vue.js 3, SignalR, Selenium, SendGrid, and Docker. It's designed for potential IT
students who want to complete the [inholland snake assignment](https://www.inholland.nl/media/0x0geu2l/snake-assignment-v1-0.pdf).

## Overview

- Provides a web page where zipped assignments can be uploaded
- Unzip uploaded code and test it using Selenium. Update the frontend in real-time via SignalR to reflect the test results
- Upload the zip file to Azure Blob Storage
- Generate a HTML report and send it to the supplied study coach email address using SendGrid.

This application is intended to be used within an existing authentication system. The student id/email address is retrieved from the request
headers. The key can be supplied via the `UserHeaderKey` environment variable.

The Dockerfile is using multiple stages to build the frontend with `node:19-alpine` and the backend with `mcr.microsoft.com/dotnet/sdk:7.0`.
The artifacts are then copied to a `selenium/standalone-chrome` image. This serves the purpose of having a single, easily deployable image,
instead of having to set up a whole bunch of microservices.

### Environment variables

To use the application, you need to set the following environment variables:

- `AzureWebJobsStorage`: The connection string for your Azure Storage account.
- `SendGridApiKey`: The API key for your SendGrid account.
- `SendGridFromEmail`: The email address you want to send the report from.
- `SendGridToEmail`: The email address you want to send the test reports to.
- `UserHeaderKey`: The key for the student email address in the request headers.
---

## Getting Started

### clone repo

```sh
git clone https://github.com/JurekInholland/AspNetAssignmentAnalyser.git && cd AspNetAssignmentAnalyser
```

### Local development
If you are using azurite, make sure to pass `azurite --skipApiVersionCheck`
```sh
# from project root
cd Api
dotnet watch

cd ui
npm install && npm run dev
open http://localhost:5173
```

### Deploy without docker

```sh
# from project root
cd ui
npm install && npm run build
rmdir ../api/wwwroot
cp -r dist/* ../api/wwwroot/
dotnet publish ./Api/Api.csproj -c Release -o out
```

### Docker
Beware that `"AzureWebJobsStorage": "UseDevelopmentStorage=true"` does not work within the container. 
```sh
# from project root
docker build -f .\Api\Dockerfile -t assignment-analyser .
docker run --rm \
-e AzureWebJobsStorage='foo' \
-e SendGridApiKey='bar' \
-e SendGridFromEmail='baz' \
-e SendGridToEmail='qux' \
-p 8080:8080 \
--name assignment-analyser assignment-analyser

open http://localhost:8080
```

### Docker compose

```sh
# from project root
docker compose up -d --build
open http://localhost:8080
```
