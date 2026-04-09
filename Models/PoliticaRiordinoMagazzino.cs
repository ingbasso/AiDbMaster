using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("PoliticheRiordinoMagazzino")]
    public class PoliticaRiordinoMagazzino
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string CodiceArticolo { get; set; } = " ";

        [Required]
        [Column(TypeName = "smallint")]
        public short CodiceMagazzino { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal ScortaMinima { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal ScortaMassima { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal LottoStandardProduzione { get; set; }

        [Required]
        [StringLength(50)]
        public string PoliticaDiRiordino { get; set; } = "G";

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal Sottolotto { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal LottoMassimo { get; set; }

        [Required]
        [Column(TypeName = "smallint")]
        public short GiorniRaggruppamento { get; set; }

        [Required]
        [StringLength(20)]
        public string PeriodoRagruppamento { get; set; } = "G";

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal ScortaDiSicurezza { get; set; }

        [Required]
        public DateTime UltimoAggiornamento { get; set; } = DateTime.Now;
    }
}
