using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("MezziTrasportoInterni")]
    public class MezzoTrasporto
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Codice Mezzo")]
        [Column("CodiceMezzo")]
        public string CodiceMezzo { get; set; } = string.Empty;

        [StringLength(15)]
        [Display(Name = "Targa")]
        [Column("Targa")]
        public string? Targa { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Descrizione")]
        [Column("Descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [Display(Name = "Portata max (Kg)")]
        [Column("PortataMaxKg")]
        public decimal PortataMaxKg { get; set; }

        [Display(Name = "Rimorchio disponibile")]
        [Column("RimorchioDisponibile")]
        public bool RimorchioDisponibile { get; set; } = false;

        [Display(Name = "Portata max con rimorchio (Kg)")]
        [Column("PortataMaxConRimorchioKg")]
        public decimal? PortataMaxConRimorchioKg { get; set; }

        [Display(Name = "Attivo")]
        [Column("Attivo")]
        public bool Attivo { get; set; } = true;

        [Display(Name = "Gru")]
        [Column("Gru")]
        public bool Gru { get; set; }

        [Display(Name = "Trasbordo")]
        [Column("Trasbordo")]
        public bool Trasbordo { get; set; }

        [Display(Name = "Autista Default")]
        [Column("AutistaDefaultId")]
        public int? AutistaDefaultId { get; set; }

        [ForeignKey("AutistaDefaultId")]
        public virtual Autista? AutistaDefault { get; set; }
    }
}
