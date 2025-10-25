# Script completo per il deployment di AiDbMaster su IIS
# Combina tutti gli script precedenti in un unico processo automatizzato

param(
    [string]$SiteName = "AiDbMaster",
    [string]$AppPoolName = "AiDbMaster", 
    [string]$SitePath = "C:\inetpub\wwwroot\AiDbMaster",
    [string]$SourcePath = "",  # Path dei file pubblicati
    [string]$Port = "80",
    [string]$ServerName = "SVRGEST",
    [string]$DatabaseName = "AIDBMASTER",
    [switch]$SkipDatabaseSetup,
    [switch]$SkipFilesCopy
)

Write-Host "=== DEPLOYMENT COMPLETO AIDBMASTER SU IIS ===" -ForegroundColor Green
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Verifica se lo script viene eseguito come amministratore
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Questo script deve essere eseguito come Amministratore!"
    exit 1
}

# Importa il modulo WebAdministration
Import-Module WebAdministration -ErrorAction SilentlyContinue
if (-not (Get-Module WebAdministration)) {
    Write-Error "Modulo WebAdministration non disponibile. Assicurati che IIS sia installato."
    exit 1
}

try {
    Write-Host "`n=== FASE 1: PREPARAZIONE AMBIENTE ===" -ForegroundColor Magenta
    
    # Ferma il sito se esiste
    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        Write-Host "Arresto sito esistente..." -ForegroundColor Yellow
        Stop-Website -Name $SiteName
    }
    
    # Ferma l'Application Pool se esiste
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Write-Host "Arresto Application Pool esistente..." -ForegroundColor Yellow
        Stop-WebAppPool -Name $AppPoolName
    }

    Write-Host "`n=== FASE 2: COPIA FILES (se richiesta) ===" -ForegroundColor Magenta
    
    if (-not $SkipFilesCopy -and $SourcePath -and (Test-Path $SourcePath)) {
        Write-Host "Copia files da: $SourcePath" -ForegroundColor Cyan
        Write-Host "Destinazione: $SitePath" -ForegroundColor Cyan
        
        # Crea la directory di destinazione se non esiste
        if (-not (Test-Path $SitePath)) {
            New-Item -ItemType Directory -Path $SitePath -Force | Out-Null
        }
        
        # Backup della configurazione esistente se presente
        $backupPath = "$SitePath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        if (Test-Path "$SitePath\appsettings.Production.json") {
            Write-Host "Backup configurazione esistente..." -ForegroundColor Yellow
            Copy-Item "$SitePath\appsettings.Production.json" "$backupPath.appsettings.Production.json" -Force
        }
        
        # Copia tutti i files
        Copy-Item -Path "$SourcePath\*" -Destination $SitePath -Recurse -Force
        Write-Host "✓ Files copiati con successo" -ForegroundColor Green
        
        # Ripristina la configurazione di produzione se esisteva
        if (Test-Path "$backupPath.appsettings.Production.json") {
            Copy-Item "$backupPath.appsettings.Production.json" "$SitePath\appsettings.Production.json" -Force
            Remove-Item "$backupPath.appsettings.Production.json" -Force
            Write-Host "✓ Configurazione di produzione ripristinata" -ForegroundColor Green
        }
    } elseif (-not $SkipFilesCopy) {
        Write-Host "⚠️  Path sorgente non specificato o non valido. Saltando copia files." -ForegroundColor Yellow
        Write-Host "   Usa il parametro -SourcePath per specificare la cartella dei files pubblicati" -ForegroundColor Gray
    }

    Write-Host "`n=== FASE 3: CONFIGURAZIONE APPLICATION POOL ===" -ForegroundColor Magenta
    
    # Crea o aggiorna Application Pool
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Write-Host "Aggiornamento Application Pool esistente..." -ForegroundColor Yellow
    } else {
        Write-Host "Creazione nuovo Application Pool..." -ForegroundColor Green
        New-WebAppPool -Name $AppPoolName
    }
    
    # Configurazione Application Pool per .NET Core
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value ApplicationPoolIdentity
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name enable32BitAppOnWin64 -Value $false
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.loadUserProfile -Value $true
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.setProfileEnvironment -Value $true
    
    # Ottimizzazioni per produzione
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.idleTimeout -Value "00:00:00"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name recycling.periodicRestart.time -Value "1.00:00:00"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.maxProcesses -Value 1
    
    # Variabili d'ambiente
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.environmentVariables -Value @{
        "ASPNETCORE_ENVIRONMENT" = "Production";
        "DOTNET_ENVIRONMENT" = "Production"
    }
    
    Write-Host "✓ Application Pool configurato" -ForegroundColor Green

    Write-Host "`n=== FASE 4: CONFIGURAZIONE PERMESSI FILES ===" -ForegroundColor Magenta
    
    $AppPoolIdentity = "IIS AppPool\$AppPoolName"
    
    # Permessi base sulla cartella principale
    $acl = Get-Acl $SitePath
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($AppPoolIdentity, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule)
    
    $accessRule2 = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule2)
    
    $accessRule3 = New-Object System.Security.AccessControl.FileSystemAccessRule("IUSR", "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule3)
    
    Set-Acl -Path $SitePath -AclObject $acl
    
    # Cartelle con permessi di scrittura
    $writableFolders = @("App_Data", "Logs", "Uploads", "Shared", "DocumentsStorage", "wwwroot\uploads", "Temp")
    
    foreach ($folder in $writableFolders) {
        $folderPath = Join-Path $SitePath $folder
        
        if (-not (Test-Path $folderPath)) {
            New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
        }
        
        $folderAcl = Get-Acl $folderPath
        $writeRule = New-Object System.Security.AccessControl.FileSystemAccessRule($AppPoolIdentity, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $folderAcl.SetAccessRule($writeRule)
        Set-Acl -Path $folderPath -AclObject $folderAcl
    }
    
    Write-Host "✓ Permessi files configurati" -ForegroundColor Green

    Write-Host "`n=== FASE 5: CONFIGURAZIONE SITO WEB ===" -ForegroundColor Magenta
    
    # Crea o aggiorna il sito
    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        Write-Host "Aggiornamento sito esistente..." -ForegroundColor Yellow
        Set-ItemProperty -Path "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
        Set-ItemProperty -Path "IIS:\Sites\$SiteName" -Name physicalPath -Value $SitePath
    } else {
        Write-Host "Creazione nuovo sito..." -ForegroundColor Green
        New-Website -Name $SiteName -Port $Port -PhysicalPath $SitePath -ApplicationPool $AppPoolName
    }
    
    # Configurazioni IIS
    Set-WebConfigurationProperty -Filter "system.webServer/security/authentication/anonymousAuthentication" -Name enabled -Value $true -PSPath "IIS:\" -Location "$SiteName"
    Set-WebConfigurationProperty -Filter "system.webServer/httpCompression" -Name doDynamicCompression -Value $true -PSPath "IIS:\" -Location "$SiteName"
    Set-WebConfigurationProperty -Filter "system.webServer/httpCompression" -Name doStaticCompression -Value $true -PSPath "IIS:\" -Location "$SiteName"
    
    Write-Host "✓ Sito web configurato" -ForegroundColor Green

    Write-Host "`n=== FASE 6: CREAZIONE WEB.CONFIG ===" -ForegroundColor Magenta
    
    $webConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\AiDbMaster.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\Logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="DOTNET_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
      <security>
        <requestFiltering removeServerHeader="true">
          <requestLimits maxAllowedContentLength="52428800" />
        </requestFiltering>
      </security>
      <httpCompression doDynamicCompression="true" doStaticCompression="true" />
      <staticContent>
        <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="30.00:00:00" />
      </staticContent>
    </system.webServer>
  </location>
</configuration>
"@

    $webConfigPath = Join-Path $SitePath "web.config"
    $webConfigContent | Out-File -FilePath $webConfigPath -Encoding UTF8
    Write-Host "✓ web.config creato" -ForegroundColor Green

    Write-Host "`n=== FASE 7: CONFIGURAZIONE DATABASE ===" -ForegroundColor Magenta
    
    if (-not $SkipDatabaseSetup) {
        # Genera script SQL per i permessi database
        $ComputerName = $env:COMPUTERNAME
        $sqlScript = @"
USE master;
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'$ComputerName\$AppPoolName')
    CREATE LOGIN [$ComputerName\$AppPoolName] FROM WINDOWS WITH DEFAULT_DATABASE=[$DatabaseName];

USE [$DatabaseName];
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$ComputerName\$AppPoolName')
    CREATE USER [$ComputerName\$AppPoolName] FOR LOGIN [$ComputerName\$AppPoolName];

ALTER ROLE [db_datareader] ADD MEMBER [$ComputerName\$AppPoolName];
ALTER ROLE [db_datawriter] ADD MEMBER [$ComputerName\$AppPoolName];
ALTER ROLE [db_ddladmin] ADD MEMBER [$ComputerName\$AppPoolName];

GRANT CREATE TABLE TO [$ComputerName\$AppPoolName];
GRANT ALTER ON SCHEMA::dbo TO [$ComputerName\$AppPoolName];
GRANT CREATE PROCEDURE TO [$ComputerName\$AppPoolName];
"@

        $sqlScriptPath = Join-Path $PSScriptRoot "Database-Permissions.sql"
        $sqlScript | Out-File -FilePath $sqlScriptPath -Encoding UTF8
        
        Write-Host "✓ Script database generato: $sqlScriptPath" -ForegroundColor Green
        Write-Host "   Esegui questo script su SQL Server Management Studio" -ForegroundColor Yellow
    } else {
        Write-Host "⚠️  Configurazione database saltata (parametro -SkipDatabaseSetup)" -ForegroundColor Yellow
    }

    Write-Host "`n=== FASE 8: AVVIO SERVIZI ===" -ForegroundColor Magenta
    
    # Avvia Application Pool
    Start-WebAppPool -Name $AppPoolName
    Write-Host "✓ Application Pool avviato" -ForegroundColor Green
    
    # Avvia sito
    Start-Website -Name $SiteName
    Write-Host "✓ Sito web avviato" -ForegroundColor Green

    Write-Host "`n=== DEPLOYMENT COMPLETATO CON SUCCESSO ===" -ForegroundColor Green
    Write-Host "`nRIEPILOGO CONFIGURAZIONE:" -ForegroundColor Yellow
    Write-Host "Sito: $SiteName" -ForegroundColor White
    Write-Host "URL: http://localhost:$Port" -ForegroundColor White
    Write-Host "Path: $SitePath" -ForegroundColor White
    Write-Host "Application Pool: $AppPoolName" -ForegroundColor White
    Write-Host "Ambiente: Production" -ForegroundColor White
    Write-Host "Database: $ServerName\$DatabaseName" -ForegroundColor White
    
    Write-Host "`nPROSSIMI PASSI:" -ForegroundColor Yellow
    Write-Host "1. Esegui lo script Database-Permissions.sql su SQL Server" -ForegroundColor White
    Write-Host "2. Testa l'applicazione navigando su http://localhost:$Port" -ForegroundColor White
    Write-Host "3. Configura certificato SSL per HTTPS (opzionale)" -ForegroundColor White
    Write-Host "4. Monitora i log nella cartella: $SitePath\Logs" -ForegroundColor White

} catch {
    Write-Error "Errore durante il deployment: $($_.Exception.Message)"
    Write-Host "Dettagli errore: $($_.Exception)" -ForegroundColor Red
    
    # Tentativo di rollback
    Write-Host "`nTentativo di rollback..." -ForegroundColor Yellow
    try {
        if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
            Stop-Website -Name $SiteName -ErrorAction SilentlyContinue
        }
        if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
            Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
        }
        Write-Host "✓ Rollback completato" -ForegroundColor Green
    } catch {
        Write-Host "✗ Errore durante il rollback" -ForegroundColor Red
    }
    
    exit 1
}
