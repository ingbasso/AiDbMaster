using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AiDbMaster.Models
{
    public enum CantiereStato
    {
        [Display(Name = "Preventivo")]
        Preventivo = 0,

        [Display(Name = "Confermato")]
        Confermato = 1,

        [Display(Name = "In fornitura")]
        InFornitura = 2,

        [Display(Name = "In posa")]
        InPosa = 3,

        [Display(Name = "Sospeso")]
        Sospeso = 4,

        [Display(Name = "Chiuso")]
        Chiuso = 5
    }

    public static class CantiereStatoExtensions
    {
        public static string GetDisplayName(this CantiereStato stato)
        {
            var member = typeof(CantiereStato).GetMember(stato.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? stato.ToString();
        }

        public static string GetBadgeCss(this CantiereStato stato)
        {
            return stato switch
            {
                CantiereStato.Preventivo => "badge bg-secondary",
                CantiereStato.Confermato => "badge bg-info text-dark",
                CantiereStato.InFornitura => "badge bg-primary",
                CantiereStato.InPosa => "badge bg-warning text-dark",
                CantiereStato.Sospeso => "badge bg-dark",
                CantiereStato.Chiuso => "badge bg-success",
                _ => "badge bg-secondary"
            };
        }
    }
}
