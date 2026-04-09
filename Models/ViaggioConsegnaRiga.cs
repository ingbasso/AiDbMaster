using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("ViaggioConsegnaRighe")]
    public class ViaggioConsegnaRiga
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("ViaggioConsegnaId")]
        public int ViaggioConsegnaId { get; set; }

        [Required]
        [Column("OrdineRigaId")]
        public int OrdineRigaId { get; set; }

        [Required]
        [Display(Name = "Quantità Assegnata")]
        [Column("QuantitaAssegnata")]
        public decimal QuantitaAssegnata { get; set; }

        [Required]
        [Display(Name = "Peso Unitario Kg")]
        [Column("PesoUnitarioKgSnapshot")]
        public decimal PesoUnitarioKgSnapshot { get; set; }

        [Required]
        [Display(Name = "Peso Totale Kg")]
        [Column("PesoTotaleKgSnapshot")]
        public decimal PesoTotaleKgSnapshot { get; set; }

        [Column("NoteRiga")]
        public string? NoteRiga { get; set; }

        [Column("DataCreazione")]
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        [ForeignKey("ViaggioConsegnaId")]
        public virtual ViaggioConsegna? ViaggioConsegna { get; set; }

        [ForeignKey("OrdineRigaId")]
        public virtual OrdiniRighe? OrdineRiga { get; set; }
    }
}
