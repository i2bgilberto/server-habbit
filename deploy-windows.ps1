# deploy-windows.ps1
# Publica la API como Windows Service
# Requiere ejecutar como Administrador

param(
    [string]$ServiceName     = "PrimeDiscipline",
    [string]$DisplayName     = "Prime Discipline API",
    [string]$InstallPath     = "C:\Services\PrimeDiscipline",
    [string]$MongoUri        = $env:MONGODB_URI,
    [string]$AspNetCoreUrls  = "http://127.0.0.1:5254"
)

$ErrorActionPreference = "Stop"
$ProjectPath = "$PSScriptRoot\src\PrimeDiscipline.WebAPI\PrimeDiscipline.WebAPI.csproj"

Write-Host "=== 1. Publicando aplicacion ===" -ForegroundColor Cyan
dotnet publish $ProjectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $InstallPath `
    /p:PublishSingleFile=true

if ($LASTEXITCODE -ne 0) { throw "Fallo el publish." }

# Escribir MONGODB_URI en el archivo de entorno del servicio
$envFile = "$InstallPath\.env"
"MONGODB_URI=$MongoUri" | Set-Content $envFile
Write-Host "MONGODB_URI escrita en $envFile" -ForegroundColor Gray

Write-Host "=== 2. Registrando Windows Service ===" -ForegroundColor Cyan
$exePath = "$InstallPath\PrimeDiscipline.WebAPI.exe"

$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Deteniendo servicio existente..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

New-Service `
    -Name        $ServiceName `
    -DisplayName $DisplayName `
    -BinaryPathName "`"$exePath`"" `
    -StartupType Automatic `
    -Description "API de gestion de habitos Prime Discipline"

Write-Host "=== 3. Configurando variable de entorno del sistema ===" -ForegroundColor Cyan
[System.Environment]::SetEnvironmentVariable("MONGODB_URI", $MongoUri, "Machine")
[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production", "Machine")
[System.Environment]::SetEnvironmentVariable("ASPNETCORE_URLS", $AspNetCoreUrls, "Machine")

Write-Host "=== 4. Iniciando servicio ===" -ForegroundColor Cyan
Start-Service -Name $ServiceName
Start-Sleep -Seconds 3

$status = (Get-Service -Name $ServiceName).Status
Write-Host "Estado del servicio: $status" -ForegroundColor $(if ($status -eq "Running") { "Green" } else { "Red" })

Write-Host ""
Write-Host "Deploy completado." -ForegroundColor Green
Write-Host "  Servicio : $ServiceName"
Write-Host "  Ruta     : $InstallPath"
Write-Host "  Url      : $AspNetCoreUrls"
Write-Host "  Health   : $AspNetCoreUrls/healthz"
