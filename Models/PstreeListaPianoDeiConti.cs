using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Pstree_ListaPianoDeiConti")]
    public class PstreeListaPianoDeiConti
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(20)]
        [Display(Name = "Codice PdC")]
        public string CodicePdC { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Descrizione PdC")]
        public string DescrizionePdC { get; set; } = string.Empty;

        [Required]
        [StringLength(1)]
        [Display(Name = "Tipo PdC")]
        public string TipoPdC { get; set; } = string.Empty;

        [Display(Name = "Non Associare")]
        public bool NonAssociare { get; set; }

        [NotMapped]
        [Display(Name = "Conto")]
        public string DescrizioneCompleta => $"{CodicePdC} - {DescrizionePdC}";

        [NotMapped]
        [Display(Name = "Tipo Conto")]
        public string TipoDescrizione => TipoPdC == "P" ? "Patrimoniale" : "Economico";
    }
}
