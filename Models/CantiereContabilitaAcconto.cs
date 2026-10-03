using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("CantiereContabilitaAcconti")]
    public class CantiereContabilitaAcconto
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("ContabilitaId")]
        public int ContabilitaId { get; set; }

        public virtual CantiereContabilita Contabilita { get; set; } = null!;

        [Column("Ordine")]
        public int Ordine { get; set; }

        [StringLength(100)]
        [Column("Descrizione")]
        public string? Descrizione { get; set; }

        /// <summary>Importo dell'acconto al netto dell'IVA.</summary>
        [Column("Imponibile", TypeName = "decimal(18,2)")]
        public decimal Imponibile { get; set; }
    }
}
