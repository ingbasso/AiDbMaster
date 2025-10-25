# Script per installare il pacchetto di deploy AiDbMaster sul server di produzione
# Eseguire come Amministratore nella cartella dove è stato estratto il pacchetto ZIP

param(
    [string]$SitePath = "C:\inetpub\wwwroot\AiDbMaster",
    [string]$SiteName = "AiDbMaster", 
    [string]$AppPoolName = "AiDbMaster",
    [string]$DatabaseServer = "SVRGEST",
    [string]$DatabaseName = "AIDBMASTER",
    [switch]$SkipDatabaseUpdate,
    [switch]$SkipPermissionsUpdate,
    [switch]$ForcePermissionsUpdate
)

Write-Host "=== INSTALLAZIONE PACCHETTO DEPLOY AIDBMASTER ===" -ForegroundColor Green
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray
Write-Host "Server: $env:COMPUTERNAME" -ForegroundColor Yellow

# Verifica amministratore
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Questo script deve essere eseguito come Amministratore!"
    exit 1
}

# Verifica che siamo nella directory corretta (dove è stato estratto il pacchetto)
if (-not (Test-Path "App") -or -not (Test-Path "Install.ps1")) {
    Write-Error "Directory non valida! Eseguire dalla cartella dove è stato estratto il pacchetto ZIP."
    Write-Host "La cartella deve contenere: App\, Scripts\, Database\, Install.ps1" -ForegroundColor Yellow
    exit 1
}

# Importa modulo IIS
Import-Module WebAdministration -ErrorAction SilentlyContinue
if (-not (Get-Module WebAdministration)) {
    Write-Error "Modulo WebAdministration non disponibile. Verificare che IIS sia installato."
    exit 1
}

