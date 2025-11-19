# ============================================================================
# AIDBMASTER - DEPLOY PRODUCTION SCRIPT
# ============================================================================
# Script per il deploy in produzione su server SVRGEST
# Database: AIDBMASTER su istanza SVRGEST
# Directory App: C:\inetpub\wwwroot\AiDbMaster
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ZipPath,
    
    [string]$ServerName = "SVRGEST",
    [string]$DatabaseName = "AIDBMASTER", 
    [string]$AppDirectory = "C:\inetpub\wwwroot\AiDbMaster",
    [string]$BackupDirectory = "C:\Backup\AiDbMaster",
    [string]$AppPoolName = "AiDbMaster",
    [switch]$SkipBackup,
    [switch]$ForceUpdateWebConfig  # Se specificato, aggiorna web.config con quello del package
)

# Colori per output
$SuccessColor = "Green"
$ErrorColor = "Red"
$InfoColor = "Cyan" 
$WarningColor = "Yellow"

# Funzioni helper
function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor $InfoColor
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $SuccessColor
}

function Write-ErrorMsg {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $ErrorColor
}

function Write-WarningMsg {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $WarningColor
}

# ============================================================================
# MAIN SCRIPT
# ============================================================================

try {
    Write-Host "============================================================================" -ForegroundColor $InfoColor
    Write-Host "AIDBMASTER - DEPLOY PRODUZIONE" -ForegroundColor $InfoColor
    Write-Host "============================================================================" -ForegroundColor $InfoColor
    Write-Host "Inizio: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor $InfoColor
    Write-Host "Server: $env:COMPUTERNAME" -ForegroundColor $InfoColor
    Write-Host "Target: $ServerName\$DatabaseName" -ForegroundColor $InfoColor
    Write-Host ""

    # ============================================================================
    # STEP 1: VERIFICHE PREREQUISITI
    # ============================================================================
    
    Write-Step "STEP 1: Verifiche prerequisiti"
    
    # Verifica privilegi amministratore
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    if (!$currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-ErrorMsg "Eseguire lo script come Amministratore"
        exit 1
    }
    Write-Success "Privilegi amministratore: OK"
    
    # Verifica file ZIP
    if (!(Test-Path $ZipPath)) {
        Write-ErrorMsg "File ZIP non trovato: $ZipPath"
        exit 1
    }
    Write-Success "File ZIP trovato: $ZipPath"
    
    # Verifica connessione SQL Server
    try {
        $sqlTest = Invoke-Sqlcmd -ServerInstance $ServerName -Database "master" -Query "SELECT 1" -ErrorAction Stop
        Write-Success "Connessione SQL Server: OK"
    }
    catch {
        Write-ErrorMsg "Impossibile connettersi a SQL Server $ServerName"
        Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $ErrorColor
        exit 1
    }
    
    # Verifica esistenza database
    try {
        $dbTest = Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -Query "SELECT 1" -ErrorAction Stop
        Write-Success "Database $DatabaseName disponibile"
    }
    catch {
        Write-ErrorMsg "Database $DatabaseName non accessibile"
        Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $ErrorColor
        exit 1
    }
    
    # Verifica directory applicazione
    if (!(Test-Path $AppDirectory)) {
        Write-ErrorMsg "Directory applicazione non trovata: $AppDirectory"
        exit 1
    }
    Write-Success "Directory applicazione: OK"
    
    # Crea directory backup se non esiste
    if (!(Test-Path $BackupDirectory)) {
        New-Item -ItemType Directory -Path $BackupDirectory -Force | Out-Null
        Write-Success "Directory backup creata: $BackupDirectory"
    } else {
        Write-Success "Directory backup: OK"
    }

    # ============================================================================
    # STEP 2: BACKUP DATABASE
    # ============================================================================
    
    if (!$SkipBackup) {
        Write-Step "STEP 2: Backup database"
        
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupFileName = "AIDBMASTER_Backup_$timestamp.bak"
        $backupPath = Join-Path $BackupDirectory $backupFileName
        
        Write-Host "Esecuzione backup database..." -ForegroundColor $InfoColor
        Write-Host "Destinazione: $backupPath" -ForegroundColor $InfoColor
        
        $backupSql = "BACKUP DATABASE [$DatabaseName] TO DISK = '$backupPath' WITH FORMAT, COMPRESSION, CHECKSUM, NAME = 'AiDbMaster Full Backup $timestamp', DESCRIPTION = 'Backup automatico pre-deploy $timestamp'"
        
        try {
            Invoke-Sqlcmd -ServerInstance $ServerName -Database "master" -Query $backupSql -QueryTimeout 600
            
            if (Test-Path $backupPath) {
                $backupSize = (Get-Item $backupPath).Length / 1MB
                $backupSizeMB = [math]::Round($backupSize, 2)
                Write-Success "Backup completato: $backupFileName ($backupSizeMB MB)"
            } else {
                Write-ErrorMsg "File backup non trovato dopo operazione"
                exit 1
            }
        }
        catch {
            Write-ErrorMsg "Errore durante backup database"
            Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $ErrorColor
            exit 1
        }
    } else {
        Write-WarningMsg "STEP 2: Backup database SALTATO (parametro -SkipBackup)"
    }

    # ============================================================================
    # STEP 3: STOP SERVIZI (prima del backup applicazione)
    # ============================================================================
    
    Write-Step "STEP 3: Stop servizi IIS (per liberare file)"
    
    try {
        # Stop Application Pool
        if (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue) {
            Stop-WebAppPool -Name $AppPoolName
            Write-Success "Application Pool fermato: $AppPoolName"
        } else {
            Write-WarningMsg "Application Pool non trovato: $AppPoolName"
        }
        
        # Stop IIS
        iisreset /stop | Out-Null
        Write-Success "IIS fermato"
        
        Start-Sleep -Seconds 5
        Write-Success "File applicazione ora liberi per backup"
    }
    catch {
        Write-ErrorMsg "Errore durante stop servizi"
        Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $ErrorColor
        exit 1
    }

    # ============================================================================
    # STEP 4: BACKUP APPLICAZIONE
    # ============================================================================
    
    Write-Step "STEP 4: Backup applicazione corrente"
    
    $appBackupName = "App_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').zip"
    $appBackupPath = Join-Path $BackupDirectory $appBackupName
    
    try {
        Compress-Archive -Path "$AppDirectory\*" -DestinationPath $appBackupPath -Force
        $appBackupSize = (Get-Item $appBackupPath).Length / 1MB
        $appBackupSizeMB = [math]::Round($appBackupSize, 2)
        Write-Success "Backup applicazione completato: $appBackupName ($appBackupSizeMB MB)"
    }
    catch {
        Write-ErrorMsg "Errore durante backup applicazione"
        Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $ErrorColor
        exit 1
    }

    # ============================================================================
    # STEP 5: ESTRAZIONE E AGGIORNAMENTO DATABASE
    # ============================================================================
    
    Write-Step "STEP 5: Aggiornamento database"
    
    # Estrai ZIP in directory temporanea
    $tempDeployDir = "C:\temp\AiDbMasterDeploy_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    New-Item -ItemType Directory -Path $tempDeployDir -Force | Out-Null
    
    try {
        Expand-Archive -Path $ZipPath -DestinationPath $tempDeployDir -Force
        Write-Success "ZIP estratto in: $tempDeployDir"
        
        # ============================================================================
        # STEP 5A: VERIFICA SCRIPT SQL GENERATI DA ENTITY FRAMEWORK
        # ============================================================================
        
        Write-Host "Verifica script SQL generati da Entity Framework..." -ForegroundColor $InfoColor
        
        # Verifica che gli script SQL siano stati generati correttamente durante il build
        $efScript = Join-Path $tempDeployDir "Database\UPDATE_PRODUZIONE_EF.sql"
        $efIncrementalScript = Join-Path $tempDeployDir "Database\UPDATE_PRODUZIONE_INCREMENTAL.sql"
        
        if (Test-Path $efScript) {
            $scriptSize = (Get-Item $efScript).Length / 1KB
            Write-Success "Script EF completo trovato: UPDATE_PRODUZIONE_EF.sql ($(($scriptSize).ToString('F1')) KB)"
        } else {
            Write-WarningMsg "Script EF completo non trovato"
        }
        
        if (Test-Path $efIncrementalScript) {
            $scriptSize = (Get-Item $efIncrementalScript).Length / 1KB
            Write-Success "Script EF incrementale trovato: UPDATE_PRODUZIONE_INCREMENTAL.sql ($(($scriptSize).ToString('F1')) KB)"
        } else {
            Write-WarningMsg "Script EF incrementale non trovato"
        }
        
        Write-Host "Utilizzo script SQL pre-generati (non serve .NET SDK sul server)" -ForegroundColor $InfoColor
        
        # ============================================================================
        # STEP 5B: ESECUZIONE SCRIPT SQL (GENERATI DURANTE IL BUILD)
        # ============================================================================
        
        # Priorità: usa script EF generati durante il build, altrimenti script manuali
        $efScript = Join-Path $tempDeployDir "Database\UPDATE_PRODUZIONE_EF.sql"
        $efIncrementalScript = Join-Path $tempDeployDir "Database\UPDATE_PRODUZIONE_INCREMENTAL.sql"
        $manualScript = Join-Path $tempDeployDir "Database\UPDATE_PRODUZIONE_SEMPLICE.sql"
        
        $scriptExecuted = $false
        
        # Prova prima con lo script EF incrementale (idempotente, generato durante il build)
        if (Test-Path $efIncrementalScript) {
            Write-Host "Esecuzione script SQL incrementale (generato da EF)..." -ForegroundColor $InfoColor
            try {
                Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -InputFile $efIncrementalScript -QueryTimeout 600
                Write-Success "Script incrementale eseguito con successo: UPDATE_PRODUZIONE_INCREMENTAL.sql"
                $scriptExecuted = $true
            }
            catch {
                Write-WarningMsg "Errore con script incrementale: $($_.Exception.Message)"
                Write-Host "Provo con script completo..." -ForegroundColor $WarningColor
            }
        }
        
        # Se lo script incrementale fallisce, prova con quello completo
        if (!$scriptExecuted -and (Test-Path $efScript)) {
            Write-Host "Esecuzione script SQL completo (generato da EF)..." -ForegroundColor $InfoColor
            try {
                Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -InputFile $efScript -QueryTimeout 600
                Write-Success "Script completo eseguito con successo: UPDATE_PRODUZIONE_EF.sql"
                $scriptExecuted = $true
            }
            catch {
                Write-WarningMsg "Errore con script completo: $($_.Exception.Message)"
                Write-Host "Provo con script manuale di fallback..." -ForegroundColor $WarningColor
            }
        }
        
        # Fallback: usa script manuale se disponibile
        if (!$scriptExecuted -and (Test-Path $manualScript)) {
            Write-Host "Esecuzione script SQL manuale (fallback)..." -ForegroundColor $InfoColor
            try {
                Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -InputFile $manualScript -QueryTimeout 300
                Write-Success "Script manuale eseguito: UPDATE_PRODUZIONE_SEMPLICE.sql"
                $scriptExecuted = $true
            }
            catch {
                Write-ErrorMsg "Errore anche con script manuale: $($_.Exception.Message)"
            }
        }
        
        # Fallback finale: usa script di allineamento completo (idempotente con TUTTE le migration)
        $allineamentoScriptPackage = Join-Path $tempDeployDir "Database\ALLINEAMENTO_PRODUZIONE_COMPLETO.sql"
        $allineamentoScriptRoot = "C:\AiDbMaster\ALLINEAMENTO_PRODUZIONE_COMPLETO.sql"
        
        if (!$scriptExecuted -and (Test-Path $allineamentoScriptPackage)) {
            Write-Host "Esecuzione script di allineamento completo dal package (ultimo fallback)..." -ForegroundColor $InfoColor
            try {
                Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -InputFile $allineamentoScriptPackage -QueryTimeout 600
                Write-Success "Script di allineamento eseguito: ALLINEAMENTO_PRODUZIONE_COMPLETO.sql"
                $scriptExecuted = $true
            }
            catch {
                Write-ErrorMsg "Errore con script di allineamento dal package: $($_.Exception.Message)"
            }
        }
        
        # Fallback finale alternativo: cerca nella directory del progetto
        if (!$scriptExecuted -and (Test-Path $allineamentoScriptRoot)) {
            Write-Host "Esecuzione script di allineamento dalla directory progetto..." -ForegroundColor $InfoColor
            try {
                Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -InputFile $allineamentoScriptRoot -QueryTimeout 600
                Write-Success "Script di allineamento eseguito dalla directory progetto"
                $scriptExecuted = $true
            }
            catch {
                Write-ErrorMsg "Errore anche con script di allineamento dalla directory progetto: $($_.Exception.Message)"
            }
        }
        
        if (!$scriptExecuted) {
            Write-WarningMsg "Nessuno script SQL trovato o eseguito con successo"
            Write-Host "Il database potrebbe non essere aggiornato!" -ForegroundColor $WarningColor
            Write-Host "AZIONE CONSIGLIATA: Eseguire manualmente lo script ALLINEAMENTO_PRODUZIONE_COMPLETO.sql" -ForegroundColor $WarningColor
        }
    }
    catch {
        Write-ErrorMsg "Errore durante aggiornamento database"
        Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $ErrorColor
        
        # Riavvia servizi in caso di errore
        Write-Host "Riavvio servizi per ripristino..." -ForegroundColor $WarningColor
        iisreset /start | Out-Null
        if (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue) {
            Start-WebAppPool -Name $AppPoolName
        }
        exit 1
    }

    # ============================================================================
    # STEP 6: DEPLOY FILE APPLICAZIONE
    # ============================================================================
    
    Write-Step "STEP 6: Deploy file applicazione"
    
    try {
        # Backup configurazione esistente
        $existingConfig = Join-Path $AppDirectory "appsettings.json"
        $existingProdConfig = Join-Path $AppDirectory "appsettings.Production.json"
        $existingWebConfig = Join-Path $AppDirectory "web.config"
        $tempConfigBackup = Join-Path $tempDeployDir "config_backup.json"
        $tempProdConfigBackup = Join-Path $tempDeployDir "config_prod_backup.json"
        $tempWebConfigBackup = Join-Path $tempDeployDir "web_config_backup.xml"
        
        if (Test-Path $existingConfig) {
            Copy-Item $existingConfig $tempConfigBackup
            Write-Success "Backup configurazione esistente salvato"
        }
        
        if (Test-Path $existingProdConfig) {
            Copy-Item $existingProdConfig $tempProdConfigBackup
            Write-Success "Backup configurazione produzione esistente salvato"
        }
        
        # Backup del web.config esistente (IMPORTANTE!)
        if (Test-Path $existingWebConfig) {
            Copy-Item $existingWebConfig $tempWebConfigBackup
            Write-Success "Backup web.config esistente salvato"
        }
        
        # Copia nuovi file applicazione
        $appSourceDir = Join-Path $tempDeployDir "App"
        if (Test-Path $appSourceDir) {
            robocopy "$appSourceDir" "$AppDirectory" /MIR /R:3 /W:5 /NP /NDL /NFL | Out-Null
            if ($LASTEXITCODE -le 7) {
                Write-Success "File applicazione copiati"
            } else {
                throw "Errore robocopy: $LASTEXITCODE"
            }
        } else {
            throw "Directory App non trovata nel package"
        }
        
        # Ripristina configurazione se esisteva
        if (Test-Path $tempConfigBackup) {
            Copy-Item $tempConfigBackup $existingConfig -Force
            Write-Success "Configurazione esistente ripristinata"
        }
        
        # Copia nuova configurazione produzione se presente nel package
        $newProdConfig = Join-Path $tempDeployDir "Config\appsettings.Production.json"
        if (Test-Path $newProdConfig) {
            Copy-Item $newProdConfig $existingProdConfig -Force
            Write-Success "Nuova configurazione produzione applicata"
        } elseif (Test-Path $tempProdConfigBackup) {
            Copy-Item $tempProdConfigBackup $existingProdConfig -Force
            Write-Success "Configurazione produzione esistente ripristinata"
        }
        
        # Ripristina web.config esistente se c'era (preserva configurazione funzionante)
        # A meno che non sia specificato -ForceUpdateWebConfig
        if ($ForceUpdateWebConfig) {
            Write-WarningMsg "Parametro -ForceUpdateWebConfig specificato: web.config verrà aggiornato con quello del package"
            # Non ripristinare il backup, usa quello nuovo dal package
        } elseif (Test-Path $tempWebConfigBackup) {
            Copy-Item $tempWebConfigBackup $existingWebConfig -Force
            Write-Success "web.config esistente ripristinato (preservata configurazione funzionante)"
        }
    }
    catch {
        Write-ErrorMsg "Errore durante deploy file applicazione"
        Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $ErrorColor
        
        # Riavvia servizi in caso di errore
        Write-Host "Riavvio servizi per ripristino..." -ForegroundColor $WarningColor
        iisreset /start | Out-Null
        if (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue) {
            Start-WebAppPool -Name $AppPoolName
        }
        exit 1
    }

    # ============================================================================
    # STEP 7: RIAVVIO SERVIZI
    # ============================================================================
    
    Write-Step "STEP 7: Riavvio servizi"
    
    try {
        # Riavvia IIS
        iisreset /start | Out-Null
        Write-Success "IIS riavviato"
        
        # Riavvia Application Pool
        if (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue) {
            Start-WebAppPool -Name $AppPoolName
            Write-Success "Application Pool riavviato: $AppPoolName"
        }
        
        Start-Sleep -Seconds 10
        Write-Success "Servizi riavviati correttamente"
    }
    catch {
        Write-ErrorMsg "Errore durante riavvio servizi"
        Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $ErrorColor
        Write-WarningMsg "Verificare manualmente lo stato dei servizi IIS"
    }

    # ============================================================================
    # STEP 8: VERIFICA POST-DEPLOY
    # ============================================================================
    
    Write-Step "STEP 8: Verifica post-deploy"
    
    try {
        # Esegui script di verifica se presente
        $verifyScript = Join-Path $tempDeployDir "Database\VERIFICA_AGGIORNAMENTO.sql"
        if (Test-Path $verifyScript) {
            Write-Host "Esecuzione verifica database..." -ForegroundColor $InfoColor
            Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -InputFile $verifyScript -QueryTimeout 60
            Write-Success "Verifica database completata"
        } else {
            Write-WarningMsg "Script VERIFICA_AGGIORNAMENTO.sql non trovato"
        }
        
        # Verifica connessione database
        $dbVerify = Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -Query "SELECT COUNT(*) as TabCount FROM INFORMATION_SCHEMA.TABLES" -ErrorAction Stop
        Write-Success "Database accessibile - Tabelle trovate: $($dbVerify.TabCount)"
        
        # Verifica specifica per tabelle critiche
        $tabelleVerifica = @(
            @{Nome = "Resources"; Descrizione = "Risorse Applicazione"},
            @{Nome = "Permissions"; Descrizione = "Permessi Ruoli"},
            @{Nome = "UserDataFilters"; Descrizione = "Filtri Dati Utente"},
            @{Nome = "AspNetUsers"; Descrizione = "Utenti Identity"},
            @{Nome = "AspNetRoles"; Descrizione = "Ruoli Identity"},
            @{Nome = "OrdiniTestate"; Descrizione = "Ordini Testate"},
            @{Nome = "OrdiniRighe"; Descrizione = "Ordini Righe"},
            @{Nome = "ListaOP"; Descrizione = "Lista Ordini Produzione"},
            @{Nome = "AnagraficaClienti"; Descrizione = "Anagrafica Clienti"},
            @{Nome = "AnagraficaArticoli"; Descrizione = "Anagrafica Articoli"},
            @{Nome = "TabellaAgenti"; Descrizione = "Tabella Agenti"},
            @{Nome = "CalendarioFermiCentriLavoro"; Descrizione = "Calendario Fermi"},
            @{Nome = "StatiOP"; Descrizione = "Stati OP"}
        )
        
        Write-Host ""
        Write-Host "Verifica tabelle critiche..." -ForegroundColor $InfoColor
        
        foreach ($tabella in $tabelleVerifica) {
            $checkQuery = "SELECT COUNT(*) as Exists FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$($tabella.Nome)'"
            $tabellaCheck = Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -Query $checkQuery -ErrorAction SilentlyContinue
            
            if ($tabellaCheck -and $tabellaCheck.Exists -eq 1) {
                Write-Success "✅ $($tabella.Descrizione) ($($tabella.Nome))"
            } else {
                Write-WarningMsg "❌ $($tabella.Descrizione) ($($tabella.Nome)) - NON TROVATA"
            }
        }
        
        # Verifica migration history
        $migrationCheck = Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -Query "SELECT TOP 3 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC" -ErrorAction SilentlyContinue
        if ($migrationCheck) {
            Write-Success "Migration history verificata - Ultime migration:"
            $migrationCheck | ForEach-Object { Write-Host "  - $($_.MigrationId)" -ForegroundColor Gray }
        }
        
        # Pulizia directory temporanea
        Remove-Item -Path $tempDeployDir -Recurse -Force
        Write-Success "Directory temporanea rimossa"
        
    }
    catch {
        Write-WarningMsg "Errore durante verifica post-deploy"
        Write-Host "Errore: $($_.Exception.Message)" -ForegroundColor $WarningColor
        Write-Host "Il deploy potrebbe essere completato correttamente. Verificare manualmente." -ForegroundColor $WarningColor
    }

    # ============================================================================
    # COMPLETAMENTO
    # ============================================================================
    
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor $SuccessColor
    Write-Host "DEPLOY PRODUZIONE COMPLETATO CON SUCCESSO" -ForegroundColor $SuccessColor
    Write-Host "============================================================================" -ForegroundColor $SuccessColor
    Write-Host "Server: $ServerName" -ForegroundColor $SuccessColor
    Write-Host "Database: $DatabaseName" -ForegroundColor $SuccessColor
    Write-Host "Applicazione: $AppDirectory" -ForegroundColor $SuccessColor
    Write-Host "Backup salvati in: $BackupDirectory" -ForegroundColor $SuccessColor
    Write-Host "Fine: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor $SuccessColor
    Write-Host ""
    Write-Host "AZIONI CONSIGLIATE - TEST POST-DEPLOY:" -ForegroundColor $InfoColor
    Write-Host ""
    Write-Host "GENERALE:" -ForegroundColor $InfoColor
    Write-Host "  1. Testare login applicazione" -ForegroundColor $InfoColor
    Write-Host "  2. Verificare dashboard principale" -ForegroundColor $InfoColor
    Write-Host "  3. Controllare log applicazione per errori" -ForegroundColor $InfoColor
    Write-Host ""
    Write-Host "MODULI NOVEMBRE 2024 (NUOVI/AGGIORNATI):" -ForegroundColor $InfoColor
    Write-Host '  4. Gestione Permessi -> Verifica ruoli e risorse' -ForegroundColor $InfoColor
    Write-Host '  5. Dashboard Consegne -> Test KPI e grafici' -ForegroundColor $InfoColor
    Write-Host '  6. Interrogazioni AI -> Test query naturali con Mistral' -ForegroundColor $InfoColor
    Write-Host '  7. Consegne Programmate -> Filtro agente automatico' -ForegroundColor $InfoColor
    Write-Host ""
    Write-Host "MODULI PRODUZIONE:" -ForegroundColor $InfoColor
    Write-Host '  8. Schedulatore OP -> Verifica drag&drop e resize eventi' -ForegroundColor $InfoColor
    Write-Host '  9. Calendario Fermi -> Test generazione fermi settimanali' -ForegroundColor $InfoColor
    Write-Host ' 10. Lista OP Dashboard -> Verifica dati produzione' -ForegroundColor $InfoColor
    Write-Host ""
    Write-Host "ANAGRAFICHE E ORDINI:" -ForegroundColor $InfoColor
    Write-Host ' 11. Anagrafica Clienti -> Verifica accesso per agenti' -ForegroundColor $InfoColor
    Write-Host ' 12. Ordini Testate/Righe -> Test creazione ordini' -ForegroundColor $InfoColor
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "====================================================================" -ForegroundColor $ErrorColor
    Write-Host "ERRORE CRITICO DURANTE DEPLOY" -ForegroundColor $ErrorColor
    Write-Host "====================================================================" -ForegroundColor $ErrorColor
    $errMsg = $_.Exception.Message
    $errLine = $_.InvocationInfo.ScriptLineNumber
    Write-Host "Errore: $errMsg" -ForegroundColor $ErrorColor
    Write-Host "Riga: $errLine" -ForegroundColor $ErrorColor
    Write-Host ""
    Write-Host "AZIONI DI RIPRISTINO:" -ForegroundColor $WarningColor
    Write-Host "1. Riavviare manualmente IIS" -ForegroundColor $WarningColor
    Write-Host "2. Verificare stato Application Pool" -ForegroundColor $WarningColor
    Write-Host "3. Controllare backup in C:\Backup\AiDbMaster" -ForegroundColor $WarningColor
    Write-Host "====================================================================" -ForegroundColor $ErrorColor
    exit 1
}



