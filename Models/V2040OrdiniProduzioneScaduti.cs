using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Vista GSTMAIL_FAVARO1.V2040_OrdiniProduzioneScaduti.
    /// Ordini di produzione con data consegna già scaduta.
    /// </summary>
    public class V2040OrdiniProduzioneScaduti
    {
        [Column("codditt", TypeName = "varchar(12)")]
        public string CodiceDitta { get; set; } = string.Empty;

        [Column("mo_anno")]
        public short AnnoOrdine { get; set; }

        [Column("mo_serie", TypeName = "varchar(3)")]
        public string SerieOrdine { get; set; } = string.Empty;

        [Column("mo_numord")]
        public int NumeroOrdine { get; set; }

        [Column("mo_riga")]
        public int Riga { get; set; }

        [Column("mo_codart", TypeName = "varchar(50)")]
        public string CodiceArticolo { get; set; } = string.Empty;

        [Column("mo_descr", TypeName = "nvarchar(255)")]
        public string? Descrizione { get; set; }

        [Column("mo_quant", TypeName = "decimal(27, 9)")]
        public decimal Quantita { get; set; }

        [Column("mo_quaeva", TypeName = "decimal(27, 9)")]
        public decimal QuantitaEvasa { get; set; }

        [Column("mo_datcons", TypeName = "datetime")]
        public DateTime DataConsegna { get; set; }
    }
}
