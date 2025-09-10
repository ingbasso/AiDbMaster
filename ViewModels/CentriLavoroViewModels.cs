using System.ComponentModel.DataAnnotations;
using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la creazione di un nuovo centro di lavoro
    /// </summary>
    public class CreateCentroLavoroViewModel
    {
        [StringLength(10, ErrorMessage = "Il codice non può superare i 10 caratteri")]
        [Display(Name = "Codice Centro")]
        public string? CodiceCentro { get; set; }

        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(100, ErrorMessage = "La descrizione non può superare i 100 caratteri")]
        [Display(Name = "Descrizione Centro")]
        public string DescrizioneCentro { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "La capacità oraria deve essere tra 1 e 1000")]
        [Display(Name = "Capacità Oraria")]
        public int? CapacitaOraria { get; set; }

        [Range(0.01, 999.99, ErrorMessage = "Il costo orario deve essere tra 0.01 e 999.99")]
        [Display(Name = "Costo Orario Standard (€)")]
        public decimal? CostoOrarioStandard { get; set; }

        [StringLength(500, ErrorMessage = "Le note non possono superare i 500 caratteri")]
        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "Attivo")]
        public bool Attivo { get; set; } = true;
    }

    /// <summary>
    /// ViewModel per la modifica di un centro di lavoro esistente
    /// </summary>
    public class EditCentroLavoroViewModel
    {
        [Required]
        public int IdCentroLavoro { get; set; }

        [StringLength(10, ErrorMessage = "Il codice non può superare i 10 caratteri")]
        [Display(Name = "Codice Centro")]
        public string? CodiceCentro { get; set; }

        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(100, ErrorMessage = "La descrizione non può superare i 100 caratteri")]
        [Display(Name = "Descrizione Centro")]
        public string DescrizioneCentro { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "La capacità oraria deve essere tra 1 e 1000")]
        [Display(Name = "Capacità Oraria")]
        public int? CapacitaOraria { get; set; }

        [Range(0.01, 999.99, ErrorMessage = "Il costo orario deve essere tra 0.01 e 999.99")]
        [Display(Name = "Costo Orario Standard (€)")]
        public decimal? CostoOrarioStandard { get; set; }

        [StringLength(500, ErrorMessage = "Le note non possono superare i 500 caratteri")]
        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "Attivo")]
        public bool Attivo { get; set; }

        [Display(Name = "Data Creazione")]
        public DateTime DataCreazione { get; set; }

        [Display(Name = "Data Ultima Modifica")]
        public DateTime? DataUltimaModifica { get; set; }
    }

    /// <summary>
    /// ViewModel per la lista dei centri di lavoro con filtri e paginazione
    /// </summary>
    public class CentriLavoroIndexViewModel
    {
        public IEnumerable<CentroLavoro> CentriLavoro { get; set; } = new List<CentroLavoro>();

        // Filtri
        [Display(Name = "Ricerca")]
        public string? Search { get; set; }

        [Display(Name = "Solo Attivi")]
        public bool? Attivo { get; set; }

        [Display(Name = "Con Capacità Definita")]
        public bool? HasCapacita { get; set; }

        [Display(Name = "Con Costo Definito")]
        public bool? HasCosto { get; set; }

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
        public string CapacitaSortParm { get; set; } = string.Empty;
        public string CostoSortParm { get; set; } = string.Empty;
        public string DataSortParm { get; set; } = string.Empty;

        // Statistiche
        public int TotaleCentriLavoro { get; set; }
        public int CentriLavoroAttivi { get; set; }
        public int CentriLavoroInattivi { get; set; }
        public int CentriConCapacita { get; set; }
        public int CentriConCosto { get; set; }
    }

    /// <summary>
    /// ViewModel per i dettagli di un centro di lavoro
    /// </summary>
    public class CentroLavoroDetailsViewModel
    {
        public CentroLavoro CentroLavoro { get; set; } = new CentroLavoro();

        // Statistiche aggiuntive
        public int GiorniDallaCreazione => (DateTime.Now - CentroLavoro.DataCreazione).Days;
        public bool ModificatoRecentemente => CentroLavoro.DataUltimaModifica?.AddDays(7) > DateTime.Now;

        // Statistiche ordini di produzione
        public int OrdiniProduzioneAssegnati { get; set; }
        public int OrdiniProduzioneAttivi { get; set; }
        public int OrdiniProduzioneCompletati { get; set; }

        // Navigazione
        public int? PreviousId { get; set; }
        public int? NextId { get; set; }
    }

    /// <summary>
    /// ViewModel per la conferma di eliminazione
    /// </summary>
    public class DeleteCentroLavoroViewModel
    {
        public CentroLavoro CentroLavoro { get; set; } = new CentroLavoro();

        [Required(ErrorMessage = "È necessario confermare l'eliminazione")]
        [Display(Name = "Confermo l'eliminazione")]
        public bool ConfirmDelete { get; set; }

        // Informazioni aggiuntive per la decisione
        public bool HasOrdiniProduzione { get; set; }
        public int OrdiniProduzioneCount { get; set; }
        public string? OrdiniProduzioneDescription { get; set; }
    }

    /// <summary>
    /// ViewModel per le statistiche dei centri di lavoro
    /// </summary>
    public class CentriLavoroStatsViewModel
    {
        public int TotaleCentriLavoro { get; set; }
        public int CentriLavoroAttivi { get; set; }
        public int CentriLavoroInattivi { get; set; }
        public int CentriConCapacita { get; set; }
        public int CentriConCosto { get; set; }
        public int CentriConCodice { get; set; }

        public double PercentualeAttivi => TotaleCentriLavoro > 0 ? (double)CentriLavoroAttivi / TotaleCentriLavoro * 100 : 0;
        public double PercentualeConCapacita => TotaleCentriLavoro > 0 ? (double)CentriConCapacita / TotaleCentriLavoro * 100 : 0;
        public double PercentualeConCosto => TotaleCentriLavoro > 0 ? (double)CentriConCosto / TotaleCentriLavoro * 100 : 0;

        public decimal? CapacitaMediaOraria { get; set; }
        public decimal? CostoMedioOrario { get; set; }
        public decimal? CapacitaTotaleOraria { get; set; }

        public DateTime? DataUltimaCreazione { get; set; }
        public DateTime? DataUltimaModifica { get; set; }

        public IEnumerable<CentroLavoroFrequencyViewModel> CentriPiuUtilizzati { get; set; } = new List<CentroLavoroFrequencyViewModel>();
        public IEnumerable<CentroLavoroFrequencyViewModel> CentriPiuRecenti { get; set; } = new List<CentroLavoroFrequencyViewModel>();
    }

    /// <summary>
    /// ViewModel per la frequenza di utilizzo dei centri di lavoro
    /// </summary>
    public class CentroLavoroFrequencyViewModel
    {
        public int IdCentroLavoro { get; set; }
        public string? CodiceCentro { get; set; }
        public string DescrizioneCentro { get; set; } = string.Empty;
        public int FrequenzaUtilizzo { get; set; }
        public DateTime UltimoUtilizzo { get; set; }
        public int? CapacitaOraria { get; set; }
        public decimal? CostoOrarioStandard { get; set; }
    }

    /// <summary>
    /// ViewModel per l'API dei centri di lavoro (per dropdown, autocomplete, etc.)
    /// </summary>
    public class CentroLavoroApiViewModel
    {
        public int Id { get; set; }
        public string? Codice { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public bool Attivo { get; set; }
        public int? CapacitaOraria { get; set; }
        public decimal? CostoOrarioStandard { get; set; }
        public string DisplayText => !string.IsNullOrEmpty(Codice) ? $"{Codice} - {Descrizione}" : Descrizione;
    }
}
