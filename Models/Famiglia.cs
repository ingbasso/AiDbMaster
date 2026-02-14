using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella delle famiglie articoli.
    /// Rappresenta le famiglie/categorie utilizzate per raggruppare gli articoli.
    /// </summary>
    [Table("TabellaFamiglie")]
    public class Famiglia
    {
        /// <summary>
        /// ID auto-incrementale - Chiave primaria
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Codice alfanumerico della famiglia (max 4 caratteri)
        /// </summary>
        [Required(ErrorMessage = "Il codice famiglia è obbligatorio")]
        [StringLength(4, ErrorMessage = "Il codice famiglia non può superare i 4 caratteri")]
        [Column(TypeName = "varchar(4)")]
        [Display(Name = "Codice Famiglia")]
        public string CodiceFamiglia { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione della famiglia
        /// </summary>
        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Column(TypeName = "varchar(50)")]
        [Display(Name = "Descrizione Famiglia")]
        public string? DescrizioneFamiglia { get; set; }

        /// <summary>
        /// Data e ora dell'ultimo aggiornamento del record
        /// </summary>
        [Display(Name = "Ultimo Aggiornamento")]
        public DateTime UltimoAggiornamento { get; set; } = DateTime.Now;
    }
}
