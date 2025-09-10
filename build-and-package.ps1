# Script per Build e Packaging dell'applicazione AiDbMaster
# Autore: AI Assistant
# Descrizione: Compila l'app, genera script migrazioni DB e crea ZIP per produzione

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\Deploy",
    [string]$ProjectPath = ".\AiDbMaster.csproj",
    [switch]$SkipBuild = $false,
    [switch]$Verbose = $false
)

# Funzione per logging
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

# Funzione per verificare se un comando esiste
function Test-Command {
    param([string]$Command)
    try {
        Get-Command $Command -ErrorAction Stop | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

# Inizio script
Write-Log "=== AVVIO BUILD E PACKAGING AIDBMASTER ===" "SUCCESS"

# Verifica prerequisiti
Write-Log "Verifica prerequisiti..."

if (-not (Test-Path $ProjectPath)) {
    Write-Log "File progetto non trovato: $ProjectPath" "ERROR"
    exit 1
}

if (-not (Test-Command "dotnet")) {
    Write-Log ".NET CLI non trovato. Installare .NET SDK." "ERROR"
    exit 1
}

# Crea cartella output
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$deployFolder = Join-Path $OutputPath "AiDbMaster-Deploy-$timestamp"
$appFolder = Join-Path $deployFolder "App"
$dbFolder = Join-Path $deployFolder "Database"

Write-Log "Creazione cartelle di output..."
New-Item -Path $deployFolder -ItemType Directory -Force | Out-Null
New-Item -Path $appFolder -ItemType Directory -Force | Out-Null
New-Item -Path $dbFolder -ItemType Directory -Force | Out-Null

try {
    # 1. RESTORE PACKAGES
    Write-Log "Ripristino pacchetti NuGet..."
    $restoreResult = dotnet restore $ProjectPath
    if ($LASTEXITCODE -ne 0) {
        Write-Log "Errore durante il restore dei pacchetti" "ERROR"
        throw "Restore fallito"
    }
    Write-Log "Pacchetti ripristinati con successo" "SUCCESS"

    # 2. BUILD DELL'APPLICAZIONE
    if (-not $SkipBuild) {
        Write-Log "Compilazione dell'applicazione in modalità $Configuration..."
        $buildArgs = @(
            "build"
            $ProjectPath
            "--configuration", $Configuration
            "--no-restore"
        )
        
        if ($Verbose) {
            $buildArgs += "--verbosity", "detailed"
        }
        
        & dotnet @buildArgs
        if ($LASTEXITCODE -ne 0) {
            Write-Log "Errore durante la compilazione" "ERROR"
            throw "Build fallita"
        }
        Write-Log "Compilazione completata con successo" "SUCCESS"
    }

    # 3. PUBLISH DELL'APPLICAZIONE
    Write-Log "Pubblicazione dell'applicazione per produzione..."
    
    # Prova prima con runtime specifico
    Write-Log "Tentativo publish con runtime win-x64..."
    $publishArgs = @(
        "publish"
        $ProjectPath
        "--configuration", $Configuration
        "--output", $appFolder
        "--runtime", "win-x64"
        "--self-contained", "false"
    )
    
    & dotnet @publishArgs
    
    # Se fallisce, prova senza runtime specifico (portable)
    if ($LASTEXITCODE -ne 0) {
        Write-Log "Publish con runtime fallita, provo modalità portable..." "WARN"
        
        # Pulisci la cartella output
        if (Test-Path $appFolder) {
            Remove-Item $appFolder -Recurse -Force
            New-Item -Path $appFolder -ItemType Directory -Force | Out-Null
        }
        
        $publishArgsPortable = @(
            "publish"
            $ProjectPath
            "--configuration", $Configuration
            "--output", $appFolder
            "--self-contained", "false"
        )
        
        & dotnet @publishArgsPortable
        if ($LASTEXITCODE -ne 0) {
            Write-Log "Errore durante la pubblicazione portable" "ERROR"
            throw "Publish fallita"
        }
        Write-Log "Pubblicazione portable completata con successo" "SUCCESS"
    } else {
        Write-Log "Pubblicazione con runtime completata con successo" "SUCCESS"
    }

    # 4. GENERAZIONE SCRIPT MIGRAZIONI DATABASE
    Write-Log "Generazione script migrazioni Entity Framework..."
    
    # Verifica se ci sono migrazioni pending
    Write-Log "Controllo migrazioni pending..."
    $migrationsCheck = dotnet ef migrations list --project $ProjectPath --no-build 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        # Genera script SQL per tutte le migrazioni dalla baseline
        $sqlScriptPath = Join-Path $dbFolder "update-database.sql"
        
        Write-Log "Generazione script SQL completo..."
        $efArgs = @(
            "ef", "migrations", "script"
            "--project", $ProjectPath
            "--output", $sqlScriptPath
            "--idempotent"
            "--no-build"
        )
        
        & dotnet @efArgs
        
        if ($LASTEXITCODE -eq 0 -and (Test-Path $sqlScriptPath)) {
            Write-Log "Script SQL generato: $sqlScriptPath" "SUCCESS"
            
            # Crea anche un file con le informazioni sulle migrazioni
            $migrationInfoPath = Join-Path $dbFolder "migrations-info.txt"
            $migrationInfo = @"
INFORMAZIONI MIGRAZIONI DATABASE
================================
Data generazione: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Progetto: $ProjectPath
Configurazione: $Configuration

ISTRUZIONI PER L'APPLICAZIONE:
1. Eseguire lo script 'update-database.sql' sul database di produzione AIDBMASTER
2. Lo script è idempotente (può essere eseguito più volte senza problemi)
3. Verificare che la connection string punti al server SVRGEST

MIGRAZIONI INCLUSE:
$migrationsCheck
"@
            Set-Content -Path $migrationInfoPath -Value $migrationInfo -Encoding UTF8
            Write-Log "File informazioni migrazioni creato: $migrationInfoPath" "SUCCESS"
        } else {
            Write-Log "Errore nella generazione dello script SQL" "WARN"
            # Crea un file vuoto per indicare che non ci sono migrazioni
            $noMigrationsPath = Join-Path $dbFolder "no-migrations-needed.txt"
            Set-Content -Path $noMigrationsPath -Value "Nessuna migrazione database necessaria per questa versione." -Encoding UTF8
        }
    } else {
        Write-Log "Impossibile verificare le migrazioni. Possibile che non ci siano migrazioni configurate." "WARN"
        $noMigrationsPath = Join-Path $dbFolder "no-migrations-configured.txt"
        Set-Content -Path $noMigrationsPath -Value "Entity Framework non configurato o nessuna migrazione trovata." -Encoding UTF8
    }

    # 5. PULIZIA FILE NON NECESSARI
    Write-Log "Rimozione file non necessari per produzione..."
    
    # File da rimuovere dalla pubblicazione
    $filesToRemove = @(
        "appsettings.Development.json",
        "*.pdb",
        "*.xml" # Documentation files
    )
    
    foreach ($pattern in $filesToRemove) {
        $files = Get-ChildItem -Path $appFolder -Name $pattern -Recurse
        foreach ($file in $files) {
            $fullPath = Join-Path $appFolder $file
            if (Test-Path $fullPath) {
                Remove-Item $fullPath -Force
                if ($Verbose) {
                    Write-Log "Rimosso: $file"
                }
            }
        }
    }

    # 6. VERIFICA FILE ESSENZIALI
    Write-Log "Verifica file essenziali..."
    $essentialFiles = @(
        "AiDbMaster.dll",
        "AiDbMaster.exe",
        "web.config",
        "appsettings.json",
        "appsettings.Production.json"
    )
    
    $missingFiles = @()
    foreach ($file in $essentialFiles) {
        $filePath = Join-Path $appFolder $file
        if (-not (Test-Path $filePath)) {
            $missingFiles += $file
        }
    }
    
    if ($missingFiles.Count -gt 0) {
        Write-Log "ATTENZIONE: File essenziali mancanti: $($missingFiles -join ', ')" "WARN"
    } else {
        Write-Log "Tutti i file essenziali sono presenti" "SUCCESS"
    }

    # 7. CREAZIONE ZIP
    Write-Log "Creazione pacchetto ZIP..."
    $zipPath = "$deployFolder.zip"
    
    # Rimuovi ZIP esistente se presente
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    
    # Crea il ZIP
    Compress-Archive -Path "$deployFolder\*" -DestinationPath $zipPath -CompressionLevel Optimal
    
    if (Test-Path $zipPath) {
        $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
        Write-Log "Pacchetto ZIP creato: $zipPath ($zipSize MB)" "SUCCESS"
    } else {
        Write-Log "Errore nella creazione del ZIP" "ERROR"
        throw "Creazione ZIP fallita"
    }

    # 8. RIEPILOGO FINALE
    Write-Log "=== RIEPILOGO OPERAZIONI ===" "SUCCESS"
    Write-Log "Cartella deploy: $deployFolder"
    Write-Log "Pacchetto ZIP: $zipPath"
    Write-Log "Dimensione ZIP: $zipSize MB"
    
    $appFiles = (Get-ChildItem -Path $appFolder -Recurse -File).Count
    $dbFiles = (Get-ChildItem -Path $dbFolder -File).Count
    
    Write-Log "File applicazione: $appFiles"
    Write-Log "File database: $dbFiles"
    
    Write-Log "=== BUILD E PACKAGING COMPLETATO CON SUCCESSO ===" "SUCCESS"
    
    # Apri la cartella di output
    if (Test-Path $OutputPath) {
        Write-Log "Apertura cartella output..."
        Start-Process explorer.exe -ArgumentList $OutputPath
    }

} catch {
    Write-Log "ERRORE DURANTE IL PROCESSO: $($_.Exception.Message)" "ERROR"
    Write-Log "Stack trace: $($_.ScriptStackTrace)" "ERROR"
    exit 1
}

# Fine script
Write-Log "Script completato. Premere un tasto per continuare..."
Read-Host
