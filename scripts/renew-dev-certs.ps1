$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$password = "123456@Aa"
$certPaths = @(
    "Services\AuthService\AuthService.API\certificate.pfx",
    "Services\UserService\UserService.API\certificate.pfx",
    "Services\InteractService\InteractService.API\certificate.pfx",
    "Services\ChatService\ChatService.API\certificate.pfx",
    "Services\NotificationService\NotificationService.API\certificate.pfx"
)

Push-Location $root
try {
    dotnet dev-certs https --check --trust | Out-Host

    $tmpCert = Join-Path $env:TEMP "fitora-localhost-dev-cert.pfx"
    dotnet dev-certs https --export-path $tmpCert --password $password | Out-Host

    foreach ($relativePath in $certPaths) {
        $target = Join-Path $root $relativePath
        if (Test-Path $target) {
            $backup = "$target.bak"
            Copy-Item -LiteralPath $target -Destination $backup -Force
        }

        Copy-Item -LiteralPath $tmpCert -Destination $target -Force
        Write-Host "Updated $relativePath"
    }

    Remove-Item -LiteralPath $tmpCert -Force -ErrorAction SilentlyContinue
} finally {
    Pop-Location
}
