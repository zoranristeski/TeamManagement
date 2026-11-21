$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $repositoryRoot 'src\SportsTeamManagement.API\SportsTeamManagement.API.csproj'
$webRoot = Join-Path $repositoryRoot 'src\SportsTeamManagement.Web'
$webTemplate = Join-Path $webRoot '.env.example'
$webEnvironment = Join-Path $webRoot '.env.local'
$apiProcess = $null
$frontendProcess = $null
$startedApi = $false
$startedFrontend = $false

function Get-HttpStatusCode {
    param([string]$Url)
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 3
        return [int]$response.StatusCode
    }
    catch {
        if ($_.Exception.Response) {
            return [int]$_.Exception.Response.StatusCode
        }
        return 0
    }
}

function Test-TcpPort {
    param([int]$Port)
    return [bool](Test-NetConnection -ComputerName localhost -Port $Port -InformationLevel Quiet -WarningAction SilentlyContinue)
}

function Stop-StartedProcessTree {
    param([int]$RootProcessId)

    $snapshot = @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue)
    $processIds = [System.Collections.Generic.List[int]]::new()
    $processIds.Add($RootProcessId)
    for ($index = 0; $index -lt $processIds.Count; $index++) {
        $parentId = $processIds[$index]
        foreach ($child in $snapshot | Where-Object { [int]$_.ParentProcessId -eq $parentId }) {
            $childId = [int]$child.ProcessId
            if (-not $processIds.Contains($childId)) {
                $processIds.Add($childId)
            }
        }
    }

    for ($index = $processIds.Count - 1; $index -ge 0; $index--) {
        Stop-Process -Id $processIds[$index] -Force -ErrorAction SilentlyContinue
    }
}

