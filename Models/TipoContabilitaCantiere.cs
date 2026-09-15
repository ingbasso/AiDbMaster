using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.Models
{
    public enum TipoContabilitaCantiere
    {
        [Display(Name = "Iniziale")]
        Iniziale = 0,

        [Display(Name = "Finale")]
        Finale = 1
    }
}
