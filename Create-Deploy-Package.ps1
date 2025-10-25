# ============================================================================
# AIDBMASTER - BUILD AND PACKAGE SCRIPT
# ============================================================================
# Questo script pulisce, compila e crea un package ZIP per il deploy in produzione
# Da eseguire sul server di sviluppo
# ============================================================================

param(
    [string]$ProjectPath = ".",
    [string]$OutputZip = "AiDbMaster-Deploy-$(Get-Date -Format 'yyyyMMdd-HHmmss').zip",
    [string]$ProjectName = "AiDbMaster"
)

# Colori per output
$SuccessColor = "Green"
$ErrorColor = "Red" 
$InfoColor = "Cyan"
$WarningColor = "Yellow"

# Funzioni helper per output colorato
function Write-Step {
    param([string]$Message)
    Write-Host "`n🔄 $Message" -ForegroundColor $InfoColor
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $SuccessColor
}

function Write-ErrorMsg {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $ErrorColor
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $WarningColor
}

# ============================================================================
# MAIN SCRIPT
# ============================================================================

try {
    Write-Host "============================================================================" -ForegroundColor $InfoColor
    Write-Host "AIDBMASTER - BUILD AND PACKAGE" -ForegroundColor $InfoColor
    Write-Host "============================================================================" -ForegroundColor $InfoColor
    Write-Host "Inizio: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor $InfoColor
    Write-Host "Directory: $((Get-Location).Path)" -ForegroundColor $InfoColor
    Write-Host ""

    # ============================================================================
    # STEP 1: Verifica prerequisiti
    # ============================================================================
    
    Write-Step "STEP 1: Verifica prerequisiti"
    
    # Verifica .NET SDK
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Success ".NET SDK versione: $dotnetVersion"
    } else {
        Write-ErrorMsg ".NET SDK non trovato. Installare .NET SDK."
        exit 1
    }
    
    # Verifica file progetto
    if (-not (Test-Path "$ProjectName.sln")) {
        Write-ErrorMsg "File $ProjectName.sln non trovato in $ProjectPath"
        exit 1
    }
    Write-Success "Progetto principale: $ProjectName.sln"
    
    # Trova il file .csproj principale
    $csprojFiles = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -Recurse | Where-Object { $_.Directory.Name -notlike "*Test*" }
    if ($csprojFiles.Count -eq 0) {
        Write-ErrorMsg "Nessun file .csproj trovato"
        exit 1
    }
    
    $mainProjectFile = $csprojFiles[0].FullName
    Write-Success "File progetto: $($csprojFiles[0].Name)"

    # ============================================================================
    # STEP 2: Pulizia file compilati
    # ============================================================================
    
    Write-Step "STEP 2: Pulizia file compilati"
    
    # Rimuovi directory di output esistenti
    $dirsToClean = @("bin", "obj", "publish")
    foreach ($dir in $dirsToClean) {
        if (Test-Path $dir) {
            Remove-Item -Path $dir -Recurse -Force
            Write-Success "Rimossa directory: $dir"
        }
    }
    
    # Rimuovi ZIP precedente se esiste
    if (Test-Path $OutputZip) {
        Remove-Item -Path $OutputZip -Force
        Write-Success "Rimosso ZIP precedente: $OutputZip"
    }

    # ============================================================================
    # STEP 3: Restore pacchetti NuGet
    # ============================================================================
    
    Write-Step "STEP 3: Restore pacchetti NuGet"
    
    dotnet restore --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Restore NuGet completato"
    } else {
        Write-ErrorMsg "Errore durante restore NuGet"
        exit 1
    }

    # ============================================================================
    # STEP 4: Build produzione (Release)
    # ============================================================================
    
    Write-Step "STEP 4: Build produzione (Release)"
    
    dotnet publish "$mainProjectFile" --configuration Release --output "./publish" --verbosity quiet --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build produzione completata"
    } else {
        Write-ErrorMsg "Errore durante build produzione"
        exit 1
    }
    
    # Verifica output build
    if (!(Test-Path "./publish")) {
        Write-ErrorMsg "Directory publish non creata"
        exit 1
    }
    
    $publishedFiles = Get-ChildItem -Path "./publish" -Recurse | Measure-Object
    Write-Success "File pubblicati: $($publishedFiles.Count)"

    # ============================================================================
    # STEP 5: Preparazione struttura package
    # ============================================================================
    
    Write-Step "STEP 5: Preparazione struttura package"
    
    # Crea directory temporanea per il package
    $tempPackageDir = "temp_package_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    New-Item -ItemType Directory -Path $tempPackageDir -Force | Out-Null
    New-Item -ItemType Directory -Path "$tempPackageDir/App" -Force | Out-Null
    New-Item -ItemType Directory -Path "$tempPackageDir/Database" -Force | Out-Null
    New-Item -ItemType Directory -Path "$tempPackageDir/Scripts" -Force | Out-Null
    New-Item -ItemType Directory -Path "$tempPackageDir/Config" -Force | Out-Null
    Write-Success "Struttura directory creata: $tempPackageDir"
    
    # Copia file applicazione dalla directory publish
    Copy-Item -Path "./publish/*" -Destination "$tempPackageDir/App" -Recurse -Force
    Write-Success "File applicazione copiati"
    
    # Copia appsettings.Production.json se esiste
    if (Test-Path "appsettings.Production.json") {
        Copy-Item "appsettings.Production.json" "$tempPackageDir/App/" -Force
        Write-Success "appsettings.Production.json copiato"
    }

    # ============================================================================
    # GENERAZIONE SCRIPT SQL DA ENTITY FRAMEWORK
    # ============================================================================
    
    Write-Host "Generazione script SQL da Entity Framework..." -ForegroundColor $InfoColor
    
    try {
        # Verifica se dotnet ef è disponibile
        $efToolCheck = dotnet tool list --global | Select-String "dotnet-ef"
        if (!$efToolCheck) {
            Write-Host "Installazione dotnet-ef tool..." -ForegroundColor $InfoColor
            dotnet tool install --global dotnet-ef
            Write-Success "dotnet-ef tool installato"
        }
        
        # Genera script SQL completo per tutte le migration
        Write-Host "Generazione script SQL completo..." -ForegroundColor $InfoColor
        $efScriptResult = dotnet ef migrations script --output "EF-Migrations.sql" --verbose 2>&1
        
        if ($LASTEXITCODE -eq 0 -and (Test-Path "EF-Migrations.sql")) {
            Write-Success "Script SQL generato: EF-Migrations.sql"
            Copy-Item -Path "EF-Migrations.sql" -Destination "$tempPackageDir/Database/" -Force
            
            # Verifica dimensione file
            $scriptSize = (Get-Item "EF-Migrations.sql").Length
            if ($scriptSize -gt 0) {
                Write-Success "Script SQL valido ($(($scriptSize/1KB).ToString('F1')) KB)"
            } else {
                Write-Warning "Script SQL vuoto - probabilmente nessuna migration da applicare"
            }
        } else {
            Write-Warning "Errore nella generazione script EF o file vuoto"
        }
        
    } catch {
        Write-Warning "Errore durante generazione script EF: $($_.Exception.Message)"
    }
    
    # ============================================================================
    # CREAZIONE SCRIPT SQL MANUALE
    # ============================================================================
    
    Write-Host "Creazione script SQL base..." -ForegroundColor $InfoColor
    
    # Crea script SQL base manualmente
    $sqlScript = @"