try {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    $node = Get-Command node -ErrorAction SilentlyContinue
    $npm = Get-Command npm.cmd -ErrorAction SilentlyContinue
    if (-not $dotnet -or -not $node -or -not $npm) {
        throw 'Install the .NET SDK, Node.js, and npm, then open a new PowerShell window.'
    }
    if (-not (Test-Path $apiProject) -or -not (Test-Path $webRoot) -or -not (Test-Path $webTemplate)) {
        throw 'Could not locate the API project, web project, or frontend environment template from the repository root.'
    }
    if (-not (Test-Path (Join-Path $webRoot 'node_modules\vite\bin\vite.js'))) {
        throw 'Frontend dependencies are missing. Run npm ci in src\SportsTeamManagement.Web and rerun this script.'
    }

    if (-not (Test-Path $webEnvironment)) {
        Copy-Item -Path $webTemplate -Destination $webEnvironment
        Write-Host 'Created src\SportsTeamManagement.Web\.env.local from .env.example.'
    }

    $configuredApiUrl = $env:VITE_API_BASE_URL
    if ([string]::IsNullOrWhiteSpace($configuredApiUrl)) {
        $apiUrlSetting = Select-String -Path $webEnvironment -Pattern '^\s*VITE_API_BASE_URL\s*=\s*(.+?)\s*$' |
            Select-Object -First 1
        if ($apiUrlSetting) {
            $configuredApiUrl = $apiUrlSetting.Matches[0].Groups[1].Value.Trim('"', "'")
        }
    }
    if ($configuredApiUrl -ne 'https://localhost:7251') {
        throw 'Set VITE_API_BASE_URL=https://localhost:7251 in src\SportsTeamManagement.Web\.env.local and restart this script.'
    }

    $secretOutput = @(& $dotnet.Source user-secrets list --project $apiProject 2>$null)
    $secretNames = @($secretOutput | ForEach-Object {
        if ($_ -match '^\s*([^=]+?)\s*=') {
            $Matches[1].Trim()
        }
    })
    $hasConnectionString = -not [string]::IsNullOrWhiteSpace($env:ConnectionStrings__DefaultConnection) -or
        $secretNames -contains 'ConnectionStrings:DefaultConnection'
    $hasAudience = -not [string]::IsNullOrWhiteSpace($env:Authentication__Audience) -or
        $secretNames -contains 'Authentication:Audience'
    if (-not $hasConnectionString -or -not $hasAudience) {
        throw 'API configuration is incomplete. Set ConnectionStrings:DefaultConnection and Authentication:Audience with dotnet user-secrets; see docs\LOCAL_DEVELOPMENT.md.'
    }

    & $dotnet.Source dev-certs https --check --trust *> $null
    if ($LASTEXITCODE -ne 0) {
        Write-Warning 'The localhost HTTPS development certificate is missing or not trusted.'
        $trustCertificate = Read-Host 'Trust the .NET HTTPS development certificate now? (Y/N)'
        if ($trustCertificate -notmatch '^(Y|YES)$') {
            throw 'Run dotnet dev-certs https --trust, then rerun this script.'
        }
        & $dotnet.Source dev-certs https --trust
        if ($LASTEXITCODE -ne 0) {
            throw 'Certificate trust was not completed. Run dotnet dev-certs https --trust and rerun this script.'
        }
        & $dotnet.Source dev-certs https --check --trust *> $null
        if ($LASTEXITCODE -ne 0) {
            throw 'The HTTPS development certificate is still not trusted. See docs\LOCAL_DEVELOPMENT.md.'
        }
    }

    if ($env:PORT) {
        Write-Warning 'PORT is set in this shell. Development ignores PORT so the HTTPS launch profile remains on its configured ports.'
    }

    $apiStatus = Get-HttpStatusCode 'https://localhost:7251/health/live'
    if ($apiStatus -eq 200) {
        Write-Host 'Using the API already responding at https://localhost:7251.'
        Write-Warning 'The running API was not started by this script, so its development seeder will not run. Restart that API with this script if sample data is missing.'
    }
    else {
        if (Test-TcpPort 7251) {
            throw 'Port 7251 is occupied but the API liveness endpoint did not respond. Close the other process and rerun this script.'
        }

        $originalSigningKey = [Environment]::GetEnvironmentVariable('Authentication__SigningKey')
        $originalSeedSetting = [Environment]::GetEnvironmentVariable('SeedData__Enabled')
        try {
            if ([string]::IsNullOrWhiteSpace($originalSigningKey)) {
                $randomBytes = New-Object byte[] 48
                $randomGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
                try {
                    $randomGenerator.GetBytes($randomBytes)
                    $env:Authentication__SigningKey = [Convert]::ToBase64String($randomBytes)
                }
                finally {
                    $randomGenerator.Dispose()
                }
                Write-Host 'Using a temporary local JWT signing key for this run; sessions will expire when the API stops.'
            }
            $env:SeedData__Enabled = 'true'

            $apiArguments = "run --project `"$apiProject`" --launch-profile https"
            $apiProcess = Start-Process -FilePath $dotnet.Source -ArgumentList $apiArguments `
                -WorkingDirectory $repositoryRoot -PassThru -NoNewWindow
            $startedApi = $true
        }
        finally {
            [Environment]::SetEnvironmentVariable('Authentication__SigningKey', $originalSigningKey)
            [Environment]::SetEnvironmentVariable('SeedData__Enabled', $originalSeedSetting)
        }

        $apiStatus = 0
        for ($attempt = 0; $attempt -lt 60 -and $apiStatus -ne 200; $attempt++) {
            $apiProcess.Refresh()
            if ($apiProcess.HasExited) {
                throw 'The API process exited before becoming healthy. Review its output and docs\LOCAL_DEVELOPMENT.md for configuration requirements.'
            }
            Start-Sleep -Seconds 1
            $apiStatus = Get-HttpStatusCode 'https://localhost:7251/health/live'
        }
        if ($apiStatus -ne 200) {
            throw 'The API process started but https://localhost:7251/health/live did not respond. Check API output and HTTPS trust.'
        }
        Write-Host 'API process started and HTTPS liveness check passed.'
    }

    $readyStatus = Get-HttpStatusCode 'https://localhost:7251/health/ready'
    if ($readyStatus -eq 200) {
        Write-Host 'Database readiness check passed.'
    }
    else {
        Write-Warning "API is live, but database readiness returned HTTP $readyStatus. Registration requires a reachable, migrated database."
    }

    $swaggerStatus = Get-HttpStatusCode 'https://localhost:7251/swagger/index.html'
    if ($swaggerStatus -eq 200) {
        Write-Host 'Swagger is available at https://localhost:7251/swagger/index.html.'
    }
    else {
        Write-Warning "Swagger returned HTTP $swaggerStatus. Confirm the API is running in Development."
    }

    $frontendStatus = Get-HttpStatusCode 'http://localhost:5173/login'
    if ($frontendStatus -eq 200) {
        Write-Host 'Using the frontend already responding at http://localhost:5173.'
    }
    else {
        if (Test-TcpPort 5173) {
            throw 'Port 5173 is occupied but the frontend did not respond. Close the other process and rerun this script.'
        }

        $commandArguments = '/d /s /c "npm.cmd run dev -- --host localhost --port 5173 --strictPort"'
        $frontendProcess = Start-Process -FilePath $env:ComSpec -ArgumentList $commandArguments `
            -WorkingDirectory $webRoot -PassThru -NoNewWindow
        $startedFrontend = $true

        for ($attempt = 0; $attempt -lt 45 -and $frontendStatus -ne 200; $attempt++) {
            $frontendProcess.Refresh()
            if ($frontendProcess.HasExited) {
                throw 'The frontend process exited before becoming available. Run npm ci in the web project and inspect its output.'
            }
            Start-Sleep -Seconds 1
            $frontendStatus = Get-HttpStatusCode 'http://localhost:5173/login'
        }
        if ($frontendStatus -ne 200) {
            throw 'The frontend process started but http://localhost:5173/login did not respond.'
        }
        Write-Host 'Frontend process started and login page responded.'
    }

    Write-Host ''
    Write-Host 'Frontend: http://localhost:5173/login'
    Write-Host 'API:      https://localhost:7251'
    Write-Host 'Swagger:  https://localhost:7251/swagger/index.html'
    Write-Host 'Health:   https://localhost:7251/health/live'
    Write-Host 'Press Enter to stop only the processes started by this script.'
    [void](Read-Host)
}
catch {
    Write-Error $_
    exit 1
}
finally {
    if ($startedFrontend -and $frontendProcess) {
        Stop-StartedProcessTree -RootProcessId $frontendProcess.Id
    }
    if ($startedApi -and $apiProcess) {
        Stop-StartedProcessTree -RootProcessId $apiProcess.Id
    }
}
