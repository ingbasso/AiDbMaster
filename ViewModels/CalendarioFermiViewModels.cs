using System.ComponentModel.DataAnnotations;
using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la creazione/modifica di un fermo
    /// </summary>
    public class CalendarioFermoViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Il codice centro è obbligatorio")]
        [Display(Name = "Centro di Lavoro")]
        public string CodiceCentro { get; set; } = string.Empty;

        [Required(ErrorMessage = "La data inizio è obbligatoria")]
        [Display(Name = "Data Inizio")]
        public DateTime DataInizioFermo { get; set; } = DateTime.Now;

        [Display(Name = "Data Fine")]
        public DateTime? DataFineFermo { get; set; }

        [Required(ErrorMessage = "Il tipo fermo è obbligatorio")]
        [Display(Name = "Tipo Fermo")]
        public TipoFermo TipoFermo { get; set; }

        [StringLength(200, ErrorMessage = "Il motivo non può superare i 200 caratteri")]
        [Display(Name = "Motivo")]
        public string? Motivo { get; set; }

        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "Pianificato")]
        public bool IsPianificato { get; set; } = true;

        /// <summary>
        /// Indica se applicare il fermo a tutti i centri
        /// </summary>
        [Display(Name = "Applica a tutti i centri")]
        public bool ApplicaATuttiICentri { get; set; } = false;
    }

    /// <summary>
    /// ViewModel per Syncfusion Scheduler (formato JSON)
    /// </summary>
    public class SchedulerEventViewModel
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Description { get; set; }
        public bool IsAllDay { get; set; }
        public string CategoryColor { get; set; } = "#3788d8";
        public string CodiceCentro { get; set; } = string.Empty;
        public string DescrizioneCentro { get; set; } = string.Empty;
        public string TipoFermo { get; set; } = string.Empty;
        public bool IsPianificato { get; set; }
        public string StatoFermo { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel per generare weekend automaticamente
    /// </summary>
    public class GeneraWeekendViewModel
    {
        [Required(ErrorMessage = "La data inizio è obbligatoria")]
        [Display(Name = "Data Inizio Periodo")]
        public DateTime DataInizio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La data fine è obbligatoria")]
        [Display(Name = "Data Fine Periodo")]
        public DateTime DataFine { get; set; } = DateTime.Today.AddMonths(1);

        [Required(ErrorMessage = "Seleziona almeno un centro")]
        [Display(Name = "Centri di Lavoro")]
        public List<string> CentriSelezionati { get; set; } = new List<string>();

        [Display(Name = "Applica a tutti i centri")]
        public bool ApplicaATutti { get; set; } = false;

        [Display(Name = "Motivo")]
        public string Motivo { get; set; } = "Weekend";
    }

    /// <summary>
    /// Risposta API per operazioni CRUD
    /// </summary>
    public class FermoApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SchedulerEventViewModel? Data { get; set; }
        public List<SchedulerEventViewModel>? DataList { get; set; }
    }
}

