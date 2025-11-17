# 🔄 Sincronizzazione Sistema Permessi con Menu

## 📋 Modifiche Effettuate

### ✅ **1. Aggiornamento PermissionSeeder.cs**

Il file `Data/PermissionSeeder.cs` è stato completamente aggiornato per rispecchiare la struttura del menu reale in `_Layout.cshtml`.

#### **Risorse Aggiunte:**
- ✨ **Home** (era "Dashboard")
- ✨ **Gruppo InterrogazioniDB** (completamente nuovo)
  - Disponibilità
  - Consegne Programmate
  - Dashboard Consegne
  - Grafici
  - Interrogazioni AI
- ✨ **Operatori** (spostato sotto Tabelle)
- ✨ **StatiOP** (sotto Tabelle)
- ✨ **TempiAsciugatura** (sotto Tabelle)
- ✨ **ListaOPDashboard** (Dashboard produzione)
- ✨ **FermiSchedulati** (era CalendarioFermi)
- ✨ **SyncfusionTest** (sotto Amministrazione)

#### **Risorse Rimosse/Corrette:**
- ❌ Gruppo "**Ordini**" (non esiste nel menu)
- ❌ **OrdiniRighe** (non nel menu principale)
- ✏️ **OrdiniTestate** spostato sotto Tabelle (non più in gruppo Ordini)

#### **Totale Risorse:** 33 (contro 25 vecchie)

---

### ✅ **2. Controller: Metodo Reset e Re-seed**

Aggiunto metodo `ResetAndReseedResources()` in `PermissionManagementController.cs` che:
1. Elimina tutti i permessi esistenti
2. Elimina tutte le risorse esistenti
3. Esegue il re-seed completo con le risorse aggiornate

**⚠️ ATTENZIONE:** Operazione distruttiva con doppia conferma!

---

### ✅ **3. UI: Pulsante Reset nella Pagina**

Aggiunto pulsante "**Reset e Re-seed**" nella pagina `PermissionManagement/Index.cshtml`:
- Colore arancione (warning)
- Doppia conferma JavaScript
- Feedback visivo durante l'operazione

---

## 🚀 Come Sincronizzare il Database

Hai **DUE opzioni** per sincronizzare il database con le nuove risorse:

### **Opzione 1: Reset Completo (CONSIGLIATO)** ⚠️