try {
    Write-Host "`n=== FASE 1: PREPARAZIONE ===" -ForegroundColor Magenta
    
    Write-Host "Verifica prerequisiti..." -ForegroundColor Cyan
    
    # Verifica .NET Runtime
    try {
        $dotnetVersion = & dotnet --version 2>$null
        if ($dotnetVersion) {
            Write-Host "✓ .NET Runtime: $dotnetVersion" -ForegroundColor Green
        } else {
            Write-Host "⚠️ .NET Runtime non rilevato" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️ .NET Runtime non rilevato" -ForegroundColor Yellow
    }
    
    # Verifica Application Pool
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Write-Host "✓ Application Pool '$AppPoolName' trovato" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Application Pool '$AppPoolName' non trovato - verrà creato" -ForegroundColor Yellow
    }
    
    # Verifica sito
    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        Write-Host "✓ Sito '$SiteName' trovato" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Sito '$SiteName' non trovato - verrà creato" -ForegroundColor Yellow
    }

    Write-Host "`n=== FASE 2: ARRESTO SERVIZI ===" -ForegroundColor Magenta
    
    # Ferma Application Pool se esiste
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Write-Host "Arresto Application Pool..." -ForegroundColor Cyan
        Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
        
        # Attendi che si fermi
        $timeout = 30
        $elapsed = 0
        do {
            Start-Sleep -Seconds 1
            $elapsed++
            $state = (Get-IISAppPool -Name $AppPoolName).State
        } while ($state -eq "Stopping" -and $elapsed -lt $timeout)
        
        Write-Host "✓ Application Pool fermato" -ForegroundColor Green
    }

    Write-Host "`n=== FASE 3: BACKUP CONFIGURAZIONE ===" -ForegroundColor Magenta
    
    $backupTimestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupPath = "$SitePath.backup.$backupTimestamp"
    
    # Backup appsettings.Production.json se esiste
    if (Test-Path "$SitePath\appsettings.Production.json") {
        Copy-Item "$SitePath\appsettings.Production.json" "$backupPath.appsettings.Production.json" -Force
        Write-Host "✓ Backup configurazione: $backupPath.appsettings.Production.json" -ForegroundColor Green
    }
    
    # Backup web.config se esiste
    if (Test-Path "$SitePath\web.config") {
        Copy-Item "$SitePath\web.config" "$backupPath.web.config" -Force
        Write-Host "✓ Backup web.config: $backupPath.web.config" -ForegroundColor Green
    }

    Write-Host "`n=== FASE 4: INSTALLAZIONE FILES ===" -ForegroundColor Magenta
    
    Write-Host "Installazione files applicazione..." -ForegroundColor Cyan
    
    # Crea directory se non esiste
    if (-not (Test-Path $SitePath)) {
        New-Item -ItemType Directory -Path $SitePath -Force | Out-Null
        Write-Host "✓ Directory creata: $SitePath" -ForegroundColor Green
    }
    
    # Copia tutti i files dell'applicazione
    Write-Host "Copia files da: .\App\" -ForegroundColor White
    Write-Host "Verso: $SitePath" -ForegroundColor White
    
    Copy-Item ".\App\*" $SitePath -Recurse -Force
    Write-Host "✓ Files applicazione copiati" -ForegroundColor Green
    
    # Ripristina configurazione di produzione se esisteva
    if (Test-Path "$backupPath.appsettings.Production.json") {
        Copy-Item "$backupPath.appsettings.Production.json" "$SitePath\appsettings.Production.json" -Force
        Remove-Item "$backupPath.appsettings.Production.json" -Force
        Write-Host "✓ Configurazione produzione ripristinata" -ForegroundColor Green
    }

    Write-Host "`n=== FASE 5: CONFIGURAZIONE IIS ===" -ForegroundColor Magenta
    
    if (-not $SkipPermissionsUpdate) {
        Write-Host "Verifica permessi esistenti..." -ForegroundColor Cyan
        
        # Verifica se i permessi sono già configurati correttamente
        $AppPoolIdentity = "IIS AppPool\$AppPoolName"
        $permissionsOk = $false
        
        if (Test-Path $SitePath) {
            try {
                $acl = Get-Acl $SitePath
                $appPoolAccess = $acl.Access | Where-Object { 
                    $_.IdentityReference -eq $AppPoolIdentity -and 
                    $_.AccessControlType -eq "Allow" -and
                    $_.FileSystemRights -match "FullControl|Modify"
                }
                
                if ($appPoolAccess) {
                    Write-Host "✓ Permessi Application Pool già configurati correttamente" -ForegroundColor Green
                    $permissionsOk = $true
                } else {
                    Write-Host "⚠️ Permessi Application Pool non trovati o insufficienti" -ForegroundColor Yellow
                }
            } catch {
                Write-Host "⚠️ Errore verifica permessi esistenti" -ForegroundColor Yellow
            }
        }
        
        # Configura permessi solo se necessario o se forzato
        if (-not $permissionsOk -or $ForcePermissionsUpdate) {
            if ($ForcePermissionsUpdate) {
                Write-Host "Riconfigurazione permessi forzata..." -ForegroundColor Yellow
            }
            Write-Host "Configurazione permessi necessaria..." -ForegroundColor Cyan
            
            if (Test-Path ".\Scripts\Fix-AppPool-Permission.ps1") {
                & ".\Scripts\Fix-AppPool-Permission.ps1" -SitePath $SitePath -AppPoolName $AppPoolName
            } else {
                Write-Host "⚠️ Script permessi non trovato - configurazione manuale necessaria" -ForegroundColor Yellow
            }
        } else {
            Write-Host "✓ Permessi già configurati - saltando riconfigurazione" -ForegroundColor Green
        }
    } else {
        Write-Host "⚠️ Verifica permessi saltata (parametro -SkipPermissionsUpdate)" -ForegroundColor Yellow
    }

    Write-Host "`n=== FASE 6: CONFIGURAZIONE DATABASE ===" -ForegroundColor Magenta
    
    if (-not $SkipDatabaseUpdate) {
        Write-Host "Configurazione e aggiornamento database..." -ForegroundColor Cyan
        
        # Verifica connection string
        $connectionString = $null
        if (Test-Path "$SitePath\appsettings.Production.json") {
            try {
                $config = Get-Content "$SitePath\appsettings.Production.json" | ConvertFrom-Json
                $connectionString = $config.ConnectionStrings.DefaultConnection
                
                if ($connectionString -like "*Integrated Security=True*") {
                    Write-Host "✓ Connection string configurata per Windows Authentication" -ForegroundColor Green
                } elseif ($connectionString -like "*User ID=*") {
                    Write-Host "⚠️ Connection string usa SQL Authentication" -ForegroundColor Yellow
                    Write-Host "  Considera di usare Windows Authentication per maggiore sicurezza" -ForegroundColor Gray
                } else {
                    Write-Host "⚠️ Connection string non riconosciuta" -ForegroundColor Yellow
                }
            } catch {
                Write-Host "⚠️ Errore lettura appsettings.Production.json" -ForegroundColor Yellow
            }
        }
        
        # 1. Verifica e configura permessi database se necessario
        Write-Host "`nVerifica permessi database..." -ForegroundColor Cyan
        
        $databasePermissionsOk = $false
        
        # Verifica se i permessi database sono già configurati
        try {
            $null = Get-Command sqlcmd -ErrorAction Stop
            
            # Test query per verificare se l'utente esiste e ha permessi
            $checkQuery = "USE [$DatabaseName]; SELECT COUNT(*) FROM sys.database_principals WHERE name = 'IIS AppPool\AiDbMaster'"
            $checkResult = & sqlcmd -S $DatabaseServer -E -Q $checkQuery -h -1 2>$null
            
            if ($LASTEXITCODE -eq 0 -and $checkResult -and $checkResult.Trim() -eq "1") {
                Write-Host "✓ Permessi database già configurati" -ForegroundColor Green
                $databasePermissionsOk = $true
            } else {
                Write-Host "⚠️ Permessi database non configurati o insufficienti" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "⚠️ Impossibile verificare permessi database" -ForegroundColor Yellow
        }
        
        # Configura permessi solo se necessario o se forzato
        if ((-not $databasePermissionsOk -or $ForcePermissionsUpdate) -and (Test-Path ".\Database\Database-Permissions-Correct.sql")) {
            if ($ForcePermissionsUpdate) {
                Write-Host "Riconfigurazione permessi database forzata..." -ForegroundColor Yellow
            }
            Write-Host "Configurazione permessi database necessaria..." -ForegroundColor Cyan
            
            try {
                Write-Host "Esecuzione script permessi database..." -ForegroundColor White
                $permissionsResult = & sqlcmd -S $DatabaseServer -E -i ".\Database\Database-Permissions-Correct.sql" 2>&1
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✓ Permessi database configurati" -ForegroundColor Green
                    # Mostra solo le righe importanti del risultato
                    $permissionsResult | Where-Object { $_ -like "*Login creato*" -or $_ -like "*Utente creato*" -or $_ -like "*già esistente*" } | 
                        ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
                } else {
                    Write-Host "⚠️ Errore configurazione permessi database" -ForegroundColor Yellow
                    Write-Host "Eseguire manualmente: .\Database\Database-Permissions-Correct.sql" -ForegroundColor White
                }
            } catch {
                Write-Host "⚠️ sqlcmd non disponibile - eseguire manualmente:" -ForegroundColor Yellow
                Write-Host "  .\Database\Database-Permissions-Correct.sql" -ForegroundColor White
            }
        } elseif ($databasePermissionsOk) {
            Write-Host "✓ Permessi database già configurati - saltando riconfigurazione" -ForegroundColor Green
        }
        
        # 2. Esegui Entity Framework migrations
        Write-Host "`nAggiornamento schema database (Entity Framework)..." -ForegroundColor Cyan
        
        try {
            # Cambia directory temporaneamente per eseguire dotnet ef
            Push-Location $SitePath
            
            # Verifica se dotnet ef è disponibile
            $efAvailable = $false
            try {
                $null = & dotnet ef --version 2>$null
                $efAvailable = $true
            } catch {
                Write-Host "⚠️ dotnet ef non disponibile - installazione..." -ForegroundColor Yellow
                try {
                    & dotnet tool install --global dotnet-ef 2>$null
                    $efAvailable = $true
                    Write-Host "✓ dotnet ef installato" -ForegroundColor Green
                } catch {
                    Write-Host "⚠️ Impossibile installare dotnet ef" -ForegroundColor Yellow
                }
            }
            
            if ($efAvailable) {
                Write-Host "Applicazione migrations..." -ForegroundColor White
                
                # Esegui database update
                $migrationResult = & dotnet ef database update --no-build 2>&1
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✓ Database aggiornato con successo" -ForegroundColor Green
                    
                    # Mostra informazioni sulle migrations applicate
                    $migrationResult | Where-Object { $_ -like "*Applying migration*" -or $_ -like "*Done*" } | 
                        ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
                } else {
                    Write-Host "⚠️ Errore durante l'aggiornamento database" -ForegroundColor Yellow
                    Write-Host "Dettagli errore:" -ForegroundColor Red
                    $migrationResult | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
                    
                    Write-Host "`nSoluzioni possibili:" -ForegroundColor Yellow
                    Write-Host "1. Verificare la connection string" -ForegroundColor White
                    Write-Host "2. Verificare i permessi database" -ForegroundColor White
                    Write-Host "3. Eseguire manualmente: dotnet ef database update" -ForegroundColor White
                }
            } else {
                Write-Host "⚠️ Entity Framework CLI non disponibile" -ForegroundColor Yellow
                Write-Host "Le migrations verranno applicate automaticamente al primo avvio dell'applicazione" -ForegroundColor White
            }
            
        } catch {
            Write-Host "⚠️ Errore durante l'aggiornamento database: $($_.Exception.Message)" -ForegroundColor Yellow
        } finally {
            Pop-Location
        }
        
        # 3. Esegui script SQL personalizzati se presenti
        if (Test-Path ".\Database\Update-Database.sql") {
            Write-Host "`nEsecuzione script SQL personalizzati..." -ForegroundColor Cyan
            
            try {
                $null = Get-Command sqlcmd -ErrorAction Stop
                
                Write-Host "Esecuzione Update-Database.sql..." -ForegroundColor White
                $updateResult = & sqlcmd -S $DatabaseServer -E -i ".\Database\Update-Database.sql" 2>&1
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✓ Script SQL personalizzati eseguiti" -ForegroundColor Green
                } else {
                    Write-Host "⚠️ Errore script SQL personalizzati" -ForegroundColor Yellow
                    Write-Host "Eseguire manualmente: .\Database\Update-Database.sql" -ForegroundColor White
                }
            } catch {
                Write-Host "⚠️ sqlcmd non disponibile per script personalizzati" -ForegroundColor Yellow
                Write-Host "Eseguire manualmente: .\Database\Update-Database.sql" -ForegroundColor White
            }
        }
        
        # 4. Verifica connessione database
        Write-Host "`nVerifica connessione database..." -ForegroundColor Cyan
        
        if ($connectionString) {
            try {
                # Estrai server e database dalla connection string
                $server = if ($connectionString -match "Data Source=([^;]+)") { $matches[1] } else { $DatabaseServer }
                $database = if ($connectionString -match "Initial Catalog=([^;]+)") { $matches[1] } else { $DatabaseName }
                
                # Test connessione semplice
                $testQuery = "SELECT 1 as TestConnection"
                $testResult = & sqlcmd -S $server -E -Q $testQuery -d $database 2>$null
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✓ Connessione database verificata" -ForegroundColor Green
                } else {
                    Write-Host "⚠️ Impossibile verificare connessione database" -ForegroundColor Yellow
                }
            } catch {
                Write-Host "⚠️ Errore verifica connessione database" -ForegroundColor Yellow
            }
        }
        
    } else {
        Write-Host "⚠️ Aggiornamento database saltato (parametro -SkipDatabaseUpdate)" -ForegroundColor Yellow
    }

    Write-Host "`n=== FASE 7: AVVIO SERVIZI ===" -ForegroundColor Magenta
    
    Write-Host "Avvio Application Pool..." -ForegroundColor Cyan
    Start-WebAppPool -Name $AppPoolName
    
    # Attendi che si avvii
    $timeout = 30
    $elapsed = 0
    do {
        Start-Sleep -Seconds 1
        $elapsed++
        $state = (Get-IISAppPool -Name $AppPoolName).State
    } while ($state -eq "Starting" -and $elapsed -lt $timeout)
    
    if ($state -eq "Started") {
        Write-Host "✓ Application Pool avviato" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Application Pool non avviato correttamente (stato: $state)" -ForegroundColor Yellow
    }
    
    # Avvia sito se esiste
    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        $siteState = (Get-Website -Name $SiteName).State
        if ($siteState -ne "Started") {
            Start-Website -Name $SiteName -ErrorAction SilentlyContinue
            Write-Host "✓ Sito web avviato" -ForegroundColor Green
        } else {
            Write-Host "✓ Sito web già in esecuzione" -ForegroundColor Green
        }
    }

    Write-Host "`n=== FASE 8: VERIFICA INSTALLAZIONE ===" -ForegroundColor Magenta
    
    Write-Host "Verifica files critici..." -ForegroundColor Cyan
    
    $criticalFiles = @("AiDbMaster.dll", "appsettings.json", "web.config")
    $allFilesPresent = $true
    
    foreach ($file in $criticalFiles) {
        $filePath = Join-Path $SitePath $file
        if (Test-Path $filePath) {
            Write-Host "✓ $file presente" -ForegroundColor Green
        } else {
            Write-Host "✗ $file MANCANTE" -ForegroundColor Red
            $allFilesPresent = $false
        }
    }
    
    # Verifica cartelle necessarie
    $requiredFolders = @("Logs", "App_Data", "Uploads", "Shared")
    foreach ($folder in $requiredFolders) {
        $folderPath = Join-Path $SitePath $folder
        if (Test-Path $folderPath) {
            Write-Host "✓ Cartella $folder presente" -ForegroundColor Green
        } else {
            New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
            Write-Host "✓ Cartella $folder creata" -ForegroundColor Green
        }
    }

    Write-Host "`n=== INSTALLAZIONE COMPLETATA ===" -ForegroundColor Green
    
    if ($allFilesPresent) {
        Write-Host "🎉 INSTALLAZIONE RIUSCITA!" -ForegroundColor Green
        
        Write-Host "`nINFORMAZIONI DEPLOY:" -ForegroundColor Yellow
        Write-Host "Sito: $SiteName" -ForegroundColor White
        Write-Host "URL: http://localhost" -ForegroundColor Cyan
        Write-Host "Path: $SitePath" -ForegroundColor White
        Write-Host "Application Pool: $AppPoolName" -ForegroundColor White
        Write-Host "Logs: $SitePath\Logs\" -ForegroundColor White
        
        Write-Host "`nPROSSIMI PASSI:" -ForegroundColor Yellow
        Write-Host "1. 🌐 Testa l'applicazione: http://localhost" -ForegroundColor White
        Write-Host "2. 📊 Controlla i log se ci sono errori" -ForegroundColor White
        Write-Host "3. 🗄️ Esegui script database se necessario" -ForegroundColor White
        Write-Host "4. 🔒 Configura HTTPS se richiesto" -ForegroundColor White
        
        Write-Host "`nPARAMETRI DISPONIBILI:" -ForegroundColor Yellow
        Write-Host "-SkipDatabaseUpdate      : Salta aggiornamento database" -ForegroundColor Gray
        Write-Host "-SkipPermissionsUpdate   : Salta verifica/aggiornamento permessi" -ForegroundColor Gray
        Write-Host "-ForcePermissionsUpdate  : Forza riconfigurazione permessi anche se già presenti" -ForegroundColor Gray
        
    } else {
        Write-Host "⚠️ INSTALLAZIONE PARZIALE" -ForegroundColor Yellow
        Write-Host "Alcuni files critici sono mancanti. Verifica il pacchetto di deploy." -ForegroundColor Red
    }

} catch {
    Write-Error "Errore durante l'installazione: $($_.Exception.Message)"
    Write-Host "Dettagli: $($_.Exception)" -ForegroundColor Red
    
    # Tentativo di rollback
    Write-Host "`nTentativo di rollback..." -ForegroundColor Yellow
    try {
        if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
            Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
        }
        Write-Host "✓ Rollback completato" -ForegroundColor Green
    } catch {
        Write-Host "✗ Errore durante il rollback" -ForegroundColor Red
    }
    
    exit 1
}
