FROM mcr.microsoft.com/dotnet/sdk:8.0.408 AS build
WORKDIR /src
COPY . .
RUN dotnet restore fitora_backend.sln \
 && dotnet build fitora_backend.sln -c Release --no-restore \
 && dotnet publish Services/AuthService/AuthService.API/AuthService.API.csproj -c Release --no-build -o /out/auth \
 && dotnet publish Services/UserService/UserService.API/UserService.API.csproj -c Release --no-build -o /out/user \
 && dotnet publish Services/InteractService/InteractService.API/InteractService.API.csproj -c Release --no-build -o /out/interact \
 && dotnet publish Services/ChatService/ChatService.API/ChatService.API.csproj -c Release --no-build -o /out/chat \
 && dotnet publish Services/NotificationService/NotificationService.API/NotificationService.API.csproj -c Release --no-build -o /out/notification \
 && dotnet publish ApiGateways/ApiGateway/ApiGateway.csproj -c Release --no-build -o /out/gateway

FROM mcr.microsoft.com/dotnet/aspnet:8.0.11 AS runtime
WORKDIR /app
# liblept5/libtesseract5 runtime libs are required by AuthService's Tesseract OCR (CCCD identity verification).
RUN apt-get update \
 && apt-get install -y --no-install-recommends liblept5 libtesseract5 \
 && rm -rf /var/lib/apt/lists/*
COPY --from=build /out /app
USER app

FROM build AS migrator
RUN dotnet tool install --tool-path /tools dotnet-ef --version 8.0.8
COPY deploy/ubuntu/migrate.sh /migrate.sh
RUN chmod +x /migrate.sh
ENTRYPOINT ["/migrate.sh"]
