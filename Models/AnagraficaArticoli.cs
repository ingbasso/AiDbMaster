using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella AnagraficaArticoli
    /// Rappresenta l'anagrafica degli articoli nel sistema
    /// </summary>
    [Table("AnagraficaArticoli")]
    public class AnagraficaArticoli
    {
        /// <summary>
        /// Identificativo univoco dell'articolo
        /// </summary>
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>
        /// Codice articolo univoco
        /// </summary>
        [Required(ErrorMessage = "Il codice articolo è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il codice articolo non può superare i 50 caratteri")]
        [Display(Name = "Codice Articolo")]
        [Column("CodiceArticolo")]
        public string CodiceArticolo { get; set; } = string.Empty;

        /// <summary>
        /// Codice alternativo dell'articolo
        /// </summary>
        [StringLength(50, ErrorMessage = "Il codice alternativo non può superare i 50 caratteri")]
        [Display(Name = "Codice Alternativo")]
        [Column("CodiceAlternativo")]
        public string? CodiceAlternativo { get; set; }

        /// <summary>
        /// Descrizione dell'articolo
        /// </summary>
        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(255, ErrorMessage = "La descrizione non può superare i 255 caratteri")]
        [Display(Name = "Descrizione")]
        [Column("Descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione ulteriore dell'articolo
        /// </summary>
        [StringLength(50, ErrorMessage = "La descrizione ulteriore non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Ulteriore")]
        [Column("DescrizioneUlteriore")]
        public string? DescrizioneUlteriore { get; set; }

        /// <summary>
        /// Tipo articolo (codice di 1 carattere)
        /// </summary>
        [StringLength(1, ErrorMessage = "Il tipo articolo deve essere di 1 carattere")]
        [Display(Name = "Tipo Articolo")]
        [Column("TipoArticolo")]
        public string? TipoArticolo { get; set; }

        /// <summary>
        /// Unità di misura principale
        /// </summary>
        [Required(ErrorMessage = "L'unità di misura è obbligatoria")]
        [StringLength(3, ErrorMessage = "L'unità di misura non può superare i 3 caratteri")]
        [Display(Name = "Unità di Misura")]
        [Column("UnitàMisura")]
        public string UnitaMisura { get; set; } = string.Empty;

        /// <summary>
        /// Seconda unità di misura
        /// </summary>
        [StringLength(3, ErrorMessage = "La seconda unità di misura non può superare i 3 caratteri")]
        [Display(Name = "Seconda Unità di Misura")]
        [Column("SecondaUnitàMisura")]
        public string? SecondaUnitaMisura { get; set; }

        /// <summary>
        /// Fattore di conversione tra unità di misura
        /// </summary>
        [Display(Name = "Conversione")]
        [Column("Conversione", TypeName = "decimal(18,6)")]
        public decimal Conversione { get; set; }

        /// <summary>
        /// Unità di misura per confezione
        /// </summary>
        [StringLength(3, ErrorMessage = "L'unità di misura confezione non può superare i 3 caratteri")]
        [Display(Name = "Unità di Misura Confezione")]
        [Column("UnitàMisuraConfezione")]
        public string? UnitaMisuraConfezione { get; set; }

        /// <summary>
        /// Fattore di conversione per confezione
        /// </summary>
        [Display(Name = "Conversione Confezione")]
        [Column("ConversioneConfezione", TypeName = "decimal(18,6)")]
        public decimal ConversioneConfezione { get; set; }
    }
}
