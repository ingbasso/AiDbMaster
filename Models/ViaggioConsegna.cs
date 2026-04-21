using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("ViaggiConsegna")]
    public class ViaggioConsegna
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data Consegna")]
        [Column("DataConsegna")]
        public DateTime DataConsegna { get; set; }

        [Required]
        [Display(Name = "Tipo Trasporto")]
        [Column("TipoTrasportoId")]
        public int TipoTrasportoId { get; set; }

        [Display(Name = "Mezzo Interno")]
        [Column("MezzoTrasportoId")]
        public int? MezzoTrasportoId { get; set; }

        [Display(Name = "Mezzo Esterno")]
        [Column("MezzoTrasportoEsternoId")]
        public int? MezzoTrasportoEsternoId { get; set; }

        [Required]
        [Display(Name = "Ora Partenza")]
        [Column("OraPartenza")]
        public TimeSpan OraPartenza { get; set; }

        [Display(Name = "Ora Arrivo")]
        [Column("OraArrivo")]
        public TimeSpan? OraArrivo { get; set; }

        [Required]
        [Display(Name = "Durata Stimata (min)")]
        [Column("DurataStimataMinuti")]
        public int DurataStimataMinuti { get; set; } = 240;

        [Required]
        [StringLength(20)]
        [Display(Name = "Stato")]
        [Column("Stato")]
        public string Stato { get; set; } = "Pianificato";

        [Display(Name = "Note")]
        [Column("Note")]
        public string? Note { get; set; }

        [Display(Name = "Autista")]
        [Column("AutistaId")]
        public int? AutistaId { get; set; }

        [Display(Name = "Costo Trasporto")]
        [Column("CostoTrasporto", TypeName = "decimal(18,2)")]
        public decimal? CostoTrasporto { get; set; }

        [Display(Name = "Prezzo Vendita")]
        [Column("PrezzoVendita", TypeName = "decimal(18,2)")]
        public decimal? PrezzoVendita { get; set; }

        [Display(Name = "Tempo Pausa (min)")]
        [Column("TempoPausa")]
        public int? TempoPausa { get; set; }

        [Display(Name = "Tempo Scarico (min)")]
        [Column("TempoScarico")]
        public int? TempoScarico { get; set; }

        [Display(Name = "Gru")]
        [Column("Gru")]
        public bool? Gru { get; set; }

        [Display(Name = "Trasbordo")]
        [Column("Trasbordo")]
        public bool? Trasbordo { get; set; }

        [Display(Name = "Con Rimorchio")]
        [Column("ConRimorchio")]
        public bool ConRimorchio { get; set; } = false;

        [Required]
        [Display(Name = "Spedizione Manuale")]
        [Column("IsManuale")]
        public bool IsManuale { get; set; } = false;

        [NotMapped]
        public decimal Margine => (PrezzoVendita ?? 0) - (CostoTrasporto ?? 0);

        [Column("DataCreazione")]
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        [StringLength(450)]
        [Column("CreatoDa")]
        public string? CreatoDa { get; set; }

        [ForeignKey("TipoTrasportoId")]
        public virtual TipoTrasporto? TipoTrasporto { get; set; }

        [ForeignKey("MezzoTrasportoId")]
        public virtual MezzoTrasporto? MezzoTrasporto { get; set; }

        [ForeignKey("MezzoTrasportoEsternoId")]
        public virtual MezzoTrasportoEsterno? MezzoTrasportoEsterno { get; set; }

        [ForeignKey("AutistaId")]
        public virtual Autista? Autista { get; set; }

        public virtual ICollection<ViaggioConsegnaRiga> Righe { get; set; } = new List<ViaggioConsegnaRiga>();

        [NotMapped]
        public TimeSpan OraArrivoEffettiva => OraArrivo ?? OraPartenza.Add(TimeSpan.FromMinutes(DurataStimataMinuti));
    }
}
