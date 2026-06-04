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

Push-Location $root
try {
    & (Join-Path $PSScriptRoot "start-deps-safe.ps1")

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

        Start-Process `
            -FilePath "dotnet" `
            -ArgumentList @("run", "--project", $service.Path, "--launch-profile", "https") `
            -WorkingDirectory $root `
            -WindowStyle Hidden `
            -RedirectStandardOutput (Join-Path $logDir "$($service.Name).out.log") `
            -RedirectStandardError (Join-Path $logDir "$($service.Name).err.log")

        Write-Host "Started $($service.Name) on configured port $($service.Port)"
    }

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
