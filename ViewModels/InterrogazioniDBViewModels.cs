using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la pagina Disponibilità
    /// </summary>
    public class DisponibilitaViewModel
    {
        // Input
        [Display(Name = "Codice Articolo")]
        public string? CodiceArticolo { get; set; }
        
        [Display(Name = "Visualizza Articoli Sostitutivi")]
        public bool VisualizzaSostitutivi { get; set; }
        
        // Date calcolate automaticamente in base ai ParametriChiave
        /// <summary>
        /// Data di inizio per il calcolo (sempre oggi)
        /// </summary>
        public DateTime DataInizio { get; set; } = DateTime.Today;
        
        /// <summary>
        /// Data di fine per il calcolo (oggi + GiorniImpegno da ParametriChiave)
        /// </summary>
        public DateTime DataFine { get; set; } = DateTime.Today;
        
        /// <summary>
        /// Numero di giorni impegno letti dalla tabella ParametriChiave
        /// </summary>
        public int GiorniImpegno { get; set; }
        
        // Output - Lista risultati
        public List<DisponibilitaRigaViewModel>? Risultati { get; set; }
        
        // Output - Lista risultati articoli sostitutivi
        public List<DisponibilitaArticoloGruppoViewModel>? ArticoliSostitutivi { get; set; }
        
        // Output - Lista risultati articoli Outlet
        public List<DisponibilitaArticoloGruppoViewModel>? ArticoliOutlet { get; set; }
        
        /// <summary>
        /// Flag che indica se è stata richiesta la ricerca Outlet
        /// </summary>
        public bool RicercaOutlet { get; set; }

        /// <summary>
        /// Se true, la ricerca Outlet mostra solo l'articolo selezionato nel filtro (invece di tutti gli Outlet)
        /// </summary>
        [Display(Name = "Solo Articolo Selezionato")]
        public bool OutletSoloSelezionato { get; set; }
        
        // Informazioni aggiuntive sull'articolo selezionato
        public string? DescrizioneArticolo { get; set; }
    }

    /// <summary>
    /// Gruppo di disponibilità per un articolo (principale o sostitutivo)
    /// </summary>
    public class DisponibilitaArticoloGruppoViewModel
    {
        public string CodiceArticolo { get; set; } = string.Empty;
        public string DescrizioneArticolo { get; set; } = string.Empty;
        public bool IsArticoloPrincipale { get; set; }
        public List<DisponibilitaRigaViewModel> Risultati { get; set; } = new List<DisponibilitaRigaViewModel>();
        
        /// <summary>
        /// Percentuale sconto extra (da TabellaClassiProvvigioni.Perc_Sconto).
        /// Null se l'articolo non ha una ClasseProvvigione assegnata.
        /// </summary>
        public decimal? PercScontoExtra { get; set; }
    }

    /// <summary>
    /// ViewModel per ogni riga di risultato della disponibilità
    /// </summary>
    public class DisponibilitaRigaViewModel
    {
        public string CodiceArticolo { get; set; } = string.Empty;
        public string DescrizioneArticolo { get; set; } = string.Empty;
        public string UnitaMisura { get; set; } = string.Empty;
        public short CodiceMagazzino { get; set; }
        
        // ANALISI DISPONIBILITA'
        public decimal Esistenza { get; set; }

        /// <summary>
        /// Quantità pronta per la consegna
        /// </summary>
        public decimal Pronto { get; set; }

        /// <summary>
        /// Impegnato attuale - solo fino ad oggi (da DB)
        /// </summary>
        public decimal ImpegnatoAttuale { get; set; }
        
        /// <summary>
        /// Disponibile attuale: se Supermarket = Esistenza, altrimenti = Esistenza - Impegnato Attuale
        /// </summary>
        public decimal DisponibileAttuale => IsSupermarket ? Esistenza : (Esistenza - ImpegnatoAttuale);
        
        // DISPONIBILITÀ PREVISTA
        
        /// <summary>
        /// Quantità da produzione programmata disponibile entro la data di riferimento
        /// (include tempo di asciugatura/maturazione)
        /// </summary>
        public decimal ProduzioneDisponibile { get; set; }
        
        /// <summary>
        /// Impegnato futuro - ordini clienti tra oggi e data riferimento
        /// </summary>
        public decimal ImpegnatoFuturo { get; set; }
        
        /// <summary>
        /// Ordinato da fornitori
        /// </summary>
        public decimal OrdinatoFornitoriDataOdierna { get; set; }
        
        /// <summary>
        /// Disponibilità prevista alla data di riferimento = 
        /// Esistenza - Impegnato Attuale - Impegnato Futuro + Produzione + Ordinato Fornitori
        /// </summary>
        public decimal DisponibilitaPrevista => Esistenza - ImpegnatoAttuale - ImpegnatoFuturo + ProduzioneDisponibile + OrdinatoFornitoriDataOdierna;
        
        /// <summary>
        /// Indica se l'articolo è Fuori Produzione (FuoriProduzione = 'S').
        /// Usato per colorare la riga in giallino.
        /// </summary>
        public bool IsFuoriProduzione { get; set; }

        /// <summary>
        /// Indica se l'articolo è gestito a Supermarket (Supermarket = 'S').
        /// Se true, la disponibilità coincide con l'esistenza (l'impegnato non viene sottratto).
        /// </summary>
        public bool IsSupermarket { get; set; }
        
        public string RowCssClass => IsSupermarket ? "table-info" : (IsFuoriProduzione ? "table-warning" : "");
        
        // ========== NOTE PREVISIONE (dinamiche) ==========
        
        /// <summary>
        /// Nota testuale sulla previsione di disponibilità (calcolata dinamicamente)
        /// Es: "Disponibili 2900 MQ a partire dal 11/03/2026"
        /// </summary>
        public string NotaPrevisione { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se la disponibilità è da verificare (progressivo negativo non recuperato)
        /// </summary>
        public bool DisponibilitaDaVerificare { get; set; }
        
        /// <summary>
        /// CSS class per la nota in base allo stato
        /// </summary>
        public string NotaCssClass => DisponibilitaDaVerificare ? "text-danger" : "text-dark";
    }

    /// <summary>
    /// ViewModel per la ricerca articoli (autocomplete)
    /// </summary>
    public class ArticoloAutocompleteViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel per il dettaglio delle produzioni programmate
    /// Utilizzato per mostrare il popup con il dettaglio delle produzioni disponibili
    /// </summary>
    public class ProduzioneDettaglioViewModel
    {
        /// <summary>
        /// Numero ordine formattato (es. "2025/100")
        /// </summary>
        public string NumeroOrdine { get; set; } = string.Empty;
        
        /// <summary>
        /// Quantità da produrre
        /// </summary>
        public decimal Quantita { get; set; }
        
        /// <summary>
        /// Data fine prevista della produzione (senza asciugatura)
        /// </summary>
        public DateTime DataFinePrevista { get; set; }
        
        /// <summary>
        /// Giorni di asciugatura/maturazione (dal mese della DataFinePrevista)
        /// </summary>
        public int GiorniAsciugatura { get; set; }
        
        /// <summary>
        /// Data di effettiva disponibilità = DataFinePrevista + GiorniAsciugatura
        /// </summary>
        public DateTime DataDisponibilita { get; set; }
        
        /// <summary>
        /// Indica se questa produzione sarà disponibile entro la data di riferimento
        /// </summary>
        public bool DisponibileEntroData { get; set; }
    }

    /// <summary>
    /// ViewModel per la risposta del dettaglio produzioni (endpoint API)
    /// </summary>
    public class DettaglioProduzioniResponse
    {
        public string CodiceArticolo { get; set; } = string.Empty;
        public string DescrizioneArticolo { get; set; } = string.Empty;
        public string UnitaMisura { get; set; } = string.Empty;
        public DateTime DataRiferimento { get; set; }
        public List<ProduzioneDettaglioViewModel> Produzioni { get; set; } = new List<ProduzioneDettaglioViewModel>();
        public decimal TotaleDisponibile { get; set; }
        public decimal TotaleNonDisponibile { get; set; }
        public int NumeroOrdiniDisponibili { get; set; }
        public int NumeroOrdiniNonDisponibili { get; set; }
    }

    /// <summary>
    /// ViewModel per il dettaglio di un ordine cliente impegnato
    /// </summary>
    public class OrdineClienteDettaglioViewModel
    {
        public short AnnoOrdine { get; set; }
        public int NumeroOrdine { get; set; }
        public DateTime DataConsegna { get; set; }
        public int CodiceCliente { get; set; }
        public string RagioneSociale { get; set; } = string.Empty;
        public decimal Quantita { get; set; }
        public decimal QuantitaEvasa { get; set; }
        public decimal QuantitaDaEvadere => Quantita - QuantitaEvasa;
        
        /// <summary>
        /// Percentuale di inclusione (es. 100 = 100%, 50 = 50%)
        /// </summary>
        public int PercentualeInclusione { get; set; }
        
        /// <summary>
        /// Contributo all'impegnato: (QuantitaDaEvadere * PercentualeInclusione / 100)
        /// </summary>
        public decimal ContributoImpegnato => QuantitaDaEvadere * PercentualeInclusione / 100m;
    }

    /// <summary>
    /// Response per il dettaglio degli ordini clienti impegnati (futuro)
    /// </summary>
    public class DettaglioImpegnatoFuturoResponse
    {
        public string CodiceArticolo { get; set; } = string.Empty;
        public string DescrizioneArticolo { get; set; } = string.Empty;
        public string UnitaMisura { get; set; } = string.Empty;
        public DateTime DataRiferimento { get; set; }
        public List<OrdineClienteDettaglioViewModel> Ordini { get; set; } = new List<OrdineClienteDettaglioViewModel>();
        public decimal TotaleImpegnatoFuturo { get; set; }
    }

    /// <summary>
    /// Response per il dettaglio degli ordini clienti impegnati (attuale/oggi)
    /// </summary>
    public class DettaglioImpegnatoAttualeResponse
    {
        public string CodiceArticolo { get; set; } = string.Empty;
        public string DescrizioneArticolo { get; set; } = string.Empty;
        public string UnitaMisura { get; set; } = string.Empty;
        public DateTime DataRiferimento { get; set; }
        public List<OrdineClienteDettaglioViewModel> Ordini { get; set; } = new List<OrdineClienteDettaglioViewModel>();
        public decimal TotaleImpegnatoAttuale { get; set; }
    }

    /// <summary>
    /// Response per il dettaglio dei movimenti che compongono la nota previsione
    /// </summary>
    public class DettaglioMovimentiNotaResponse
    {
        public string CodiceArticolo { get; set; } = string.Empty;
        public string DescrizioneArticolo { get; set; } = string.Empty;
        public string UnitaMisura { get; set; } = string.Empty;
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public string NotaGenerata { get; set; } = string.Empty;
        public List<MovimentoNotaViewModel> Movimenti { get; set; } = new List<MovimentoNotaViewModel>();
    }

    /// <summary>
    /// ViewModel per un singolo movimento nella timeline della nota
    /// </summary>
    public class MovimentoNotaViewModel
    {
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Esistenza", "Ordine Cliente", "Ordine Produzione"
        public string TipoCssClass => Tipo switch
        {
            "Esistenza" => "bg-secondary",
            "Ordine Cliente" => "bg-danger",
            "Ordine Produzione" => "bg-success",
            _ => "bg-light"
        };
        public string Descrizione { get; set; } = string.Empty;
        public decimal Quantita { get; set; } // positivo o negativo
        public decimal Progressivo { get; set; }
        public string ProgressivoCssClass => Progressivo >= 0 ? "text-success" : "text-danger";
        public int? GiorniAsciugatura { get; set; }
    }

    // =====================================================
    // ViewModels per Consegne Programmate
    // =====================================================

    /// <summary>
    /// ViewModel per la pagina Consegne Programmate (filtri e risultati)
    /// </summary>
    public class ConsegneProgrammateViewModel
    {
        // Filtri Input
        [Display(Name = "Da Data Consegna")]
        [DataType(DataType.Date)]
        public DateTime? DataConsegnaDa { get; set; }

        [Display(Name = "A Data Consegna")]
        [DataType(DataType.Date)]
        public DateTime? DataConsegnaA { get; set; }

        [Display(Name = "Cliente")]
        public int? CodiceCliente { get; set; }

        [Display(Name = "Regione")]
        public string? Regione { get; set; }

        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }

        [Display(Name = "Comune")]
        public string? Comune { get; set; }

        [Display(Name = "Agente")]
        public short? CodiceAgente { get; set; }

        [Display(Name = "Ordina Per")]
        public string OrdinamentoPer { get; set; } = "DataConsegna"; // DataConsegna, Cliente, Ordine

        // Output - Lista risultati (ordini con righe)
        public List<OrdineConsegnaViewModel> Ordini { get; set; } = new List<OrdineConsegnaViewModel>();

        // Informazioni aggiuntive per filtri selezionati
        public string? RagioneSocialeCliente { get; set; }
        public string? NomeAgente { get; set; }
    }

    /// <summary>
    /// ViewModel per un singolo ordine con le sue righe (testata + dettaglio)
    /// </summary>
    public class OrdineConsegnaViewModel
    {
        // Campi Testata Ordine
        public int Id { get; set; }
        public int CodiceCliente { get; set; }
        public string TipoOrdine { get; set; } = string.Empty;
        public short AnnoOrdine { get; set; }
        public string SerieOrdine { get; set; } = string.Empty;
        public int NumeroOrdine { get; set; }
        public DateTime DataOrdine { get; set; }
        public string? RiferimentoOrdine { get; set; }
        public DateTime? DataConsegnaTestata { get; set; }
        public short CodiceAgente { get; set; }
        public string? NoteTestata { get; set; }

        // Campi Cliente (da AnagraficaClienti)
        public string RagioneSociale { get; set; } = string.Empty;
        public string? DescrizioneUlteriore { get; set; }
        public string? Indirizzo { get; set; }
        public string? Cap { get; set; }
        public string? Citta { get; set; }
        public string? Provincia { get; set; }
        public string? Regione { get; set; }
        public string? CodiceFiscale { get; set; }
        public string? PartitaIva { get; set; }
        public string? Telefono { get; set; }

        // Flag e dati Destinazione Diversa
        public bool HasDestinazioneDiversa { get; set; }
        public string? DescrizioneDestinazione { get; set; }
        public string? IndirizzoDestinazione { get; set; }
        public string? CapDestinazione { get; set; }
        public string? LocalitaDestinazione { get; set; }
        public string? ProvinciaDestinazione { get; set; }

        // Campi Agente (da TabellaAgenti)
        public string? NomeAgente { get; set; }
        public string? TelefonoAgente { get; set; }

        // Flag Email Inviata
        /// <summary>Indica se almeno una riga di questo ordine ha ricevuto una notifica email</summary>
        public bool HasEmailInviata { get; set; }
        /// <summary>Data dell'ultimo invio email per questo ordine</summary>
        public DateTime? DataEmailInviata { get; set; }

        // Righe dell'ordine
        public List<RigaOrdineConsegnaViewModel> Righe { get; set; } = new List<RigaOrdineConsegnaViewModel>();

        // Proprietà calcolate
        public string NumeroOrdineCompleto => $"{TipoOrdine}{AnnoOrdine}/{SerieOrdine}/{NumeroOrdine:D6}";
        public string IndirizzoCompletoCliente => FormattaIndirizzo(Indirizzo, Cap, Citta, Provincia);
        public int NumeroRighe => Righe.Count;
        public decimal TotaleOrdine => Righe.Sum(r => r.ValoreRiga);
        public decimal TotaleQuantita => Righe.Sum(r => r.Quantita);
        public decimal TotaleQuantitaDaEvadere => Righe.Sum(r => r.QuantitaRimanente);

        private static string FormattaIndirizzo(string? indirizzo, string? cap, string? citta, string? provincia)
        {
            var parti = new List<string>();
            if (!string.IsNullOrEmpty(indirizzo)) parti.Add(indirizzo);
            if (!string.IsNullOrEmpty(cap)) parti.Add(cap);
            if (!string.IsNullOrEmpty(citta)) parti.Add(citta);
            if (!string.IsNullOrEmpty(provincia)) parti.Add($"({provincia})");
            return string.Join(", ", parti);
        }
    }

    /// <summary>
    /// ViewModel per una singola riga dell'ordine
    /// </summary>
    public class RigaOrdineConsegnaViewModel
    {
        // Campi Riga Ordine
        public int Id { get; set; }
        public string TipoOrdine { get; set; } = string.Empty;
        public short AnnoOrdine { get; set; }
        public string SerieOrdine { get; set; } = string.Empty;
        public int NumeroOrdine { get; set; }
        public int RigaOrdine { get; set; }
        public short CodiceMagazzino { get; set; }
        public string CodiceArticolo { get; set; } = string.Empty;
        public string? DescrizioneArticolo { get; set; }
        public DateTime DataConsegna { get; set; }
        public string? UnitaMisura { get; set; }
        public decimal Quantita { get; set; }
        public string? UnitaMisuraColli { get; set; }
        public decimal NumeroColli { get; set; }
        public decimal ColliEvasi { get; set; }
        public decimal QuantitaEvasa { get; set; }
        public decimal Prezzo { get; set; }
        public int PercentualeInclusione { get; set; }
        public string? NoteRiga { get; set; }
        public decimal ValoreRiga { get; set; }

        // Campi Disponibilità Magazzino (Magazzino 1)
        public decimal Esistenza { get; set; }
        public decimal ImpegnatoAttuale { get; set; }
        public decimal Disponibile => Esistenza - ImpegnatoAttuale;

        // Proprietà calcolate
        public decimal QuantitaRimanente => Quantita - QuantitaEvasa;
        public decimal ColliRimanenti => NumeroColli - ColliEvasi;
        public string DescrizioneArticoloCompleta => !string.IsNullOrEmpty(DescrizioneArticolo) 
            ? $"{CodiceArticolo} - {DescrizioneArticolo}" 
            : CodiceArticolo;
        public string StatoEvasione => QuantitaEvasa <= 0 ? "Da Evadere" 
            : QuantitaEvasa < Quantita ? "Parzialmente Evasa" 
            : "Completamente Evasa";
        public string StatoEvasioneCssClass => StatoEvasione switch
        {
            "Da Evadere" => "badge bg-danger",
            "Parzialmente Evasa" => "badge bg-warning text-dark",
            "Completamente Evasa" => "badge bg-success",
            _ => "badge bg-secondary"
        };
    }

    // ===== VIEWMODELS AVVISO MERCE PRONTA =====

    /// <summary>
    /// ViewModel principale per la pagina Avviso Merce Pronta.
    /// Contiene la lista di ordini con righe disponibili, pronte per l'invio email.
    /// </summary>
    public class AvvisoMerceProntaViewModel
    {
        /// <summary>Data inizio range (oggi)</summary>
        public DateTime DataDa { get; set; }

        /// <summary>Data fine range (oggi + GiorniEmail)</summary>
        public DateTime DataA { get; set; }

        /// <summary>Giorni di range configurati (da TabellaOpzioni)</summary>
        public int GiorniEmail { get; set; }

        /// <summary>Lista ordini con righe disponibili</summary>
        public List<OrdineEmailViewModel> Ordini { get; set; } = new List<OrdineEmailViewModel>();

        /// <summary>Numero totale di righe disponibili</summary>
        public int TotaleRigheDisponibili => Ordini.Sum(o => o.Righe.Count);

        /// <summary>Messaggio informativo (es. errori, avvisi)</summary>
        public string? Messaggio { get; set; }

        /// <summary>Tipo messaggio (success, warning, danger, info)</summary>
        public string? TipoMessaggio { get; set; }
    }

    /// <summary>
    /// ViewModel per un ordine nell'avviso merce pronta (testata + righe disponibili).
    /// </summary>
    public class OrdineEmailViewModel
    {
        // Chiave ordine
        public string TipoOrdine { get; set; } = string.Empty;
        public short AnnoOrdine { get; set; }
        public string SerieOrdine { get; set; } = string.Empty;
        public int NumeroOrdine { get; set; }

        // Dati cliente
        public int CodiceCliente { get; set; }
        public string RagioneSociale { get; set; } = string.Empty;
        public string? EmailCliente { get; set; }

        // Dati agente
        public short CodiceAgente { get; set; }
        public string? NomeAgente { get; set; }
        public string? EmailAgente { get; set; }

        // Dati ordine
        public DateTime DataOrdine { get; set; }
        public string? RiferimentoOrdine { get; set; }
        public string? Porto { get; set; }
        public string? DescrizionePorto { get; set; }
        public string? PortoCssClass { get; set; }

        // Righe disponibili
        public List<RigaEmailViewModel> Righe { get; set; } = new List<RigaEmailViewModel>();

        // Flag Email Inviata (almeno una riga dell'ordine)
        /// <summary>Indica se almeno una riga di questo ordine ha ricevuto una notifica email</summary>
        public bool HasEmailInviata { get; set; }
        /// <summary>Data dell'ultimo invio email per questo ordine</summary>
        public DateTime? DataEmailInviata { get; set; }

        // Proprietà calcolate
        public string NumeroOrdineCompleto => $"{AnnoOrdine}/{SerieOrdine}/{NumeroOrdine:D6}";
    }

    /// <summary>
    /// ViewModel per una singola riga ordine nell'avviso merce pronta.
    /// Contiene dati sulla disponibilità e un flag per selezione.
    /// </summary>
    public class RigaEmailViewModel
    {
        // Chiave riga
        public string TipoOrdine { get; set; } = string.Empty;
        public short AnnoOrdine { get; set; }
        public string SerieOrdine { get; set; } = string.Empty;
        public int NumeroOrdine { get; set; }
        public int RigaOrdine { get; set; }

        // Dati articolo
        public string CodiceArticolo { get; set; } = string.Empty;
        public string? DescrizioneArticolo { get; set; }
        public string? UnitaMisura { get; set; }

        // Quantità ordine
        public decimal Quantita { get; set; }
        public decimal QuantitaEvasa { get; set; }
        public decimal QuantitaRimanente => Quantita - QuantitaEvasa;

        // Data consegna riga
        public DateTime DataConsegna { get; set; }

        // Disponibilità magazzino
        public decimal Esistenza { get; set; }

        /// <summary>
        /// True se la riga è disponibile (Esistenza >= QuantitaRimanente)
        /// </summary>
        public bool IsDisponibile => Esistenza >= QuantitaRimanente;

        /// <summary>
        /// True se l'articolo è in conflitto (presente in più ordini e disponibilità insufficiente per tutti)
        /// </summary>
        public bool IsConflitto { get; set; }

        /// <summary>
        /// Messaggio di conflitto (spiega perché c'è conflitto)
        /// </summary>
        public string? MessaggioConflitto { get; set; }

        /// <summary>
        /// True se l'email è già stata inviata per questa riga
        /// </summary>
        public bool EmailGiaInviata { get; set; }

        /// <summary>
        /// Data dell'ultimo invio email (se già inviata)
        /// </summary>
        public DateTime? DataUltimoInvio { get; set; }

        /// <summary>
        /// Selezionata dall'operatore per l'invio email
        /// </summary>
        public bool Selezionata { get; set; }

        /// <summary>
        /// Chiave unica per identificare la riga nel form (TipoOrdine|Anno|Serie|Numero|Riga)
        /// </summary>
        public string ChiaveRiga => $"{TipoOrdine}|{AnnoOrdine}|{SerieOrdine}|{NumeroOrdine}|{RigaOrdine}";

        /// <summary>
        /// CSS class per lo sfondo della riga
        /// </summary>
        public string RowCssClass
        {
            get
            {
                if (EmailGiaInviata) return "table-secondary"; // Grigio: già inviata
                if (IsConflitto) return "table-warning";       // Giallo: conflitto disponibilità
                return "";                                      // Bianco: disponibile
            }
        }
    }

    /// <summary>
    /// ViewModel per la richiesta di invio email (POST dal form)
    /// </summary>
    public class InvioEmailRequest
    {
        /// <summary>Lista chiavi righe selezionate (formato: TipoOrdine|Anno|Serie|Numero|Riga)</summary>
        public List<string> RigheSelezionate { get; set; } = new List<string>();
    }

    // ===== VIEWMODELS LISTA EMAIL INVIATE =====

    /// <summary>
    /// ViewModel per la pagina Lista Email Inviate.
    /// </summary>
    public class ListaEmailInviateViewModel
    {
        /// <summary>Lista dei record email inviate con dettagli ordine</summary>
        public List<EmailInviataDettaglioViewModel> EmailInviate { get; set; } = new List<EmailInviataDettaglioViewModel>();

        /// <summary>Totale email inviate</summary>
        public int TotaleEmail => EmailInviate.Count;
    }

    /// <summary>
    /// ViewModel per un singolo record di email inviata con tutti i dettagli ordine/cliente.
    /// </summary>
    public class EmailInviataDettaglioViewModel
    {
        // Dati InvioEmail
        public int ID { get; set; }
        public string TipoOrdine { get; set; } = string.Empty;
        public short AnnoOrdine { get; set; }
        public string SerieOrdine { get; set; } = string.Empty;
        public int NumeroOrdine { get; set; }
        public int RigaOrdine { get; set; }
        public DateTime DataInvio { get; set; }
        public string Contabilizzato { get; set; } = "N";

        // Dati Riga Ordine
        public string? CodiceArticolo { get; set; }
        public string? DescrizioneArticolo { get; set; }
        public string? UnitaMisura { get; set; }
        public decimal Quantita { get; set; }
        public decimal QuantitaEvasa { get; set; }
        public decimal QuantitaRimanente => Quantita - QuantitaEvasa;
        public DateTime? DataConsegna { get; set; }

        // Dati Cliente
        public int CodiceCliente { get; set; }
        public string? RagioneSociale { get; set; }

        // Dati Agente
        public string? NomeAgente { get; set; }

        // Proprietà calcolate
        public string NumeroOrdineCompleto => $"{AnnoOrdine}/{SerieOrdine.Trim()}/{NumeroOrdine:D6}";
        public string ContabilizzatoDescrizione => Contabilizzato == "S" ? "Sì" : "No";
        public string ContabilizzatoCssClass => Contabilizzato == "S" ? "badge bg-success" : "badge bg-secondary";
    }
}

