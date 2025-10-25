# 🔍 Interrogazioni DB - Documentazione

## Panoramica

È stata creata una nuova sezione **"Interrogazioni DB"** nel sistema AiDbMaster per eseguire query avanzate sul database e ottenere informazioni dettagliate su disponibilità articoli e consegne programmate.

---

## 📦 File Creati

### 1. **ViewModel**
- `ViewModels/InterrogazioniDBViewModels.cs`
  - `DisponibilitaViewModel`: Form di ricerca e risultati disponibilità
  - `DisponibilitaRigaViewModel`: Singola riga di risultato
  - `ConsegneProgrammateViewModel`: Placeholder per consegne programmate
  - `ConsegnaProgrammataRigaViewModel`: Placeholder per risultati consegne

### 2. **Controller**
- `Controllers/InterrogazioniDBController.cs`
  - `Index()`: Pagina principale menu Interrogazioni DB
  - `Disponibilita()` GET/POST: Gestione ricerca disponibilità
  - `SearchArticoli()`: API per autocomplete articoli
  - `ConsegneProgrammate()` GET/POST: Placeholder consegne programmate

### 3. **Views**
- `Views/InterrogazioniDB/Index.cshtml`: Pagina principale menu
- `Views/InterrogazioniDB/Disponibilita.cshtml`: Form e griglia disponibilità
- `Views/InterrogazioniDB/ConsegneProgrammate.cshtml`: Placeholder consegne

### 4. **Menu**
- Aggiornato `Views/Shared/_Layout.cshtml` con nuova voce "Interrogazioni DB"

---

## 🎯 Funzionalità Implementate

### ✅ **Disponibilità Articolo**

**URL**: `/InterrogazioniDB/Disponibilita`

**Funzionalità:**
1. **Ricerca Articolo**: Dropdown autocompletante (Select2) che cerca per:
   - Codice Articolo
   - Descrizione
   - Minimo 2 caratteri per attivare la ricerca

2. **Campo Data**: Presente ma non utilizzato nella logica (come richiesto)
   - Default: Data odierna
   - Pronto per implementazione futura

3. **Risultati Visualizzati**:
   - Tabella con tutti i magazzini contenenti l'articolo
   - **Campi Mostrati**:
     - Codice Articolo
     - Magazzino
     - **Esistenza** (da DB)
     - **Impegnato** (attualmente 0 - da implementare)
     - **Disponibile** (calcolato: Esistenza - Impegnato)
     - **Ordinato Fornitori** (da DB)
     - **Totale Previsto** (calcolato: Esistenza + Ord. Fornitori)

4. **Statistiche Riepilogative**:
   - Numero totale magazzini
   - Totale esistenza
   - Totale disponibile
   - Totale ordinato fornitori

5. **Colorazione Automatica**:
   - 🟢 **Verde**: Disponibile > 0
   - 🟡 **Giallo**: Disponibile = 0
   - 🔴 **Rosso**: Disponibile < 0

### ⏳ **Consegne Programmate** (Placeholder)

**URL**: `/InterrogazioniDB/ConsegneProgrammate`

**Status**: In fase di implementazione
- Form placeholder presente
- Struttura pronta per sviluppo futuro

---

## 🔧 Tecnologie Utilizzate

### Select2 (jQuery Plugin)
- **CDN**: https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/
- **Tema**: Bootstrap 5
- **Configurazione**: AJAX con ritardo 250ms, minimo 2 caratteri

### Bootstrap 5
- Card per layout
- Tabelle responsive
- Alert per messaggi

### Entity Framework Core
- Query LINQ su `ProgressiviArticoli`
- Include su `AnagraficaArticoli` per descrizione

---

## 📊 Query Database

### Query Principale (Disponibilità)

```sql
SELECT 
    CodiceArticolo,
    CodiceMagazzino,
    Esistenza,
    0 AS ImpegnatoDataOdierna, -- Da implementare
    OrdinatoFornitoriDataOdierna,
    (Esistenza - 0) AS Disponibile,
    (Esistenza + OrdinatoFornitoriDataOdierna) AS TotalePrevisto
FROM ProgressiviArticoli
WHERE CodiceArticolo = @CodiceArticolo
ORDER BY CodiceMagazzino
```

### Query Autocomplete

```sql
SELECT TOP 20
    CodiceArticolo AS id,
    CodiceArticolo + ' - ' + Descrizione AS text
FROM AnagraficaArticoli
WHERE CodiceArticolo LIKE '%' + @term + '%'
   OR Descrizione LIKE '%' + @term + '%'
ORDER BY CodiceArticolo
```

---

## 🚀 Come Testare

### 1. **Avvia l'applicazione**
```bash
dotnet run
```

### 2. **Accedi al sistema**
- Login con credenziali esistenti
- Tutti gli utenti autenticati hanno accesso

### 3. **Naviga alla sezione**
- **Sidebar** → Interrogazioni DB → Disponibilità

