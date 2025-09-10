# Script per BUILD e creazione pacchetto ZIP per produzione
# Eseguire dal PC di sviluppo nella directory del progetto

param(
    [string]$OutputPath = ".\Release-Package",
    [string]$Version = (Get-Date -Format "yyyyMMdd-HHmm"),
    [switch]$SkipTests = $false,
    [switch]$NoClean
)

$ErrorActionPreference = "Stop"

Write-Host "=============================================" -ForegroundColor Green
Write-Host "    BUILD PACCHETTO PRODUZIONE" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host "Versione: $Version" -ForegroundColor Yellow
Write-Host "Output: $OutputPath" -ForegroundColor Yellow
Write-Host "Directory corrente: $(Get-Location)" -ForegroundColor Cyan
Write-Host ""

# Verifica prerequisiti
Write-Host "=== VERIFICA PREREQUISITI ===" -ForegroundColor Cyan

if (-not (Test-Path "AiDbMaster.csproj")) {
    Write-Error "❌ File AiDbMaster.csproj non trovato! Esegui lo script dalla directory del progetto."
    exit 1
}
Write-Host "✅ File progetto trovato" -ForegroundColor Green

try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -like "8.*") {
        Write-Host "✅ .NET 8.0: $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Warning "⚠️ .NET version: $dotnetVersion (consigliato 8.x)"
    }
} catch {
    Write-Error "❌ .NET SDK non installato!"
    exit 1
}

Write-Host ""

# Clean di default (a meno che non sia specificato -NoClean)
if (-not $NoClean) {
    Write-Host "=== PULIZIA PROGETTO ===" -ForegroundColor Cyan
    Write-Host "🧹 Cleaning..." -ForegroundColor White
    dotnet clean --configuration Release
    Write-Host "✅ Clean completato" -ForegroundColor Green
    Write-Host ""
}

# Restore dipendenze
Write-Host "=== RESTORE DIPENDENZE ===" -ForegroundColor Cyan
Write-Host "📦 Restore delle dipendenze..." -ForegroundColor White
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Errore durante il restore!"
    exit 1
}
Write-Host "✅ Restore completato" -ForegroundColor Green
Write-Host ""

# Build
Write-Host "=== BUILD PROGETTO ===" -ForegroundColor Cyan
Write-Host "🔨 Build in modalità Release..." -ForegroundColor White
dotnet build --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Errore durante il build!"
    exit 1
}
Write-Host "✅ Build completato" -ForegroundColor Green
Write-Host ""

# Test (opzionale)
if (-not $SkipTests) {
    Write-Host "=== ESECUZIONE TEST ===" -ForegroundColor Cyan
    Write-Host "🧪 Esecuzione test..." -ForegroundColor White
    try {
        dotnet test --configuration Release --no-build --verbosity minimal
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Tutti i test superati" -ForegroundColor Green
        } else {
            Write-Warning "⚠️ Alcuni test falliti, ma continuo (usa -SkipTests per saltare)"
        }
    } catch {
        Write-Warning "⚠️ Errore nell'esecuzione test, ma continuo"
    }
    Write-Host ""
}

# Publish
Write-Host "=== PUBLISH APPLICAZIONE ===" -ForegroundColor Cyan
$publishPath = "$OutputPath\AiDbMaster-$Version"
Write-Host "📤 Publishing in: $publishPath" -ForegroundColor White

# Rimuovi directory di output se esiste
if (Test-Path $publishPath) {
    Remove-Item $publishPath -Recurse -Force
}

dotnet publish --configuration Release --output $publishPath --no-build --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Errore durante il publish!"
    exit 1
}
Write-Host "✅ Publish completato" -ForegroundColor Green
Write-Host ""

# Aggiungi script di deploy
Write-Host "=== PREPARAZIONE SCRIPT DEPLOY ===" -ForegroundColor Cyan
Write-Host "📋 Aggiunta script di deploy..." -ForegroundColor White

# Copia gli script necessari
$scriptsToInclude = @(
    "Deploy-From-Package.ps1"
)

foreach ($script in $scriptsToInclude) {
    if (Test-Path $script) {
        Copy-Item $script -Destination $publishPath -Force
        Write-Host "  ✅ $script copiato" -ForegroundColor Green
    }
}

