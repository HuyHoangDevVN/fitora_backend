param(
    [string]$ArtifactRoot = "C:\fitora_backend_artifacts",
    [string]$IisRoot = "C:\inetpub\wwwroot",
    [string]$ServiceRoot = "C:\publish",
    [string]$BackupRoot = "C:\fitora_backend_backups"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Import-Module WebAdministration -ErrorAction Stop

$components = @(
    @{ Name = "authservice"; Type = "IIS"; TargetPath = (Join-Path $IisRoot "authservice"); ArtifactPath = (Join-Path $ArtifactRoot "authservice"); Dll = "AuthService.API.dll"; Port = 5002 },
    @{ Name = "userservice"; Type = "WindowsService"; TargetPath = (Join-Path $ServiceRoot "userservice"); ArtifactPath = (Join-Path $ArtifactRoot "userservice"); Dll = "UserService.API.dll"; Port = 5004 },
    @{ Name = "interactservice"; Type = "WindowsService"; TargetPath = (Join-Path $ServiceRoot "interactservice"); ArtifactPath = (Join-Path $ArtifactRoot "interactservice"); Dll = "InteractService.API.dll"; Port = 5006 },
    @{ Name = "chatservice"; Type = "IIS"; TargetPath = (Join-Path $IisRoot "chatservice"); ArtifactPath = (Join-Path $ArtifactRoot "chatservice"); Dll = "ChatService.API.dll"; Port = 5008 },
    @{ Name = "notificationservice"; Type = "IIS"; TargetPath = (Join-Path $IisRoot "notificationservice"); ArtifactPath = (Join-Path $ArtifactRoot "notificationservice"); Dll = "NotificationService.API.dll"; Port = 5010 },
    @{ Name = "apigateway"; Type = "IIS"; TargetPath = (Join-Path $IisRoot "apigateway"); ArtifactPath = (Join-Path $ArtifactRoot "apigateway"); Dll = "ApiGateway.dll"; Port = 4469 }
)

function Test-PublishDirectory {
    param([hashtable]$Component)

    $artifact = [string]$Component.ArtifactPath
    if (-not (Test-Path -LiteralPath $artifact)) {
        throw "Artifact directory missing for $($Component.Name): $artifact"
    }

    foreach ($required in @($Component.Dll, "$([System.IO.Path]::GetFileNameWithoutExtension($Component.Dll)).deps.json", "$([System.IO.Path]::GetFileNameWithoutExtension($Component.Dll)).runtimeconfig.json")) {
        $requiredPath = Join-Path $artifact $required
        if (-not (Test-Path -LiteralPath $requiredPath)) {
            throw "Required publish file missing for $($Component.Name): $required"
        }
    }

    if ($Component.Name -eq "apigateway") {
        $ocelotPath = Join-Path $artifact "ocelot.json"
        if (-not (Test-Path -LiteralPath $ocelotPath)) {
            throw "ApiGateway artifact is missing ocelot.json"
        }
        $ocelot = Get-Content -LiteralPath $ocelotPath -Raw | ConvertFrom-Json
        $selfRoute = $ocelot.Routes | Where-Object {
            $_.DownstreamHostAndPorts.Host -contains "localhost" -and $_.DownstreamHostAndPorts.Port -contains 4469
        }
        if ($selfRoute) {
            throw "ApiGateway artifact contains a self-referencing downstream route to port 4469."
        }
    }
}

function Copy-PublishedFiles {
    param(
        [string]$Source,
        [string]$Target,
        [string]$Backup
    )

    $preserveNames = @(
        "appsettings.Production.json",
        ".env",
        "certificate.pfx",
        "DataProtection-Keys"
    )

    $preserved = @()
    foreach ($name in $preserveNames) {
        $path = Join-Path $Target $name
        if (Test-Path -LiteralPath $path) {
            $preserved += @{ Name = $name; Path = $path }
        }
    }

    Get-ChildItem -LiteralPath $Target -Force | Remove-Item -Recurse -Force
    Copy-Item -Path (Join-Path $Source "*") -Destination $Target -Recurse -Force

    foreach ($item in $preserved) {
        $backupItem = Join-Path $Backup $item.Name
        if (Test-Path -LiteralPath $backupItem) {
            Copy-Item -LiteralPath $backupItem -Destination $item.Path -Recurse -Force
        }
    }
}

function Stop-Component {
    param([hashtable]$Component)

    if ($Component.Type -eq "WindowsService") {
        $service = Get-Service -Name $Component.Name -ErrorAction Stop
        if ($service.Status -ne "Stopped") {
            Stop-Service -Name $Component.Name -Force -ErrorAction Stop
            $service.WaitForStatus("Stopped", [TimeSpan]::FromSeconds(60))
        }
        return
    }

    $target = [System.IO.Path]::GetFullPath([string]$Component.TargetPath)
    $site = Get-Website | Where-Object {
        [System.IO.Path]::GetFullPath($_.PhysicalPath) -ieq $target -or $_.Name -ieq $Component.Name
    } | Select-Object -First 1
    if (-not $site) {
        throw "No IIS site found for $($Component.Name) at $target"
    }

    Stop-Website -Name $site.Name
    Stop-WebAppPool -Name $site.applicationPool
}

function Start-Component {
    param([hashtable]$Component)

    if ($Component.Type -eq "WindowsService") {
        Start-Service -Name $Component.Name -ErrorAction Stop
        $service = Get-Service -Name $Component.Name -ErrorAction Stop
        $service.WaitForStatus("Running", [TimeSpan]::FromSeconds(60))
        return
    }

    $target = [System.IO.Path]::GetFullPath([string]$Component.TargetPath)
    $site = Get-Website | Where-Object {
        [System.IO.Path]::GetFullPath($_.PhysicalPath) -ieq $target -or $_.Name -ieq $Component.Name
    } | Select-Object -First 1
    if (-not $site) {
        throw "No IIS site found for $($Component.Name) at $target"
    }

    Start-WebAppPool -Name $site.applicationPool
    Start-Website -Name $site.Name
}

function Test-Port {
    param([int]$Port)

    $connection = Test-NetConnection -ComputerName "127.0.0.1" -Port $Port -WarningAction SilentlyContinue
    if (-not $connection.TcpTestSucceeded) {
        throw "Port check failed for 127.0.0.1:$Port"
    }
}

function Test-HttpStatus {
    param(
        [string]$Url,
        [int[]]$ExpectedStatus
    )

    try {
        $response = Invoke-WebRequest -Uri $Url -Method Post -Body "{}" -ContentType "application/json" -UseBasicParsing -TimeoutSec 30
        $statusCode = [int]$response.StatusCode
    }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }
        else {
            throw
        }
    }

    if ($ExpectedStatus -notcontains $statusCode) {
        throw "Health check $Url returned HTTP $statusCode. Expected: $($ExpectedStatus -join ', ')"
    }
}

