$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$logDir = Join-Path $root ".logs\services"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

function Test-Port {
    param([int] $Port)
    try {
        return (Test-NetConnection -ComputerName 127.0.0.1 -Port $Port -WarningAction SilentlyContinue).TcpTestSucceeded
    } catch {
        return $false
    }
}

function Ensure-DockerContainer {
    param(
        [string] $Name,
        [string] $Image,
        [string[]] $RunArgs
    )

    $existing = docker ps -a --filter "name=^/$Name$" --format "{{.Names}}"
    if ($existing -eq $Name) {
        docker start $Name | Out-Null
        return
    }

    docker run -d --name $Name @RunArgs $Image | Out-Null
}

function Ensure-RabbitQueue {
    param([string] $Name)

    docker exec fitora-rabbitmq rabbitmqadmin `
        --username=guest `
        --password=guest `
        declare queue name=$Name durable=true | Out-Null
}

Push-Location $root
try {
    docker compose -f "Services\docker-compose.yml" up -d | Out-Null
    Ensure-DockerContainer -Name "fitora-redis" -Image "redis:7-alpine" -RunArgs @("-p", "6379:6379")
    Ensure-DockerContainer -Name "fitora-rabbitmq" -Image "rabbitmq:3-management" -RunArgs @("-p", "5672:5672", "-p", "15672:15672")
    Ensure-DockerContainer -Name "fitora-elasticsearch" -Image "docker.elastic.co/elasticsearch/elasticsearch:8.15.3" -RunArgs @(
        "-p", "9200:9200",
        "-e", "discovery.type=single-node",
        "-e", "xpack.security.enabled=false",
        "-e", "ES_JAVA_OPTS=-Xms512m` -Xmx512m"
    )

    Ensure-RabbitQueue -Name "user_registration_queue"
    Ensure-RabbitQueue -Name "noti_queue"
    Ensure-RabbitQueue -Name "noti_realtime_queue"

    $services = @(
        @{ Name = "AuthService.API"; Path = "Services\AuthService\AuthService.API\AuthService.API.csproj"; Port = 5002 },
        @{ Name = "UserService.API"; Path = "Services\UserService\UserService.API\UserService.API.csproj"; Port = 5004 },
        @{ Name = "InteractService.API"; Path = "Services\InteractService\InteractService.API\InteractService.API.csproj"; Port = 5006 },
        @{ Name = "ChatService.API"; Path = "Services\ChatService\ChatService.API\ChatService.API.csproj"; Port = 5008 },
        @{ Name = "NotificationService.API"; Path = "Services\NotificationService\NotificationService.API\NotificationService.API.csproj"; Port = 5010 },
        @{ Name = "ApiGateway"; Path = "ApiGateways\ApiGateway\ApiGateway.csproj"; Port = 4469 }
    )

    foreach ($service in $services) {
        if (Test-Port -Port $service.Port) {
            Write-Host "$($service.Name) already has port $($service.Port) open; skipping start."
            continue
        }

        $stdout = Join-Path $logDir "$($service.Name).out.log"
        $stderr = Join-Path $logDir "$($service.Name).err.log"
        $args = @("run", "--project", $service.Path, "--launch-profile", "http")

        Start-Process `
            -FilePath "dotnet" `
            -ArgumentList $args `
            -WorkingDirectory $root `
            -WindowStyle Hidden `
            -RedirectStandardOutput $stdout `
            -RedirectStandardError $stderr

        Write-Host "Started $($service.Name) on http://localhost:$($service.Port)"
    }

    Start-Sleep -Seconds 8

    Write-Host ""
    Write-Host "Port status:"
    foreach ($service in $services) {
        $status = if (Test-Port -Port $service.Port) { "UP" } else { "DOWN" }
        Write-Host "$($service.Name) $($service.Port): $status"
    }

    Write-Host ""
    Write-Host "Logs: $logDir"
} finally {
    Pop-Location
}
