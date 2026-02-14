using System.ComponentModel.DataAnnotations;
using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la creazione di una nuova famiglia
    /// </summary>
    public class CreateFamigliaViewModel
    {
        [Required(ErrorMessage = "Il codice famiglia è obbligatorio")]
        [StringLength(4, ErrorMessage = "Il codice famiglia non può superare i 4 caratteri")]
        [Display(Name = "Codice Famiglia")]
        public string CodiceFamiglia { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Famiglia")]
        public string? DescrizioneFamiglia { get; set; }
    }

    /// <summary>
    /// ViewModel per la modifica di una famiglia esistente
    /// </summary>
    public class EditFamigliaViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Il codice famiglia è obbligatorio")]
        [StringLength(4, ErrorMessage = "Il codice famiglia non può superare i 4 caratteri")]
        [Display(Name = "Codice Famiglia")]
        public string CodiceFamiglia { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Famiglia")]
        public string? DescrizioneFamiglia { get; set; }

        [Display(Name = "Ultimo Aggiornamento")]
        public DateTime UltimoAggiornamento { get; set; }
    }

    /// <summary>
    /// ViewModel per la lista delle famiglie con filtri e paginazione
    /// </summary>
    public class FamiglieIndexViewModel
    {
        public IEnumerable<Famiglia> Famiglie { get; set; } = new List<Famiglia>();

        // Filtri
        [Display(Name = "Ricerca")]
        public string? Search { get; set; }

        // Ordinamento
        public string SortOrder { get; set; } = "codice";

        // Paginazione
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Parametri per l'ordinamento (per le viste)
        public string CodiceSortParm { get; set; } = string.Empty;
        public string DescrizioneSortParm { get; set; } = string.Empty;
        public string DataSortParm { get; set; } = string.Empty;

        // Statistiche
        public int TotaleFamiglie { get; set; }
    }

    /// <summary>
    /// ViewModel per i dettagli di una famiglia
    /// </summary>
    public class FamigliaDetailsViewModel
    {
        public Famiglia Famiglia { get; set; } = new Famiglia();

        // Navigazione
        public int? PreviousId { get; set; }
        public int? NextId { get; set; }
    }

    /// <summary>
    /// ViewModel per la conferma di eliminazione
    /// </summary>
    public class DeleteFamigliaViewModel
    {
        public Famiglia Famiglia { get; set; } = new Famiglia();

        [Required(ErrorMessage = "È necessario confermare l'eliminazione")]
        [Display(Name = "Confermo l'eliminazione")]
        public bool ConfirmDelete { get; set; }
    }

    /// <summary>
    /// ViewModel per le statistiche delle famiglie
    /// </summary>
    public class FamiglieStatsViewModel
    {
        public int TotaleFamiglie { get; set; }
        public DateTime? DataUltimoAggiornamento { get; set; }
    }

    /// <summary>
    /// ViewModel per l'API delle famiglie (per dropdown, autocomplete, etc.)
    /// </summary>
    public class FamigliaApiViewModel
    {
        public int ID { get; set; }
        public string CodiceFamiglia { get; set; } = string.Empty;
        public string? DescrizioneFamiglia { get; set; }
        public string DisplayText => $"{CodiceFamiglia} - {DescrizioneFamiglia ?? "N/A"}";
    }
}
