# Script per verificare i permessi della cartella AiDbMaster

param(
    [string]$SitePath = "C:\inetpub\wwwroot\AiDbMaster"
)

Write-Host "=== VERIFICA PERMESSI AIDBMASTER ===" -ForegroundColor Green
Write-Host "Cartella: $SitePath" -ForegroundColor Yellow

if (-not (Test-Path $SitePath)) {
    Write-Host "ERRORE: Cartella non trovata!" -ForegroundColor Red
    exit 1
}

Write-Host "`n1. PERMESSI CARTELLA PRINCIPALE:" -ForegroundColor Cyan
$acl = Get-Acl $SitePath
$acl.Access | Where-Object { $_.AccessControlType -eq "Allow" } | 
    Select-Object IdentityReference, FileSystemRights, InheritanceFlags | 
    Format-Table -AutoSize

Write-Host "`n2. VERIFICA IDENTITA' APPLICATION POOL:" -ForegroundColor Cyan
$appPoolIdentity = "IIS AppPool\AiDbMaster"
$hasAppPoolAccess = $acl.Access | Where-Object { 
    $_.IdentityReference -eq $appPoolIdentity -and 
    $_.AccessControlType -eq "Allow" -and
    $_.FileSystemRights -match "FullControl|Modify"
}

if ($hasAppPoolAccess) {
    Write-Host "✓ Application Pool Identity configurata correttamente" -ForegroundColor Green
    Write-Host "  Identità: $appPoolIdentity" -ForegroundColor White
    Write-Host "  Permessi: $($hasAppPoolAccess.FileSystemRights)" -ForegroundColor White
} else {
    Write-Host "✗ Application Pool Identity NON configurata!" -ForegroundColor Red
    Write-Host "  Manca: $appPoolIdentity" -ForegroundColor Yellow
}

Write-Host "`n3. VERIFICA CARTELLE SCRIVIBILI:" -ForegroundColor Cyan
$writableFolders = @("App_Data", "Logs", "Uploads", "Shared", "DocumentsStorage", "Temp")

foreach ($folder in $writableFolders) {
    $folderPath = Join-Path $SitePath $folder
    
    if (Test-Path $folderPath) {
        $folderAcl = Get-Acl $folderPath
        $folderAccess = $folderAcl.Access | Where-Object { 
            $_.IdentityReference -eq $appPoolIdentity -and 
            $_.AccessControlType -eq "Allow" -and
            $_.FileSystemRights -match "FullControl|Modify"
        }
        
        if ($folderAccess) {
            Write-Host "✓ $folder - Permessi OK" -ForegroundColor Green
        } else {
            Write-Host "✗ $folder - Permessi MANCANTI" -ForegroundColor Red
        }
    } else {
        Write-Host "⚠ $folder - Cartella NON ESISTENTE" -ForegroundColor Yellow
    }
}

Write-Host "`n4. VERIFICA PERMESSI IIS STANDARD:" -ForegroundColor Cyan
$iisUsers = @("IIS_IUSRS", "IUSR")

foreach ($user in $iisUsers) {
    $userAccess = $acl.Access | Where-Object { 
        $_.IdentityReference -eq $user -and 
        $_.AccessControlType -eq "Allow"
    }
    
    if ($userAccess) {
        Write-Host "✓ $user - $($userAccess.FileSystemRights)" -ForegroundColor Green
    } else {
        Write-Host "✗ $user - MANCANTE" -ForegroundColor Red
    }
}

Write-Host "`n5. IDENTITA' ERRATE DA RIMUOVERE:" -ForegroundColor Cyan
$wrongIdentities = @("SVRGEST\AiDbMaster", "Everyone")

foreach ($identity in $wrongIdentities) {
    $wrongAccess = $acl.Access | Where-Object { 
        $_.IdentityReference -eq $identity
    }
    
    if ($wrongAccess) {
        Write-Host "⚠ TROVATA IDENTITA' ERRATA: $identity" -ForegroundColor Yellow
        Write-Host "  Permessi: $($wrongAccess.FileSystemRights)" -ForegroundColor Red
        Write-Host "  RIMUOVERE QUESTA IDENTITA'!" -ForegroundColor Red
    }
}

Write-Host "`n=== RIEPILOGO ===" -ForegroundColor Green
Write-Host "Cartella verificata: $SitePath" -ForegroundColor White
Write-Host "Identità corretta: IIS AppPool\AiDbMaster" -ForegroundColor White
Write-Host "Data verifica: $(Get-Date)" -ForegroundColor Gray
