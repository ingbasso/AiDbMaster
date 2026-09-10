using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello della tabella SendMailTempInvio nel database GSTMAIL_FAVARO1.
    /// Coda temporanea delle email in attesa di invio.
    /// </summary>
    [Table("SendMailTempInvio")]
    public class SendMailTempInvio
    {
        [Required]
        [StringLength(12)]
        [Column("codazi", TypeName = "varchar(12)")]
        [Display(Name = "Codice azienda")]
        public string CodiceAzienda { get; set; } = string.Empty;

        [Required]
        [StringLength(12)]
        [Column("codditt", TypeName = "varchar(12)")]
        [Display(Name = "Codice ditta")]
        public string CodiceDitta { get; set; } = string.Empty;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("sm_progr")]
        [Display(Name = "Progressivo")]
        public int Progressivo { get; set; }

        [Required]
        [Column("sm_c1", TypeName = "varchar(max)")]
        [Display(Name = "Contenuto")]
        public string Contenuto { get; set; } = string.Empty;

        [Column("sm_datains", TypeName = "datetime")]
        [Display(Name = "Data inserimento")]
        public DateTime? DataInserimento { get; set; }

        [Column("sm_tipo")]
        [Display(Name = "Tipo")]
        public int? Tipo { get; set; }

        [Required]
        [StringLength(255)]
        [Column("sm_email", TypeName = "varchar(255)")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("sm_profileName", TypeName = "varchar(255)")]
        [Display(Name = "Profilo")]
        public string ProfileName { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("sm_mailFROM", TypeName = "varchar(255)")]
        [Display(Name = "Mittente")]
        public string? MailFrom { get; set; }

        [StringLength(255)]
        [Column("sm_mailto", TypeName = "varchar(255)")]
        [Display(Name = "Destinatario")]
        public string? MailTo { get; set; }

        [StringLength(255)]
        [Column("sm_mailcc", TypeName = "varchar(255)")]
        [Display(Name = "CC")]
        public string? MailCc { get; set; }

        [StringLength(255)]
        [Column("sm_mailbcc", TypeName = "varchar(255)")]
        [Display(Name = "BCC")]
        public string? MailBcc { get; set; }

        [StringLength(255)]
        [Column("sm_mailsubj", TypeName = "varchar(255)")]
        [Display(Name = "Oggetto")]
        public string? MailSubject { get; set; }
    }
}
