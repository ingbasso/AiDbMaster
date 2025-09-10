# Script Semplificato per Build e Packaging - AiDbMaster
# Versione semplificata per risolvere problemi di compatibilità SDK

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\Deploy"
)

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch ($Level) {
        "ERROR" { "Red" }
        "WARN" { "Yellow" }
        "SUCCESS" { "Green" }
        default { "White" }
    }
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
}

Write-Log "=== BUILD SEMPLIFICATO AIDBMASTER ===" "SUCCESS"

# Crea cartelle
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$deployFolder = Join-Path $OutputPath "AiDbMaster-Deploy-$timestamp"
$appFolder = Join-Path $deployFolder "App"
$dbFolder = Join-Path $deployFolder "Database"

New-Item -Path $deployFolder -ItemType Directory -Force | Out-Null
New-Item -Path $appFolder -ItemType Directory -Force | Out-Null
New-Item -Path $dbFolder -ItemType Directory -Force | Out-Null

try {
    # 1. CLEAN
    Write-Log "Pulizia progetto..."
    dotnet clean --configuration $Configuration
    
    # 2. RESTORE
    Write-Log "Restore pacchetti..."
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "Restore fallito" }
    
    # 3. BUILD
    Write-Log "Build progetto..."
    dotnet build --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build fallita" }
    
    # 4. PUBLISH (modalità portable - più compatibile)
    Write-Log "Publish modalità portable..."
    dotnet publish --configuration $Configuration --output $appFolder --no-build
    if ($LASTEXITCODE -ne 0) { throw "Publish fallita" }
    
    Write-Log "Pubblicazione completata con successo!" "SUCCESS"
    
    # 5. MIGRAZIONI DATABASE (opzionale)
    Write-Log "Tentativo generazione script migrazioni..."
    try {
        $sqlScript = Join-Path $dbFolder "update-database.sql"
        dotnet ef migrations script --output $sqlScript --idempotent
        
        if (Test-Path $sqlScript) {
            Write-Log "Script SQL generato: $sqlScript" "SUCCESS"
        } else {
            Write-Log "Nessuno script SQL generato (normale se non ci sono migrazioni)" "WARN"
            Set-Content -Path (Join-Path $dbFolder "no-migrations.txt") -Value "Nessuna migrazione necessaria"
        }
    }
    catch {
        Write-Log "Entity Framework non configurato o errore migrazioni: $($_.Exception.Message)" "WARN"
        Set-Content -Path (Join-Path $dbFolder "ef-error.txt") -Value "Errore EF: $($_.Exception.Message)"
    }
    
    # 6. PULIZIA FILE NON NECESSARI
    Write-Log "Pulizia file di sviluppo..."
    $filesToRemove = @("appsettings.Development.json", "*.pdb")
    foreach ($pattern in $filesToRemove) {
        Get-ChildItem -Path $appFolder -Name $pattern -Recurse | ForEach-Object {
            $file = Join-Path $appFolder $_
            if (Test-Path $file) { Remove-Item $file -Force }
        }
    }
    
    # 7. VERIFICA FILE ESSENZIALI
    $essentialFiles = @("AiDbMaster.dll", "web.config", "appsettings.json", "appsettings.Production.json")
    $missing = @()
    foreach ($file in $essentialFiles) {
        if (-not (Test-Path (Join-Path $appFolder $file))) {
            $missing += $file
        }
    }
    
    if ($missing.Count -gt 0) {
        Write-Log "ATTENZIONE - File mancanti: $($missing -join ', ')" "WARN"
    } else {
        Write-Log "Tutti i file essenziali sono presenti" "SUCCESS"
    }
    
    # 8. CREAZIONE ZIP
    Write-Log "Creazione ZIP..."
    $zipPath = "$deployFolder.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    
    Compress-Archive -Path "$deployFolder\*" -DestinationPath $zipPath -CompressionLevel Optimal
    
    if (Test-Path $zipPath) {
        $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
        Write-Log "ZIP creato: $zipPath ($zipSize MB)" "SUCCESS"
    }
    
    # 9. RIEPILOGO
    Write-Log "=== COMPLETATO CON SUCCESSO ===" "SUCCESS"
    Write-Log "Cartella: $deployFolder"
    Write-Log "ZIP: $zipPath"
    
    # Apri cartella
    Start-Process explorer.exe -ArgumentList $OutputPath
    
} catch {
    Write-Log "ERRORE: $($_.Exception.Message)" "ERROR"
    exit 1
}

Write-Log "Premere un tasto per continuare..."
Read-Host

