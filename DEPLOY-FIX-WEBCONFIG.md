# FIX DEPLOY - Preservazione web.config

## 🔍 PROBLEMA IDENTIFICATO (19 Nov 2024)

Durante il deploy in produzione, il file `web.config` complesso generato dallo script causava **errore 500** perché conteneva regole avanzate (HTTPS redirect, security headers, custom errors) che causavano conflitti.

L'applicazione funzionava perfettamente con il `web.config` semplice esistente sul server.

---

## ✅ SOLUZIONE IMPLEMENTATA

### **Deploy-Production.ps1 - Modificato (STEP 6)**

Lo script ora:
1. **Fa backup** del `web.config` esistente PRIMA del robocopy
2. Copia i nuovi file applicazione con robocopy
3. **Ripristina** automaticamente il `web.config` esistente (preserva la configurazione funzionante)

### **Codice aggiunto:**

```powershell
# Backup del web.config esistente (IMPORTANTE!)
if (Test-Path $existingWebConfig) {
    Copy-Item $existingWebConfig $tempWebConfigBackup
    Write-Success "Backup web.config esistente salvato"
}

# ... robocopy ...

# Ripristina web.config esistente se c'era (preserva configurazione funzionante)
if (Test-Path $tempWebConfigBackup) {
    Copy-Item $tempWebConfigBackup $existingWebConfig -Force
    Write-Success "web.config esistente ripristinato (preservata configurazione funzionante)"
}
```

---

## 🎯 COMPORTAMENTO DEPLOY

### **Primo Deploy (web.config non esiste):**
- Usa il `web.config` dal package (quello generato da `dotnet publish`)
- Semplice e funzionante

### **Deploy Successivi (web.config esiste):**
- ✅ **Preserva sempre** il `web.config` esistente (default)
- ✅ Non sovrascrive mai la configurazione funzionante
- ✅ Aggiorna solo i file .dll, .json, .css, .js, ecc.

### **Deploy con Aggiornamento web.config (quando necessario):**
Se hai fatto modifiche **essenziali** al web.config in sviluppo che devono essere applicate in produzione:

```powershell
.\Deploy-Production.ps1 -ZipPath ".\AiDbMasterDeploy-Final-Clean.zip" -ForceUpdateWebConfig
```

⚠️ **ATTENZIONE**: Usa `-ForceUpdateWebConfig` **solo quando strettamente necessario**! 
Il web.config è configurazione IIS specifica del server.

---

## 📋 PROCEDURA DEPLOY CORRETTA

### **1. Build & Package (PC Sviluppo):**
```powershell
cd C:\AiDbMaster
.\build-and-package.ps1
# Genera: AiDbMasterDeploy-Final-Clean.zip
```

### **2. Copia ZIP su Server Produzione:**
```powershell
Copy-Item "C:\AiDbMaster\AiDbMasterDeploy-Final-Clean.zip" "\\SVRGEST\C$\Deploy\AiDbMaster\"
```

### **3. Deploy sul Server:**
```powershell
cd C:\Deploy\AiDbMaster
.\Deploy-Production.ps1 -ZipPath ".\AiDbMasterDeploy-Final-Clean.zip"
```

Il `web.config` funzionante verrà **automaticamente preservato**! ✅

---

## 🔧 web.config.template

È stato creato anche un file `web.config.template` di riferimento con una configurazione **semplice e funzionante**.

Se necessario, puoi copiarlo manualmente sul server:
```powershell
Copy-Item "C:\AiDbMaster\web.config.template" "C:\inetpub\wwwroot\AiDbMaster\web.config"
```

---

## 📖 QUANDO MODIFICARE web.config

### **✅ CASI COMUNI (no modifica web.config necessaria):**

| Modifica in Sviluppo | Cosa Fare | Note |
|----------------------|-----------|------|
| Nuovo Controller/View/Model | Deploy normale | Compilato nel .dll |
| Modifica logica C# | Deploy normale | Compilato nel .dll |
| Nuova connection string | Modifica `appsettings.Production.json` | Script lo gestisce |
| Nuova API key | Modifica `appsettings.Production.json` | Script lo gestisce |
| Cambio livello logging | Modifica `appsettings.Production.json` | Script lo gestisce |
| Nuova libreria NuGet | Deploy normale | Inclusa nel package |

### **⚠️ CASI RARI (potrebbe servire modifica web.config):**

| Scenario | Soluzione Consigliata |
|----------|----------------------|
| Nuova environment variable ASP.NET Core | **Opzione A**: Aggiungi manualmente in produzione<br>**Opzione B**: Deploy con `-ForceUpdateWebConfig` |
| Cambio hosting model (InProcess ↔ OutOfProcess) | Modifica manualmente in produzione (test prima!) |
| Nuovi security headers | Valuta se necessario in prod, poi aggiungi manualmente |
| HTTPS redirect rules | Configurazione specifica del server, **non** deployare |
| Custom error pages | Configurazione specifica del server, **non** deployare |

### **🎯 REGOLA GENERALE:**

- **web.config = Configurazione IIS** (specifica del server)
- **appsettings.json = Configurazione Applicazione** (deployata normalmente)

**Il 99% delle modifiche va in `appsettings.json`, non in `web.config`!**

---

## 🚨 TROUBLESHOOTING

### **Se il deploy dà ancora errore 500:**

1. **Verifica che il web.config sia stato ripristinato:**
   ```powershell
   Get-Content "C:\inetpub\wwwroot\AiDbMaster\web.config" | Select-String "stdoutLogFile"
   ```
   Deve contenere: `stdoutLogFile=".\Logs\stdout"`

2. **Verifica permessi database:**
   ```sql
   -- Su SQL Server SVRGEST, database AIDBMASTER
   SELECT name, type_desc 
   FROM sys.database_principals 
   WHERE name = 'IIS APPPOOL\AiDbMaster';
   ```
   L'utente deve esistere e avere ruolo `db_owner`.

3. **Leggi i log applicazione:**
   ```powershell
   Get-ChildItem "C:\inetpub\wwwroot\AiDbMaster\Logs\" -Filter "stdout*.log" | 
       Sort-Object LastWriteTime -Descending | 
       Select-Object -First 1 | 
       Get-Content
   ```

---

## 📦 FILES MODIFICATI

- `Deploy-Production.ps1` - Aggiunta preservazione web.config
- `web.config.template` - Template di riferimento (nuovo)
- `DEPLOY-FIX-WEBCONFIG.md` - Questa documentazione (nuovo)

---

## ✅ STATO ATTUALE

- **Applicazione in Produzione**: ✅ Funzionante (con web.config ripristinato)
- **Deploy Script**: ✅ Corretto (preserva web.config)
- **Database**: ✅ Seed completato (33 risorse, permessi OK)
- **Permessi SQL**: ✅ IIS APPPOOL\AiDbMaster configurato

---

**Data Fix**: 19 Novembre 2024  
**Sessione**: Deploy in Produzione su SVRGEST

