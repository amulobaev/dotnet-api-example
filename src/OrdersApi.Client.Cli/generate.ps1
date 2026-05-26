# Regenerate Refit client from swagger.json using Refitter CLI.
# Run from the repository root:
#   .\src\OrdersApi.Client.Cli\generate.ps1
#
# Or from this directory:
#   .\generate.ps1
#
# Prerequisites: Refitter installed as a global tool
#   dotnet tool install -g refitter

$projectDir = $PSScriptRoot

Write-Host "Generating Refit client from swagger.json..." -ForegroundColor Cyan

Push-Location $projectDir
try {
    dotnet refitter --settings-file .refitter
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Refitter CLI failed (exit code $LASTEXITCODE)"
        exit $LASTEXITCODE
    }
    Write-Host "Done. Commit Generated/Output.cs to update the contract." -ForegroundColor Green
} finally {
    Pop-Location
}
