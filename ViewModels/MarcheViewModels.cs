using System.ComponentModel.DataAnnotations;
using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la creazione di una nuova marca
    /// </summary>
    public class CreateMarcaViewModel
    {
        [Required(ErrorMessage = "Il codice marca è obbligatorio")]
        [Display(Name = "Codice Marca")]
        public short CodiceMarca { get; set; }

        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Marca")]
        public string? DescrizioneMarca { get; set; }
    }

    /// <summary>
    /// ViewModel per la modifica di una marca esistente
    /// </summary>
    public class EditMarcaViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Il codice marca è obbligatorio")]
        [Display(Name = "Codice Marca")]
        public short CodiceMarca { get; set; }

        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Marca")]
        public string? DescrizioneMarca { get; set; }

        [Display(Name = "Ultimo Aggiornamento")]
        public DateTime UltimoAggiornamento { get; set; }
    }

    /// <summary>
    /// ViewModel per la lista delle marche con filtri e paginazione
    /// </summary>
    public class MarcheIndexViewModel
    {
        public IEnumerable<Marca> Marche { get; set; } = new List<Marca>();

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
        public int TotaleMarche { get; set; }
    }

    /// <summary>
    /// ViewModel per i dettagli di una marca
    /// </summary>
    public class MarcaDetailsViewModel
    {
        public Marca Marca { get; set; } = new Marca();

        // Navigazione
        public int? PreviousId { get; set; }
        public int? NextId { get; set; }
    }

    /// <summary>
    /// ViewModel per la conferma di eliminazione
    /// </summary>
    public class DeleteMarcaViewModel
    {
        public Marca Marca { get; set; } = new Marca();

        [Required(ErrorMessage = "È necessario confermare l'eliminazione")]
        [Display(Name = "Confermo l'eliminazione")]
        public bool ConfirmDelete { get; set; }
    }

    /// <summary>
    /// ViewModel per le statistiche delle marche
    /// </summary>
    public class MarcheStatsViewModel
    {
        public int TotaleMarche { get; set; }
        public DateTime? DataUltimoAggiornamento { get; set; }
    }

    /// <summary>
    /// ViewModel per l'API delle marche (per dropdown, autocomplete, etc.)
    /// </summary>
    public class MarcaApiViewModel
    {
        public int ID { get; set; }
        public short CodiceMarca { get; set; }
        public string? DescrizioneMarca { get; set; }
        public string DisplayText => $"{CodiceMarca} - {DescrizioneMarca ?? "N/A"}";
    }
}