New-Item -ItemType Directory -Path $BackupRoot -Force | Out-Null

foreach ($component in $components) {
    Test-PublishDirectory -Component $component
    if (-not (Test-Path -LiteralPath $component.TargetPath)) {
        throw "Target path does not exist for $($component.Name): $($component.TargetPath)"
    }
}

$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$deployed = @()

foreach ($component in $components) {
    $backup = Join-Path $BackupRoot "$($component.Name)-$timestamp"
    Copy-Item -LiteralPath $component.TargetPath -Destination $backup -Recurse -Force

    try {
        Write-Host "Deploying $($component.Name) to $($component.TargetPath)"
        Stop-Component -Component $component
        Copy-PublishedFiles -Source $component.ArtifactPath -Target $component.TargetPath -Backup $backup
        Start-Component -Component $component
        Test-Port -Port ([int]$component.Port)
        $deployed += @{ Component = $component; Backup = $backup }
    }
    catch {
        Write-Host "Deployment failed for $($component.Name). Rolling back from $backup"
        Stop-Component -Component $component
        Get-ChildItem -LiteralPath $component.TargetPath -Force | Remove-Item -Recurse -Force
        Copy-Item -Path (Join-Path $backup "*") -Destination $component.TargetPath -Recurse -Force
        Start-Component -Component $component
        throw
    }
}

Test-HttpStatus -Url "http://127.0.0.1:4469/auth/auth/login" -ExpectedStatus @(400, 401, 405)
Test-HttpStatus -Url "http://127.0.0.1:4469/chat/negotiate" -ExpectedStatus @(200, 401)
Test-HttpStatus -Url "http://127.0.0.1:4469/noti/negotiate" -ExpectedStatus @(200, 401)

Write-Host "Backend deployment completed. Backup timestamp: $timestamp"
