# Script per aggiungere il permesso mancante per Application Pool Identity

param(
    [string]$SitePath = "C:\inetpub\wwwroot\AiDbMaster",
    [string]$AppPoolName = "AiDbMaster"
)

Write-Host "=== CORREZIONE PERMESSI APPLICATION POOL ===" -ForegroundColor Green

# Verifica se eseguito come amministratore
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Eseguire come Amministratore!"
    exit 1
}

Write-Host "Cartella: $SitePath" -ForegroundColor Yellow
Write-Host "Application Pool: $AppPoolName" -ForegroundColor Yellow

# Identità corretta dell'Application Pool
$AppPoolIdentity = "IIS AppPool\$AppPoolName"

Write-Host "`n1. VERIFICA PERMESSI ATTUALI:" -ForegroundColor Cyan
$acl = Get-Acl $SitePath
Write-Host "Permessi trovati:" -ForegroundColor White
$acl.Access | Where-Object { $_.AccessControlType -eq "Allow" } | 
    Select-Object IdentityReference, FileSystemRights | 
    Format-Table -AutoSize

Write-Host "`n2. VERIFICA APPLICATION POOL IDENTITY:" -ForegroundColor Cyan
$existingAppPoolAccess = $acl.Access | Where-Object { 
    $_.IdentityReference -eq $AppPoolIdentity -and 
    $_.AccessControlType -eq "Allow"
}

if ($existingAppPoolAccess) {
    Write-Host "✓ Application Pool Identity già configurata" -ForegroundColor Green
    Write-Host "  Permessi: $($existingAppPoolAccess.FileSystemRights)" -ForegroundColor White
} else {
    Write-Host "✗ Application Pool Identity MANCANTE - Aggiunta in corso..." -ForegroundColor Yellow
    
    # Aggiungi il permesso per Application Pool Identity
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        $AppPoolIdentity, 
        "FullControl", 
        "ContainerInherit,ObjectInherit", 
        "None", 
        "Allow"
    )
    
    $acl.SetAccessRule($accessRule)
    Set-Acl -Path $SitePath -AclObject $acl
    
    Write-Host "✓ Permesso aggiunto per: $AppPoolIdentity" -ForegroundColor Green
}

Write-Host "`n3. VERIFICA ALTRI PERMESSI NECESSARI:" -ForegroundColor Cyan

# Verifica IUSR
$iusrAccess = $acl.Access | Where-Object { 
    $_.IdentityReference -like "*IUSR*" -and 
    $_.AccessControlType -eq "Allow"
}

if (-not $iusrAccess) {
    Write-Host "Aggiunta permesso per IUSR..." -ForegroundColor Yellow
    $iusrRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "IUSR", 
        "ReadAndExecute", 
        "ContainerInherit,ObjectInherit", 
        "None", 
        "Allow"
    )
    $acl.SetAccessRule($iusrRule)
    Set-Acl -Path $SitePath -AclObject $acl
    Write-Host "✓ Permesso IUSR aggiunto" -ForegroundColor Green
}

Write-Host "`n4. CONFIGURAZIONE CARTELLE SCRIVIBILI:" -ForegroundColor Cyan
$writableFolders = @("App_Data", "Logs", "Uploads", "Shared", "DocumentsStorage", "Temp")

foreach ($folder in $writableFolders) {
    $folderPath = Join-Path $SitePath $folder
    
    # Crea cartella se non esiste
    if (-not (Test-Path $folderPath)) {
        New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
        Write-Host "Creata cartella: $folder" -ForegroundColor Green
    }
    
    # Imposta permessi FullControl per Application Pool
    $folderAcl = Get-Acl $folderPath
    $folderRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        $AppPoolIdentity, 
        "FullControl", 
        "ContainerInherit,ObjectInherit", 
        "None", 
        "Allow"
    )
    $folderAcl.SetAccessRule($folderRule)
    Set-Acl -Path $folderPath -AclObject $folderAcl
    
    Write-Host "✓ Permessi configurati per: $folder" -ForegroundColor Green
}

Write-Host "`n5. VERIFICA FINALE:" -ForegroundColor Cyan
$finalAcl = Get-Acl $SitePath
Write-Host "Permessi finali:" -ForegroundColor White
$finalAcl.Access | Where-Object { $_.AccessControlType -eq "Allow" } | 
    Select-Object IdentityReference, FileSystemRights | 
    Format-Table -AutoSize

Write-Host "`n=== CORREZIONE COMPLETATA ===" -ForegroundColor Green
Write-Host "Application Pool Identity configurata: $AppPoolIdentity" -ForegroundColor White
Write-Host "Cartelle scrivibili configurate: $($writableFolders.Count)" -ForegroundColor White
