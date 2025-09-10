# 🔐 GUIDA CONFIGURAZIONE UTENTI PRODUZIONE - AiDbMaster

## ⚠️ PROBLEMA IDENTIFICATO
Nel server di produzione mancano le configurazioni degli utenti necessari per:
- **IIS Application Pool** - per eseguire l'applicazione
- **SQL Server** - per accedere al database AIDBMASTER

## 📋 CHECKLIST PROBLEMI DA RISOLVERE

### ❌ Problemi attuali:
- [ ] Utente `IIS APPPOOL\AiDbMaster` non ha permessi sulla directory dell'applicazione
- [ ] Utente `IIS APPPOOL\AiDbMaster` non ha accesso al database SQL Server
- [ ] Application Pool potrebbe non essere configurato correttamente
- [ ] Permessi di sicurezza Windows mancanti

---

## 🛠️ SOLUZIONE 1: CONFIGURAZIONE IIS APPLICATION POOL

### **Passo 1: Verifica Application Pool**

Apri **IIS Manager** sul server di produzione e verifica:

```powershell
# PowerShell - Verifica Application Pool esistente
Import-Module WebAdministration
Get-WebAppPoolState -Name "AiDbMaster"
Get-WebApplication | Where-Object {$_.ApplicationPool -eq "AiDbMaster"}
```

### **Passo 2: Crea/Configura Application Pool**

Se non esiste, crealo:

```powershell
# Crea Application Pool
New-WebAppPool -Name "AiDbMaster" -Force

# Configura Application Pool
Set-WebConfigurationProperty -PSPath "IIS:\AppPools\AiDbMaster" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-WebConfigurationProperty -PSPath "IIS:\AppPools\AiDbMaster" -Name "managedRuntimeVersion" -Value ""
Set-WebConfigurationProperty -PSPath "IIS:\AppPools\AiDbMaster" -Name "enable32BitAppOnWin64" -Value $false

# Avvia Application Pool
Start-WebAppPool -Name "AiDbMaster"
```

### **Passo 3: Assegna Application Pool al Sito**

```powershell
# Assegna l'Application Pool al sito web
Set-WebConfigurationProperty -PSPath "IIS:\Sites\Default Web Site\AiDbMaster" -Name "applicationPool" -Value "AiDbMaster"

# Oppure se hai un sito dedicato:
# Set-WebConfigurationProperty -PSPath "IIS:\Sites\AiDbMaster" -Name "applicationPool" -Value "AiDbMaster"
```

---

## 🗂️ SOLUZIONE 2: PERMESSI DIRECTORY APPLICAZIONE

### **Passo 1: Identifica la Directory**

La directory dell'applicazione dovrebbe essere:
```
C:\inetpub\wwwroot\AiDbMaster\
```

### **Passo 2: Configura Permessi Windows**

Esegui questi comandi in **PowerShell come Amministratore**:

```powershell
# Directory dell'applicazione
$AppPath = "C:\inetpub\wwwroot\AiDbMaster"
$AppPoolIdentity = "IIS APPPOOL\AiDbMaster"

# Concedi permessi lettura ed esecuzione
icacls $AppPath /grant "${AppPoolIdentity}:(OI)(CI)RX" /T

# Concedi permessi scrittura su cartelle specifiche
icacls "$AppPath\Logs" /grant "${AppPoolIdentity}:(OI)(CI)F" /T
icacls "$AppPath\DocumentsStorage" /grant "${AppPoolIdentity}:(OI)(CI)F" /T
icacls "$AppPath\Uploads" /grant "${AppPoolIdentity}:(OI)(CI)F" /T

# Verifica permessi
icacls $AppPath | findstr "IIS APPPOOL"
```

### **Passo 3: Crea Cartelle Mancanti**

```powershell
# Crea cartelle necessarie se non esistono
$Folders = @("Logs", "DocumentsStorage", "Uploads")
foreach ($folder in $Folders) {
    $folderPath = Join-Path $AppPath $folder
    if (-not (Test-Path $folderPath)) {
        New-Item -Path $folderPath -ItemType Directory -Force
        Write-Host "✓ Creata cartella: $folderPath"
    }
    
    # Imposta permessi
    icacls $folderPath /grant "${AppPoolIdentity}:(OI)(CI)F" /T
}
```

---

## 🗄️ SOLUZIONE 3: CONFIGURAZIONE DATABASE SQL SERVER

### **Opzione A: Usa Account SA (Attuale - Più Semplice)**

Se vuoi continuare con l'account SA (più semplice ma meno sicuro):

