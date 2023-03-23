FROM node:19-alpine AS build-ui
WORKDIR /app
COPY ./ui .
RUN npm install
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-app
WORKDIR /app
COPY . .
RUN dotnet publish Api/Api.csproj -c Release -o out --self-contained -r linux-x64 /p:PublishTrimmed=true /p:PublishSingleFile=true

FROM selenium/standalone-chrome:beta
WORKDIR /app
COPY --from=build-app /app/out .
COPY --from=build-ui /app/dist ./wwwroot
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
  DOTNET_RUNNING_IN_CONTAINER=true \
  ASPNETCORE_URLS=http://+:8080 \
  SE_OPTS="-port 4444"
ENV DISPLAY=:99

ENTRYPOINT ["./Api", "--urls", "http://0.0.0.0:8080"]
