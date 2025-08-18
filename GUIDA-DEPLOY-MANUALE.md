# 🛠️ GUIDA DEPLOY MANUALE - AiDocMaster

## 📋 INFORMAZIONI PRODUZIONE
- **Sito Produzione**: `C:\inetpub\wwwroot\AiDocMaster`
- **Database**: `AIDOCMASTER` su `SRVPSTREE\SQLEXPRESS`
- **Dominio**: `https://www.aidocmaster.it`
- **File Compilati**: `C:\Prove Cursor\AiDocMaster\publish-production`

---

## 🚀 PROCEDURA MANUALE SICURA

### **PASSO 1: BACKUP COMPLETO** ⚠️

```powershell
# Sul server di produzione
$BackupDate = Get-Date -Format "yyyyMMdd-HHmmss"
$BackupPath = "C:\Backup\AiDocMaster-$BackupDate"

# 1. Crea cartella backup
New-Item -ItemType Directory -Path $BackupPath -Force

# 2. Backup sito completo
Copy-Item "C:\inetpub\wwwroot\AiDocMaster\*" $BackupPath -Recurse -Force
Write-Host "✅ Backup sito: $BackupPath" -ForegroundColor Green

# 3. Backup database
sqlcmd -S "SRVPSTREE\SQLEXPRESS" -E -Q "BACKUP DATABASE AIDOCMASTER TO DISK = '$BackupPath\AIDOCMASTER-backup.bak'"
Write-Host "✅ Backup database: $BackupPath\AIDOCMASTER-backup.bak" -ForegroundColor Green
```

### **PASSO 2: FERMA IIS** 🛑

```powershell
# Sul server di produzione
Write-Host "🛑 Fermo IIS..." -ForegroundColor Yellow

# Metodo 1: Stop Application Pool
Import-Module WebAdministration
Stop-WebAppPool -Name "AiDocMaster" -ErrorAction SilentlyContinue

# Metodo 2: Stop sito web
Stop-WebSite -Name "AiDocMaster" -ErrorAction SilentlyContinue

# Metodo 3: Reset completo IIS (se necessario)
# iisreset /stop

Write-Host "✅ IIS fermato" -ForegroundColor Green
```

### **PASSO 3: PRESERVA DATI UTENTE** 💾

```powershell
# Sul server di produzione
$TempDataPath = "C:\temp\preserve-data"
New-Item -ItemType Directory -Path $TempDataPath -Force

# Preserva cartelle con dati utente
$FoldersToPreserve = @("DocumentsStorage", "Uploads", "Logs")

foreach ($folder in $FoldersToPreserve) {
    $source = "C:\inetpub\wwwroot\AiDocMaster\$folder"
    $dest = "$TempDataPath\$folder"
    
    if (Test-Path $source) {
        Copy-Item $source $dest -Recurse -Force
        Write-Host "✅ Preservato: $folder" -ForegroundColor Green
    }
}
```

### **PASSO 4: TRASFERISCI FILE COMPILATI** 📁

**Sul PC di sviluppo:**
```powershell
# Comprimi i file compilati
$SourcePath = "C:\Prove Cursor\AiDocMaster\publish-production"
$ZipPath = "C:\temp\AiDocMaster-Release-$(Get-Date -Format 'yyyyMMdd-HHmmss').zip"

Compress-Archive -Path "$SourcePath\*" -DestinationPath $ZipPath -Force
Write-Host "✅ Pacchetto creato: $ZipPath" -ForegroundColor Green
```

**Trasferisci il file ZIP sul server di produzione in `C:\temp\`**

### **PASSO 5: DEPLOY NUOVI FILE** 🚀

```powershell
# Sul server di produzione
$ZipFile = "C:\temp\AiDocMaster-Release-*.zip"  # Nome del tuo ZIP
$TargetPath = "C:\inetpub\wwwroot\AiDocMaster"

# 1. Rimuovi file vecchi (mantenendo struttura)
Write-Host "🗑️ Rimuovo file vecchi..." -ForegroundColor Yellow
Get-ChildItem $TargetPath -File -Recurse | Remove-Item -Force
Get-ChildItem $TargetPath -Directory | Where-Object { $_.Name -notin @("DocumentsStorage", "Uploads", "Logs") } | Remove-Item -Recurse -Force

# 2. Estrai nuovi file
Write-Host "📦 Estraggo nuovi file..." -ForegroundColor Yellow
Expand-Archive -Path $ZipFile -DestinationPath $TargetPath -Force

