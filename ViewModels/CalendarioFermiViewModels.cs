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
    /// ViewModel per generare fermi settimanali (turni notturni + weekend)
    /// </summary>
    public class GeneraFermiSettimanaliViewModel
    {
        [Required(ErrorMessage = "L'anno è obbligatorio")]
        [Range(2020, 2100, ErrorMessage = "Anno deve essere tra 2020 e 2100")]
        [Display(Name = "Anno")]
        public int Anno { get; set; } = DateTime.Now.Year;

        [Required(ErrorMessage = "Da Settimana è obbligatorio")]
        [Range(1, 53, ErrorMessage = "Da Settimana deve essere tra 1 e 53")]
        [Display(Name = "Da Settimana")]
        public int DaSettimana { get; set; } = 1;

        [Required(ErrorMessage = "A Settimana è obbligatorio")]
        [Range(1, 53, ErrorMessage = "A Settimana deve essere tra 1 e 53")]
        [Display(Name = "A Settimana")]
        public int ASettimana { get; set; } = 1;

        [Display(Name = "Applica a tutti i centri")]
        public bool ApplicaATutti { get; set; } = true;

        [Display(Name = "Codice Centro")]
        public string? CodiceCentro { get; set; }

        [Display(Name = "Motivo")]
        public string? Motivo { get; set; } = "Fermo programmato";
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

