using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("CantiereReferenti")]
    public class CantiereReferente
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("CantiereId")]
        public int CantiereId { get; set; }

        [ForeignKey(nameof(CantiereId))]
        public virtual Cantiere Cantiere { get; set; } = null!;

        [Required]
        [Column("ReferenteId")]
        public int ReferenteId { get; set; }

        [ForeignKey(nameof(ReferenteId))]
        public virtual ReferenteCantiere Referente { get; set; } = null!;

        [StringLength(80)]
        [Display(Name = "Ruolo")]
        [Column("Ruolo")]
        public string? Ruolo { get; set; }
    }
}
