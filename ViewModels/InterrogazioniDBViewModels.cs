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
        
        [Display(Name = "Data Riferimento")]
        [DataType(DataType.Date)]
        public DateTime? DataRiferimento { get; set; }
        
        // Output - Lista risultati
        public List<DisponibilitaRigaViewModel>? Risultati { get; set; }
        
        // Output - Lista risultati articoli sostitutivi
        public List<DisponibilitaArticoloGruppoViewModel>? ArticoliSostitutivi { get; set; }
        
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
        
        // DISPONIBILITÀ ATTUALE
        public decimal Esistenza { get; set; }
        
        /// <summary>
        /// Impegnato attuale - solo fino ad oggi (da DB)
        /// </summary>
        public decimal ImpegnatoAttuale { get; set; }
        
        /// <summary>
        /// Disponibile attuale = Esistenza - Impegnato Attuale
        /// </summary>
        public decimal DisponibileAttuale => Esistenza - ImpegnatoAttuale;
        
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
        
        // CSS class per colorare la riga in base alla disponibilità
        public string RowCssClass => DisponibileAttuale > 0 ? "table-success" : 
                                     DisponibileAttuale == 0 ? "table-warning" : "table-danger";
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
}