-- Script per aggiornare il database AiDbMaster
-- Generato automaticamente il $(Get-Date)

USE [AIDBMASTER];
GO

-- Verifica e crea la tabella __EFMigrationsHistory se non esiste
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory](
        [MigrationId] [nvarchar](150) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
    );
    PRINT 'Tabella __EFMigrationsHistory creata';
END
ELSE
BEGIN
    PRINT 'Tabella __EFMigrationsHistory già esistente';
END
GO

-- Verifica permessi Application Pool Identity
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'IIS AppPool\AiDbMaster')
BEGIN
    PRINT 'ATTENZIONE: Utente "IIS AppPool\AiDbMaster" non trovato nel database!';
    PRINT 'Eseguire prima lo script Database-Permissions-Correct.sql';
END
ELSE
BEGIN
    PRINT 'Utente "IIS AppPool\AiDbMaster" configurato correttamente';
END
GO

PRINT '=== SCRIPT DATABASE COMPLETATO ===';
"@

    $sqlScript | Out-File "$tempPackageDir/Database/Update-Database.sql" -Encoding UTF8
    Write-Success "Script SQL base creato"

    # Copia script di permessi database se esiste
    if (Test-Path "Database-Permissions-Correct.sql") {
        Copy-Item "Database-Permissions-Correct.sql" "$tempPackageDir/Database/" -Force
        Write-Success "Script permessi database copiato"
    }

    # ============================================================================
    # COPIA SCRIPT DI DEPLOY
    # ============================================================================
    
    Write-Host "Copia script di deploy..." -ForegroundColor $InfoColor
    
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
            Copy-Item $script "$tempPackageDir/Scripts/" -Force
            Write-Success "$script copiato"
        } else {
            Write-Warning "$script non trovato"
        }
    }

    # ============================================================================
    # CREAZIONE SCRIPT DI INSTALLAZIONE SEMPLICE
    # ============================================================================
    
    Write-Host "Creazione script installazione semplice..." -ForegroundColor $InfoColor
    
    $installScript = @"
# Script di installazione semplice per AiDbMaster
# Eseguire come Amministratore

Write-Host '=== INSTALLAZIONE AIDBMASTER ===' -ForegroundColor Green

# Verifica amministratore
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')) {
    Write-Error 'Eseguire come Amministratore!'
    exit 1
}

