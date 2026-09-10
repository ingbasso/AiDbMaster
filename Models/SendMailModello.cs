using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello della tabella SendMailModelli nel database GSTMAIL_FAVARO1.
    /// Contiene i modelli/template delle email da inviare.
    /// </summary>
    [Table("SendMailModelli")]
    public class SendMailModello
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("sm_tipo")]
        [Display(Name = "Tipo")]
        public int Tipo { get; set; }

        [Required]
        [StringLength(255)]
        [Column("sm_descrtipo", TypeName = "varchar(255)")]
        [Display(Name = "Descrizione tipo")]
        public string DescrizioneTipo { get; set; } = string.Empty;

        [Column("sm_attivo")]
        [Display(Name = "Attivo")]
        public bool Attivo { get; set; }

        [Column("sm_stored", TypeName = "nvarchar(max)")]
        [Display(Name = "Stored procedure")]
        public string? Stored { get; set; }

        [Required]
        [StringLength(255)]
        [Column("sm_profileName", TypeName = "varchar(255)")]
        [Display(Name = "Profilo")]
        public string ProfileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("sm_mailFROM", TypeName = "varchar(255)")]
        [Display(Name = "Mittente")]
        public string MailFrom { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("sm_mailto", TypeName = "varchar(255)")]
        [Display(Name = "Destinatario")]
        public string MailTo { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("sm_mailtocc", TypeName = "varchar(255)")]
        [Display(Name = "CC")]
        public string? MailToCc { get; set; }

        [StringLength(255)]
        [Column("sm_mailtobcc", TypeName = "varchar(255)")]
        [Display(Name = "BCC")]
        public string? MailToBcc { get; set; }

        [Required]
        [StringLength(255)]
        [Column("sm_mailsubj", TypeName = "varchar(255)")]
        [Display(Name = "Oggetto")]
        public string MailSubject { get; set; } = string.Empty;

        [Column("sm_numcol")]
        [Display(Name = "Numero colonne")]
        public int NumColonne { get; set; }

        [Required]
        [Column("sm_headermess", TypeName = "text")]
        [Display(Name = "Intestazione messaggio")]
        public string HeaderMessaggio { get; set; } = string.Empty;

        [Column("sm_headertab", TypeName = "text")]
        [Display(Name = "Intestazione tabella")]
        public string? HeaderTabella { get; set; }

        [Required]
        [Column("sm_footermess", TypeName = "text")]
        [Display(Name = "Piè di pagina")]
        public string FooterMessaggio { get; set; } = string.Empty;

        [Column("sm_attachment", TypeName = "text")]
        [Display(Name = "Allegato")]
        public string? Attachment { get; set; }

        [Column("sm_specialAddress")]
        [Display(Name = "Indirizzo speciale")]
        public bool SpecialAddress { get; set; }

        [Required]
        [StringLength(100)]
        [Column("sm_GruppoInvio", TypeName = "varchar(100)")]
        [Display(Name = "Gruppo invio")]
        public string GruppoInvio { get; set; } = string.Empty;
    }
}
