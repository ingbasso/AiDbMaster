using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella delle marche.
    /// Rappresenta le marche/brand degli articoli nel sistema.
    /// </summary>
    [Table("TabellaMarche")]
    public class Marca
    {
        /// <summary>
        /// ID auto-incrementale - Chiave primaria
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Codice numerico della marca
        /// </summary>
        [Required(ErrorMessage = "Il codice marca è obbligatorio")]
        [Display(Name = "Codice Marca")]
        public short CodiceMarca { get; set; }

        /// <summary>
        /// Descrizione della marca
        /// </summary>
        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Column(TypeName = "varchar(50)")]
        [Display(Name = "Descrizione Marca")]
        public string? DescrizioneMarca { get; set; }

        /// <summary>
        /// Data e ora dell'ultimo aggiornamento del record
        /// </summary>
        [Display(Name = "Ultimo Aggiornamento")]
        public DateTime UltimoAggiornamento { get; set; } = DateTime.Now;
    }
}
