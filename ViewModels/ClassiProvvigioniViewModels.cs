using System.ComponentModel.DataAnnotations;
using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la creazione di una nuova classe provvigione
    /// </summary>
    public class CreateClasseProvvigioneViewModel
    {
        [Required(ErrorMessage = "Il codice classe è obbligatorio")]
        [Display(Name = "Codice Classe")]
        public short CodiceClasse { get; set; }

        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Classe")]
        public string? DescrizioneClasse { get; set; }

        [Required(ErrorMessage = "La percentuale di sconto è obbligatoria")]
        [Range(0, 100, ErrorMessage = "La percentuale deve essere tra 0 e 100")]
        [Display(Name = "% Sconto")]
        public decimal Perc_Sconto { get; set; }
    }

    /// <summary>
    /// ViewModel per la modifica di una classe provvigione esistente
    /// </summary>
    public class EditClasseProvvigioneViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Il codice classe è obbligatorio")]
        [Display(Name = "Codice Classe")]
        public short CodiceClasse { get; set; }

        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Classe")]
        public string? DescrizioneClasse { get; set; }

        [Required(ErrorMessage = "La percentuale di sconto è obbligatoria")]
        [Range(0, 100, ErrorMessage = "La percentuale deve essere tra 0 e 100")]
        [Display(Name = "% Sconto")]
        public decimal Perc_Sconto { get; set; }

        [Display(Name = "Ultimo Aggiornamento")]
        public DateTime UltimoAggiornamento { get; set; }
    }

    /// <summary>
    /// ViewModel per la lista delle classi provvigioni con filtri e paginazione
    /// </summary>
    public class ClassiProvvigioniIndexViewModel
    {
        public IEnumerable<ClasseProvvigione> ClassiProvvigioni { get; set; } = new List<ClasseProvvigione>();

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
        public string PercScontoSortParm { get; set; } = string.Empty;
        public string DataSortParm { get; set; } = string.Empty;

        // Statistiche
        public int TotaleClassi { get; set; }
        public decimal? PercScontoMedia { get; set; }
        public decimal? PercScontoMin { get; set; }
        public decimal? PercScontoMax { get; set; }
    }

    /// <summary>
    /// ViewModel per i dettagli di una classe provvigione
    /// </summary>
    public class ClasseProvvigioneDetailsViewModel
    {
        public ClasseProvvigione ClasseProvvigione { get; set; } = new ClasseProvvigione();

        // Navigazione
        public int? PreviousId { get; set; }
        public int? NextId { get; set; }
    }

    /// <summary>
    /// ViewModel per la conferma di eliminazione
    /// </summary>
    public class DeleteClasseProvvigioneViewModel
    {
        public ClasseProvvigione ClasseProvvigione { get; set; } = new ClasseProvvigione();

        [Required(ErrorMessage = "È necessario confermare l'eliminazione")]
        [Display(Name = "Confermo l'eliminazione")]
        public bool ConfirmDelete { get; set; }
    }

    /// <summary>
    /// ViewModel per le statistiche delle classi provvigioni
    /// </summary>
    public class ClassiProvvigioniStatsViewModel
    {
        public int TotaleClassi { get; set; }
        public decimal? PercScontoMedia { get; set; }
        public decimal? PercScontoMin { get; set; }
        public decimal? PercScontoMax { get; set; }
        public DateTime? DataUltimoAggiornamento { get; set; }
    }

    /// <summary>
    /// ViewModel per l'API delle classi provvigioni (per dropdown, autocomplete, etc.)
    /// </summary>
    public class ClasseProvvigioneApiViewModel
    {
        public int ID { get; set; }
        public short CodiceClasse { get; set; }
        public string? DescrizioneClasse { get; set; }
        public decimal Perc_Sconto { get; set; }
        public string DisplayText => $"{CodiceClasse} - {DescrizioneClasse ?? "N/A"} ({Perc_Sconto:N2}%)";
    }
}