### 4. **Esegui una ricerca**
1. Clicca sul campo "Codice Articolo"
2. Digita almeno 2 caratteri (es: "AB")
3. Seleziona un articolo dalla lista
4. (Opzionale) Seleziona una data
5. Clicca "Cerca Disponibilità"

### 5. **Verifica i risultati**
- Dovrebbero apparire tutti i magazzini con quell'articolo
- Le statistiche riepilogative in alto
- La tabella dettagliata
- La riga dei totali in fondo

---

## 📝 Note Implementative

### Campo "Impegnato"
✅ **IMPLEMENTATO**: Il campo `ImpegnatoDataOdierna` è stato aggiunto e la logica implementata.

**Funzionamento**:
- Il campo `ImpegnatoDataOdierna` in `ProgressiviArticoli` contiene le quantità impegnate fino ad OGGI
- Se `DataRiferimento <= OGGI`: usa il valore dal DB (già pre-calcolato)
- Se `DataRiferimento > OGGI`: aggiunge gli impegni futuri calcolati da `OrdiniRighe`

**Calcolo Impegni Futuri**:
```csharp
ImpegnatoFuturo = SUM(Quantita - QuantitaEvasa)
FROM OrdiniRighe
WHERE TipoOrdine = 'R'  // Solo ordini clienti
  AND CodiceArticolo = @CodiceArticolo
  AND DataConsegna > OGGI
  AND DataConsegna <= @DataRiferimento
  AND (Quantita - QuantitaEvasa) > 0
```

**Formula Finale**:
- `Impegnato Totale = ImpegnatoDataOdierna (DB) + Impegnato Futuro (calcolato)`
- `Disponibile = Esistenza - Impegnato Totale`

### Campo "Data Riferimento"
✅ **IMPLEMENTATO**: Il campo ora viene utilizzato per calcolare la disponibilità futura.

**Funzionamento**:
- Se `NULL` o `<= OGGI`: usa solo i dati attuali dal DB
- Se `> OGGI`: calcola gli impegni futuri fino alla data specificata
- Permette di fare proiezioni di disponibilità nel tempo

**Casi d'Uso**:
- Verificare disponibilità per una consegna futura
- Pianificare ordini di acquisto
- Analizzare impegni a lungo termine

---

## 🔐 Permessi

**Accesso**: Tutti gli utenti autenticati
- Nessuna restrizione per ruolo
- Richiede solo `[Authorize]`

**Per limitare l'accesso** (se necessario):
```csharp
[Authorize(Roles = "Admin,Manager")]
public class InterrogazioniDBController : Controller
```

---

## 🎨 UI/UX

### Colori Card
- **Intestazioni**: `bg-info text-white` (azzurro)
- **Successo**: `bg-success text-white` (verde)
- **Warning**: `bg-warning text-dark` (giallo)
- **Danger**: `bg-danger text-white` (rosso)

### Icone Bootstrap Icons
- 🔍 `bi-search`: Ricerca
- 📦 `bi-boxes`: Disponibilità
- 📅 `bi-calendar-event`: Consegne
- 📊 `bi-table`: Risultati
- 📈 `bi-graph-up`: Statistiche

---

## 🐛 Troubleshooting

### Select2 non funziona
**Problema**: Dropdown non si apre o non cerca

**Soluzione**:
1. Verifica che jQuery sia caricato prima di Select2
2. Controlla la console browser per errori JavaScript
3. Verifica che l'URL API sia corretto

### Nessun risultato
**Problema**: Query restituisce 0 risultati

**Possibili cause**:
1. Articolo non esiste
2. Articolo senza giacenze in nessun magazzino
3. CodiceArticolo case-sensitive (verificare)

**Debug**:
```csharp
_logger.LogInformation("Searching for article: {CodiceArticolo}", model.CodiceArticolo);
```

### Errore compilazione
**Problema**: Build fallisce

**Soluzione**:
1. Verifica che tutti i file siano stati creati
2. Esegui `dotnet clean` e poi `dotnet build`
3. Verifica i namespace e le using directives

---

## 🔮 Sviluppi Futuri

### Disponibilità
1. ✅ Implementare logica `ImpegnatoDataOdierna`
2. ✅ Utilizzare campo `DataRiferimento` per proiezioni
3. ✅ Aggiungere filtri per magazzino
4. ✅ Export Excel/PDF
5. ✅ Grafici disponibilità nel tempo
6. ✅ Storico movimenti

### Consegne Programmate
1. ✅ Implementare query ordini fornitori
2. ✅ Filtri per data e fornitore
3. ✅ Visualizzazione calendario
4. ✅ Alert per consegne imminenti
5. ✅ Integrazione con email notifiche

---

## 📞 Supporto

Per domande o problemi:
1. Verifica questo documento
2. Controlla i log dell'applicazione
3. Usa il debugger per tracciare le query
4. Consulta la documentazione Entity Framework Core

---

**Versione**: 1.0  
**Data Creazione**: 20 Ottobre 2025  
**Ultimo Aggiornamento**: 20 Ottobre 2025

