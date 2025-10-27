using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Tipo di fermo per i centri di lavoro
    /// </summary>
    public enum TipoFermo
    {
        /// <summary>
        /// Fermo per weekend (sabato/domenica)
        /// </summary>
        [Display(Name = "Week End")]
        WeekEnd = 0,

        /// <summary>
        /// Fermo per festività
        /// </summary>
        [Display(Name = "Festivo")]
        Festivo = 1,

        /// <summary>
        /// Fermo per turno notturno (22:00-06:00)
        /// </summary>
        [Display(Name = "Turno Notturno")]
        TurnoNotturno = 2
    }
}