# Importa modulo IIS
Import-Module WebAdministration -ErrorAction SilentlyContinue

# Ferma Application Pool
Write-Host 'Arresto Application Pool...' -ForegroundColor Cyan
Stop-WebAppPool -Name 'AiDbMaster' -ErrorAction SilentlyContinue

# Copia files
Write-Host 'Copia files applicazione...' -ForegroundColor Cyan
`$sitePath = 'C:\inetpub\wwwroot\AiDbMaster'
if (-not (Test-Path `$sitePath)) {
    New-Item -ItemType Directory -Path `$sitePath -Force | Out-Null
}
Copy-Item '.\App\*' `$sitePath -Recurse -Force

# Avvia Application Pool
Write-Host 'Avvio Application Pool...' -ForegroundColor Cyan
Start-WebAppPool -Name 'AiDbMaster'

Write-Host '=== INSTALLAZIONE COMPLETATA ===' -ForegroundColor Green
Write-Host 'Testa applicazione: http://localhost' -ForegroundColor Cyan
"@

    $installScript | Out-File "$tempPackageDir/Install-Simple.ps1" -Encoding UTF8
    Write-Success "Script installazione semplice creato"

    # ============================================================================
    # CREAZIONE ISTRUZIONI
    # ============================================================================
    
    $istruzioni = @"
============================================================================
AIDBMASTER - PACKAGE PER DEPLOY PRODUZIONE
============================================================================
Data Creazione: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')
Server Sviluppo: $env:COMPUTERNAME
Directory Sorgenti: $ProjectPath

TARGET PRODUZIONE:
- Server: SVRGEST
- Database: AIDBMASTER (istanza SVRGEST)
- Directory App: C:\inetpub\wwwroot\AiDbMaster

CONTENUTO PACKAGE:
- App/          → File applicazione compilata
- Database/     → Script SQL per aggiornamento database
- Scripts/      → Script di configurazione IIS e permessi
- Config/       → Template configurazioni

ISTRUZIONI DEPLOY:
1. Copiare questo ZIP sul server SVRGEST
2. Estrarre in una directory temporanea
3. Eseguire Install-Simple.ps1 come Amministratore

SCRIPT SQL INCLUSI:
- EF-Migrations.sql              → Script SQL completo generato da EF
- Update-Database.sql            → Script SQL base
- Database-Permissions-Correct.sql → Permessi database

CONFIGURAZIONE:
- Verificare appsettings.Production.json
- Controllare connection string per database AIDBMASTER
- Verificare permessi IIS

Per supporto contattare il team di sviluppo.
============================================================================
"@
    
    $istruzioni | Out-File "$tempPackageDir/ISTRUZIONI-DEPLOY.txt" -Encoding UTF8
    Write-Success "File ISTRUZIONI-DEPLOY.txt creato"

    # ============================================================================
    # STEP 6: Creazione ZIP finale
    # ============================================================================
    
    Write-Step "STEP 6: Creazione ZIP finale"
    
    # Crea ZIP con tutto il contenuto
    Compress-Archive -Path "$tempPackageDir/*" -DestinationPath $OutputZip -Force
    
    if (Test-Path $OutputZip) {
        $zipInfo = Get-Item $OutputZip
        Write-Success "ZIP creato: $OutputZip"
        Write-Success "Dimensione: $([math]::Round($zipInfo.Length / 1MB, 2)) MB"
        Write-Success "Percorso completo: $($zipInfo.FullName)"
    } else {
        Write-ErrorMsg "Errore nella creazione del ZIP"
        exit 1
    }
    
    # Pulizia directory temporanea
    Remove-Item -Path $tempPackageDir -Recurse -Force
    Write-Success "Directory temporanea rimossa"

    # Pulizia file temporanei
    if (Test-Path "EF-Migrations.sql") {
        Remove-Item "EF-Migrations.sql" -Force
    }

    # ============================================================================
    # COMPLETAMENTO
    # ============================================================================
    
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor $SuccessColor
    Write-Host "BUILD AND PACKAGE COMPLETATO CON SUCCESSO" -ForegroundColor $SuccessColor
    Write-Host "============================================================================" -ForegroundColor $SuccessColor
    Write-Host "File ZIP: $OutputZip" -ForegroundColor $SuccessColor
    Write-Host "Pronto per il deploy su SVRGEST" -ForegroundColor $SuccessColor
    Write-Host "Fine: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor $SuccessColor
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor $ErrorColor
    Write-Host "ERRORE DURANTE BUILD AND PACKAGE" -ForegroundColor $ErrorColor
    Write-Host "============================================================================" -ForegroundColor $ErrorColor
    Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $ErrorColor
    Write-Host "Riga: $($_.InvocationInfo.ScriptLineNumber)" -ForegroundColor $ErrorColor
    Write-Host "============================================================================" -ForegroundColor $ErrorColor
    exit 1
}