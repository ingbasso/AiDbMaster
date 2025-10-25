# Script per configurare l'ambiente di produzione per AiDbMaster
# Configura variabili d'ambiente, certificati SSL e ottimizzazioni

param(
    [string]$SiteName = "AiDbMaster",
    [string]$AppPoolName = "AiDbMaster",
    [string]$SitePath = "C:\inetpub\wwwroot\AiDbMaster",
    [string]$Domain = "aidbmaster.it"
)

Write-Host "=== CONFIGURAZIONE AMBIENTE PRODUZIONE AIDBMASTER ===" -ForegroundColor Green

# Verifica se lo script viene eseguito come amministratore
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Questo script deve essere eseguito come Amministratore!"
    exit 1
}

try {
    Write-Host "`n1. CONFIGURAZIONE VARIABILI D'AMBIENTE..." -ForegroundColor Cyan
    
    # Imposta l'ambiente come Production per l'Application Pool
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.environmentVariables -Value @{
        "ASPNETCORE_ENVIRONMENT" = "Production";
        "DOTNET_ENVIRONMENT" = "Production"
    }
    
    Write-Host "✓ Variabile ASPNETCORE_ENVIRONMENT impostata su Production" -ForegroundColor Green

    Write-Host "`n2. OTTIMIZZAZIONI APPLICATION POOL..." -ForegroundColor Cyan
    
    # Configurazioni per l'ambiente di produzione
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.idleTimeout -Value "00:00:00"  # Disabilita idle timeout
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name recycling.periodicRestart.time -Value "1.00:00:00"  # Riavvio ogni 24 ore
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.maxProcesses -Value 1  # Processo singolo
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.pingingEnabled -Value $true
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.pingInterval -Value "00:00:30"
    
    Write-Host "✓ Application Pool ottimizzato per produzione" -ForegroundColor Green

    Write-Host "`n3. CONFIGURAZIONE LOGGING..." -ForegroundColor Cyan
    
    # Crea cartella per i log se non esiste
    $logsPath = Join-Path $SitePath "Logs"
    if (-not (Test-Path $logsPath)) {
        New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
        Write-Host "Creata cartella Logs: $logsPath" -ForegroundColor Green
    }
    
    # Imposta permessi per la cartella Logs
    $AppPoolIdentity = "IIS AppPool\$AppPoolName"
    $logsAcl = Get-Acl $logsPath
    $logsRule = New-Object System.Security.AccessControl.FileSystemAccessRule($AppPoolIdentity, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $logsAcl.SetAccessRule($logsRule)
    Set-Acl -Path $logsPath -AclObject $logsAcl
    
    Write-Host "✓ Cartella Logs configurata" -ForegroundColor Green

    Write-Host "`n4. CONFIGURAZIONE COMPRESSIONE E CACHING..." -ForegroundColor Cyan
    
    # Abilita compressione dinamica e statica
    Set-WebConfigurationProperty -Filter "system.webServer/httpCompression" -Name doDynamicCompression -Value $true -PSPath "IIS:\" -Location "$SiteName"
    Set-WebConfigurationProperty -Filter "system.webServer/httpCompression" -Name doStaticCompression -Value $true -PSPath "IIS:\" -Location "$SiteName"
    
    # Configura caching per file statici
    Set-WebConfigurationProperty -Filter "system.webServer/staticContent" -Name clientCache.cacheControlMode -Value UseMaxAge -PSPath "IIS:\" -Location "$SiteName"
    Set-WebConfigurationProperty -Filter "system.webServer/staticContent" -Name clientCache.cacheControlMaxAge -Value "30.00:00:00" -PSPath "IIS:\" -Location "$SiteName"
    
    Write-Host "✓ Compressione e caching configurati" -ForegroundColor Green

    Write-Host "`n5. CONFIGURAZIONE SICUREZZA..." -ForegroundColor Cyan
    
    # Rimuovi header server per sicurezza
    Set-WebConfigurationProperty -Filter "system.webServer/security/requestFiltering" -Name removeServerHeader -Value $true -PSPath "IIS:\" -Location "$SiteName"
    
    # Configura dimensioni massime richieste (per upload file)
    Set-WebConfigurationProperty -Filter "system.webServer/security/requestFiltering/requestLimits" -Name maxAllowedContentLength -Value 52428800 -PSPath "IIS:\" -Location "$SiteName"  # 50MB
    
    Write-Host "✓ Configurazioni di sicurezza applicate" -ForegroundColor Green

    Write-Host "`n6. VERIFICA CONFIGURAZIONE HTTPS..." -ForegroundColor Cyan
    
    # Verifica se esiste un binding HTTPS
    $httpsBinding = Get-WebBinding -Name $SiteName -Protocol "https" -ErrorAction SilentlyContinue
    
    if (-not $httpsBinding) {
        Write-Host "⚠️  Nessun binding HTTPS trovato" -ForegroundColor Yellow
        Write-Host "Per configurare HTTPS:" -ForegroundColor White
        Write-Host "1. Ottieni un certificato SSL per $Domain" -ForegroundColor White
        Write-Host "2. Installa il certificato nel Certificate Store" -ForegroundColor White
        Write-Host "3. Aggiungi binding HTTPS:" -ForegroundColor White
        Write-Host "   New-WebBinding -Name '$SiteName' -Protocol https -Port 443 -HostHeader '$Domain'" -ForegroundColor Cyan
    } else {
        Write-Host "✓ Binding HTTPS già configurato" -ForegroundColor Green
    }

    Write-Host "`n7. CREAZIONE WEB.CONFIG PER ASP.NET CORE..." -ForegroundColor Cyan
    
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
      
      <!-- Sicurezza -->
      <security>
        <requestFiltering removeServerHeader="true">
          <requestLimits maxAllowedContentLength="52428800" />
        </requestFiltering>
      </security>
      
      <!-- Compressione -->
      <httpCompression doDynamicCompression="true" doStaticCompression="true">
        <dynamicTypes>
          <add mimeType="application/json" enabled="true" />
          <add mimeType="application/javascript" enabled="true" />
          <add mimeType="text/css" enabled="true" />
          <add mimeType="text/html" enabled="true" />
        </dynamicTypes>
      </httpCompression>
      
      <!-- Caching per file statici -->
      <staticContent>
        <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="30.00:00:00" />
      </staticContent>
      
      <!-- Rewrite rules per HTTPS (opzionale) -->
      <!--
      <rewrite>
        <rules>
          <rule name="Redirect to HTTPS" stopProcessing="true">
            <match url="(.*)" />
            <conditions>
              <add input="{HTTPS}" pattern="off" ignoreCase="true" />
            </conditions>
            <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
          </rule>
        </rules>
      </rewrite>
      -->
      
    </system.webServer>
  </location>
</configuration>
"@

    $webConfigPath = Join-Path $SitePath "web.config"
    $webConfigContent | Out-File -FilePath $webConfigPath -Encoding UTF8
    Write-Host "✓ web.config creato/aggiornato" -ForegroundColor Green

    Write-Host "`n8. RIAVVIO SERVIZI..." -ForegroundColor Cyan
    
    # Riavvia Application Pool
    Restart-WebAppPool -Name $AppPoolName
    Write-Host "✓ Application Pool riavviato" -ForegroundColor Green

    Write-Host "`n=== CONFIGURAZIONE PRODUZIONE COMPLETATA ===" -ForegroundColor Green
    Write-Host "`nCONFIGURAZIONI APPLICATE:" -ForegroundColor Yellow
    Write-Host "✓ Ambiente impostato su Production" -ForegroundColor White
    Write-Host "✓ Application Pool ottimizzato" -ForegroundColor White
    Write-Host "✓ Logging configurato" -ForegroundColor White
    Write-Host "✓ Compressione e caching abilitati" -ForegroundColor White
    Write-Host "✓ Configurazioni di sicurezza applicate" -ForegroundColor White
    Write-Host "✓ web.config per ASP.NET Core creato" -ForegroundColor White
    
    Write-Host "`nPROSSIMI PASSI RACCOMANDATI:" -ForegroundColor Yellow
    Write-Host "1. Configura certificato SSL per HTTPS" -ForegroundColor White
    Write-Host "2. Testa l'applicazione: http://localhost" -ForegroundColor White
    Write-Host "3. Monitora i log in: $logsPath" -ForegroundColor White
    Write-Host "4. Configura backup automatici del database" -ForegroundColor White

} catch {
    Write-Error "Errore durante la configurazione: $($_.Exception.Message)"
    Write-Host "Dettagli errore: $($_.Exception)" -ForegroundColor Red
    exit 1
}
