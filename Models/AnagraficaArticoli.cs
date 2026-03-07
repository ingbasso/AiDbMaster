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

        /// <summary>
        /// Make or Buy: M=Make (produci internamente), B=Buy (acquista da terzi)
        /// </summary>
        [StringLength(1, ErrorMessage = "Il campo Make or Buy deve essere di 1 carattere")]
        [Display(Name = "Make or Buy")]
        [Column("MakeOrBuy", TypeName = "varchar(1)")]
        public string? MakeOrBuy { get; set; }

        // ===== NUOVI CAMPI =====

        /// <summary>
        /// Codice marca dell'articolo (FK verso TabellaMarche.CodiceMarca).
        /// Nullable: se non valorizzato, l'articolo non ha una marca assegnata.
        /// </summary>
        [Display(Name = "Marca")]
        [Column("Marca")]
        public short? Marca { get; set; }

        /// <summary>
        /// Codice famiglia dell'articolo (FK verso TabellaFamiglie.CodiceFamiglia).
        /// Nullable: se non valorizzato, l'articolo non ha una famiglia assegnata.
        /// </summary>
        [StringLength(4)]
        [Display(Name = "Famiglia")]
        [Column("Famiglia", TypeName = "varchar(4)")]
        public string? Famiglia { get; set; }

        /// <summary>
        /// Codice classe provvigione dell'articolo (FK verso TabellaClassiProvvigioni.CodiceClasse).
        /// Nullable: se non valorizzato, l'articolo non ha una classe provvigione assegnata.
        /// </summary>
        [Display(Name = "Classe Provvigione")]
        [Column("ClasseProvvigione")]
        public short? ClasseProvvigione { get; set; }

        /// <summary>
        /// Indica se l'articolo è Outlet (vecchio, da vendere il prima possibile).
        /// Valori: 'S' = Sì, 'N' = No. Default: 'N'.
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Outlet")]
        [Column("Outlet", TypeName = "varchar(1)")]
        public string Outlet { get; set; } = "N";

        /// <summary>
        /// Indica se l'articolo è fuori produzione.
        /// Valori: 'S' = Sì, 'N' = No. Default: 'N'.
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Fuori Produzione")]
        [Column("FuoriProduzione", TypeName = "varchar(1)")]
        public string FuoriProduzione { get; set; } = "N";

        /// <summary>
        /// Indica se l'articolo è gestito a Supermarket (scaffale).
        /// Se 'S', la disponibilità coincide con l'esistenza (l'impegnato non viene sottratto).
        /// Valori: 'S' = Sì, 'N' = No. Default: 'N'.
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Supermarket")]
        [Column("Supermarket", TypeName = "varchar(1)")]
        public string Supermarket { get; set; } = "N";

        /// <summary>
        /// Percentuale di sconto Outlet applicabile all'articolo.
        /// Nullable: se non valorizzato, l'articolo non ha uno sconto Outlet.
        /// </summary>
        [Display(Name = "% Sconto Outlet")]
        [Column("Perc_Sconto_Outlet", TypeName = "decimal(18,2)")]
        public decimal? PercScontoOutlet { get; set; }

        // ===== NAVIGATION PROPERTIES =====

        /// <summary>
        /// Navigation property verso la marca collegata
        /// </summary>
        [ForeignKey("Marca")]
        public virtual Models.Marca? MarcaNavigation { get; set; }

        /// <summary>
        /// Navigation property verso la famiglia collegata
        /// </summary>
        [ForeignKey("Famiglia")]
        public virtual Models.Famiglia? FamigliaNavigation { get; set; }

        /// <summary>
        /// Navigation property verso la classe provvigione collegata
        /// </summary>
        [ForeignKey("ClasseProvvigione")]
        public virtual ClasseProvvigione? ClasseProvvigioneNavigation { get; set; }
    }
}
