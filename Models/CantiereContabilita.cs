using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("CantiereContabilita")]
    public class CantiereContabilita
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("CantiereId")]
        public int CantiereId { get; set; }

        public virtual Cantiere Cantiere { get; set; } = null!;

        [Column("Tipo")]
        public TipoContabilitaCantiere Tipo { get; set; }

        [Column("AliquotaIva", TypeName = "decimal(5,2)")]
        public decimal AliquotaIva { get; set; } = 22;

        [Column("ImponibileAcconto", TypeName = "decimal(18,2)")]
        public decimal ImponibileAcconto { get; set; }

        [StringLength(150)]
        [Column("RiferimentoOrdine")]
        public string? RiferimentoOrdine { get; set; }

        [Column("Note")]
        public string? Note { get; set; }

        [Column("DataUltimaModifica")]
        public DateTime? DataUltimaModifica { get; set; }

        public virtual ICollection<CantiereContabilitaRiga> Righe { get; set; } = new List<CantiereContabilitaRiga>();
    }
}
