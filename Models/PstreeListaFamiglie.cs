using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Pstree_ListaFamiglie")]
    public class PstreeListaFamiglie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Codice Famiglia")]
        public string CodiceFamiglia { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Nome Famiglia")]
        public string NomeFamiglia { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Descrizione Famiglia")]
        public string? DescrizioneFamiglia { get; set; }

        [Display(Name = "ID Codice Conto")]
        public int IdCodiceConto { get; set; }

        [Display(Name = "ID Famiglia Padre")]
        public int? IdFamigliaPadre { get; set; }

        [ForeignKey("IdCodiceConto")]
        public virtual PstreeStrutturaContoEconomico? ContoEconomico { get; set; }

        [ForeignKey("IdFamigliaPadre")]
        public virtual PstreeListaFamiglie? FamigliaPadre { get; set; }

        [NotMapped]
        [Display(Name = "Famiglia")]
        public string DescrizioneCompleta => $"{Id} - {NomeFamiglia}";

        [NotMapped]
        public bool IsFamigliaPrincipale => IdFamigliaPadre == null;

        [NotMapped]
        public bool IsSottoFamiglia => IdFamigliaPadre != null;
    }
}
