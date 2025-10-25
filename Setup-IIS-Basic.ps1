# Script base per configurare IIS per AiDbMaster
# Eseguire come Amministratore

param(
    [string]$SiteName = "AiDbMaster",
    [string]$AppPoolName = "AiDbMaster", 
    [string]$SitePath = "C:\inetpub\wwwroot\AiDbMaster"
)

Write-Host "=== CONFIGURAZIONE IIS AIDBMASTER ===" -ForegroundColor Green

# Verifica amministratore
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Error "Eseguire come Amministratore!"
    exit 1
}

# Importa modulo IIS
Import-Module WebAdministration -ErrorAction SilentlyContinue

Write-Host "1. Configurazione Application Pool..." -ForegroundColor Cyan

# Configura Application Pool
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value ApplicationPoolIdentity
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name enable32BitAppOnWin64 -Value $false

Write-Host "Application Pool configurato" -ForegroundColor Green

Write-Host "2. Configurazione permessi..." -ForegroundColor Cyan

# Identità Application Pool
$AppPoolIdentity = "IIS AppPool\$AppPoolName"

# Permessi cartella principale
$acl = Get-Acl $SitePath
$rule1 = New-Object System.Security.AccessControl.FileSystemAccessRule($AppPoolIdentity, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule1)
$rule2 = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule2)
Set-Acl -Path $SitePath -AclObject $acl

Write-Host "Permessi base configurati" -ForegroundColor Green

Write-Host "3. Creazione cartelle..." -ForegroundColor Cyan

# Crea cartelle necessarie
$folders = "App_Data", "Logs", "Uploads", "Shared", "DocumentsStorage", "Temp"

$folders | ForEach-Object {
    $folderPath = Join-Path $SitePath $_
    if (-not (Test-Path $folderPath)) {
        New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
        Write-Host "Creata: $_" -ForegroundColor White
    }
    
    # Permessi scrittura
    $folderAcl = Get-Acl $folderPath
    $writeRule = New-Object System.Security.AccessControl.FileSystemAccessRule($AppPoolIdentity, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $folderAcl.SetAccessRule($writeRule)
    Set-Acl -Path $folderPath -AclObject $folderAcl
}

Write-Host "Cartelle configurate" -ForegroundColor Green

Write-Host "4. Configurazione sito..." -ForegroundColor Cyan

# Aggiorna sito esistente
Set-ItemProperty -Path "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName

Write-Host "Sito aggiornato" -ForegroundColor Green

Write-Host "5. Riavvio Application Pool..." -ForegroundColor Cyan
Restart-WebAppPool -Name $AppPoolName

Write-Host "=== CONFIGURAZIONE COMPLETATA ===" -ForegroundColor Green
Write-Host "Sito: $SiteName" -ForegroundColor Yellow
Write-Host "Application Pool: $AppPoolName" -ForegroundColor Yellow
Write-Host "Path: $SitePath" -ForegroundColor Yellow
