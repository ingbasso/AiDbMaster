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
    /// ViewModel per la pagina Consegne Programmate (da implementare)
    /// </summary>
    public class ConsegneProgrammateViewModel
    {
        [Display(Name = "Codice Articolo")]
        public string? CodiceArticolo { get; set; }
        
        [Display(Name = "Data Inizio")]
        [DataType(DataType.Date)]
        public DateTime? DataInizio { get; set; }
        
        [Display(Name = "Data Fine")]
        [DataType(DataType.Date)]
        public DateTime? DataFine { get; set; }
        
        // Lista risultati (da implementare)
        public List<ConsegnaProgrammataRigaViewModel>? Risultati { get; set; }
    }

    /// <summary>
    /// ViewModel per ogni riga di consegna programmata (da implementare)
    /// </summary>
    public class ConsegnaProgrammataRigaViewModel
    {
        public string CodiceArticolo { get; set; } = string.Empty;
        public DateTime DataConsegnaPrevista { get; set; }
        public decimal Quantita { get; set; }
        public string Fornitore { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
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
    }

    /// <summary>
    /// Response per il dettaglio degli ordini clienti impegnati
    /// </summary>
    public class DettaglioImpegnatoFuturoResponse
    {
        public string CodiceArticolo { get; set; } = string.Empty;
        public string DescrizioneArticolo { get; set; } = string.Empty;
        public DateTime DataRiferimento { get; set; }
        public List<OrdineClienteDettaglioViewModel> Ordini { get; set; } = new List<OrdineClienteDettaglioViewModel>();
        public decimal TotaleImpegnatoFuturo { get; set; }
    }
}

