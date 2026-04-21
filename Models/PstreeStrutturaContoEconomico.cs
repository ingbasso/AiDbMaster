using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Pstree_StrutturaContoEconomico")]
    public class PstreeStrutturaContoEconomico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdCodiceConto { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Descrizione Conto")]
        public string DescrizioneConto { get; set; } = string.Empty;

        [Required]
        [StringLength(1)]
        [Display(Name = "Tipo Conto")]
        public string TipoConto { get; set; } = string.Empty;

        [Display(Name = "Parent ID")]
        public int ParentId { get; set; }

        [Display(Name = "Ordine")]
        public int Ordine { get; set; }

        [Display(Name = "Livello")]
        public int Livello { get; set; }

        [Display(Name = "Voce Rettifica")]
        public bool VoceRettifica { get; set; }

        [Display(Name = "Voce Rimanenza")]
        public bool VoceRimanenza { get; set; }

        [Display(Name = "Gruppo Percentuale")]
        public int GruppoPercentuale { get; set; }

        [Required]
        [StringLength(1)]
        [Display(Name = "Costi F/D")]
        public string CostiFD { get; set; } = "N";

        [Display(Name = "Cash Flow Economico")]
        public bool CashFlowEconomico { get; set; }

        [NotMapped]
        public PstreeStrutturaContoEconomico? Parent { get; set; }

        [NotMapped]
        public ICollection<PstreeStrutturaContoEconomico>? Figli { get; set; }

        [NotMapped]
        public bool IsRadice => ParentId == 0;

        [NotMapped]
        public bool HasFigli => Figli?.Any() ?? false;

        [NotMapped]
        [Display(Name = "Descrizione Completa")]
        public string DescrizioneCompleta => $"{IdCodiceConto} - {DescrizioneConto}";

        [NotMapped]
        [Display(Name = "Tipo")]
        public string TipoContoDescrizione => TipoConto switch
        {
            "F" => "Foglia", "R" => "Ricavo", "C" => "Costo",
            "T" => "Totale", "S" => "Sottototale", _ => TipoConto
        };
    }
}
