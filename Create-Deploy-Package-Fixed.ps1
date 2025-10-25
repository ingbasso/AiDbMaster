# Script per creare il pacchetto di deploy completo di AiDbMaster
# Eseguire dalla cartella del progetto (dove si trova AiDbMaster.sln)

param(
    [string]$OutputPath = ".\Deploy-Package",
    [string]$ProjectName = "AiDbMaster",
    [string]$Configuration = "Release"
)

Write-Host "=== CREAZIONE PACCHETTO DEPLOY AIDBMASTER ===" -ForegroundColor Green
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Verifica che siamo nella directory corretta
if (-not (Test-Path "AiDbMaster.sln")) {
    Write-Error "File AiDbMaster.sln non trovato. Eseguire dalla cartella del progetto!"
    exit 1
}

$deployDate = Get-Date -Format "yyyyMMdd-HHmmss"
$packageName = "AiDbMaster-Deploy-$deployDate"
$tempPath = Join-Path $OutputPath $packageName
$zipPath = "$OutputPath\$packageName.zip"

Write-Host "Nome pacchetto: $packageName" -ForegroundColor Yellow
Write-Host "Path temporaneo: $tempPath" -ForegroundColor Yellow
Write-Host "File finale: $zipPath" -ForegroundColor Yellow

Write-Host "`n1. PREPARAZIONE CARTELLE..." -ForegroundColor Cyan

# Crea cartelle di lavoro
if (Test-Path $tempPath) {
    Remove-Item $tempPath -Recurse -Force
}
New-Item -ItemType Directory -Path $tempPath -Force | Out-Null
New-Item -ItemType Directory -Path "$tempPath\App" -Force | Out-Null
New-Item -ItemType Directory -Path "$tempPath\Scripts" -Force | Out-Null
New-Item -ItemType Directory -Path "$tempPath\Database" -Force | Out-Null

Write-Host "✓ Cartelle create" -ForegroundColor Green

Write-Host "`n2. PUBBLICAZIONE APPLICAZIONE..." -ForegroundColor Cyan

# Pubblica l'applicazione
$publishPath = "$tempPath\App"
$publishCommand = "dotnet publish $ProjectName -c $Configuration -o `"$publishPath`" --self-contained false"

Write-Host "Comando: $publishCommand" -ForegroundColor White
Invoke-Expression $publishCommand

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Applicazione pubblicata" -ForegroundColor Green
} else {
    Write-Error "Errore durante la pubblicazione dell'applicazione"
    exit 1
}

Write-Host "`n3. COPIA CONFIGURAZIONI..." -ForegroundColor Cyan

# Copia appsettings.Production.json se esiste
if (Test-Path "appsettings.Production.json") {
    Copy-Item "appsettings.Production.json" "$publishPath\" -Force
    Write-Host "✓ appsettings.Production.json copiato" -ForegroundColor Green
}

Write-Host "`n4. GENERAZIONE SCRIPT DATABASE..." -ForegroundColor Cyan

# Genera script per setup database base
Write-Host "Generazione script database base..." -ForegroundColor White

# Crea il contenuto dello script SQL
$sqlLines = @()
$sqlLines += "-- Script per aggiornare il database AiDbMaster"
$sqlLines += "-- Generato automaticamente"
$sqlLines += ""
$sqlLines += "USE [AIDBMASTER];"
$sqlLines += "GO"
$sqlLines += ""
$sqlLines += "-- Verifica e crea la tabella __EFMigrationsHistory se non esiste"
$sqlLines += "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')"
$sqlLines += "BEGIN"
$sqlLines += "    CREATE TABLE [dbo].[__EFMigrationsHistory]("
$sqlLines += "        [MigrationId] [nvarchar](150) NOT NULL,"
$sqlLines += "        [ProductVersion] [nvarchar](32) NOT NULL,"
$sqlLines += "        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)"
$sqlLines += "    );"
$sqlLines += "    PRINT 'Tabella __EFMigrationsHistory creata';"
$sqlLines += "END"
$sqlLines += "ELSE"
$sqlLines += "BEGIN"
$sqlLines += "    PRINT 'Tabella __EFMigrationsHistory già esistente';"
$sqlLines += "END"
$sqlLines += "GO"
$sqlLines += ""
$sqlLines += "PRINT '=== SCRIPT DATABASE COMPLETATO ===';"

$sqlContent = $sqlLines -join "`r`n"
$sqlContent | Out-File "$tempPath\Database\Update-Database.sql" -Encoding UTF8
Write-Host "✓ Script database base generato" -ForegroundColor Green

# Copia script di permessi database se esiste
if (Test-Path "Database-Permissions-Correct.sql") {
    Copy-Item "Database-Permissions-Correct.sql" "$tempPath\Database\" -Force
    Write-Host "✓ Script permessi database copiato" -ForegroundColor Green
}

Write-Host "`n5. COPIA SCRIPT DI DEPLOY..." -ForegroundColor Cyan

# Lista degli script da includere
$deployScripts = @(
    "Setup-IIS-Basic.ps1",
    "Setup-Database-Simple.ps1", 
    "Deploy-First-Time.ps1",
    "Check-Permissions.ps1",
    "Fix-AppPool-Permission.ps1",
    "Install-Deploy-Package.ps1"
)

