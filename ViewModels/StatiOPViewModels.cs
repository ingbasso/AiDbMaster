using System.ComponentModel.DataAnnotations;
using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la creazione di un nuovo stato OP
    /// </summary>
    public class CreateStatoOPViewModel
    {
        [Required(ErrorMessage = "Il codice stato è obbligatorio")]
        [StringLength(2, ErrorMessage = "Il codice non può superare i 2 caratteri")]
        [Display(Name = "Codice Stato")]
        public string CodiceStato { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(20, ErrorMessage = "La descrizione non può superare i 20 caratteri")]
        [Display(Name = "Descrizione Stato")]
        public string DescrizioneStato { get; set; } = string.Empty;

        [Range(1, 999, ErrorMessage = "L'ordine deve essere tra 1 e 999")]
        [Display(Name = "Ordine Visualizzazione")]
        public int Ordine { get; set; } = 1;

        [Display(Name = "Attivo")]
        public bool Attivo { get; set; } = true;
    }

    /// <summary>
    /// ViewModel per la modifica di uno stato OP esistente
    /// </summary>
    public class EditStatoOPViewModel
    {
        [Required]
        public int IdStato { get; set; }

        [Required(ErrorMessage = "Il codice stato è obbligatorio")]
        [StringLength(2, ErrorMessage = "Il codice non può superare i 2 caratteri")]
        [Display(Name = "Codice Stato")]
        public string CodiceStato { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(20, ErrorMessage = "La descrizione non può superare i 20 caratteri")]
        [Display(Name = "Descrizione Stato")]
        public string DescrizioneStato { get; set; } = string.Empty;

        [Range(1, 999, ErrorMessage = "L'ordine deve essere tra 1 e 999")]
        [Display(Name = "Ordine Visualizzazione")]
        public int Ordine { get; set; }

        [Display(Name = "Attivo")]
        public bool Attivo { get; set; }
    }

    /// <summary>
    /// ViewModel per la lista degli stati OP con filtri e paginazione
    /// </summary>
    public class StatiOPIndexViewModel
    {
        public IEnumerable<StatoOP> StatiOP { get; set; } = new List<StatoOP>();

        // Filtri
        [Display(Name = "Ricerca")]
        public string? Search { get; set; }

        [Display(Name = "Solo Attivi")]
        public bool? Attivo { get; set; }

        [Display(Name = "Codice Stato")]
        public string? CodiceStato { get; set; }

        // Ordinamento
        public string SortOrder { get; set; } = "ordine";

        // Paginazione
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Parametri per l'ordinamento (per le viste)
        public string CodiceSortParm { get; set; } = string.Empty;
        public string DescrizioneSortParm { get; set; } = string.Empty;
        public string AttivoSortParm { get; set; } = string.Empty;
        public string OrdineSortParm { get; set; } = string.Empty;

        // Statistiche
        public int TotaleStatiOP { get; set; }
        public int StatiOPAttivi { get; set; }
        public int StatiOPInattivi { get; set; }
        public int OrdiniTotali { get; set; }
    }

    /// <summary>
    /// ViewModel per i dettagli di uno stato OP
    /// </summary>
    public class StatoOPDetailsViewModel
    {
        public StatoOP StatoOP { get; set; } = new StatoOP();

        // Statistiche ordini di produzione
        public int OrdiniProduzioneAssegnati { get; set; }
        public int OrdiniProduzioneAttivi { get; set; }
        public DateTime? UltimoOrdineAssegnato { get; set; }
        public DateTime? PrimoOrdineAssegnato { get; set; }

        // Navigazione
        public int? PreviousId { get; set; }
        public int? NextId { get; set; }

        // Statistiche aggiuntive
        public double PercentualeUtilizzo { get; set; }
        public int OrdiniUltimoMese { get; set; }
        public int OrdiniUltimaSettimana { get; set; }
    }

    /// <summary>
    /// ViewModel per la conferma di eliminazione
    /// </summary>
    public class DeleteStatoOPViewModel
    {
        public StatoOP StatoOP { get; set; } = new StatoOP();

        [Required(ErrorMessage = "È necessario confermare l'eliminazione")]
        [Display(Name = "Confermo l'eliminazione")]
        public bool ConfirmDelete { get; set; }

        // Informazioni aggiuntive per la decisione
        public bool HasOrdiniProduzione { get; set; }
        public int OrdiniProduzioneCount { get; set; }
        public string? OrdiniProduzioneDescription { get; set; }
        public bool IsSystemState { get; set; } // Per stati di sistema non eliminabili
    }

    /// <summary>
    /// ViewModel per le statistiche degli stati OP
    /// </summary>
    public class StatiOPStatsViewModel
    {
        public int TotaleStatiOP { get; set; }
        public int StatiOPAttivi { get; set; }
        public int StatiOPInattivi { get; set; }
        public int TotaleOrdiniProduzione { get; set; }

        public double PercentualeAttivi => TotaleStatiOP > 0 ? (double)StatiOPAttivi / TotaleStatiOP * 100 : 0;

        // Distribuzione ordini per stato
        public Dictionary<string, int> DistribuzioneOrdiniPerStato { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> PercentualeOrdiniPerStato { get; set; } = new Dictionary<string, double>();

        // Stati più utilizzati
        public IEnumerable<StatoOPFrequencyViewModel> StatiPiuUtilizzati { get; set; } = new List<StatoOPFrequencyViewModel>();
        public IEnumerable<StatoOPFrequencyViewModel> StatiMenoUtilizzati { get; set; } = new List<StatoOPFrequencyViewModel>();

        // Trend temporali
        public DateTime? DataUltimoUtilizzo { get; set; }
        public string? StatoPiuUtilizzato { get; set; }
        public string? StatoMenoUtilizzato { get; set; }
    }

    /// <summary>
    /// ViewModel per la frequenza di utilizzo degli stati OP
    /// </summary>
    public class StatoOPFrequencyViewModel
    {
        public int IdStato { get; set; }
        public string CodiceStato { get; set; } = string.Empty;
        public string DescrizioneStato { get; set; } = string.Empty;
        public int FrequenzaUtilizzo { get; set; }
        public DateTime? UltimoUtilizzo { get; set; }
        public DateTime? PrimoUtilizzo { get; set; }
        public int Ordine { get; set; }
        public bool Attivo { get; set; }
        public double PercentualeUtilizzo { get; set; }
        public string DisplayText => $"{CodiceStato} - {DescrizioneStato}";
    }

    /// <summary>
    /// ViewModel per l'API degli stati OP (per dropdown, autocomplete, etc.)
    /// </summary>
    public class StatoOPApiViewModel
    {
        public int Id { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public bool Attivo { get; set; }
        public int Ordine { get; set; }
        public string DisplayText => $"{Codice} - {Descrizione}";
        public string ColorClass => GetColorClass();

        private string GetColorClass()
        {
            return Codice.ToUpper() switch
            {
                "ES" => "bg-info",      // Emesso - Blu
                "PR" => "bg-warning",   // Produzione - Giallo
                "CH" => "bg-success",   // Chiuso - Verde
                "SO" => "bg-secondary", // Sospeso - Grigio
                "AN" => "bg-danger",    // Annullato - Rosso
                _ => "bg-primary"       // Default - Blu primario
            };
        }
    }

    /// <summary>
    /// ViewModel per il riepilogo stati OP per dashboard
    /// </summary>
    public class StatiOPSummaryViewModel
    {
        public int TotaleStatiOP { get; set; }
        public int StatiOPAttivi { get; set; }
        public int OrdiniEmessi { get; set; }
        public int OrdiniInProduzione { get; set; }
        public int OrdiniChiusi { get; set; }
        public int OrdiniSospesi { get; set; }
        public int OrdiniAnnullati { get; set; }

        // Percentuali
        public double PercentualeEmessi => TotaleOrdini > 0 ? (double)OrdiniEmessi / TotaleOrdini * 100 : 0;
        public double PercentualeInProduzione => TotaleOrdini > 0 ? (double)OrdiniInProduzione / TotaleOrdini * 100 : 0;
        public double PercentualeChiusi => TotaleOrdini > 0 ? (double)OrdiniChiusi / TotaleOrdini * 100 : 0;
        public double PercentualeSospesi => TotaleOrdini > 0 ? (double)OrdiniSospesi / TotaleOrdini * 100 : 0;

        public int TotaleOrdini => OrdiniEmessi + OrdiniInProduzione + OrdiniChiusi + OrdiniSospesi + OrdiniAnnullati;

        // Trend
        public int VariazioneOrdiniUltimoMese { get; set; }
        public string TendenzaOrdini => VariazioneOrdiniUltimoMese >= 0 ? "crescita" : "decrescita";
    }

    /// <summary>
    /// ViewModel per la gestione dell'ordine degli stati
    /// </summary>
    public class ReorderStatiOPViewModel
    {
        public List<StatoOPOrderItem> Stati { get; set; } = new List<StatoOPOrderItem>();
    }

    /// <summary>
    /// Item per il riordinamento degli stati
    /// </summary>
    public class StatoOPOrderItem
    {
        public int IdStato { get; set; }
        public string CodiceStato { get; set; } = string.Empty;
        public string DescrizioneStato { get; set; } = string.Empty;
        public int Ordine { get; set; }
        public bool Attivo { get; set; }
    }

    /// <summary>
    /// ViewModel per l'analisi dei flussi di stato
    /// </summary>
    public class StatoOPFlowAnalysisViewModel
    {
        public string StatoCorrente { get; set; } = string.Empty;
        public Dictionary<string, int> TransizioniVersoProssimo { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> TransizioniDaPrecedente { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, TimeSpan> TempoMedioNelloStato { get; set; } = new Dictionary<string, TimeSpan>();
        public int OrdiniBloccatiNelloStato { get; set; }
        public TimeSpan TempoMedioTransizione { get; set; }
    }

    /// <summary>
    /// ViewModel per la configurazione degli stati di sistema
    /// </summary>
    public class StatiOPConfigViewModel
    {
        [Display(Name = "Stato Iniziale Default")]
        public int? StatoInizialeDefault { get; set; }

        [Display(Name = "Stato Finale Default")]
        public int? StatoFinaleDefault { get; set; }

        [Display(Name = "Stati di Sistema (non eliminabili)")]
        public List<int> StatiDiSistema { get; set; } = new List<int>();

        [Display(Name = "Abilita Transizioni Automatiche")]
        public bool AbilitaTransizioniAutomatiche { get; set; }

        [Display(Name = "Notifiche Cambio Stato")]
        public bool NotificheCambioStato { get; set; }

        public IEnumerable<StatoOP> StatiDisponibili { get; set; } = new List<StatoOP>();
    }
}
