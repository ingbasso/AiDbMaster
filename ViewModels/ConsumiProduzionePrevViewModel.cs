using System.ComponentModel.DataAnnotations;
using System.Data;

namespace AiDbMaster.ViewModels
{
    public class ConsumiProduzionePrevViewModel
    {
        [Display(Name = "Da Data Produzione")]
        [DataType(DataType.Date)]
        public DateTime? DataDa { get; set; }

        [Display(Name = "A Data Produzione")]
        [DataType(DataType.Date)]
        public DateTime? DataA { get; set; }

        public DataTable? Risultati { get; set; }

        public bool RicercaEffettuata { get; set; }
    }
}
