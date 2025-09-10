using System.ComponentModel.DataAnnotations;
using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la creazione di una nuova lavorazione
    /// </summary>
    public class CreateLavorazioneViewModel
    {
        [StringLength(1, ErrorMessage = "Il codice deve essere di 1 carattere")]
        [Display(Name = "Codice Lavorazione")]
        public string? CodiceLavorazione { get; set; }

        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(100, ErrorMessage = "La descrizione non può superare i 100 caratteri")]
        [Display(Name = "Descrizione Lavorazione")]
        public string DescrizioneLavorazione { get; set; } = string.Empty;

        [Display(Name = "Attiva")]
        public bool Attivo { get; set; } = true;
    }

    /// <summary>
    /// ViewModel per la modifica di una lavorazione esistente
    /// </summary>
    public class EditLavorazioneViewModel
    {
        [Required]
        public int IdLavorazione { get; set; }

        [StringLength(1, ErrorMessage = "Il codice deve essere di 1 carattere")]
        [Display(Name = "Codice Lavorazione")]
        public string? CodiceLavorazione { get; set; }

        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(100, ErrorMessage = "La descrizione non può superare i 100 caratteri")]
        [Display(Name = "Descrizione Lavorazione")]
        public string DescrizioneLavorazione { get; set; } = string.Empty;

        [Display(Name = "Attiva")]
        public bool Attivo { get; set; }

        [Display(Name = "Data Creazione")]
        public DateTime DataCreazione { get; set; }

        [Display(Name = "Data Ultima Modifica")]
        public DateTime? DataUltimaModifica { get; set; }
    }

    /// <summary>
    /// ViewModel per la lista delle lavorazioni con filtri e paginazione
    /// </summary>
    public class LavorazioniIndexViewModel
    {
        public IEnumerable<Lavorazioni> Lavorazioni { get; set; } = new List<Lavorazioni>();

        // Filtri
        [Display(Name = "Ricerca")]
        public string? Search { get; set; }

        [Display(Name = "Solo Attive")]
        public bool? Attivo { get; set; }

        // Ordinamento
        public string SortOrder { get; set; } = "descrizione";

        // Paginazione
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Parametri per l'ordinamento (per le viste)
        public string CodiceSortParm { get; set; } = string.Empty;
        public string DescrizioneSortParm { get; set; } = string.Empty;
        public string AttivoSortParm { get; set; } = string.Empty;
        public string DataSortParm { get; set; } = string.Empty;

        // Statistiche
        public int TotaleLavorazioni { get; set; }
        public int LavorazioniAttive { get; set; }
        public int LavorazioniInattive { get; set; }
    }

    /// <summary>
    /// ViewModel per i dettagli di una lavorazione
    /// </summary>
    public class LavorazioneDetailsViewModel
    {
        public Lavorazioni Lavorazione { get; set; } = new Lavorazioni();

        // Statistiche aggiuntive
        public int GiorniDallaCreazione => (DateTime.Now - Lavorazione.DataCreazione).Days;
        public bool ModificataRecentemente => Lavorazione.DataUltimaModifica?.AddDays(7) > DateTime.Now;

        // Navigazione
        public int? PreviousId { get; set; }
        public int? NextId { get; set; }
    }

    /// <summary>
    /// ViewModel per la conferma di eliminazione
    /// </summary>
    public class DeleteLavorazioneViewModel
    {
        public Lavorazioni Lavorazione { get; set; } = new Lavorazioni();

        [Required(ErrorMessage = "È necessario confermare l'eliminazione")]
        [Display(Name = "Confermo l'eliminazione")]
        public bool ConfirmDelete { get; set; }

        // Informazioni aggiuntive per la decisione
        public bool HasRelatedData { get; set; }
        public int RelatedDataCount { get; set; }
        public string? RelatedDataDescription { get; set; }
    }

    /// <summary>
    /// ViewModel per le statistiche delle lavorazioni
    /// </summary>
    public class LavorazioniStatsViewModel
    {
        public int TotaleLavorazioni { get; set; }
        public int LavorazioniAttive { get; set; }
        public int LavorazioniInattive { get; set; }
        public int LavorazioniConCodice { get; set; }
        public int LavorazioniSenzaCodice { get; set; }

        public double PercentualeAttive => TotaleLavorazioni > 0 ? (double)LavorazioniAttive / TotaleLavorazioni * 100 : 0;
        public double PercentualeConCodice => TotaleLavorazioni > 0 ? (double)LavorazioniConCodice / TotaleLavorazioni * 100 : 0;

        public DateTime? DataUltimaCreazione { get; set; }
        public DateTime? DataUltimaModifica { get; set; }

        public IEnumerable<LavorazioneFrequencyViewModel> LavorazioniPiuRecenti { get; set; } = new List<LavorazioneFrequencyViewModel>();
    }

    /// <summary>
    /// ViewModel per la frequenza di utilizzo delle lavorazioni
    /// </summary>
    public class LavorazioneFrequencyViewModel
    {
        public int IdLavorazione { get; set; }
        public string? CodiceLavorazione { get; set; }
        public string DescrizioneLavorazione { get; set; } = string.Empty;
        public int FrequenzaUtilizzo { get; set; }
        public DateTime UltimoUtilizzo { get; set; }
    }

    /// <summary>
    /// ViewModel per l'API delle lavorazioni (per dropdown, autocomplete, etc.)
    /// </summary>
    public class LavorazioneApiViewModel
    {
        public int Id { get; set; }
        public string? Codice { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public bool Attivo { get; set; }
        public string DisplayText => !string.IsNullOrEmpty(Codice) ? $"{Codice} - {Descrizione}" : Descrizione;
    }
}
