using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella delle lavorazioni
    /// </summary>
    [Table("Lavorazioni")]
    public class Lavorazioni
    {
        /// <summary>
        /// Identificativo univoco della lavorazione (IDENTITY(1,1))
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdLavorazione { get; set; }

        /// <summary>
        /// Codice della lavorazione (varchar(1) NULL)
        /// </summary>
        [StringLength(1)]
        [Column(TypeName = "varchar(1)")]
        public string? CodiceLavorazione { get; set; }

        /// <summary>
        /// Descrizione della lavorazione (NOT NULL)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DescrizioneLavorazione { get; set; } = string.Empty;

        /// <summary>
        /// Indica se la lavorazione è attiva
        /// </summary>
        public bool Attivo { get; set; } = true;

        /// <summary>
        /// Data di creazione del record
        /// </summary>
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        /// <summary>
        /// Data ultima modifica
        /// </summary>
        public DateTime? DataUltimaModifica { get; set; }

        // Navigazione
        /// <summary>
        /// Lista degli ordini di produzione che utilizzano questa lavorazione
        /// </summary>
        public virtual ICollection<ListaOP> OrdiniProduzione { get; set; } = new List<ListaOP>();
    }
}
