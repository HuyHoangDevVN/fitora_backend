SHELL := powershell.exe
.SHELLFLAGS := -NoProfile -ExecutionPolicy Bypass -Command

.PHONY: help urls logs deps-start deps-stop start-full stop-full \
	start-auth start-user start-interact start-chat start-notification start-gateway \
	stop-auth stop-user stop-interact stop-chat stop-notification stop-gateway

help:
	@Write-Host "Fitora backend commands"
	@Write-Host ""
	@Write-Host "make deps-start          Start MongoDB, Redis, RabbitMQ"
	@Write-Host "make deps-stop           Stop MongoDB, Redis, RabbitMQ"
	@Write-Host "make start-full          Start dependencies and all services"
	@Write-Host "make stop-full           Stop all API services"
	@Write-Host ""
	@Write-Host "make start-auth          Start AuthService.API"
	@Write-Host "make start-user          Start UserService.API"
	@Write-Host "make start-interact      Start InteractService.API"
	@Write-Host "make start-chat          Start ChatService.API"
	@Write-Host "make start-notification  Start NotificationService.API"
	@Write-Host "make start-gateway       Start ApiGateway"
	@Write-Host ""
	@Write-Host "make stop-auth           Stop port 5002"
	@Write-Host "make stop-user           Stop port 5004"
	@Write-Host "make stop-interact       Stop port 5006"
	@Write-Host "make stop-chat           Stop port 5008"
	@Write-Host "make stop-notification   Stop port 5010"
	@Write-Host "make stop-gateway        Stop port 4469"
	@Write-Host ""
	@Write-Host "make urls                Print service URLs"
	@Write-Host "make logs                List service log files"

urls:
	@Write-Host "Gateway:       http://localhost:4469/swagger/index.html"
	@Write-Host "Auth:          https://localhost:5002/swagger/index.html"
	@Write-Host "User:          https://localhost:5004/swagger/index.html"
	@Write-Host "Interact:      https://localhost:5006/swagger/index.html"
	@Write-Host "Chat:          https://localhost:5008/swagger/index.html"
	@Write-Host "Notification:  https://localhost:5010/swagger/index.html"
	@Write-Host "RabbitMQ UI:   http://localhost:15672  guest / guest"

logs:
	@rtk powershell -Command "Get-ChildItem -Path .logs\services -ErrorAction SilentlyContinue | Select-Object Name,Length,LastWriteTime | Sort-Object Name | Format-Table -AutoSize"

deps-start:
	@rtk powershell -ExecutionPolicy Bypass -File scripts\start-deps.ps1

deps-stop:
	@rtk docker stop mongodb fitora-redis fitora-rabbitmq

start-full:
	@rtk powershell -ExecutionPolicy Bypass -File scripts\start-full-services.ps1

stop-full: stop-gateway stop-notification stop-chat stop-interact stop-user stop-auth

start-auth:
	@rtk dotnet run --project Services\AuthService\AuthService.API\AuthService.API.csproj --launch-profile https

start-user:
	@rtk dotnet run --project Services\UserService\UserService.API\UserService.API.csproj --launch-profile https

start-interact:
	@rtk dotnet run --project Services\InteractService\InteractService.API\InteractService.API.csproj --launch-profile https

start-chat:
	@rtk dotnet run --project Services\ChatService\ChatService.API\ChatService.API.csproj --launch-profile https

start-notification:
	@rtk dotnet run --project Services\NotificationService\NotificationService.API\NotificationService.API.csproj --launch-profile https

start-gateway:
	@rtk dotnet run --project ApiGateways\ApiGateway\ApiGateway.csproj --launch-profile http

stop-auth:
	@rtk powershell -Command '$$p=(Get-NetTCPConnection -LocalPort 5002 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty OwningProcess); if ($$p) { Stop-Process -Id $$p -Force }'

stop-user:
	@rtk powershell -Command '$$p=(Get-NetTCPConnection -LocalPort 5004 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty OwningProcess); if ($$p) { Stop-Process -Id $$p -Force }'

stop-interact:
	@rtk powershell -Command '$$p=(Get-NetTCPConnection -LocalPort 5006 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty OwningProcess); if ($$p) { Stop-Process -Id $$p -Force }'

stop-chat:
	@rtk powershell -Command '$$p=(Get-NetTCPConnection -LocalPort 5008 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty OwningProcess); if ($$p) { Stop-Process -Id $$p -Force }'

stop-notification:
	@rtk powershell -Command '$$p=(Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty OwningProcess); if ($$p) { Stop-Process -Id $$p -Force }'

stop-gateway:
	@rtk powershell -Command '$$p=(Get-NetTCPConnection -LocalPort 4469 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty OwningProcess); if ($$p) { Stop-Process -Id $$p -Force }'
