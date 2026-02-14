using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella TabellaOpzioni.
    /// Tabella di configurazione chiave-valore per le opzioni di sistema.
    /// Ogni record rappresenta un'opzione con il suo nome e valore.
    /// </summary>
    [Table("TabellaOpzioni")]
    public class Opzione
    {
        /// <summary>
        /// ID auto-incrementale - Chiave primaria
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { get; set; }

        /// <summary>
        /// Nome dell'opzione (chiave univoca di configurazione).
        /// Es: "SmtpServer", "SmtpPort", "EmailSender", "GiorniScadenzaMerce", ecc.
        /// </summary>
        [Required]
        [StringLength(255)]
        [Display(Name = "Nome Opzione")]
        [Column("NomeOpzione", TypeName = "varchar(255)")]
        public string NomeOpzione { get; set; } = string.Empty;

        /// <summary>
        /// Valore dell'opzione (può contenere testi lunghi).
        /// Es: "mail.favaro1.com", "587", "21", ecc.
        /// </summary>
        [Required]
        [Display(Name = "Valore Opzione")]
        [Column("ValoreOpzione", TypeName = "varchar(max)")]
        public string ValoreOpzione { get; set; } = string.Empty;
    }
}