# 3. Ripristina dati utente
foreach ($folder in $FoldersToPreserve) {
    $source = "$TempDataPath\$folder"
    $dest = "$TargetPath\$folder"
    
    if (Test-Path $source) {
        Copy-Item $source $dest -Recurse -Force
        Write-Host "✅ Ripristinato: $folder" -ForegroundColor Green
    }
}

Write-Host "✅ Deploy completato!" -ForegroundColor Green
```

### **PASSO 6: VERIFICA CONFIGURAZIONI** ⚙️

```powershell
# Sul server di produzione
$ConfigFiles = @(
    "C:\inetpub\wwwroot\AiDocMaster\web.config",
    "C:\inetpub\wwwroot\AiDocMaster\appsettings.Production.json"
)

foreach ($file in $ConfigFiles) {
    if (Test-Path $file) {
        Write-Host "✅ Trovato: $file" -ForegroundColor Green
    } else {
        Write-Host "❌ MANCANTE: $file" -ForegroundColor Red
    }
}

# Verifica file principale
if (Test-Path "C:\inetpub\wwwroot\AiDocMaster\AiDocMaster.dll") {
    Write-Host "✅ Applicazione principale: AiDocMaster.dll" -ForegroundColor Green
} else {
    Write-Host "❌ MANCANTE: AiDocMaster.dll" -ForegroundColor Red
}
```

### **PASSO 7: RIAVVIA IIS** ▶️

```powershell
# Sul server di produzione
Write-Host "▶️ Riavvio IIS..." -ForegroundColor Yellow

# Metodo 1: Start Application Pool
Start-WebAppPool -Name "AiDocMaster" -ErrorAction SilentlyContinue

# Metodo 2: Start sito web
Start-WebSite -Name "AiDocMaster" -ErrorAction SilentlyContinue

# Metodo 3: Reset completo IIS
# iisreset /start

# Attendi stabilizzazione
Start-Sleep -Seconds 10

Write-Host "✅ IIS riavviato" -ForegroundColor Green
```

### **PASSO 8: TEST FINALE** 🧪

```powershell
# Sul server di produzione
Write-Host "🧪 Test finale..." -ForegroundColor Yellow

# Test 1: HTTP Status
try {
    $response = Invoke-WebRequest -Uri "https://www.aidocmaster.it" -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Sito raggiungibile: HTTP $($response.StatusCode)" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Risposta inaspettata: HTTP $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Errore connessione: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Database
try {
    sqlcmd -S "SRVPSTREE\SQLEXPRESS" -E -Q "SELECT TOP 1 * FROM INFORMATION_SCHEMA.TABLES" -d AIDOCMASTER
    Write-Host "✅ Database raggiungibile" -ForegroundColor Green
} catch {
    Write-Host "❌ Database non raggiungibile: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Login page
try {
    $loginResponse = Invoke-WebRequest -Uri "https://www.aidocmaster.it/Account/Login" -UseBasicParsing -TimeoutSec 30
    if ($loginResponse.StatusCode -eq 200) {
        Write-Host "✅ Pagina login funzionante" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Errore pagina login: $($_.Exception.Message)" -ForegroundColor Red
}
```

---

## 🚨 TROUBLESHOOTING

### **Errore HTTP 500**
```powershell
# Controlla i log
Get-Content "C:\inetpub\wwwroot\AiDocMaster\Logs\stdout*.log" -Tail 50
```

### **Service Unavailable**
```powershell
# Verifica Application Pool
Get-WebAppPoolState -Name "AiDocMaster"
# Se stopped: Start-WebAppPool -Name "AiDocMaster"
```

### **File bloccati**
```powershell
# Ferma completamente IIS
iisreset /stop
# Riprova il deploy
# Riavvia IIS
iisreset /start
```

### **Rollback Emergenza**
```powershell
# Ripristina dal backup
$BackupPath = "C:\Backup\AiDocMaster-[DATA]"  # Usa l'ultimo backup
iisreset /stop
Remove-Item "C:\inetpub\wwwroot\AiDocMaster\*" -Recurse -Force
Copy-Item "$BackupPath\*" "C:\inetpub\wwwroot\AiDocMaster\" -Recurse -Force
iisreset /start
```

---

## ✅ CHECKLIST FINALE

- [ ] ✅ Backup completato
- [ ] ✅ IIS fermato
- [ ] ✅ Dati utente preservati
- [ ] ✅ File trasferiti
- [ ] ✅ Nuovi file deployati
- [ ] ✅ Configurazioni verificate
- [ ] ✅ IIS riavviato
- [ ] ✅ Sito testato e funzionante
- [ ] ✅ Database accessibile
- [ ] ✅ Login funzionante

**🎉 DEPLOY COMPLETATO CON SUCCESSO!** 