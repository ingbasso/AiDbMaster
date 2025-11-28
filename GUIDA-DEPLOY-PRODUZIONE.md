# 🚀 GUIDA AL DEPLOY IN PRODUZIONE - AiDbMaster

Questa guida spiega come fare il deploy di **AiDbMaster** sul server di produzione **SVRGEST** usando gli script automatici.

## 📋 INDICE

1. [Prerequisiti](#prerequisiti)
2. [STEP 1: Build e Package (Server Sviluppo)](#step-1-build-e-package-server-sviluppo)
3. [STEP 2: Deploy Produzione (Server SVRGEST)](#step-2-deploy-produzione-server-svrgest)
4. [Parametri Opzionali](#parametri-opzionali)
5. [Risoluzione Problemi](#risoluzione-problemi)
6. [Rollback](#rollback)

---

## 📦 PREREQUISITI

### Sul Server di Sviluppo:
- ✅ .NET SDK 9.0+ installato
- ✅ Accesso alla directory del progetto `C:\AiDbMaster`
- ✅ PowerShell con privilegi amministratore

### Sul Server di Produzione (SVRGEST):
- ✅ Windows Server con IIS configurato
- ✅ ASP.NET Core Hosting Bundle 9.0+ installato
- ✅ SQL Server con database **AIDBMASTER** già creato
- ✅ Application Pool **AiDbMaster** configurato
- ✅ Directory `C:\inetpub\wwwroot\AiDbMaster` esistente
- ✅ PowerShell con privilegi amministratore

---

## 🛠️ STEP 1: BUILD E PACKAGE (Server Sviluppo)

Questo step **crea il pacchetto ZIP** con tutti i file necessari per il deploy.

### 1.1 Apri PowerShell come Amministratore

```powershell
cd C:\AiDbMaster
```

### 1.2 Esegui lo script di build

```powershell
.\Build-and-Package.ps1
```

### 1.3 Cosa fa lo script?

1. ✅ **Verifica prerequisiti** (.NET SDK, file progetto)
2. ✅ **Pulisce** le directory `bin`, `obj`, `publish`
3. ✅ **Restore** dei pacchetti NuGet
4. ✅ **Compila** l'applicazione in modalità `Release`
5. ✅ **Genera script SQL** automaticamente da Entity Framework:
   - `UPDATE_PRODUZIONE_EF.sql` → Script completo con tutte le migration
   - `UPDATE_PRODUZIONE_INCREMENTAL.sql` → Script incrementale idempotente
6. ✅ **Crea struttura package**:
   ```
   AiDbMasterDeploy-Final-Clean.zip
   ├── App/                    → File applicazione compilata
   ├── Database/               → Script SQL per aggiornamento DB
   │   ├── UPDATE_PRODUZIONE_EF.sql
   │   └── UPDATE_PRODUZIONE_INCREMENTAL.sql
   ├── Config/                 → Template configurazione produzione
   │   └── appsettings.Production.json
   └── ISTRUZIONI-DEPLOY.txt   → Istruzioni dettagliate
   ```
7. ✅ **Comprime** tutto in un file ZIP

### 1.4 Output

Al termine vedrai un messaggio come questo:

```
============================================================================
BUILD AND PACKAGE COMPLETATO CON SUCCESSO
============================================================================
File ZIP: AiDbMasterDeploy-Final-Clean.zip
Pronto per il deploy su SVRGEST
Fine: 17/11/2024 14:30:00
```

Il file ZIP si trova nella directory corrente: `C:\AiDbMaster\AiDbMasterDeploy-Final-Clean.zip`

---

## 🚀 STEP 2: DEPLOY PRODUZIONE (Server SVRGEST)

Questo step **installa** il pacchetto sul server di produzione.

### 2.1 Trasferisci il file ZIP sul server SVRGEST

Puoi usare:
- **Copia manuale** su rete condivisa
- **Remote Desktop** e copia&incolla
- **FTP/SFTP**

Esempio: copia il file in `C:\Deploy\AiDbMaster\AiDbMasterDeploy-Final-Clean.zip`

### 2.2 Connettiti al server SVRGEST

- Usa **Remote Desktop** per connetterti a **SVRGEST**
- Apri **PowerShell come Amministratore**

### 2.3 Posizionati nella directory dove hai copiato il file ZIP

```powershell
cd C:\Deploy\AiDbMaster
```

### 2.4 Scarica lo script Deploy-Production.ps1 (se non l'hai già)

Copia il file `Deploy-Production.ps1` dal progetto di sviluppo nella stessa directory del ZIP.

### 2.5 Esegui lo script di deploy

```powershell
.\Deploy-Production.ps1 -ZipPath ".\AiDbMasterDeploy-Final-Clean.zip"
```

### 2.6 Cosa fa lo script?

1. ✅ **Verifica prerequisiti**:
   - Privilegi amministratore
   - File ZIP esistente
   - Connessione a SQL Server **SVRGEST**
   - Database **AIDBMASTER** accessibile
   - Directory applicazione `C:\inetpub\wwwroot\AiDbMaster`

2. ✅ **Backup automatico database**:
   - Crea un backup completo in `C:\Backup\AiDbMaster\AIDBMASTER_Backup_YYYYMMDD_HHMMSS.bak`
   - Usa compressione e checksum

3. ✅ **Stop servizi IIS**:
   - Ferma l'Application Pool **AiDbMaster**
   - Ferma IIS per liberare i file

4. ✅ **Backup applicazione corrente**:
   - Crea uno ZIP di backup in `C:\Backup\AiDbMaster\App_Backup_YYYYMMDD_HHMMSS.zip`

5. ✅ **Aggiornamento database**:
   - Estrae il pacchetto in una directory temporanea
   - Esegue gli script SQL generati da Entity Framework
   - Priorità: Incrementale → Completo → Manuale → Allineamento

6. ✅ **Deploy file applicazione**:
   - Copia i nuovi file da `App/` a `C:\inetpub\wwwroot\AiDbMaster`
   - **Preserva** le configurazioni esistenti (`appsettings.json`, `appsettings.Production.json`)
   - Applica nuova configurazione se presente nel package

7. ✅ **Riavvio servizi**:
   - Riavvia IIS
   - Riavvia Application Pool **AiDbMaster**
   - Attende 10 secondi per stabilizzazione

8. ✅ **Verifica post-deploy**:
   - Controlla connessione al database
   - Verifica esistenza tabelle critiche (Resources, Permissions, OrdiniTestate, etc.)
   - Mostra ultime 3 migration applicate
   - Pulisce directory temporanea

### 2.7 Output

Al termine vedrai un messaggio come questo:

```
============================================================================
DEPLOY PRODUZIONE COMPLETATO CON SUCCESSO
============================================================================
Server: SVRGEST
Database: AIDBMASTER
Applicazione: C:\inetpub\wwwroot\AiDbMaster
Backup salvati in: C:\Backup\AiDbMaster
Fine: 17/11/2024 15:00:00

AZIONI CONSIGLIATE - TEST POST-DEPLOY:
  1. Testare login applicazione
  2. Verificare dashboard principale
  3. Controllare log applicazione per errori
  ...
```

### 2.8 Test applicazione

Apri il browser e vai su:

```
http://SVRGEST/
```

oppure

```
http://localhost/
```

(dipende dalla configurazione IIS)

---

## ⚙️ PARAMETRI OPZIONALI

### Build-and-Package.ps1

```powershell
.\Build-and-Package.ps1 `
    -ProjectPath "C:\MioProgetto" `
    -OutputZip "MioPacchetto.zip"
```

| Parametro | Default | Descrizione |
|-----------|---------|-------------|
| `-ProjectPath` | `.` (directory corrente) | Path del progetto da compilare |
| `-OutputZip` | `AiDbMasterDeploy-Final-Clean.zip` | Nome del file ZIP di output |

### Deploy-Production.ps1

```powershell
.\Deploy-Production.ps1 `
    -ZipPath ".\AiDbMasterDeploy-Final-Clean.zip" `
    -ServerName "SVRGEST" `
    -DatabaseName "AIDBMASTER" `
    -AppDirectory "C:\inetpub\wwwroot\AiDbMaster" `
    -BackupDirectory "C:\Backup\AiDbMaster" `
    -AppPoolName "AiDbMaster" `
    -SkipBackup
```

| Parametro | Default | Descrizione |
|-----------|---------|-------------|
| `-ZipPath` | **OBBLIGATORIO** | Path completo del file ZIP |
| `-ServerName` | `SVRGEST` | Nome istanza SQL Server |
| `-DatabaseName` | `AIDBMASTER` | Nome database |
| `-AppDirectory` | `C:\inetpub\wwwroot\AiDbMaster` | Directory applicazione IIS |
| `-BackupDirectory` | `C:\Backup\AiDbMaster` | Directory backup |
| `-AppPoolName` | `AiDbMaster` | Nome Application Pool IIS |
| `-SkipBackup` | (switch) | Salta il backup del database |

**Esempio con parametri personalizzati:**

```powershell
.\Deploy-Production.ps1 `
    -ZipPath "C:\Deploy\MioZip.zip" `
    -ServerName "MIOSERVER" `
    -DatabaseName "MIODB"
```

---

## 🔧 RISOLUZIONE PROBLEMI

### Problema: ".NET SDK non trovato"

**Soluzione:**
- Sul **server di sviluppo**: Installa .NET SDK 9.0+
- Sul **server di produzione**: Installa solo ASP.NET Core Hosting Bundle (non serve SDK completo)

### Problema: "Application Pool non trovato"

**Soluzione:**
```powershell
# Crea manualmente l'Application Pool
Import-Module WebAdministration
New-WebAppPool -Name "AiDbMaster"
Set-ItemProperty -Path "IIS:\AppPools\AiDbMaster" -Name managedRuntimeVersion -Value ""
```

### Problema: "Database non accessibile"

**Soluzione:**
1. Verifica che SQL Server sia in esecuzione
2. Verifica la connection string in `appsettings.Production.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=SVRGEST;Database=AIDBMASTER;Trusted_Connection=true;TrustServerCertificate=true;"
   }
   ```
3. Verifica i permessi dell'Application Pool sul database

### Problema: "Script SQL fallisce"

**Soluzione:**
1. Lo script proverà automaticamente più alternative:
   - Script incrementale (idempotente)
   - Script completo
   - Script manuale
   - Script di allineamento
2. Se tutti falliscono, esegui manualmente `UPDATE_PRODUZIONE_EF.sql` da SQL Server Management Studio

### Problema: "Errore durante copia file"

**Soluzione:**
1. Verifica che IIS sia stato fermato correttamente
2. Controlla i permessi sulla directory `C:\inetpub\wwwroot\AiDbMaster`
3. Riavvia il server se necessario

---

## 🔄 ROLLBACK

Se il deploy fallisce o l'applicazione non funziona, puoi ripristinare la versione precedente.

### Rollback Applicazione

1. **Ferma IIS:**
   ```powershell
   iisreset /stop
   Stop-WebAppPool -Name "AiDbMaster"
   ```

2. **Ripristina backup applicazione:**
   ```powershell
   # Trova l'ultimo backup
   $lastBackup = Get-ChildItem "C:\Backup\AiDbMaster\App_Backup_*.zip" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
   
   # Estrai il backup
   Expand-Archive -Path $lastBackup.FullName -DestinationPath "C:\inetpub\wwwroot\AiDbMaster" -Force
   ```

3. **Riavvia IIS:**
   ```powershell
   iisreset /start
   Start-WebAppPool -Name "AiDbMaster"
   ```

### Rollback Database

1. **Trova l'ultimo backup:**
   ```powershell
   Get-ChildItem "C:\Backup\AiDbMaster\AIDBMASTER_Backup_*.bak" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
   ```

2. **Ripristina da SQL Server Management Studio:**
   - Connettiti a **SVRGEST**
   - Right-click su **AIDBMASTER** → Tasks → Restore → Database
   - Seleziona il file `.bak` trovato sopra
   - Clicca OK

   **OPPURE usa SQL:**
   ```sql
   USE master;
   GO
   
   -- Disconnetti tutti gli utenti
   ALTER DATABASE AIDBMASTER SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
   
   -- Ripristina backup
   RESTORE DATABASE AIDBMASTER
   FROM DISK = 'C:\Backup\AiDbMaster\AIDBMASTER_Backup_20241117_150000.bak'
   WITH REPLACE;
   
   -- Riconnetti utenti
   ALTER DATABASE AIDBMASTER SET MULTI_USER;
   GO
   ```

---

## 📝 CHECKLIST POST-DEPLOY

Dopo ogni deploy, verifica:

- [ ] Login applicazione funziona
- [ ] Dashboard principale carica correttamente
- [ ] Gestione Permessi: ruoli e risorse visibili
- [ ] Dashboard Consegne: KPI e grafici funzionanti
- [ ] Interrogazioni AI: test query con Mistral
- [ ] Consegne Programmate: filtro agente automatico
- [ ] Schedulatore OP: drag&drop e resize funzionanti
- [ ] Anagrafica Clienti: accesso corretto per agenti
- [ ] Log applicazione senza errori critici

---

## 📞 SUPPORTO

Per problemi o domande, contatta il team di sviluppo.

**File Log:**
- Applicazione: `C:\inetpub\wwwroot\AiDbMaster\Logs\`
- IIS: `C:\inetpub\logs\LogFiles\`

**Comandi utili:**
```powershell
# Stato Application Pool
Get-WebAppPoolState -Name "AiDbMaster"

# Log eventi IIS
Get-EventLog -LogName Application -Source "ASP.NET Core*" -Newest 20

# Riavvio IIS completo
iisreset
```

---

**Buon Deploy! 🚀**

