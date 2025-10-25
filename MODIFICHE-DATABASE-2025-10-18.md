# 📊 Modifiche Database - 18 Ottobre 2025

Documento riepilogativo di tutte le modifiche apportate al database del progetto AiDbMaster.

---

## 📑 Indice

1. [Tabella OrdiniRighe](#1-tabella-ordinirighe)
2. [Tabella ProgressiviArticoli](#2-tabella-progressiviarticoli)
3. [Tabella CentriLavoro](#3-tabella-centrilavoro)
4. [Tabella Lavorazioni](#4-tabella-lavorazioni)
5. [Tabella ListaOP](#5-tabella-listaop)
6. [Tabella OrdiniTestate](#6-tabella-ordinitestate)
7. [Tabella AnagraficaClienti](#7-tabella-anagraficaclienti)
8. [Tabella AnagraficaFornitori](#8-tabella-anagraficafornitori)
9. [Tabella ArticoliSostitutivi](#9-tabella-articolisostitutivi)
10. [Nuova Tabella CalendarioFermiCentriLavoro](#10-nuova-tabella-calendariofermicenttilavoro)
11. [Riepilogo Migrazioni](#-riepilogo-migrazioni)

---

## 1. Tabella `OrdiniRighe`

### ✅ Campi Aggiunti
| Campo | Tipo | Descrizione |
|-------|------|-------------|
| `PercentualeInclusione` | `int` | Percentuale di inclusione dell'ordine |

### ❌ Campi Eliminati
| Campo Vecchio | Descrizione |
|---------------|-------------|
| `mo_colpre` | Colli prenotati |
| `mo_quapre` | Quantità prenotata |
| `mo_flevapre` | Flag evasione prenotazione |
| `mo_scont1` | Sconto 1 |
| `mo_scont2` | Sconto 2 |
| `mo_scont3` | Sconto 3 |
| `mo_provv` | Provvigione |
| `mo_codiva` | Codice IVA |
| `mo_preziva` | Prezzo con IVA |
| `mo_prezvalc` | Prezzo in valuta |
| `mo_prelist` | Prezzo listino |

### 🔄 Campi Rinominati
| Campo Vecchio | Campo Nuovo | Tipo |
|---------------|-------------|------|
| `mo_coleva` | `ColliEvasi` | `decimal` |
| `mo_quaeva` | `QuantitaEvasa` | `decimal` |
| `mo_note` | `NoteRiga` | `string?` |

### 📋 Migrazioni
- `20251018063601_AddPercentualeInclusioneToOrdiniRighe`
- `20251018074916_RemoveFieldsFromOrdiniRighe`
- `20251018075703_RenameColumnsInOrdiniRighe`

---

## 2. Tabella `ProgressiviArticoli`

### ❌ Campi Eliminati
| Campo Vecchio | Descrizione |
|---------------|-------------|
| `ImpegnatoTotale` | Quantità totale impegnata |
| `Prenotato` | Quantità prenotata |
| `Impegnato` | Quantità impegnata |
| `Ordinato` | Quantità ordinata (vecchio) |

### 🔄 Campi Rinominati
| Campo Vecchio | Campo Nuovo | Tipo Vecchio | Tipo Nuovo |
|---------------|-------------|--------------|------------|
| `Ordinato` | `OrdinatoFornitoriDataOdierna` | `decimal(18,2)` | `decimal(27,9)` |

### 📐 Campi con Tipo Modificato
| Campo | Tipo Vecchio | Tipo Nuovo |
|-------|--------------|------------|
| `Esistenza` | `decimal(18,2)` | `decimal(27,9)` |
| `OrdinatoFornitoriDataOdierna` | `decimal(18,2)` | `decimal(27,9)` |

### 📋 Migrazioni
- `20251018065708_UpdateProgressiviArticoliRemoveFieldsAndRenameOrdinato`
- `20251018070724_RemoveImpegnatoTotaleAndPrenotatoFromProgressiviArticoli`
- `20251018071537_ChangeDecimalPrecisionProgressiviArticoli`

---

## 3. Tabella `CentriLavoro`

### 🔑 Modifica Primary Key
| Prima | Dopo |
|-------|------|
| `IdCentroLavoro` (int, Identity) | `CodiceCentro` (string, varchar(10)) |

### ❌ Campi Eliminati
| Campo Vecchio | Tipo |
|---------------|------|
| `IdCentroLavoro` | `int` (Identity) |

### ⚙️ Impatto
- La tabella `ListaOP` è stata aggiornata per utilizzare `CodiceCentro` come foreign key
- Tutti i dati esistenti sono stati preservati durante la migrazione

### 📋 Migrazioni
- `20251018081844_ChangeCentriLavoroPrimaryKeyToCodiceCentro`

---

## 4. Tabella `Lavorazioni`

### 🔑 Modifica Primary Key
| Prima | Dopo |
|-------|------|
| `IdLavorazione` (int, Identity) | `CodiceLavorazione` (string, varchar(1)) |

### ❌ Campi Eliminati
| Campo Vecchio | Tipo |
|---------------|------|
| `IdLavorazione` | `int` (Identity) |

### ⚙️ Impatto
- La tabella `ListaOP` è stata aggiornata per utilizzare `CodiceLavorazione` come foreign key
- Tutti i dati esistenti sono stati preservati durante la migrazione
- I codici duplicati sono stati gestiti automaticamente assegnando codici univoci

### 📋 Migrazioni
- `20251018084704_ChangeLavorazioniPrimaryKeyToCodiceLavorazione`

---

## 5. Tabella `ListaOP`

### ❌ Campi Eliminati
| Campo Vecchio | Tipo |
|---------------|------|
| `IdCentroLavoro` | `int` |
| `IdLavorazione` | `int` |

### ✅ Campi Aggiunti
| Campo Nuovo | Tipo | Descrizione |
|-------------|------|-------------|
| `CodiceCentro` | `string (varchar(10))` | FK verso CentriLavoro |
| `CodiceLavorazione` | `string (varchar(1))` | FK verso Lavorazioni |
| `Modificato` | `bit` | Indica se l'ordine è stato modificato |

### 🔗 Relazioni Aggiornate
- **FK CentriLavoro**: da `IdCentroLavoro` a `CodiceCentro`
- **FK Lavorazioni**: da `IdLavorazione` a `CodiceLavorazione`

### 📋 Migrazioni
- `20251018081844_ChangeCentriLavoroPrimaryKeyToCodiceCentro` (per CodiceCentro)
- `20251018084704_ChangeLavorazioniPrimaryKeyToCodiceLavorazione` (per CodiceLavorazione)
- `20251018120113_AddModificatoToListaOP`

---

## 6. Tabella `OrdiniTestate`

### ❌ Campi Eliminati
| Campo Vecchio | Tipo | Descrizione |
|---------------|------|-------------|
| `td_tipobf` | `smallint` | Tipo bolla/fattura |
| `td_magaz` | `smallint` | Codice magazzino |
| `TotaleColli` | `int` | Totale colli |

### 🔄 Campi Rinominati
| Campo Vecchio | Campo Nuovo | Tipo |
|---------------|-------------|------|
| `td_riferim` | `RiferimentoOrdine` | `string(50)?` |
| `td_note` | `NoteTestata` | `string?` |

### 📋 Migrazioni
- `20251018115410_RemoveFieldsFromOrdiniTestate`
- `20251018115804_RenameColumnsInOrdiniTestate`

---

## 7. Tabella `AnagraficaClienti`

### 🔄 Campi Rinominati
| Campo Vecchio | Campo Nuovo | Tipo |
|---------------|-------------|------|
| `an_tipo` | `Tipo` | `string(1)` |
| `an_descr2` | `DescrizioneUlteriore` | `string(50)?` |
| `an_codfis` | `CodiceFiscale` | `string(16)?` |
| `an_pariva` | `PartitaIva` | `string(11)?` |

### ❌ Campi Eliminati
| Campo Vecchio | Tipo | Descrizione |
|---------------|------|-------------|
| `an_faxtlx` | `string(18)?` | Fax/Telex |

### 📋 Migrazioni
- `20251018120858_UpdateAnagraficaClientiFields`

---

## 8. Tabella `AnagraficaFornitori`

### 🔄 Campi Rinominati
| Campo Vecchio | Campo Nuovo | Tipo |
|---------------|-------------|------|
| `an_tipo` | `Tipo` | `string(1)` |
| `an_descr2` | `DescrizioneUlteriore` | `string(50)?` |
| `an_codfis` | `CodiceFiscale` | `string(16)?` |
| `an_pariva` | `PartitaIva` | `string(11)?` |

### ❌ Campi Eliminati
| Campo Vecchio | Tipo | Descrizione |
|---------------|------|-------------|
| `an_faxtlx` | `string(18)?` | Fax/Telex |

### 📋 Migrazioni
- `20251018121354_UpdateAnagraficaFornitoriFields`

---

## 9. Tabella `ArticoliSostitutivi`

### 🔄 Campi Rinominati
| Campo Vecchio | Campo Nuovo | Tipo |
|---------------|-------------|------|
| `apa_note` | `Note` | `string?` |

### 📋 Migrazioni
- `20251018121630_RenameNoteColumnInArticoliSostitutivi`

---

## 10. Nuova Tabella `CalendarioFermiCentriLavoro`

### ✨ Tabella Completamente Nuova

Questa tabella gestisce il calendario dei fermi programmati per i centri di lavoro.

### 📊 Struttura Completa

| Campo | Tipo | Nullable | Descrizione |
|-------|------|----------|-------------|
| `Id` | `int` (Identity) | No | Primary Key |
| `CodiceCentro` | `nvarchar(10)` | No | FK verso CentriLavoro |
| `DataInizioFermo` | `datetime2` | No | Data e ora inizio fermo |
| `DataFineFermo` | `datetime2` | Sì | Data e ora fine fermo |
| `TipoFermo` | `int` (enum) | No | Tipo: WeekEnd (0), Festivo (1) |
| `Motivo` | `nvarchar(200)` | Sì | Descrizione motivo |
| `Note` | `nvarchar(max)` | Sì | Note aggiuntive |
| `IsPianificato` | `bit` | No | Indica se pianificato |
| `DataCreazione` | `datetime2` | No | Timestamp creazione |
| `DataUltimaModifica` | `datetime2` | Sì | Timestamp ultima modifica |

### 🔗 Relazioni
- **Foreign Key**: `CodiceCentro` → `CentriLavoro.CodiceCentro` (ON DELETE RESTRICT)

### 📑 Indici Creati
- `IX_CalendarioFermiCentriLavoro_CodiceCentro`
- `IX_CalendarioFermiCentriLavoro_DataInizioFermo`
- `IX_CalendarioFermiCentriLavoro_DataFineFermo`
- `IX_CalendarioFermiCentriLavoro_TipoFermo`

### 📐 Enum TipoFermo
```csharp
public enum TipoFermo
{
    [Display(Name = "Week End")]
    WeekEnd = 0,
    
    [Display(Name = "Festivo")]
    Festivo = 1
}
```

### 💡 Proprietà Calcolate (NotMapped)
- `IsFermoAttivo` - Verifica se il fermo è attualmente in corso
- `IsFermoFuturo` - Verifica se il fermo è programmato per il futuro
- `IsFermoTerminato` - Verifica se il fermo è terminato
- `DurataFermo` - Calcola la durata del fermo (TimeSpan)
- `DurataFormattata` - Durata in formato leggibile
- `StatoFermo` - Stato testuale (Programmato/In Corso/Terminato)
- `StatoFermoCssClass` - Classe CSS per il badge dello stato
- `DescrizioneCompleta` - Descrizione completa del fermo
- `PeriodoFormattato` - Periodo in formato leggibile

### 📋 Migrazioni
- `20251018122342_CreateCalendarioFermiCentriLavoro`

---

## 📊 Riepilogo Migrazioni

### Lista Completa delle Migrazioni Applicate (in ordine cronologico)

1. **20251018063601_AddPercentualeInclusioneToOrdiniRighe**
   - Aggiunto campo `PercentualeInclusione` a `OrdiniRighe`

2. **20251018065708_UpdateProgressiviArticoliRemoveFieldsAndRenameOrdinato**
   - Rimossi campi da `ProgressiviArticoli`
   - Rinominato `Ordinato` in `OrdinatoFornitoriDataOdierna`

3. **20251018070724_RemoveImpegnatoTotaleAndPrenotatoFromProgressiviArticoli**
   - Rimossi `ImpegnatoTotale` e `Prenotato` da `ProgressiviArticoli`

4. **20251018071537_ChangeDecimalPrecisionProgressiviArticoli**
   - Modificata precisione decimale da (18,2) a (27,9)

5. **20251018074916_RemoveFieldsFromOrdiniRighe**
   - Rimossi 11 campi da `OrdiniRighe`

6. **20251018075703_RenameColumnsInOrdiniRighe**
   - Rinominati 3 campi in `OrdiniRighe`

7. **20251018081844_ChangeCentriLavoroPrimaryKeyToCodiceCentro**
   - Cambiata primary key di `CentriLavoro`
   - Aggiornata `ListaOP` con `CodiceCentro`

8. **20251018084704_ChangeLavorazioniPrimaryKeyToCodiceLavorazione**
   - Cambiata primary key di `Lavorazioni`
   - Aggiornata `ListaOP` con `CodiceLavorazione`

9. **20251018115410_RemoveFieldsFromOrdiniTestate**
   - Rimossi 3 campi da `OrdiniTestate`

10. **20251018115804_RenameColumnsInOrdiniTestate**
    - Rinominati 2 campi in `OrdiniTestate`

11. **20251018120113_AddModificatoToListaOP**
    - Aggiunto campo `Modificato` a `ListaOP`

12. **20251018120858_UpdateAnagraficaClientiFields**
    - Rinominati 4 campi e rimosso 1 campo da `AnagraficaClienti`

13. **20251018121354_UpdateAnagraficaFornitoriFields**
    - Rinominati 4 campi e rimosso 1 campo da `AnagraficaFornitori`

14. **20251018121630_RenameNoteColumnInArticoliSostitutivi**
    - Rinominato campo in `ArticoliSostitutivi`

15. **20251018122342_CreateCalendarioFermiCentriLavoro**
    - Creata nuova tabella `CalendarioFermiCentriLavoro`

---

## 📈 Statistiche Complessive

### 📊 Riepilogo Modifiche

| Categoria | Conteggio |
|-----------|-----------|
| Tabelle Modificate | 9 |
| Nuove Tabelle | 1 |
| Campi Aggiunti | 4 |
| Campi Eliminati | 27 |
| Campi Rinominati | 17 |
| Campi con Tipo Modificato | 2 |
| Primary Key Modificate | 2 |
| Foreign Key Modificate | 2 |
| Migrazioni Totali | 15 |

### 🎯 Obiettivi Raggiunti

✅ **Normalizzazione Database**: Rimossi campi ridondanti e obsoleti  
✅ **Convenzioni .NET**: Tutti i nomi dei campi seguono le convenzioni PascalCase  
✅ **Struttura Pulita**: Eliminati campi inutilizzati per semplificare la manutenzione  
✅ **Performance**: Modificata precisione decimale dove necessario  
✅ **Flessibilità**: Primary key su codici stringa per maggiore flessibilità  
✅ **Nuove Funzionalità**: Aggiunto calendario fermi per pianificazione avanzata  

---

## 🔧 Note Tecniche

### Gestione Dati Durante le Migrazioni

- **Preservazione Dati**: Tutti i dati esistenti sono stati preservati durante le migrazioni
- **Conversione Primary Key**: Le migrazioni per CentriLavoro e Lavorazioni includono script SQL personalizzati per gestire la conversione dei dati
- **Gestione Duplicati**: Durante la conversione di `Lavorazioni`, i codici duplicati sono stati gestiti automaticamente
- **Default Constraints**: Le colonne eliminate con default constraints sono state gestite con script SQL dinamici

### Comandi Utilizzati

```bash
# Per ogni migrazione
dotnet ef migrations add <NomeMigrazione> --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext

# Compilazione dopo ogni modifica
dotnet build
```

### Backup e Sicurezza

⚠️ **IMPORTANTE**: Prima di applicare queste modifiche in produzione:
1. Eseguire un backup completo del database
2. Testare le migrazioni in ambiente di sviluppo/staging
3. Verificare l'integrità dei dati dopo ogni migrazione
4. Pianificare una finestra di manutenzione se necessario

---

## 📅 Informazioni Documento

- **Data**: 18 Ottobre 2025
- **Progetto**: AiDbMaster
- **Database**: SQL Server
- **ORM**: Entity Framework Core 8.0.2
- **Framework**: .NET 8.0

---

**Documento generato automaticamente** ✨


