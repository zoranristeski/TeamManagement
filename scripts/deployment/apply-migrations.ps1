[CmdletBinding()]
param()

$secureConnectionString = Read-Host "Enter the migration operator connection string" -AsSecureString
$connectionStringPointer = [IntPtr]::Zero
$previousConnectionString = [Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Process")
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')

try {
    $connectionStringPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureConnectionString)
    $env:ConnectionStrings__DefaultConnection = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($connectionStringPointer)

    Push-Location $repoRoot
    try {
        dotnet ef database update `
            --project .\src\SportsTeamManagement.Infrastructure\SportsTeamManagement.Infrastructure.csproj `
            --startup-project .\src\SportsTeamManagement.API\SportsTeamManagement.API.csproj `
            --configuration Release
        if ($LASTEXITCODE -ne 0) {
            throw "EF Core database update failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    if ($null -eq $previousConnectionString) {
        Remove-Item Env:\ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
    }
    else {
        $env:ConnectionStrings__DefaultConnection = $previousConnectionString
    }

    if ($connectionStringPointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($connectionStringPointer)
    }
    $secureConnectionString.Dispose()
}
