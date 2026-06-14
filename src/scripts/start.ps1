$ErrorActionPreference = "Stop"

$sourceRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $sourceRoot "docker\docker-compose.yaml"
$backendEnvFile = Join-Path $sourceRoot "UrbanCommunity.Api\.env.development"
$aiEnvFile = Join-Path $sourceRoot "..\..\ai\.env.development"

foreach ($requiredFile in @($composeFile, $backendEnvFile, $aiEnvFile)) {
    if (-not (Test-Path -LiteralPath $requiredFile)) {
        throw "Required Docker startup file was not found: $requiredFile"
    }
}

docker compose `
    --file $composeFile `
    --env-file $backendEnvFile `
    --env-file $aiEnvFile `
    up --detach --build
