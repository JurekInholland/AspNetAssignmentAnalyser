# 🐍 Snake assignment analyser

Built with Asp .Net Core 7.0 and Vue.js 3.0.

### clone repo

```bash
git clone repo/url && cd repo
```

### Local development

```bash
# from project root
cd api
dotnet watch

cd ui
npm install; npm run dev
open localhost:5173
```

### Environment variables
- SendGridApiKey={sendgrid api key},
- SendGridFromEmail={from email},
- SendGridToEmail={to email}
- UserHeaderKey=X-User

### Deploy without docker

```bash
# from project root
cd ui
npm install; npm run build
rmdir ../api/wwwroot
cp -r dist/* ../api/wwwroot/
dotnet publish ./Api/Api.csproj -c Release -o out
```

### Run with docker compose

```bash
# from project root
docker compose up -d --build
```

### Docker

```bash
# from project root
docker build -f .\Api\Dockerfile -t assignment-analyser .
docker run -p 80:80 --name assignment-analyser assignment-analyser
```
