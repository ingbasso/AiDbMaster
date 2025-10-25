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
        
        [Display(Name = "Data Riferimento")]
        [DataType(DataType.Date)]
        public DateTime? DataRiferimento { get; set; }
        
        // Output - Lista risultati
        public List<DisponibilitaRigaViewModel>? Risultati { get; set; }
        
        // Informazioni aggiuntive sull'articolo selezionato
        public string? DescrizioneArticolo { get; set; }
    }

    /// <summary>
    /// ViewModel per ogni riga di risultato della disponibilità
    /// </summary>
    public class DisponibilitaRigaViewModel
    {
        public string CodiceArticolo { get; set; } = string.Empty;
        public short CodiceMagazzino { get; set; }
        public decimal Esistenza { get; set; }
        
        /// <summary>
        /// Quantità impegnata da ordini clienti.
        /// Include: impegnato fino ad oggi (da DB) + impegnato futuro (calcolato se data > oggi)
        /// </summary>
        public decimal ImpegnatoDataOdierna { get; set; }
        
        public decimal OrdinatoFornitoriDataOdierna { get; set; }
        
        // Campi calcolati (utili per la visualizzazione)
        /// <summary>
        /// Disponibile = Esistenza - Impegnato
        /// </summary>
        public decimal Disponibile => Esistenza - ImpegnatoDataOdierna;
        
        /// <summary>
        /// Totale previsto = Esistenza + Ordinato da Fornitori
        /// </summary>
        public decimal TotalePrevisto => Esistenza + OrdinatoFornitoriDataOdierna;
        
        // CSS class per colorare la riga in base alla disponibilità
        public string RowCssClass => Disponibile > 0 ? "table-success" : 
                                     Disponibile == 0 ? "table-warning" : "table-danger";
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
}

