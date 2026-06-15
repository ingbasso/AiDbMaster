using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("ViaggioConsegnaDestinazioni")]
    public class ViaggioConsegnaDestinazione
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("ViaggioConsegnaId")]
        public int ViaggioConsegnaId { get; set; }

        [Required]
        [Column("CodiceCliente")]
        public int CodiceCliente { get; set; }

        [Column("CodiceDestinazione")]
        public int? CodiceDestinazione { get; set; }

        [Display(Name = "Gru")]
        [Column("Gru")]
        public bool Gru { get; set; } = false;

        [Display(Name = "Trasbordo")]
        [Column("Trasbordo")]
        public bool Trasbordo { get; set; } = false;

        [Display(Name = "Prezzo Vendita")]
        [Column("PrezzoVendita", TypeName = "decimal(18,2)")]
        public decimal PrezzoVendita { get; set; } = 0;

        [Display(Name = "Ordine Consegna")]
        [Column("OrdineConsegna")]
        public int OrdineConsegna { get; set; } = 0;

        [Column("Note")]
        public string? Note { get; set; }

        [ForeignKey("ViaggioConsegnaId")]
        public virtual ViaggioConsegna? ViaggioConsegna { get; set; }
    }
}