1. **Verifica che l'account SA sia abilitato:**
```sql
-- Esegui in SQL Server Management Studio
SELECT name, is_disabled FROM sys.server_principals WHERE name = 'sa'

-- Se disabilitato, abilitalo:
ALTER LOGIN sa ENABLE
```

2. **La connection string attuale dovrebbe funzionare:**
```json
"DefaultConnection": "Data Source=SVRGEST;Initial Catalog=AIDBMASTER;User ID=sa;Password=Novoincipient3!;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False"
```

### **Opzione B: Crea Utente Dedicato (Raccomandato)**

Per maggiore sicurezza, crea un utente dedicato:

1. **Esegui lo script di creazione utente:**
```powershell
# Sul server SQL, esegui il file:
sqlcmd -S SVRGEST -E -i "create-dedicated-user.sql"
```

2. **Aggiorna la connection string:**
```json
"DefaultConnection": "Data Source=SVRGEST;Initial Catalog=AIDBMASTER;User ID=AiDbMasterApp;Password=AiDb2024!Secure#;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False"
```

### **Opzione C: Usa Windows Authentication con Application Pool Identity**

Per usare l'identità dell'Application Pool:

1. **Configura SQL Server per Windows Authentication:**
```sql
-- Crea login per l'Application Pool Identity
CREATE LOGIN [IIS APPPOOL\AiDbMaster] FROM WINDOWS
GO

USE [AIDBMASTER]
GO

-- Crea utente nel database
CREATE USER [IIS APPPOOL\AiDbMaster] FOR LOGIN [IIS APPPOOL\AiDbMaster]
GO

-- Assegna ruoli necessari
ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\AiDbMaster]
ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\AiDbMaster]
ALTER ROLE db_ddladmin ADD MEMBER [IIS APPPOOL\AiDbMaster]
GO
```

2. **Aggiorna la connection string:**
```json
"DefaultConnection": "Data Source=SVRGEST;Initial Catalog=AIDBMASTER;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False"
```

---

## 🧪 SCRIPT DI TEST COMPLETO

Salva questo script come `Test-AiDbMaster-Permissions.ps1`:

