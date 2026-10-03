using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Costo unitario e prezzo medio di vendita di un articolo,
    /// usati come proposta nella contabilità del cantiere.
    /// </summary>
    [Table("CostiArticoliCantiere")]
    public class CostoArticoloCantiere
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("CodiceArticolo")]
        public string CodiceArticolo { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("Descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [Column("CostoUnitario", TypeName = "decimal(18,4)")]
        public decimal CostoUnitario { get; set; }

        [Column("PrezzoMedioVendita", TypeName = "decimal(18,4)")]
        public decimal PrezzoMedioVendita { get; set; }

        [Column("DataUltimaModifica")]
        public DateTime DataUltimaModifica { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Column("UtenteModifica")]
        public string? UtenteModifica { get; set; }
    }
}
