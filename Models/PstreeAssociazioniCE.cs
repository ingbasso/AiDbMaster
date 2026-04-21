using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Pstree_AssociazioniCE")]
    public class PstreeAssociazioniCE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Codice PdC")]
        public string CodicePdC { get; set; } = string.Empty;

        [Display(Name = "ID Codice Conto")]
        public int IdCodiceConto { get; set; }

        [ForeignKey("CodicePdC")]
        public virtual PstreeListaPianoDeiConti? PianoDeiConti { get; set; }

        [ForeignKey("IdCodiceConto")]
        public virtual PstreeStrutturaContoEconomico? ContoEconomico { get; set; }
    }
}
