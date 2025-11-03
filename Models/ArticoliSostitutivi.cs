using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella ArticoliSostitutivi
    /// Rappresenta la relazione bidirezionale tra articoli che possono sostituirsi a vicenda
    /// </summary>
    [Table("ArticoliSostitutivi")]
    public class ArticoliSostitutivi
    {
        /// <summary>
        /// Codice articolo principale
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("CodiceArticolo")]
        public string CodiceArticolo { get; set; } = string.Empty;

        /// <summary>
        /// Codice articolo sostitutivo
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("CodiceArticoloSostitutivo")]
        public string CodiceArticoloSostitutivo { get; set; } = string.Empty;

        /// <summary>
        /// Note sulla relazione di sostituzione
        /// </summary>
        [Column("Note")]
        public string? Note { get; set; }

        /// <summary>
        /// Data ultimo aggiornamento della relazione
        /// </summary>
        [Column("UltimoAggiornamento")]
        public DateTime? UltimoAggiornamento { get; set; }

        // Proprietà calcolate per compatibilità con il vecchio controller

        /// <summary>
        /// Descrizione completa della relazione (proprietà calcolata)
        /// </summary>
        [NotMapped]
        public string DescrizioneCompleta => $"{CodiceArticolo} → {CodiceArticoloSostitutivo}";

        /// <summary>
        /// Tipo di relazione (proprietà calcolata, sempre "Sostitutivo")
        /// </summary>
        [NotMapped]
        public string TipoRelazione => "Sostitutivo";

        /// <summary>
        /// Indica se ha note (proprietà calcolata)
        /// </summary>
        [NotMapped]
        public bool HasNote => !string.IsNullOrWhiteSpace(Note);

        /// <summary>
        /// Chiave composta per identificare univocamente la relazione
        /// </summary>
        [NotMapped]
        public string ChiaveComposta => $"{CodiceArticolo}|{CodiceArticoloSostitutivo}";
    }
}
