# ⚡ AGGIORNAMENTO RAPIDO - AiDocMaster

## 🎯 **3 METODI PER AGGIORNARE**

### 🥇 **METODO 1: ZIP PRECOMPILATO (RACCOMANDATO)**
```powershell
# 💻 SUL PC DI SVILUPPO:
.\Build-Release-Package.ps1

# 🖥️ SUL SERVER (cartella temporanea):
Expand-Archive -Path "AiDocMaster-YYYYMMDD-HHMM.zip" -DestinationPath "C:\Temp\Deploy"
cd C:\Temp\Deploy
.\Deploy-From-Package.ps1
```

### 🚀 **METODO 2: SINCRONIZZAZIONE SORGENTI**
```powershell
# Sul server di produzione dalla directory del progetto aggiornato:
# 1. Sincronizza i sorgenti
.\Sync-To-Production.ps1

# 2. Vai nella cartella sorgenti del server
cd C:\inetpub\AiDocMaster

# 3. Esegui l'aggiornamento automatico
.\Update-Production.ps1
```

### 👨‍💻 **METODO 3: MANUALE**
```powershell
# Backup
$BackupPath = "C:\Backup\AiDocMaster-$(Get-Date -Format 'yyyyMMdd-HHmm')"
Copy-Item "C:\inetpub\wwwroot\AiDocMaster" -Destination "$BackupPath\App" -Recurse -Force

# Stop
Stop-WebAppPool -Name "AiDocMasterAppPool"

# Deploy
dotnet publish --configuration Release --output "C:\inetpub\wwwroot\AiDocMaster" --force

# Start
Start-WebAppPool -Name "AiDocMasterAppPool"

# Test
Invoke-WebRequest -Uri "https://www.aidocmaster.it" -UseBasicParsing
```

---

## ✅ **DOPO L'AGGIORNAMENTO - TEST VELOCE**

1. **Vai su**: `https://www.aidocmaster.it`
2. **Login**: Accedi come amministratore
3. **Test nuova funzione**: Vai in "Gestione Utenti" → Clicca "Password" su un utente
4. **Verifica**: Cambia password e controlla messaggio successo

---

## 🔧 **COMANDI UTILI POST-AGGIORNAMENTO**

```powershell
# Stato applicazione
Get-WebAppPoolState -Name "AiDocMasterAppPool"

# Log errori
Get-Content "C:\inetpub\wwwroot\AiDocMaster\Logs\stdout*.log" | Select-Object -Last 10

# Riavvio se necessario
Restart-WebAppPool -Name "AiDocMasterAppPool"

# Test HTTPS
Invoke-WebRequest -Uri "https://www.aidocmaster.it" -UseBasicParsing

# Verifica binding
Get-WebBinding -Name "AiDocMaster"
```

---

## 🚨 **ROLLBACK VELOCE (se serve)**

```powershell
Stop-WebAppPool -Name "AiDocMasterAppPool"
$LastBackup = Get-ChildItem "C:\Backup\AiDocMaster*" | Sort-Object Name -Descending | Select-Object -First 1
Copy-Item "$($LastBackup.FullName)\App\*" -Destination "C:\inetpub\wwwroot\AiDocMaster" -Recurse -Force
Start-WebAppPool -Name "AiDocMasterAppPool"
```

---

## 📝 **COSA OTTERRAI**

✅ **Cambio password utenti**: Nuova funzione per amministratori  
✅ **HTTPS potenziato**: Sicurezza migliorata  
✅ **CORS configurato**: Supporto dominio pubblico  
✅ **Performance**: Caching e compressione ottimizzati  

---

## 📁 **STRUTTURA TUO SERVER**

```
C:\inetpub\
├── AiDocMaster\                ← 📂 SORGENTI (qui copi i file aggiornati)
└── wwwroot\AiDocMaster\        ← 🌐 SITO LIVE (pubblicato automaticamente)
```

---

## 📊 **CONFRONTO METODI**

| Metodo | Tempo | Downtime | Difficoltà | Ambiente |
|--------|-------|----------|------------|----------|
| **ZIP Precompilato** | 6-10 min | 2-3 min | ⭐ Facile | Professionale |
| **Sincronizzazione** | 8-12 min | 3-5 min | ⭐⭐ Medio | Sviluppo |
| **Manuale** | 10-15 min | 5-10 min | ⭐⭐⭐ Avanzato | Debug |

---

## 📞 **SUPPORTO**

**File dettagliati:**
- `DEPLOY-CON-ZIP.md` - **GUIDA ZIP PRECOMPILATO** 🥇
- `AGGIORNAMENTO-TUO-SERVER.md` - **GUIDA SINCRONIZZAZIONE** ⭐
- `Build-Release-Package.ps1` - Script build ZIP
- `Deploy-From-Package.ps1` - Script deploy ZIP
- `Sync-To-Production.ps1` - Script sincronizzazione sorgenti
- `Update-Production.ps1` - Script aggiornamento automatico
- `AGGIORNAMENTO-PRODUZIONE.md` - Guida completa
- `DEPLOY-GUIDE-PRODUZIONE-PUBBLICA.md` - Configurazioni avanzate

**🎯 Tutto pronto in 6-10 minuti con il metodo ZIP!** 