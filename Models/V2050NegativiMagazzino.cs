using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Vista GSTMAIL_FAVARO1.V2050_NegativiMagazzino.
    /// Articoli con esistenza negativa.
    /// </summary>
    public class V2050NegativiMagazzino
    {
        [Column("codditt", TypeName = "varchar(12)")]
        public string CodiceDitta { get; set; } = string.Empty;

        [Column("ap_codart", TypeName = "varchar(50)")]
        public string CodiceArticolo { get; set; } = string.Empty;

        [Column("ar_descr", TypeName = "varchar(255)")]
        public string Descrizione { get; set; } = string.Empty;

        [Column("ap_magaz")]
        public short CodiceMagazzino { get; set; }

        [Column("ap_esist", TypeName = "decimal(27, 9)")]
        public decimal Esistenza { get; set; }

        [Column("tb_desmaga", TypeName = "varchar(50)")]
        public string? DescrizioneMagazzino { get; set; }

        [Column("ar_stainv", TypeName = "varchar(1)")]
        public string StatoInventario { get; set; } = string.Empty;
    }
}
