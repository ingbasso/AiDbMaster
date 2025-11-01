using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Rappresenta i tempi di asciugatura (maturazione) dei prodotti per ogni mese dell'anno.
    /// Utilizzata per calcolare quando il materiale prodotto diventa vendibile.
    /// </summary>
    [Table("TempiAsciugatura")]
    public class TempiAsciugatura
    {
        /// <summary>
        /// Identificativo del mese (1-12)
        /// </summary>
        [Key]
        [Column("IdMese")]
        public int IdMese { get; set; }

        /// <summary>
        /// Nome del mese (es. Gennaio, Febbraio, ecc.)
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("Mese")]
        public string Mese { get; set; } = string.Empty;

        /// <summary>
        /// Numero di giorni necessari per l'asciugatura/maturazione del prodotto
        /// </summary>
        [Required]
        [Column("GiorniAsciugatura")]
        public int GiorniAsciugatura { get; set; }
    }
}

