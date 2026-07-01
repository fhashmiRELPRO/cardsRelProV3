# start-services.ps1 — starts Auth, User, Search services in background
# Usage: powershell -ExecutionPolicy Bypass -File start-services.ps1
# Prerequisites: run setup-secrets.ps1 once before first use

$root = $PSScriptRoot
$logDir = "$root\.logs"
New-Item -ItemType Directory -Force $logDir | Out-Null

# Kill existing instances
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -match "RelPro\.(Auth|User|Search)\.Api" } |
    Stop-Process -Force -ErrorAction SilentlyContinue

Start-Sleep -Seconds 1

$services = @(
    @{ Name="Auth.Service";   Project="src\Services\Auth.Service\RelPro.Auth.Api";     Port=5010 },
    @{ Name="User.Service";   Project="src\Services\User.Service\RelPro.User.Api";     Port=5020 },
    @{ Name="Search.Service"; Project="src\Services\Search.Service\RelPro.Search.Api"; Port=5030 }
)

foreach ($svc in $services) {
    $log = "$logDir\$($svc.Name).log"
    Start-Process -FilePath "dotnet" `
        -ArgumentList "run", "--project", "$root\$($svc.Project)", "--no-build" `
        -RedirectStandardOutput $log -RedirectStandardError "$log.err"
    Write-Host "Started $($svc.Name) -> http://localhost:$($svc.Port) (log: $log)"
}

Write-Host "`nWaiting 6s for startup..."
Start-Sleep -Seconds 6

foreach ($svc in $services) {
    try {
        $code = (curl.exe -s -o nul -w "%{http_code}" "http://localhost:$($svc.Port)/swagger/index.html" 2>$null)
        $status = if ($code -eq "200") { "OK" } else { "HTTP $code" }
    } catch { $status = "no response" }
    Write-Host "$($svc.Name) (port $($svc.Port)): $status"
}
