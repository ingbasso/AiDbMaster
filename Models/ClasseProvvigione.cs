using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella delle classi provvigioni.
    /// Rappresenta le classi di sconto/provvigione utilizzate nel sistema.
    /// </summary>
    [Table("TabellaClassiProvvigioni")]
    public class ClasseProvvigione
    {
        /// <summary>
        /// ID auto-incrementale - Chiave primaria
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Codice numerico della classe provvigione
        /// </summary>
        [Required(ErrorMessage = "Il codice classe è obbligatorio")]
        [Display(Name = "Codice Classe")]
        public short CodiceClasse { get; set; }

        /// <summary>
        /// Descrizione della classe provvigione
        /// </summary>
        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Classe")]
        public string? DescrizioneClasse { get; set; }

        /// <summary>
        /// Percentuale di sconto/provvigione associata alla classe
        /// </summary>
        [Required(ErrorMessage = "La percentuale di sconto è obbligatoria")]
        [Column(TypeName = "decimal(27,9)")]
        [Display(Name = "% Sconto")]
        public decimal Perc_Sconto { get; set; }

        /// <summary>
        /// Data e ora dell'ultimo aggiornamento del record
        /// </summary>
        [Display(Name = "Ultimo Aggiornamento")]
        public DateTime UltimoAggiornamento { get; set; } = DateTime.Now;
    }
}
