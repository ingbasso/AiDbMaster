using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("CantiereOrdini")]
    public class CantiereOrdine
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
        [Column("OrdineTestataId")]
        public int OrdineTestataId { get; set; }

        [ForeignKey(nameof(OrdineTestataId))]
        public virtual OrdiniTestate Ordine { get; set; } = null!;

        [Column("DataCollegamento")]
        public DateTime DataCollegamento { get; set; } = DateTime.Now;
    }
}