# Crea il file README per il deploy
$readmeLines = @(
    "# PACCHETTO PRODUZIONE AIDBMASTER",
    "Versione: $Version",
    "Build: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
    "",
    "## DEPLOY SUL SERVER:",
    "1. Estrai questo ZIP in una cartella temporanea sul server",
    "2. Apri PowerShell come amministratore", 
    "3. Naviga nella cartella estratta",
    "4. Esegui: .\Deploy-From-Package.ps1",
    "",
    "## CONTENUTO:",
    "- File applicazione compilati",
    "- Script di deploy automatico",
    "- Configurazioni di produzione",
    "",
    "## REQUISITI SERVER:",
    "- Windows Server con IIS",
    "- .NET 8.0 Runtime (non SDK)", 
    "- Database AIDBMASTER esistente",
    "- Certificato SSL per aidbmaster.it"
)

$readmeLines | Out-File -FilePath "$publishPath\README-DEPLOY.txt" -Encoding UTF8
Write-Host "  ✅ README-DEPLOY.txt creato" -ForegroundColor Green

# Crea informazioni sulla versione
$versionInfo = @{
    Version = $Version
    BuildDate = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    BuildMachine = $env:COMPUTERNAME
    BuildUser = $env:USERNAME
    GitCommit = if (Test-Path ".git") { try { git rev-parse HEAD } catch { "N/A" } } else { "N/A" }
    Features = @(
        "Cambio password utenti",
        "Sicurezza HTTPS potenziata", 
        "Policy CORS configurate",
        "Headers di sicurezza ottimizzati"
    )
}

$versionInfo | ConvertTo-Json -Depth 3 | Out-File -FilePath "$publishPath\version.json" -Encoding UTF8
Write-Host "  ✅ version.json creato" -ForegroundColor Green
Write-Host ""

# Creazione ZIP
Write-Host "=== CREAZIONE PACCHETTO ZIP ===" -ForegroundColor Cyan
$zipPath = "$OutputPath\AiDbMaster-$Version.zip"

Write-Host "📦 Creazione ZIP: $zipPath" -ForegroundColor White

# Rimuovi ZIP esistente se presente
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

# Crea ZIP usando .NET compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($publishPath, $zipPath)

# Verifica dimensione ZIP
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
Write-Host "✅ ZIP creato: $zipSize MB" -ForegroundColor Green
Write-Host ""

# Pulizia directory temporanea (mantieni solo ZIP)
Write-Host "🧹 Pulizia directory temporanea..." -ForegroundColor White
Remove-Item $publishPath -Recurse -Force
Write-Host "✅ Pulizia completata" -ForegroundColor Green
Write-Host ""

# Verifica finale
Write-Host "=== VERIFICA FINALE ===" -ForegroundColor Cyan
if (Test-Path $zipPath) {
    Write-Host "✅ Pacchetto ZIP verificato" -ForegroundColor Green
    
    # Lista contenuto ZIP per verifica
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
    $fileCount = $zip.Entries.Count
    $zip.Dispose()
    
    Write-Host "📊 Statistiche pacchetto:" -ForegroundColor Cyan
    Write-Host "  - File nel pacchetto: $fileCount" -ForegroundColor White
    Write-Host "  - Dimensione ZIP: $zipSize MB" -ForegroundColor White
    Write-Host "  - Percorso: $zipPath" -ForegroundColor White
} else {
    Write-Error "❌ Errore: ZIP non trovato!"
    exit 1
}

Write-Host ""

# Riepilogo finale
Write-Host "=============================================" -ForegroundColor Green
Write-Host "      PACCHETTO PRONTO!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "📦 File ZIP: $zipPath" -ForegroundColor Yellow
Write-Host "📁 Dimensione: $zipSize MB" -ForegroundColor Cyan
Write-Host "🏷️ Versione: $Version" -ForegroundColor Cyan
Write-Host ""
Write-Host "🚀 PROSSIMI PASSI:" -ForegroundColor Cyan
Write-Host "1. Trasferisci il file ZIP sul server di produzione" -ForegroundColor White
Write-Host "2. Estrai il ZIP in una cartella temporanea" -ForegroundColor White
Write-Host "3. Esegui: .\Deploy-From-Package.ps1" -ForegroundColor White
Write-Host ""
Write-Host "💡 Il pacchetto include:" -ForegroundColor Yellow
Write-Host "  • Applicazione compilata" -ForegroundColor White
Write-Host "  • Script di deploy automatico" -ForegroundColor White
Write-Host "  • Configurazioni di produzione" -ForegroundColor White
Write-Host "  • README con istruzioni" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Build completato con successo!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green 