using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("CantiereContabilitaRighe")]
    public class CantiereContabilitaRiga
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("ContabilitaId")]
        public int ContabilitaId { get; set; }

        public virtual CantiereContabilita Contabilita { get; set; } = null!;

        [Column("PadreId")]
        public int? PadreId { get; set; }

        public virtual CantiereContabilitaRiga? Padre { get; set; }

        public virtual ICollection<CantiereContabilitaRiga> Figli { get; set; } = new List<CantiereContabilitaRiga>();

        [Column("Ordine")]
        public int Ordine { get; set; }

        /// <summary>
        /// Riga di posa: quantità uguale al padre, prezzo vendita = padre − altri figli.
        /// </summary>
        [Column("IsPosa")]
        public bool IsPosa { get; set; }

        /// <summary>
        /// Sconto sull'intera contabilità: non ha figli né posa.
        /// L'importo è positivo in cella e viene sottratto dal totale vendita.
        /// </summary>
        [Column("IsSconto")]
        public bool IsSconto { get; set; }

        /// <summary>
        /// Articolo singolo: una riga sola, senza materiale e senza posa.
        /// Quantità, costo, ricarico e prezzo di vendita si scrivono a mano.
        /// </summary>
        [Column("IsArticoloSingolo")]
        public bool IsArticoloSingolo { get; set; }

        /// <summary>
        /// Se è vero, la riga padre o l'articolo singolo compare nella proforma.
        /// I figli e la posa non vanno in stampa in ogni caso.
        /// </summary>
        [Column("MostraInProforma")]
        public bool MostraInProforma { get; set; } = true;

        [StringLength(50)]
        [Column("CodiceArticolo")]
        public string? CodiceArticolo { get; set; }

        [StringLength(255)]
        [Column("Descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [StringLength(10)]
        [Column("UnitaMisura")]
        public string? UnitaMisura { get; set; }

        [Column("QuantitaPosata", TypeName = "decimal(18,4)")]
        public decimal? QuantitaPosata { get; set; }

        [Column("CostoMaterialeServizio", TypeName = "decimal(18,4)")]
        public decimal? CostoMaterialeServizio { get; set; }

        /// <summary>
        /// Percentuale di ricarico sul costo, usata solo dai figli diversi dalla posa.
        /// </summary>
        [Column("RicaricoPercentuale", TypeName = "decimal(9,4)")]
        public decimal? RicaricoPercentuale { get; set; }

        /// <summary>
        /// Prezzo unitario di vendita. Sul padre e sulla posa è riferito alla loro quantità.
        /// Sugli altri figli è il costo (con ricarico) ripartito sulla quantità del padre.
        /// </summary>
        [Column("PrezzoVenditaCliente", TypeName = "decimal(18,4)")]
        public decimal? PrezzoVenditaCliente { get; set; }

        [StringLength(200)]
        [Column("Note")]
        public string? Note { get; set; }
    }
}
