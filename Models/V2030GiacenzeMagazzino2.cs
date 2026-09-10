using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Vista GSTMAIL_FAVARO1.V2030_Giacenze_Magazzino_2.
    /// Giacenze presenti nel magazzino 2.
    /// </summary>
    public class V2030GiacenzeMagazzino2
    {
        [Column("codditt", TypeName = "varchar(12)")]
        public string CodiceDitta { get; set; } = string.Empty;

        [Column("ap_codart", TypeName = "varchar(50)")]
        public string CodiceArticolo { get; set; } = string.Empty;

        [Column("ar_descr", TypeName = "varchar(255)")]
        public string Descrizione { get; set; } = string.Empty;

        [Column("ap_esist", TypeName = "decimal(27, 9)")]
        public decimal Esistenza { get; set; }

        [Column("tb_descfam", TypeName = "varchar(50)")]
        public string? Famiglia { get; set; }

        [Column("DataUltimoCarico", TypeName = "datetime")]
        public DateTime? DataUltimoCarico { get; set; }
    }
}
