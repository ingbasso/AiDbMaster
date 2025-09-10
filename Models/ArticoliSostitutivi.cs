using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella ArticoliSostitutivi
    /// Rappresenta le relazioni di sostituzione tra articoli nel sistema
    /// </summary>
    [Table("ArticoliSostitutivi")]
    public class ArticoliSostitutivi
    {
        /// <summary>
        /// Codice dell'articolo principale
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Il codice articolo è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il codice articolo non può superare i 50 caratteri")]
        [Display(Name = "Codice Articolo")]
        [Column("CodiceArticolo")]
        public string CodiceArticolo { get; set; } = string.Empty;

        /// <summary>
        /// Codice dell'articolo sostitutivo
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Il codice articolo sostitutivo è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il codice articolo sostitutivo non può superare i 50 caratteri")]
        [Display(Name = "Codice Articolo Sostitutivo")]
        [Column("CodiceArticoloSostitutivo")]
        public string CodiceArticoloSostitutivo { get; set; } = string.Empty;

        /// <summary>
        /// Note sulla sostituzione dell'articolo
        /// </summary>
        [Display(Name = "Note")]
        [Column("apa_note")]
        public string? Note { get; set; }

        /// <summary>
        /// Descrizione completa della relazione di sostituzione
        /// </summary>
        [NotMapped]
        public string DescrizioneCompleta
        {
            get
            {
                var descrizione = $"{CodiceArticolo} → {CodiceArticoloSostitutivo}";
                
                if (!string.IsNullOrEmpty(Note))
                {
                    descrizione += $" ({Note})";
                }
                
                return descrizione;
            }
        }

        /// <summary>
        /// Tipo di relazione per la visualizzazione
        /// </summary>
        [NotMapped]
        public string TipoRelazione
        {
            get
            {
                return "Sostituzione";
            }
        }

        /// <summary>
        /// Indica se la sostituzione ha note aggiuntive
        /// </summary>
        [NotMapped]
        public bool HasNote
        {
            get
            {
                return !string.IsNullOrEmpty(Note);
            }
        }

        /// <summary>
        /// Chiave composta per identificare univocamente la relazione
        /// </summary>
        [NotMapped]
        public string ChiaveComposta
        {
            get
            {
                return $"{CodiceArticolo}|{CodiceArticoloSostitutivo}";
            }
        }

        /// <summary>
        /// Rappresentazione testuale della relazione per ricerche
        /// </summary>
        [NotMapped]
        public string TestoRicerca
        {
            get
            {
                var testo = $"{CodiceArticolo} {CodiceArticoloSostitutivo}";
                
                if (!string.IsNullOrEmpty(Note))
                {
                    testo += $" {Note}";
                }
                
                return testo.ToLower();
            }
        }
    }
}
