$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")

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

    Write-Host "Dependencies are running:"
    docker ps --filter "name=mongodb" --filter "name=fitora-redis" --filter "name=fitora-rabbitmq" --filter "name=fitora-elasticsearch" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
} finally {
    Pop-Location
}