foreach ($script in $deployScripts) {
    if (Test-Path $script) {
        Copy-Item $script "$tempPath\Scripts\" -Force
        Write-Host "✓ $script copiato" -ForegroundColor Green
    } else {
        Write-Host "⚠ $script non trovato" -ForegroundColor Yellow
    }
}

Write-Host "`n6. CREAZIONE SCRIPT DI INSTALLAZIONE SEMPLICE..." -ForegroundColor Cyan

# Crea uno script di installazione semplice
$installLines = @()
$installLines += "# Script di installazione semplice per AiDbMaster"
$installLines += "# Eseguire come Amministratore"
$installLines += ""
$installLines += "Write-Host '=== INSTALLAZIONE AIDBMASTER ===' -ForegroundColor Green"
$installLines += ""
$installLines += "# Verifica amministratore"
$installLines += "if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')) {"
$installLines += "    Write-Error 'Eseguire come Amministratore!'"
$installLines += "    exit 1"
$installLines += "}"
$installLines += ""
$installLines += "# Importa modulo IIS"
$installLines += "Import-Module WebAdministration -ErrorAction SilentlyContinue"
$installLines += ""
$installLines += "# Ferma Application Pool"
$installLines += "Write-Host 'Arresto Application Pool...' -ForegroundColor Cyan"
$installLines += "Stop-WebAppPool -Name 'AiDbMaster' -ErrorAction SilentlyContinue"
$installLines += ""
$installLines += "# Copia files"
$installLines += "Write-Host 'Copia files applicazione...' -ForegroundColor Cyan"
$installLines += "`$sitePath = 'C:\inetpub\wwwroot\AiDbMaster'"
$installLines += "if (-not (Test-Path `$sitePath)) {"
$installLines += "    New-Item -ItemType Directory -Path `$sitePath -Force | Out-Null"
$installLines += "}"
$installLines += "Copy-Item '.\App\*' `$sitePath -Recurse -Force"
$installLines += ""
$installLines += "# Avvia Application Pool"
$installLines += "Write-Host 'Avvio Application Pool...' -ForegroundColor Cyan"
$installLines += "Start-WebAppPool -Name 'AiDbMaster'"
$installLines += ""
$installLines += "Write-Host '=== INSTALLAZIONE COMPLETATA ===' -ForegroundColor Green"
$installLines += "Write-Host 'Testa applicazione: http://localhost' -ForegroundColor Cyan"

$installContent = $installLines -join "`r`n"
$installContent | Out-File "$tempPath\Install-Simple.ps1" -Encoding UTF8
Write-Host "✓ Script installazione semplice creato" -ForegroundColor Green

Write-Host "`n7. CREAZIONE README..." -ForegroundColor Cyan

$readmeLines = @()
$readmeLines += "# Pacchetto Deploy AiDbMaster"
$readmeLines += "Generato il: $(Get-Date)"
$readmeLines += "Versione: $packageName"
$readmeLines += ""
$readmeLines += "## Contenuto Pacchetto"
$readmeLines += ""
$readmeLines += "### App/"
$readmeLines += "Contiene tutti i files dell'applicazione pubblicata."
$readmeLines += ""
$readmeLines += "### Scripts/"
$readmeLines += "Script di configurazione e deploy."
$readmeLines += ""
$readmeLines += "### Database/"
$readmeLines += "Script per configurazione database."
$readmeLines += ""
$readmeLines += "## Installazione Rapida"
$readmeLines += ""
$readmeLines += "1. Estrai il pacchetto ZIP"
$readmeLines += "2. Esegui Install-Simple.ps1 come Amministratore"
$readmeLines += "3. Testa su http://localhost"
$readmeLines += ""
$readmeLines += "## Installazione Completa"
$readmeLines += ""
$readmeLines += "Usa Install-Deploy-Package.ps1 per installazione completa con verifiche."

$readmeContent = $readmeLines -join "`r`n"
$readmeContent | Out-File "$tempPath\README.md" -Encoding UTF8
Write-Host "✓ README creato" -ForegroundColor Green

Write-Host "`n8. CREAZIONE ARCHIVIO ZIP..." -ForegroundColor Cyan

# Rimuovi zip esistente se presente
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

# Crea l'archivio ZIP
Compress-Archive -Path "$tempPath\*" -DestinationPath $zipPath -Force
Write-Host "✓ Archivio creato: $zipPath" -ForegroundColor Green

# Calcola dimensione
$zipSize = (Get-Item $zipPath).Length / 1MB
Write-Host "Dimensione: $([math]::Round($zipSize, 2)) MB" -ForegroundColor White

Write-Host "`n9. PULIZIA..." -ForegroundColor Cyan
Remove-Item $tempPath -Recurse -Force
Write-Host "✓ Files temporanei rimossi" -ForegroundColor Green

Write-Host "`n=== PACCHETTO CREATO CON SUCCESSO ===" -ForegroundColor Green
Write-Host "File: $zipPath" -ForegroundColor Cyan
Write-Host "Dimensione: $([math]::Round($zipSize, 2)) MB" -ForegroundColor White

Write-Host "`nPROSSIMI PASSI:" -ForegroundColor Yellow
Write-Host "1. Trasferisci il file ZIP sul server SVRGEST" -ForegroundColor White
Write-Host "2. Estrai il contenuto in una cartella temporanea" -ForegroundColor White
Write-Host "3. Esegui Install-Simple.ps1 come Amministratore" -ForegroundColor White
