using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello della tabella SendMailStorico nel database GSTMAIL_FAVARO1.
    /// Contiene lo storico delle email già elaborate o da elaborare.
    /// </summary>
    [Table("SendMailStorico")]
    public class SendMailStorico
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
        [StringLength(255)]
        [Column("sm_tipoproc", TypeName = "varchar(255)")]
        [Display(Name = "Tipo processo")]
        public string TipoProcesso { get; set; } = string.Empty;

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

        [Required]
        [StringLength(255)]
        [Column("sm_mailsubj", TypeName = "varchar(255)")]
        [Display(Name = "Oggetto")]
        public string MailSubject { get; set; } = string.Empty;

        [Column("sm_mailbody", TypeName = "text")]
        [Display(Name = "Corpo email")]
        public string? MailBody { get; set; }

        [Column("sm_mailimp")]
        [Display(Name = "Importanza")]
        public int MailImportanza { get; set; }

        [StringLength(255)]
        [Column("sm_mailcc", TypeName = "varchar(255)")]
        [Display(Name = "CC")]
        public string? MailCc { get; set; }

        [StringLength(255)]
        [Column("sm_mailbcc", TypeName = "varchar(255)")]
        [Display(Name = "BCC")]
        public string? MailBcc { get; set; }

        [StringLength(255)]
        [Column("sm_mailattch", TypeName = "varchar(255)")]
        [Display(Name = "Allegato")]
        public string? MailAttachment { get; set; }

        [Column("sm_mailhtml")]
        [Display(Name = "HTML")]
        public int MailHtml { get; set; }

        [Required]
        [StringLength(1)]
        [Column("sm_stato", TypeName = "varchar(1)")]
        [Display(Name = "Stato")]
        public string Stato { get; set; } = string.Empty;

        [Column("sm_dataprev", TypeName = "datetime")]
        [Display(Name = "Data prevista")]
        public DateTime DataPrevista { get; set; }

        [Column("sm_dataesec", TypeName = "datetime")]
        [Display(Name = "Data esecuzione")]
        public DateTime? DataEsecuzione { get; set; }

        [Column("sm_note", TypeName = "text")]
        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Column("sm_tipo")]
        [Display(Name = "Tipo")]
        public int Tipo { get; set; }
    }
}
