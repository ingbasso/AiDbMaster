using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Indisponibilita")]
    public class Indisponibilita
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>Tipo di soggetto: "Autista" oppure "Mezzo".</summary>
        [Required]
        [StringLength(20)]
        [Display(Name = "Tipo")]
        [Column("Tipo")]
        public string Tipo { get; set; } = TipoIndisponibilita.Mezzo;

        [Display(Name = "Autista")]
        [Column("AutistaId")]
        public int? AutistaId { get; set; }

        [Display(Name = "Mezzo Interno")]
        [Column("MezzoTrasportoId")]
        public int? MezzoTrasportoId { get; set; }

        [Required]
        [Display(Name = "Data Inizio")]
        [Column("DataInizio")]
        public DateTime DataInizio { get; set; }

        [Required]
        [Display(Name = "Data Fine")]
        [Column("DataFine")]
        public DateTime DataFine { get; set; }

        /// <summary>Se true vale tutta la giornata; altrimenti si usa la fascia OraInizio-OraFine.</summary>
        [Required]
        [Display(Name = "Giornata Intera")]
        [Column("GiornoIntero")]
        public bool GiornoIntero { get; set; } = true;

        [Display(Name = "Ora Inizio")]
        [Column("OraInizio")]
        public TimeSpan? OraInizio { get; set; }

        [Display(Name = "Ora Fine")]
        [Column("OraFine")]
        public TimeSpan? OraFine { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Causale")]
        [Column("Causale")]
        public string Causale { get; set; } = string.Empty;

        [Display(Name = "Note")]
        [Column("Note")]
        public string? Note { get; set; }

        [Column("DataCreazione")]
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        [StringLength(450)]
        [Column("CreatoDa")]
        public string? CreatoDa { get; set; }

        [ForeignKey("AutistaId")]
        public virtual Autista? Autista { get; set; }

        [ForeignKey("MezzoTrasportoId")]
        public virtual MezzoTrasporto? MezzoTrasporto { get; set; }
    }

    public static class TipoIndisponibilita
    {
        public const string Autista = "Autista";
        public const string Mezzo = "Mezzo";
    }

    public static class CausaliIndisponibilita
    {
        public static readonly string[] PerAutista = { "Ferie", "Malattia", "Permesso", "Riposo", "Altro" };
        public static readonly string[] PerMezzo = { "Manutenzione", "Revisione", "Guasto", "Fermo", "Altro" };
    }
}
