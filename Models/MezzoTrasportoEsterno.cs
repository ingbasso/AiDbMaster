using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella MezziTrasportoEsterni.
    /// Rappresenta i mezzi di trasporto esterni (vettori/corrieri) con le relative località e costi.
    /// </summary>
    [Table("MezziTrasportoEsterni")]
    public class MezzoTrasportoEsterno
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Comune")]
        [Column("Comune", TypeName = "varchar(50)")]
        public string Comune { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Provincia")]
        [Column("Provincia", TypeName = "varchar(50)")]
        public string? Provincia { get; set; }

        [StringLength(50)]
        [Display(Name = "Regione")]
        [Column("Regione", TypeName = "varchar(50)")]
        public string? Regione { get; set; }

        [StringLength(50)]
        [Display(Name = "Nome Vettore")]
        [Column("NomeVettore", TypeName = "varchar(50)")]
        public string? NomeVettore { get; set; }

        [StringLength(50)]
        [Display(Name = "Tipo Mezzo")]
        [Column("TipoMezzo", TypeName = "varchar(50)")]
        public string? TipoMezzo { get; set; }

        [Required]
        [Display(Name = "Costo")]
        [Column("Costo")]
        public double Costo { get; set; }

        [Display(Name = "Portata Max")]
        [Column("PortataMax")]
        public int PortataMax { get; set; }

        [Display(Name = "Gru")]
        [Column("Gru")]
        public bool Gru { get; set; }

        [Display(Name = "Trasbordo")]
        [Column("Trasbordo")]
        public bool Trasbordo { get; set; }

        [Display(Name = "Note")]
        [Column("Note", TypeName = "varchar(max)")]
        public string? Note { get; set; }
    }
}
