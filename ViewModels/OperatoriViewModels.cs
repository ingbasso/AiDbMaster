using System.ComponentModel.DataAnnotations;
using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la creazione di un nuovo operatore
    /// </summary>
    public class CreateOperatoreViewModel
    {
        [Required(ErrorMessage = "Il codice operatore è obbligatorio")]
        [StringLength(10, ErrorMessage = "Il codice non può superare i 10 caratteri")]
        [Display(Name = "Codice Operatore")]
        public string CodiceOperatore { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il nome non può superare i 50 caratteri")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il cognome non può superare i 50 caratteri")]
        [Display(Name = "Cognome")]
        public string Cognome { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido")]
        [StringLength(100, ErrorMessage = "L'email non può superare i 100 caratteri")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Inserire un numero di telefono valido")]
        [StringLength(20, ErrorMessage = "Il telefono non può superare i 20 caratteri")]
        [Display(Name = "Telefono")]
        public string? Telefono { get; set; }

        [Display(Name = "Data Assunzione")]
        [DataType(DataType.Date)]
        public DateTime? DataAssunzione { get; set; }

        [Range(1, 5, ErrorMessage = "Il livello di competenza deve essere tra 1 e 5")]
        [Display(Name = "Livello Competenza (1-5)")]
        public int? LivelloCompetenza { get; set; }

        [StringLength(500, ErrorMessage = "Le note non possono superare i 500 caratteri")]
        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "Attivo")]
        public bool Attivo { get; set; } = true;
    }

    /// <summary>
    /// ViewModel per la modifica di un operatore esistente
    /// </summary>
    public class EditOperatoreViewModel
    {
        [Required]
        public int IdOperatore { get; set; }

        [Required(ErrorMessage = "Il codice operatore è obbligatorio")]
        [StringLength(10, ErrorMessage = "Il codice non può superare i 10 caratteri")]
        [Display(Name = "Codice Operatore")]
        public string CodiceOperatore { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il nome non può superare i 50 caratteri")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il cognome non può superare i 50 caratteri")]
        [Display(Name = "Cognome")]
        public string Cognome { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido")]
        [StringLength(100, ErrorMessage = "L'email non può superare i 100 caratteri")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Inserire un numero di telefono valido")]
        [StringLength(20, ErrorMessage = "Il telefono non può superare i 20 caratteri")]
        [Display(Name = "Telefono")]
        public string? Telefono { get; set; }

        [Display(Name = "Data Assunzione")]
        [DataType(DataType.Date)]
        public DateTime? DataAssunzione { get; set; }

        [Range(1, 5, ErrorMessage = "Il livello di competenza deve essere tra 1 e 5")]
        [Display(Name = "Livello Competenza (1-5)")]
        public int? LivelloCompetenza { get; set; }

        [StringLength(500, ErrorMessage = "Le note non possono superare i 500 caratteri")]
        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "Attivo")]
        public bool Attivo { get; set; }

        // Proprietà di sola lettura per informazioni aggiuntive
        [Display(Name = "Nome Completo")]
        public string NomeCompleto => $"{Nome} {Cognome}";

        public int AnniServizio => DataAssunzione.HasValue ? DateTime.Now.Year - DataAssunzione.Value.Year : 0;
    }

    /// <summary>
    /// ViewModel per la lista degli operatori con filtri e paginazione
    /// </summary>
    public class OperatoriIndexViewModel
    {
        public IEnumerable<Operatore> Operatori { get; set; } = new List<Operatore>();

        // Filtri
        [Display(Name = "Ricerca")]
        public string? Search { get; set; }

        [Display(Name = "Solo Attivi")]
        public bool? Attivo { get; set; }

        [Display(Name = "Livello Competenza")]
        public int? LivelloCompetenza { get; set; }

        [Display(Name = "Con Email")]
        public bool? HasEmail { get; set; }

        [Display(Name = "Con Telefono")]
        public bool? HasTelefono { get; set; }

        [Display(Name = "Assunti dal")]
        [DataType(DataType.Date)]
        public DateTime? DataAssunzioneDa { get; set; }

        [Display(Name = "Assunti fino al")]
        [DataType(DataType.Date)]
        public DateTime? DataAssunzioneA { get; set; }

        // Ordinamento
        public string SortOrder { get; set; } = "cognome";

        // Paginazione
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Parametri per l'ordinamento (per le viste)
        public string CodiceSortParm { get; set; } = string.Empty;
        public string NomeSortParm { get; set; } = string.Empty;
        public string CognomeSortParm { get; set; } = string.Empty;
        public string EmailSortParm { get; set; } = string.Empty;
        public string AttivoSortParm { get; set; } = string.Empty;
        public string LivelloSortParm { get; set; } = string.Empty;
        public string DataAssunzioneSortParm { get; set; } = string.Empty;

        // Statistiche
        public int TotaleOperatori { get; set; }
        public int OperatoriAttivi { get; set; }
        public int OperatoriInattivi { get; set; }
        public int OperatoriConEmail { get; set; }
        public int OperatoriConTelefono { get; set; }
        public double LivelloCompetenzaMedia { get; set; }
    }

    /// <summary>
    /// ViewModel per i dettagli di un operatore
    /// </summary>
    public class OperatoreDetailsViewModel
    {
        public Operatore Operatore { get; set; } = new Operatore();

        // Statistiche aggiuntive
        public int AnniServizio => Operatore.DataAssunzione.HasValue ? DateTime.Now.Year - Operatore.DataAssunzione.Value.Year : 0;
        public int MesiServizio => Operatore.DataAssunzione.HasValue ? ((DateTime.Now.Year - Operatore.DataAssunzione.Value.Year) * 12) + DateTime.Now.Month - Operatore.DataAssunzione.Value.Month : 0;
        public bool AssuntoRecentemente => Operatore.DataAssunzione?.AddMonths(6) > DateTime.Now;

        // Statistiche ordini di produzione
        public int OrdiniProduzioneAssegnati { get; set; }
        public int OrdiniProduzioneAttivi { get; set; }
        public int OrdiniProduzioneCompletati { get; set; }
        public DateTime? UltimoOrdineAssegnato { get; set; }

        // Navigazione
        public int? PreviousId { get; set; }
        public int? NextId { get; set; }
    }

    /// <summary>
    /// ViewModel per la conferma di eliminazione
    /// </summary>
    public class DeleteOperatoreViewModel
    {
        public Operatore Operatore { get; set; } = new Operatore();

        [Required(ErrorMessage = "È necessario confermare l'eliminazione")]
        [Display(Name = "Confermo l'eliminazione")]
        public bool ConfirmDelete { get; set; }

        // Informazioni aggiuntive per la decisione
        public bool HasOrdiniProduzione { get; set; }
        public int OrdiniProduzioneCount { get; set; }
        public string? OrdiniProduzioneDescription { get; set; }
    }

    /// <summary>
    /// ViewModel per le statistiche degli operatori
    /// </summary>
    public class OperatoriStatsViewModel
    {
        public int TotaleOperatori { get; set; }
        public int OperatoriAttivi { get; set; }
        public int OperatoriInattivi { get; set; }
        public int OperatoriConEmail { get; set; }
        public int OperatoriConTelefono { get; set; }
        public int OperatoriConDataAssunzione { get; set; }

        public double PercentualeAttivi => TotaleOperatori > 0 ? (double)OperatoriAttivi / TotaleOperatori * 100 : 0;
        public double PercentualeConEmail => TotaleOperatori > 0 ? (double)OperatoriConEmail / TotaleOperatori * 100 : 0;
        public double PercentualeConTelefono => TotaleOperatori > 0 ? (double)OperatoriConTelefono / TotaleOperatori * 100 : 0;

        public double LivelloCompetenzaMedia { get; set; }
        public int LivelloCompetenzaMinimo { get; set; }
        public int LivelloCompetenzaMassimo { get; set; }

        public double AnniServizioMedio { get; set; }
        public DateTime? DataAssunzionePiuRecente { get; set; }
        public DateTime? DataAssunzionePiuAntica { get; set; }

        // Distribuzione per livello di competenza
        public Dictionary<int, int> DistribuzioneLivelli { get; set; } = new Dictionary<int, int>();

        // Operatori più recenti e più esperti
        public IEnumerable<OperatoreFrequencyViewModel> OperatoriPiuRecenti { get; set; } = new List<OperatoreFrequencyViewModel>();
        public IEnumerable<OperatoreFrequencyViewModel> OperatoriPiuEsperti { get; set; } = new List<OperatoreFrequencyViewModel>();
        public IEnumerable<OperatoreFrequencyViewModel> OperatoriPiuAttivi { get; set; } = new List<OperatoreFrequencyViewModel>();
    }

    /// <summary>
    /// ViewModel per la frequenza di utilizzo degli operatori
    /// </summary>
    public class OperatoreFrequencyViewModel
    {
        public int IdOperatore { get; set; }
        public string CodiceOperatore { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;
        public string NomeCompleto => $"{Nome} {Cognome}";
        public int FrequenzaUtilizzo { get; set; }
        public DateTime? UltimoUtilizzo { get; set; }
        public int? LivelloCompetenza { get; set; }
        public DateTime? DataAssunzione { get; set; }
        public int AnniServizio => DataAssunzione.HasValue ? DateTime.Now.Year - DataAssunzione.Value.Year : 0;
    }

    /// <summary>
    /// ViewModel per l'API degli operatori (per dropdown, autocomplete, etc.)
    /// </summary>
    public class OperatoreApiViewModel
    {
        public int Id { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;
        public string NomeCompleto => $"{Nome} {Cognome}";
        public bool Attivo { get; set; }
        public int? LivelloCompetenza { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string DisplayText => $"{Codice} - {NomeCompleto}";
        public string DisplayTextWithLevel => LivelloCompetenza.HasValue ? $"{Codice} - {NomeCompleto} (Liv. {LivelloCompetenza})" : DisplayText;
    }

    /// <summary>
    /// ViewModel per il riepilogo operatori per dashboard
    /// </summary>
    public class OperatoriSummaryViewModel
    {
        public int TotaleOperatori { get; set; }
        public int OperatoriAttivi { get; set; }
        public int NuoviOperatoriUltimoMese { get; set; }
        public double LivelloCompetenzaMedia { get; set; }
        public int OperatoriLivelloEsperto { get; set; } // Livello 4-5
        public int OperatoriInFormazioneOrdini { get; set; } // Con ordini attivi
    }

    /// <summary>
    /// ViewModel per la ricerca avanzata operatori
    /// </summary>
    public class OperatoriSearchViewModel
    {
        [Display(Name = "Ricerca Generale")]
        public string? GeneralSearch { get; set; }

        [Display(Name = "Codice Operatore")]
        public string? CodiceOperatore { get; set; }

        [Display(Name = "Nome")]
        public string? Nome { get; set; }

        [Display(Name = "Cognome")]
        public string? Cognome { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Telefono")]
        public string? Telefono { get; set; }

        [Display(Name = "Livello Competenza Minimo")]
        public int? LivelloCompetenzaMin { get; set; }

        [Display(Name = "Livello Competenza Massimo")]
        public int? LivelloCompetenzaMax { get; set; }

        [Display(Name = "Assunto dal")]
        [DataType(DataType.Date)]
        public DateTime? DataAssunzioneDa { get; set; }

        [Display(Name = "Assunto fino al")]
        [DataType(DataType.Date)]
        public DateTime? DataAssunzioneA { get; set; }

        [Display(Name = "Solo Attivi")]
        public bool? SoloAttivi { get; set; }

        [Display(Name = "Con Email")]
        public bool? ConEmail { get; set; }

        [Display(Name = "Con Telefono")]
        public bool? ConTelefono { get; set; }

        [Display(Name = "Con Ordini Assegnati")]
        public bool? ConOrdiniAssegnati { get; set; }
    }
}