1. Avvia l'applicazione
2. Login come **Admin**
3. Vai su **Amministrazione → Gestione Permessi**
4. Clicca il pulsante **"Reset e Re-seed"** (giallo)
5. Conferma **2 volte** (è un'operazione distruttiva!)
6. Attendi il completamento e il reload della pagina

**Risultato:**
- ✅ Tutte le risorse allineate al menu
- ✅ Permessi Admin ricreati automaticamente
- ✅ Permessi Agenti ricreati (Home, Clienti, Ordini, Consegne Programmate, Dashboard Consegne)

---

### **Opzione 2: Sincronizzazione Incrementale** 🔄

1. Login come **Admin**
2. Vai su **Amministrazione → Gestione Permessi**
3. Clicca il pulsante **"Sincronizza Risorse"**

**Risultato:**
- ✅ Aggiunge solo le risorse mancanti
- ⚠️ Non corregge quelle errate (gruppo Ordini, parent ID sbagliati)

**Nota:** Questa opzione funziona solo per risorse con `[RegisterResource]` attribute nei controller (attualmente solo 3 controller lo hanno).

---

## 📊 Struttura Finale delle Risorse

### 🏠 **Root (5 gruppi)**
1. Home
2. Tabelle (gruppo)
3. Produzione (gruppo)
4. Interrogazioni DB (gruppo) ← **NUOVO**
5. Amministrazione (gruppo)

### 📁 **Tabelle (13 pagine)**
- Anagrafica Articoli
- Anagrafica Clienti
- Anagrafica Fornitori
- Articoli Sostitutivi
- Progressivi Articoli
- Agenti
- Magazzini
- Lavorazioni
- Centri di Lavoro
- Operatori ← **SPOSTATO QUI**
- Stati OP ← **NUOVO**
- Gestione Ordini CF
- Tempi di Asciugatura ← **NUOVO**

### ⚙️ **Produzione (4 pagine)**
- Dashboard ← **NUOVO**
- Schedulatore OP
- Ordini di Produzione
- Fermi Schedulati

### 🔍 **Interrogazioni DB (5 pagine)** ← **GRUPPO NUOVO**
- Disponibilità
- Consegne Programmate
- Dashboard Consegne
- Grafici (solo Admin/Manager)
- Interrogazioni AI (solo Admin/Manager)

### 🛠️ **Amministrazione (6 pagine)**
- Gestione Utenti
- Gestione Ruoli
- Gestione Permessi
- Converti Agenti in Utenti
- Impostazioni AI
- Test Syncfusion ← **NUOVO**

---

## 🎯 Permessi Default per Ruolo Agenti

Dopo il reset, gli **Agenti** avranno accesso a:

| Risorsa | View | Create | Edit | Delete |
|---------|------|--------|------|--------|
| Home | ✅ | ❌ | ❌ | ❌ |
| Anagrafica Clienti | ✅ | ❌ | ✅ | ❌ |
| Gestione Ordini CF | ✅ | ❌ | ✅ | ❌ |
| Consegne Programmate | ✅ | ❌ | ❌ | ❌ |
| Dashboard Consegne | ✅ | ❌ | ❌ | ❌ |

---

## ⚠️ Note Importanti

1. **Backup Database**: Prima del reset, considera di fare un backup del database se hai configurazioni personalizzate importanti.

2. **Permessi Personalizzati**: Il reset eliminerà TUTTI i permessi personalizzati configurati manualmente per i vari ruoli. Dovrai riconfigurarli dopo il reset.

3. **ID Risorse**: Gli ID delle risorse cambieranno dopo il reset. Se hai riferimenti hard-coded agli ID nel codice, potrebbero non funzionare più.

4. **Utenti Attivi**: Gli utenti loggati potrebbero avere permessi in cache. Consigliato farli riloggare dopo il reset.

5. **Auto-Registrazione**: I controller con `[RegisterResource]` attribute si auto-registreranno all'avvio dell'app, ma per ora solo 3 controller hanno questo attribute:
   - `AnagraficaArticoliController`
   - `AnagraficaClientiController`
   - `AnagraficaFornitoriController`

---

## 🔧 Sviluppi Futuri

### Consigliato aggiungere `[RegisterResource]` ai controller mancanti:

```csharp
[RegisterResource(
    "NomeController", 
    "Titolo Menu",
    MenuIcon = "bi-icon",
    MenuOrder = 100,
    ParentResourceId = 2 // 2=Tabelle, 16=Produzione, 21=InterrogazioniDB, 27=Amministrazione
)]
public class NomeController : Controller
{
    // ...
}
```

### Controller da aggiornare:
- InterrogazioniDBController
- DashboardConsegneController
- InterrogazioniAIController
- GraficiController
- ListaOPController
- SchedulatoreOPController
- FermiSchedulatiController
- StatiOPController
- OperatoriController
- TempiAsciugaturaController
- ... e altri

---

## ✅ Checklist Post-Reset

Dopo aver eseguito il reset, verifica:

- [ ] Login come Admin funziona
- [ ] Tutte le voci menu sono visibili per Admin
- [ ] Login come Agente funziona
- [ ] Agente vede solo: Home, Clienti, Ordini, Consegne, Dashboard Consegne
- [ ] Pagina "Gestione Permessi" mostra tutte le 33 risorse
- [ ] Nessun alert "Risorse non configurate" (tutte dovrebbero essere IsConfigured=true)
- [ ] Permessi salvabili correttamente
- [ ] Menu riflette esattamente i permessi configurati

---

## 📞 Support

Per problemi o domande:
- Controlla i log dell'applicazione per errori durante il reset
- Verifica la console browser per errori JavaScript
- Controlla che il database sia accessibile e scrivibile

---

**Data creazione documento:** 17 Novembre 2024  
**Versione:** 1.0  
**Autore:** AI Assistant

