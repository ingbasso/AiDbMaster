# Script di Build e Packaging - AiDbMaster

## Descrizione
Script PowerShell per compilare l'applicazione ASP.NET Core, generare gli script di migrazione del database e creare un pacchetto ZIP pronto per il deployment su server di produzione.

## Prerequisiti
- **.NET 8.0 SDK** installato
- **Entity Framework Core Tools** installati (`dotnet tool install --global dotnet-ef`)
- **PowerShell 5.0+** o **PowerShell Core**

## Utilizzo Base

### Esecuzione Standard
```powershell
.\build-and-package.ps1
```

### Parametri Disponibili

| Parametro | Descrizione | Valore Default | Esempio |
|-----------|-------------|----------------|---------|
| `-Configuration` | Configurazione di build | `Release` | `-Configuration Debug` |
| `-OutputPath` | Cartella di output | `.\Deploy` | `-OutputPath C:\Deploy` |
| `-ProjectPath` | Percorso file progetto | `.\AiDbMaster.csproj` | `-ProjectPath .\MyApp.csproj` |
| `-SkipBuild` | Salta la fase di build | `false` | `-SkipBuild` |
| `-Verbose` | Output dettagliato | `false` | `-Verbose` |

### Esempi di Utilizzo

```powershell
# Build standard
.\build-and-package.ps1

# Build con output dettagliato
.\build-and-package.ps1 -Verbose

# Build saltando la compilazione (se già compilato)
.\build-and-package.ps1 -SkipBuild

# Build con cartella di output personalizzata
.\build-and-package.ps1 -OutputPath "C:\Releases"

# Build in modalità Debug
.\build-and-package.ps1 -Configuration Debug
```

## Output dello Script

### Struttura Cartella di Output
```
Deploy/
└── AiDbMaster-Deploy-YYYYMMDD-HHMMSS/
    ├── App/                          # Applicazione pubblicata
    │   ├── wwwroot/                  # File statici
    │   ├── Views/                    # View Razor
    │   ├── AiDbMaster.dll           # Applicazione principale
    │   ├── AiDbMaster.exe           # Eseguibile
    │   ├── web.config               # Configurazione IIS
    │   ├── appsettings.json         # Configurazione base
    │   └── appsettings.Production.json # Configurazione produzione
    └── Database/                     # Script database
        ├── update-database.sql       # Script migrazioni SQL
        └── migrations-info.txt       # Informazioni migrazioni
```

### File ZIP Finale
- **Nome**: `AiDbMaster-Deploy-YYYYMMDD-HHMMSS.zip`
- **Contenuto**: Tutta la struttura sopra compressa
- **Dimensione**: Tipicamente 10-50 MB

## Operazioni Eseguite dallo Script

### 1. Verifica Prerequisiti
- ✅ Controllo presenza .NET CLI
- ✅ Controllo esistenza file progetto
- ✅ Creazione cartelle di output

### 2. Build Applicazione
- ✅ Restore pacchetti NuGet
- ✅ Compilazione in modalità Release
- ✅ Pubblicazione per runtime Windows x64

### 3. Generazione Script Database
- ✅ Verifica migrazioni Entity Framework
- ✅ Generazione script SQL idempotente
- ✅ Creazione file informazioni migrazioni

### 4. Ottimizzazione per Produzione
- ✅ Rimozione file di sviluppo (.pdb, .xml)
- ✅ Rimozione appsettings.Development.json
- ✅ Verifica presenza file essenziali

### 5. Packaging
- ✅ Creazione archivio ZIP
- ✅ Apertura cartella output
- ✅ Riepilogo operazioni

## Risoluzione Problemi

### Errore: ".NET CLI non trovato"
**Soluzione**: Installare .NET 8.0 SDK da https://dotnet.microsoft.com/download

### Errore: "Entity Framework Tools non trovato"
**Soluzione**: 
```powershell
dotnet tool install --global dotnet-ef
```

### Errore: "File progetto non trovato"
**Soluzione**: Verificare che il file `AiDbMaster.csproj` sia nella cartella corrente o specificare il percorso con `-ProjectPath`

### Errore: "Errore durante la compilazione"
**Soluzione**: 
1. Verificare che il codice compili correttamente in Visual Studio
2. Eseguire `dotnet build` manualmente per vedere errori dettagliati
3. Usare il parametro `-Verbose` per output dettagliato

### Errore: "Impossibile verificare le migrazioni"
**Soluzione**: 
1. Verificare che Entity Framework sia configurato correttamente
2. Controllare la connection string nel file appsettings.json
3. Le migrazioni verranno saltate ma l'app sarà comunque pacchettizzata

## Note Importanti

### Database
- Lo script genera script SQL **idempotenti** (possono essere eseguiti più volte)
- Gli script includono **tutte le migrazioni** dalla baseline
- Se non ci sono migrazioni, viene creato un file informativo

### Sicurezza
- Il file `appsettings.Production.json` contiene la connection string di produzione
- **ATTENZIONE**: La password del database è in chiaro nel file di configurazione

### Performance
- L'applicazione è pubblicata come **framework-dependent** (richiede .NET Runtime sul server)
- Runtime target: **win-x64** (Windows 64-bit)
- Modalità hosting IIS: **in-process** per migliori performance

## Deployment sul Server

Dopo aver eseguito questo script:

1. **Copiare il file ZIP** sul server di produzione
2. **Estrarre** il contenuto in `C:\inetpub\wwwroot\AiDbMaster`
3. **Eseguire lo script SQL** `update-database.sql` sul database AIDBMASTER
4. **Configurare IIS** (sarà oggetto del secondo script)
5. **Testare l'applicazione**

## Log e Debug

Lo script produce output colorato:
- 🟢 **Verde**: Operazioni completate con successo
- 🟡 **Giallo**: Avvisi (non bloccanti)
- 🔴 **Rosso**: Errori (bloccanti)
- ⚪ **Bianco**: Informazioni generali

Per debug dettagliato, usare sempre il parametro `-Verbose`.

