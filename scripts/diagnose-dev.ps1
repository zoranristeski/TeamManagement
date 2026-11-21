$ErrorActionPreference = 'Continue'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$webRoot = Join-Path $repositoryRoot 'src\SportsTeamManagement.Web'
$environmentPath = Join-Path $webRoot '.env.local'
if (-not (Test-Path $environmentPath)) {
    $environmentPath = Join-Path $webRoot '.env.example'
}

$apiBaseUrl = $env:VITE_API_BASE_URL
if ([string]::IsNullOrWhiteSpace($apiBaseUrl)) {
    $setting = Select-String -Path $environmentPath -Pattern '^\s*VITE_API_BASE_URL\s*=\s*(.+?)\s*$' |
        Select-Object -First 1
    if ($setting) {
        $apiBaseUrl = $setting.Matches[0].Groups[1].Value.Trim('"', "'")
    }
}

$failed = $false
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host 'FAIL .NET SDK: dotnet is not on PATH.'
    $failed = $true
}
else {
    & $dotnet.Source dev-certs https --check --trust *> $null
    if ($LASTEXITCODE -eq 0) {
        Write-Host 'PASS trusted .NET HTTPS development certificate'
    }
    else {
        Write-Host 'FAIL HTTPS certificate is missing or untrusted. Run dotnet dev-certs https --trust.'
        $failed = $true
    }
}

if ($apiBaseUrl -ne 'https://localhost:7251') {
    Write-Host "FAIL API base URL: expected https://localhost:7251, found '$apiBaseUrl'."
    $failed = $true
}
else {
    Write-Host "PASS API base URL: $apiBaseUrl"
}

function Test-Endpoint {
    param([string]$Name, [string]$Url, [int[]]$ExpectedStatus = @(200))
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5
        $status = [int]$response.StatusCode
    }
    catch {
        if ($_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
        }
        else {
            $status = 0
        }
    }
    if ($ExpectedStatus -contains $status) {
        Write-Host "PASS $Name (HTTP $status)"
        return $true
    }
    Write-Host "FAIL $Name (HTTP $status)"
    return $false
}

if (-not (Test-Endpoint 'API HTTPS liveness' "$apiBaseUrl/health/live")) { $failed = $true }
if (-not (Test-Endpoint 'Database readiness' "$apiBaseUrl/health/ready")) { $failed = $true }
if (-not (Test-Endpoint 'Development Swagger' "$apiBaseUrl/swagger/index.html")) { $failed = $true }
if (-not (Test-Endpoint 'Frontend login page' 'http://localhost:5173/login')) { $failed = $true }

try {
    $preflight = Invoke-WebRequest -Uri "$apiBaseUrl/api/auth/register" -Method Options -UseBasicParsing -TimeoutSec 5 `
        -Headers @{
            Origin = 'http://localhost:5173'
            'Access-Control-Request-Method' = 'POST'
            'Access-Control-Request-Headers' = 'content-type,x-club-id'
        }
    $allowedOrigin = $preflight.Headers['Access-Control-Allow-Origin']
    $allowedHeaders = $preflight.Headers['Access-Control-Allow-Headers']
    if ($allowedOrigin -eq 'http://localhost:5173' -and $allowedHeaders -match '(?i)x-club-id') {
        Write-Host 'PASS CORS preflight for http://localhost:5173'
    }
    else {
        Write-Host 'FAIL CORS preflight did not allow the configured frontend origin and headers.'
        $failed = $true
    }
}
catch {
    Write-Host 'FAIL CORS preflight. Confirm API availability, trusted HTTPS certificate, and Development CORS settings.'
    $failed = $true
}

if ($failed) {
    Write-Host 'Diagnostics found failures. See docs\LOCAL_DEVELOPMENT.md for setup and troubleshooting.'
    exit 1
}
Write-Host 'All local development checks passed.'
