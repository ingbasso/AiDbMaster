using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("CantiereContabilitaRighe")]
    public class CantiereContabilitaRiga
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("ContabilitaId")]
        public int ContabilitaId { get; set; }

        public virtual CantiereContabilita Contabilita { get; set; } = null!;

        [Column("PadreId")]
        public int? PadreId { get; set; }

        public virtual CantiereContabilitaRiga? Padre { get; set; }

        public virtual ICollection<CantiereContabilitaRiga> Figli { get; set; } = new List<CantiereContabilitaRiga>();

        [Column("Ordine")]
        public int Ordine { get; set; }

        [StringLength(50)]
        [Column("CodiceArticolo")]
        public string? CodiceArticolo { get; set; }

        [StringLength(255)]
        [Column("Descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [StringLength(10)]
        [Column("UnitaMisura")]
        public string? UnitaMisura { get; set; }

        [Column("QuantitaPosata", TypeName = "decimal(18,4)")]
        public decimal? QuantitaPosata { get; set; }

        [Column("CostoMaterialeServizio", TypeName = "decimal(18,4)")]
        public decimal? CostoMaterialeServizio { get; set; }

        [Column("PrezzoVenditaCliente", TypeName = "decimal(18,4)")]
        public decimal? PrezzoVenditaCliente { get; set; }

        [StringLength(200)]
        [Column("Note")]
        public string? Note { get; set; }
    }
}
