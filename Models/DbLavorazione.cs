using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("DB_Lavorazioni")]
    public class DbLavorazione
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string CodiceDistinta { get; set; } = " ";

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal RigaCiclo { get; set; }

        [Required]
        [Column(TypeName = "smallint")]
        public short CodiceReparto { get; set; }

        [Required]
        [Column(TypeName = "smallint")]
        public short CodiceCentro { get; set; }

        [Required]
        [Column(TypeName = "smallint")]
        public short Fase { get; set; }

        [Required]
        [Column(TypeName = "smallint")]
        public short CodiceLavorazione { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal TempoAttrezzaggioCentro { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal TempoEsecuzioneCentro { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal TempoAttrezzaggioManodopera { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal TempoEsecuzioneManodopera { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal PerPezzi { get; set; } = 1;

        [Required]
        public int TavolePerStack { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal TavoleOraTeoriche { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal Efficienza { get; set; } = 100;

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal TavoleOraReali { get; set; }

        public string? Note { get; set; }

        [Required]
        [Column("DataInizioValidità")]
        public DateTime DataInizioValidita { get; set; }

        [Required]
        [Column("DataFineValidità")]
        public DateTime DataFineValidita { get; set; }

        [Required]
        public DateTime UltimoAggiornamento { get; set; } = DateTime.Now;

        // Proprietà di navigazione
        [ForeignKey("CodiceLavorazione")]
        public virtual Lavorazioni? Lavorazione { get; set; }
    }
}
