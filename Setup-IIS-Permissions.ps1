# Script per configurare i permessi IIS per AiDbMaster
# Eseguire come Amministratore

param(
    [string]$SiteName = "AiDbMaster",
    [string]$AppPoolName = "AiDbMaster",
    [string]$SitePath = "C:\inetpub\wwwroot\AiDbMaster",
    [string]$Port = "80"
)

Write-Host "=== CONFIGURAZIONE PERMESSI IIS PER AIDBMASTER ===" -ForegroundColor Green
Write-Host "Sito: $SiteName" -ForegroundColor Yellow
Write-Host "Application Pool: $AppPoolName" -ForegroundColor Yellow
Write-Host "Path: $SitePath" -ForegroundColor Yellow

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
    Write-Host "`n1. CREAZIONE/CONFIGURAZIONE APPLICATION POOL..." -ForegroundColor Cyan
    
    # Verifica se l'Application Pool esiste già
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Write-Host "Application Pool '$AppPoolName' già esistente. Aggiornamento configurazione..." -ForegroundColor Yellow
    } else {
        Write-Host "Creazione Application Pool '$AppPoolName'..." -ForegroundColor Green
        New-WebAppPool -Name $AppPoolName
    }
    
    # Configurazione Application Pool per .NET Core
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value ApplicationPoolIdentity
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name enable32BitAppOnWin64 -Value $false
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.loadUserProfile -Value $true
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.setProfileEnvironment -Value $true
    
    Write-Host "✓ Application Pool configurato correttamente" -ForegroundColor Green

    Write-Host "`n2. CONFIGURAZIONE PERMESSI CARTELLE..." -ForegroundColor Cyan
    
    # Identità dell'Application Pool
    $AppPoolIdentity = "IIS AppPool\$AppPoolName"
    
    # Permessi sulla cartella principale del sito
    Write-Host "Impostazione permessi su: $SitePath" -ForegroundColor Yellow
    
    # Rimuovi permessi ereditati e imposta permessi espliciti
    $acl = Get-Acl $SitePath
    
    # Aggiungi permessi per l'Application Pool Identity
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($AppPoolIdentity, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule)
    
    # Aggiungi permessi per IIS_IUSRS (gruppo standard IIS)
    $accessRule2 = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule2)
    
    # Aggiungi permessi per IUSR (utente anonimo IIS)
    $accessRule3 = New-Object System.Security.AccessControl.FileSystemAccessRule("IUSR", "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule3)
    
    Set-Acl -Path $SitePath -AclObject $acl
    Write-Host "✓ Permessi base impostati" -ForegroundColor Green

    # Cartelle che necessitano di permessi di scrittura
    $writableFolders = @(
        "App_Data",
        "Logs", 
        "Uploads",
        "Shared",
        "DocumentsStorage",
        "wwwroot\uploads",
        "Temp"
    )
    
    Write-Host "`n3. CONFIGURAZIONE CARTELLE CON PERMESSI DI SCRITTURA..." -ForegroundColor Cyan
    
    foreach ($folder in $writableFolders) {
        $folderPath = Join-Path $SitePath $folder
        
        # Crea la cartella se non esiste
        if (-not (Test-Path $folderPath)) {
            New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
            Write-Host "Creata cartella: $folder" -ForegroundColor Green
        }
        
        # Imposta permessi di scrittura completi
        $folderAcl = Get-Acl $folderPath
        $writeRule = New-Object System.Security.AccessControl.FileSystemAccessRule($AppPoolIdentity, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $folderAcl.SetAccessRule($writeRule)
        Set-Acl -Path $folderPath -AclObject $folderAcl
        
        Write-Host "✓ Permessi di scrittura impostati per: $folder" -ForegroundColor Green
    }

    Write-Host "`n4. VERIFICA/CREAZIONE SITO WEB..." -ForegroundColor Cyan
    
    # Verifica se il sito esiste già
    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        Write-Host "Sito web '$SiteName' già esistente. Aggiornamento configurazione..." -ForegroundColor Yellow
        Set-ItemProperty -Path "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
    } else {
        Write-Host "Creazione sito web '$SiteName'..." -ForegroundColor Green
        New-Website -Name $SiteName -Port $Port -PhysicalPath $SitePath -ApplicationPool $AppPoolName
    }
    
    Write-Host "✓ Sito web configurato correttamente" -ForegroundColor Green

    Write-Host "`n5. CONFIGURAZIONI AGGIUNTIVE IIS..." -ForegroundColor Cyan
    
    # Abilita autenticazione anonima
    Set-WebConfigurationProperty -Filter "system.webServer/security/authentication/anonymousAuthentication" -Name enabled -Value $true -PSPath "IIS:\" -Location "$SiteName"
    
    # Configura default documents
    Clear-WebConfiguration -Filter "system.webServer/defaultDocument/files" -PSPath "IIS:\" -Location "$SiteName"
    Add-WebConfiguration -Filter "system.webServer/defaultDocument/files" -Value @{value="index.html"} -PSPath "IIS:\" -Location "$SiteName"
    Add-WebConfiguration -Filter "system.webServer/defaultDocument/files" -Value @{value="default.html"} -PSPath "IIS:\" -Location "$SiteName"
    
    Write-Host "✓ Configurazioni IIS completate" -ForegroundColor Green

    Write-Host "`n6. RIAVVIO APPLICATION POOL..." -ForegroundColor Cyan
    Restart-WebAppPool -Name $AppPoolName
    Write-Host "✓ Application Pool riavviato" -ForegroundColor Green

    Write-Host "`n=== CONFIGURAZIONE COMPLETATA CON SUCCESSO ===" -ForegroundColor Green
    Write-Host "Il sito AiDbMaster è ora configurato correttamente su IIS" -ForegroundColor White
    Write-Host "URL: http://localhost:$Port" -ForegroundColor Yellow
    Write-Host "Path fisico: $SitePath" -ForegroundColor Yellow
    Write-Host "Application Pool: $AppPoolName" -ForegroundColor Yellow

} catch {
    Write-Error "Errore durante la configurazione: $($_.Exception.Message)"
    Write-Host "Dettagli errore: $($_.Exception)" -ForegroundColor Red
    exit 1
}