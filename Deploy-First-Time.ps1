# Script per il primo deploy di AiDbMaster
# Eseguire come Amministratore sul server SVRGEST

param(
    [string]$SourcePath = "",  # Path dei file pubblicati (es: C:\Publish\AiDbMaster)
    [string]$SitePath = "C:\inetpub\wwwroot\AiDbMaster",
    [string]$SiteName = "AiDbMaster",
    [string]$AppPoolName = "AiDbMaster"
)

Write-Host "=== PRIMO DEPLOY AIDBMASTER ===" -ForegroundColor Green
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Verifica amministratore
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Eseguire come Amministratore!"
    exit 1
}

# Verifica path sorgente
if (-not $SourcePath) {
    Write-Host "ATTENZIONE: Path sorgente non specificato" -ForegroundColor Yellow
    Write-Host "Specificare il path con: -SourcePath 'C:\Path\To\Published\Files'" -ForegroundColor White
    Write-Host "Oppure copiare manualmente i file in: $SitePath" -ForegroundColor White
    $SourcePath = Read-Host "Inserisci il path dei file pubblicati (o INVIO per saltare)"
    if (-not $SourcePath) {
        Write-Host "Saltando copia files - assicurati di copiarli manualmente" -ForegroundColor Yellow
    }
}

Write-Host "`n1. PREPARAZIONE DEPLOY..." -ForegroundColor Cyan

# Ferma Application Pool
Write-Host "Arresto Application Pool..." -ForegroundColor Yellow
Import-Module WebAdministration -ErrorAction SilentlyContinue
if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
    Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    Write-Host "✓ Application Pool fermato" -ForegroundColor Green
}

Write-Host "`n2. BACKUP CONFIGURAZIONE ESISTENTE..." -ForegroundColor Cyan

# Backup appsettings.Production.json se esiste
$backupPath = "$SitePath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
if (Test-Path "$SitePath\appsettings.Production.json") {
    Copy-Item "$SitePath\appsettings.Production.json" "$backupPath.appsettings.Production.json" -Force
    Write-Host "✓ Backup configurazione: $backupPath.appsettings.Production.json" -ForegroundColor Green
}

Write-Host "`n3. COPIA FILES APPLICAZIONE..." -ForegroundColor Cyan

if ($SourcePath -and (Test-Path $SourcePath)) {
    Write-Host "Copia da: $SourcePath" -ForegroundColor White
    Write-Host "Verso: $SitePath" -ForegroundColor White
    
    # Crea directory se non esiste
    if (-not (Test-Path $SitePath)) {
        New-Item -ItemType Directory -Path $SitePath -Force | Out-Null
    }
    
    # Copia tutti i files
    Copy-Item -Path "$SourcePath\*" -Destination $SitePath -Recurse -Force
    Write-Host "✓ Files copiati" -ForegroundColor Green
    
    # Ripristina configurazione di produzione se esisteva
    if (Test-Path "$backupPath.appsettings.Production.json") {
        Copy-Item "$backupPath.appsettings.Production.json" "$SitePath\appsettings.Production.json" -Force
        Remove-Item "$backupPath.appsettings.Production.json" -Force
        Write-Host "✓ Configurazione produzione ripristinata" -ForegroundColor Green
    }
} else {
    Write-Host "⚠️ Copia files saltata - path non valido o non specificato" -ForegroundColor Yellow
}

Write-Host "`n4. CREAZIONE WEB.CONFIG..." -ForegroundColor Cyan

# Crea web.config ottimizzato per produzione
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
      
      <!-- Caching -->
      <staticContent>
        <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="30.00:00:00" />
      </staticContent>
      
      <!-- Protezione cartelle sensibili -->
      <location path="Logs">
        <system.web>
          <authorization>
            <deny users="*"/>
          </authorization>
        </system.web>
      </location>
      
      <location path="App_Data">
        <system.web>
          <authorization>
            <deny users="*"/>
          </authorization>
        </system.web>
      </location>
      
    </system.webServer>
  </location>
</configuration>
"@

$webConfigPath = Join-Path $SitePath "web.config"
$webConfigContent | Out-File -FilePath $webConfigPath -Encoding UTF8
Write-Host "✓ web.config creato" -ForegroundColor Green

Write-Host "`n5. VERIFICA CARTELLE NECESSARIE..." -ForegroundColor Cyan

# Assicurati che esistano le cartelle necessarie
$requiredFolders = @("Logs", "App_Data", "Uploads", "Shared", "DocumentsStorage", "Temp")
foreach ($folder in $requiredFolders) {
    $folderPath = Join-Path $SitePath $folder
    if (-not (Test-Path $folderPath)) {
        New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
        Write-Host "Creata cartella: $folder" -ForegroundColor Green
    }
}

Write-Host "`n6. AVVIO APPLICATION POOL..." -ForegroundColor Cyan

# Avvia Application Pool
Start-WebAppPool -Name $AppPoolName
Write-Host "✓ Application Pool avviato" -ForegroundColor Green

# Avvia sito se fermato
if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    $siteState = (Get-Website -Name $SiteName).State
    if ($siteState -ne "Started") {
        Start-Website -Name $SiteName
        Write-Host "✓ Sito web avviato" -ForegroundColor Green
    }
}

Write-Host "`n7. VERIFICA DEPLOY..." -ForegroundColor Cyan

# Verifica files principali
$criticalFiles = @("AiDbMaster.dll", "appsettings.json", "web.config")
$missingFiles = @()

foreach ($file in $criticalFiles) {
    $filePath = Join-Path $SitePath $file
    if (Test-Path $filePath) {
        Write-Host "✓ $file presente" -ForegroundColor Green
    } else {
        Write-Host "✗ $file MANCANTE" -ForegroundColor Red
        $missingFiles += $file
    }
}

Write-Host "`n=== DEPLOY COMPLETATO ===" -ForegroundColor Green

if ($missingFiles.Count -eq 0) {
    Write-Host "✅ DEPLOY RIUSCITO!" -ForegroundColor Green
    Write-Host "`nTesta l'applicazione:" -ForegroundColor Yellow
    Write-Host "URL: http://localhost" -ForegroundColor Cyan
    Write-Host "Logs: $SitePath\Logs\" -ForegroundColor Cyan
} else {
    Write-Host "⚠️ DEPLOY PARZIALE - Files mancanti:" -ForegroundColor Yellow
    $missingFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host "`nCopia manualmente i files mancanti e riprova" -ForegroundColor White
}

Write-Host "`nPROSSIMI PASSI:" -ForegroundColor Yellow
Write-Host "1. Testa l'applicazione navigando su http://localhost" -ForegroundColor White
Write-Host "2. Controlla i log in caso di errori: $SitePath\Logs\" -ForegroundColor White
Write-Host "3. Verifica la connessione al database" -ForegroundColor White
