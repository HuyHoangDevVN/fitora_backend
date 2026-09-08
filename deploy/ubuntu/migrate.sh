#!/usr/bin/env sh
set -eu

run_migration() {
  database="$1"
  project="$2"
  startup="$3"
  echo "Applying migrations for ${database}..."
  ConnectionStrings__Database="Server=mariadb;Port=3306;Database=${database};User=fitora;Password=${MYSQL_PASSWORD};" \
    /tools/dotnet-ef database update --configuration Release --no-build --project "$project" --startup-project "$startup"
}

run_migration fitora_auth Services/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj Services/AuthService/AuthService.API/AuthService.API.csproj
run_migration fitora_user Services/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj Services/UserService/UserService.API/UserService.API.csproj
run_migration fitora_interact Services/InteractService/InteractService.Infrastructure/InteractService.Infrastructure.csproj Services/InteractService/InteractService.API/InteractService.API.csproj
run_migration fitora_notification Services/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj Services/NotificationService/NotificationService.API/NotificationService.API.csproj
