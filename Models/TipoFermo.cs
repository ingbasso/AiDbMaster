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
        Festivo = 1
    }
}