```powershell
# Test completo permessi AiDbMaster
param(
    [string]$AppPath = "C:\inetpub\wwwroot\AiDbMaster",
    [string]$AppPoolName = "AiDbMaster"
)

Write-Host "=============================================" -ForegroundColor Green
Write-Host "    TEST PERMESSI AIDBMASTER PRODUZIONE" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Test 1: Application Pool
Write-Host "`n1. VERIFICA APPLICATION POOL:" -ForegroundColor Cyan
try {
    Import-Module WebAdministration -ErrorAction Stop
    $poolState = Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop
    Write-Host "✓ Application Pool '$AppPoolName': $($poolState.Value)" -ForegroundColor Green
} catch {
    Write-Host "✗ Errore Application Pool: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Directory Applicazione
Write-Host "`n2. VERIFICA DIRECTORY APPLICAZIONE:" -ForegroundColor Cyan
if (Test-Path $AppPath) {
    Write-Host "✓ Directory applicazione esistente: $AppPath" -ForegroundColor Green
    
    # Verifica permessi
    $permissions = icacls $AppPath | Select-String "IIS APPPOOL\\$AppPoolName"
    if ($permissions) {
        Write-Host "✓ Permessi IIS APPPOOL trovati" -ForegroundColor Green
        $permissions | ForEach-Object { Write-Host "  $($_.Line)" -ForegroundColor Gray }
    } else {
        Write-Host "✗ Permessi IIS APPPOOL mancanti!" -ForegroundColor Red
    }
} else {
    Write-Host "✗ Directory applicazione non trovata: $AppPath" -ForegroundColor Red
}

# Test 3: File Applicazione
Write-Host "`n3. VERIFICA FILE APPLICAZIONE:" -ForegroundColor Cyan
$requiredFiles = @("AiDbMaster.dll", "web.config", "appsettings.Production.json")
foreach ($file in $requiredFiles) {
    $filePath = Join-Path $AppPath $file
    if (Test-Path $filePath) {
        Write-Host "✓ File trovato: $file" -ForegroundColor Green
    } else {
        Write-Host "✗ File mancante: $file" -ForegroundColor Red
    }
}

# Test 4: Cartelle Speciali
Write-Host "`n4. VERIFICA CARTELLE SPECIALI:" -ForegroundColor Cyan
$specialFolders = @("Logs", "DocumentsStorage", "Uploads")
foreach ($folder in $specialFolders) {
    $folderPath = Join-Path $AppPath $folder
    if (Test-Path $folderPath) {
        Write-Host "✓ Cartella esistente: $folder" -ForegroundColor Green
        
        # Test scrittura
        $testFile = Join-Path $folderPath "test-write.tmp"
        try {
            "test" | Out-File $testFile -ErrorAction Stop
            Remove-Item $testFile -ErrorAction SilentlyContinue
            Write-Host "  ✓ Scrittura OK" -ForegroundColor Green
        } catch {
            Write-Host "  ✗ Errore scrittura: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ Cartella mancante: $folder" -ForegroundColor Red
    }
}

# Test 5: Connessione Database
Write-Host "`n5. TEST CONNESSIONE DATABASE:" -ForegroundColor Cyan
try {
    # Leggi connection string da appsettings.Production.json
    $settingsPath = Join-Path $AppPath "appsettings.Production.json"
    if (Test-Path $settingsPath) {
        $settings = Get-Content $settingsPath | ConvertFrom-Json
        $connectionString = $settings.ConnectionStrings.DefaultConnection
        Write-Host "✓ Connection string trovata" -ForegroundColor Green
        
        # Test connessione (richiede SQL Server PowerShell module)
        # Uncomment se hai il modulo installato:
        # Import-Module SqlServer
        # $result = Invoke-Sqlcmd -ConnectionString $connectionString -Query "SELECT 1 as Test" -ErrorAction Stop
        # Write-Host "✓ Connessione database OK" -ForegroundColor Green
        
    } else {
        Write-Host "✗ File appsettings.Production.json non trovato" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Errore test database: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=============================================" -ForegroundColor Green
Write-Host "    TEST COMPLETATO" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
```

---

## 🚀 PROCEDURA RAPIDA DI RISOLUZIONE

### **Esegui questi comandi in ordine sul server di produzione:**

```powershell
# 1. Configura Application Pool
Import-Module WebAdministration
New-WebAppPool -Name "AiDbMaster" -Force
Set-WebConfigurationProperty -PSPath "IIS:\AppPools\AiDbMaster" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-WebConfigurationProperty -PSPath "IIS:\AppPools\AiDbMaster" -Name "managedRuntimeVersion" -Value ""
Start-WebAppPool -Name "AiDbMaster"

# 2. Configura Permessi Directory
$AppPath = "C:\inetpub\wwwroot\AiDbMaster"
$AppPoolIdentity = "IIS APPPOOL\AiDbMaster"
icacls $AppPath /grant "${AppPoolIdentity}:(OI)(CI)RX" /T

# 3. Crea e configura cartelle speciali
$Folders = @("Logs", "DocumentsStorage", "Uploads")
foreach ($folder in $Folders) {
    $folderPath = Join-Path $AppPath $folder
    New-Item -Path $folderPath -ItemType Directory -Force
    icacls $folderPath /grant "${AppPoolIdentity}:(OI)(CI)F" /T
}

# 4. Assegna Application Pool al sito
Set-WebConfigurationProperty -PSPath "IIS:\Sites\Default Web Site\AiDbMaster" -Name "applicationPool" -Value "AiDbMaster"

# 5. Riavvia tutto
Restart-WebAppPool -Name "AiDbMaster"
iisreset
```

### **Per il database (scegli una opzione):**

**Opzione Semplice (SA):**
- Verifica che l'account SA sia abilitato
- Usa la connection string attuale

**Opzione Sicura (Utente Dedicato):**
```sql
-- Esegui in SQL Server Management Studio
sqlcmd -S SVRGEST -E -i "create-dedicated-user.sql"
```

---

## ✅ VERIFICA FINALE

Dopo aver applicato le correzioni:

1. **Testa l'applicazione:**
   ```
   https://www.aidbmaster.it
   ```

2. **Controlla i log:**
   ```
   C:\inetpub\wwwroot\AiDbMaster\Logs\
   ```

3. **Verifica Application Pool:**
   ```powershell
   Get-WebAppPoolState -Name "AiDbMaster"
   ```

4. **Test permessi:**
   ```powershell
   .\Test-AiDbMaster-Permissions.ps1
   ```

---

## 🆘 RISOLUZIONE PROBLEMI

### **Se l'applicazione non si avvia:**
- Controlla i log in `C:\inetpub\wwwroot\AiDbMaster\Logs\`
- Verifica che .NET 8.0 Runtime sia installato
- Controlla che tutti i file DLL siano presenti

### **Se ci sono errori di database:**
- Verifica la connection string in `appsettings.Production.json`
- Testa la connessione con SQL Server Management Studio
- Controlla che il database AIDBMASTER esista

### **Se ci sono errori di permessi:**
- Esegui il test script per identificare i problemi specifici
- Verifica che l'Application Pool sia in esecuzione
- Controlla i permessi Windows sulle cartelle

---

**🎯 Questa guida dovrebbe risolvere completamente il problema degli utenti mancanti nel server di produzione!**
