# Guida al Deployment di AiDbMaster su IIS

Questa guida ti aiuterà a configurare correttamente AiDbMaster sul server di produzione SVRGEST con IIS.

## 📋 Prerequisiti

- **Server**: SVRGEST con Windows Server e IIS installato
- **Database**: SQL Server con database AIDBMASTER già creato
- **Path fisico**: `C:\inetpub\wwwroot\AiDbMaster`
- **Permessi**: Accesso amministratore sul server

## 🚀 Script di Deployment

Sono stati creati 4 script PowerShell per automatizzare il processo:

### 1. `Setup-IIS-Permissions.ps1` - Configurazione Base IIS
**Cosa fa:**
- Crea e configura l'Application Pool per .NET Core
- Imposta i permessi corretti sulle cartelle
- Configura il sito web su IIS
- Crea le cartelle necessarie (Logs, Uploads, etc.)

**Come usare:**
```powershell
# Esegui come Amministratore
.\Setup-IIS-Permissions.ps1
```

### 2. `Setup-Database-Permissions.ps1` - Permessi Database
**Cosa fa:**
- Genera script T-SQL per configurare i permessi database
- Crea login e utente per l'Application Pool Identity
- Assegna i ruoli necessari per Entity Framework

**Come usare:**
```powershell
# Genera lo script SQL
.\Setup-Database-Permissions.ps1

# Poi esegui il file .sql generato su SQL Server Management Studio
```

### 3. `Setup-Production-Config.ps1` - Ottimizzazioni Produzione
**Cosa fa:**
- Configura variabili d'ambiente per Production
- Ottimizza l'Application Pool per produzione
- Abilita compressione e caching
- Crea web.config ottimizzato

**Come usare:**
```powershell
.\Setup-Production-Config.ps1
```

### 4. `Deploy-AiDbMaster.ps1` - Deployment Completo
**Cosa fa:**
- Combina tutti gli script precedenti
- Copia i files dell'applicazione (opzionale)
- Configura tutto in un unico processo

**Come usare:**
```powershell
# Deployment completo senza copia files
.\Deploy-AiDbMaster.ps1 -SkipFilesCopy

# Deployment con copia files da una cartella
.\Deploy-AiDbMaster.ps1 -SourcePath "C:\Path\To\Published\Files"
```

## 📝 Procedura Passo-Passo

### Passo 1: Preparazione
1. Apri PowerShell come **Amministratore** sul server SVRGEST
2. Naviga nella cartella dove hai salvato gli script
3. Assicurati che IIS sia installato e funzionante

### Passo 2: Deployment Rapido
```powershell
# Opzione A: Deployment completo automatico
.\Deploy-AiDbMaster.ps1 -SkipFilesCopy

# Opzione B: Deployment manuale passo-passo
.\Setup-IIS-Permissions.ps1
.\Setup-Database-Permissions.ps1
.\Setup-Production-Config.ps1
```

### Passo 3: Configurazione Database
1. Apri **SQL Server Management Studio**
2. Connettiti al server **SVRGEST**
3. Apri il file `Database-Permissions.sql` generato
4. Esegui lo script per configurare i permessi

### Passo 4: Copia Files Applicazione
Se non hai usato il parametro `-SourcePath`, copia manualmente i files:
1. Pubblica l'applicazione da Visual Studio in modalità **Release**
2. Copia tutti i files nella cartella `C:\inetpub\wwwroot\AiDbMaster`
3. Assicurati che il file `appsettings.Production.json` sia presente

### Passo 5: Test e Verifica
1. Apri un browser e naviga su `http://localhost`
2. Verifica che l'applicazione si carichi correttamente
3. Controlla i log nella cartella `C:\inetpub\wwwroot\AiDbMaster\Logs`

## ⚙️ Configurazioni Applicate

### Application Pool
- **Nome**: AiDbMaster
- **Identità**: ApplicationPoolIdentity
- **Runtime**: No Managed Code (.NET Core)
- **Ambiente**: Production
- **Ottimizzazioni**: Idle timeout disabilitato, riavvio ogni 24h

### Permessi Files
- **Application Pool Identity**: FullControl su tutte le cartelle
- **IIS_IUSRS**: ReadAndExecute
- **IUSR**: ReadAndExecute
- **Cartelle scrivibili**: App_Data, Logs, Uploads, Shared, DocumentsStorage, Temp

### Database
- **Login**: `SVRGEST\AiDbMaster` (Application Pool Identity)
- **Ruoli**: db_datareader, db_datawriter, db_ddladmin
- **Permessi**: CREATE TABLE, ALTER SCHEMA, CREATE PROCEDURE

### IIS
- **Compressione**: Abilitata (statica e dinamica)
- **Caching**: 30 giorni per file statici
- **Sicurezza**: Server header rimosso, max upload 50MB
- **Autenticazione**: Anonima abilitata

## 🔧 Risoluzione Problemi

### Errore "500.19 - Internal Server Error"
- **Causa**: web.config mancante o malformato
- **Soluzione**: Esegui `Setup-Production-Config.ps1` per ricreare web.config

### Errore di connessione database
- **Causa**: Permessi database non configurati
- **Soluzione**: Esegui lo script `Database-Permissions.sql` su SQL Server

### Errore "403 - Forbidden"
- **Causa**: Permessi files insufficienti
- **Soluzione**: Esegui `Setup-IIS-Permissions.ps1` per riconfigurare i permessi

### Application Pool si ferma continuamente
- **Causa**: Errore nell'applicazione o configurazione
- **Soluzione**: Controlla i log in `C:\inetpub\wwwroot\AiDbMaster\Logs\stdout`

## 📊 Monitoraggio

### Log Files
- **Applicazione**: `C:\inetpub\wwwroot\AiDbMaster\Logs\`
- **IIS**: `C:\inetpub\logs\LogFiles\W3SVC1\`
- **Event Viewer**: Windows Logs > Application

### Performance Counters
- **Application Pool**: Monitoraggio memoria e CPU
- **Database**: Connessioni attive e query performance
- **IIS**: Request/sec e response time

## 🔒 Sicurezza

### Raccomandazioni
1. **HTTPS**: Configura certificato SSL per produzione
2. **Firewall**: Limita accesso solo alle porte necessarie
3. **Backup**: Configura backup automatici database e files
4. **Updates**: Mantieni aggiornati Windows, IIS e .NET Runtime

### Configurazione HTTPS (Opzionale)
```powershell
# Dopo aver installato il certificato SSL
New-WebBinding -Name "AiDbMaster" -Protocol https -Port 443 -HostHeader "aidbmaster.it"
```

## 📞 Supporto

Se incontri problemi durante il deployment:
1. Controlla i log dell'applicazione
2. Verifica che tutti i prerequisiti siano soddisfatti
3. Esegui nuovamente gli script di configurazione
4. Controlla la connessione al database SQL Server

---

**Nota**: Tutti gli script sono progettati per essere eseguiti più volte senza problemi. Se qualcosa non funziona, puoi rieseguire lo script corrispondente per ripristinare la configurazione corretta.
