using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Tipo di fermo per i centri di lavoro
    /// </summary>
    public enum TipoFermo
    {
        /// <summary>
        /// Fermo per turno notturno (20:00-06:00)
        /// </summary>
        [Display(Name = "Fermo Notturno")]
        TurnoNotturno = 1,

        /// <summary>
        /// Fermo per weekend (sabato/domenica)
        /// </summary>
        [Display(Name = "Weekend")]
        WeekEnd = 2,

        /// <summary>
        /// Fermo per manutenzione programmata
        /// </summary>
        [Display(Name = "Manutenzione")]
        Manutenzione = 3,

        /// <summary>
        /// Fermo per festività
        /// </summary>
        [Display(Name = "Festivo")]
        Festivo = 4
    }
}


