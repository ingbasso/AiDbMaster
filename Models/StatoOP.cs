using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella degli stati degli ordini di produzione
    /// </summary>
    [Table("StatiOP")]
    public class StatoOP
    {
        /// <summary>
        /// Identificativo univoco dello stato
        /// </summary>
        [Key]
        public int IdStato { get; set; }

        /// <summary>
        /// Codice dello stato (ES = Emesso, PR = Produzione, CH = Chiuso, SO = Sospeso)
        /// </summary>
        [Required]
        [StringLength(2)]
        public string CodiceStato { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione dello stato
        /// </summary>
        [Required]
        [StringLength(20)]
        public string DescrizioneStato { get; set; } = string.Empty;

        /// <summary>
        /// Indica se lo stato è attivo
        /// </summary>
        public bool Attivo { get; set; } = true;

        /// <summary>
        /// Ordine di visualizzazione
        /// </summary>
        public int Ordine { get; set; }

        // Navigazione
        /// <summary>
        /// Lista degli ordini di produzione con questo stato
        /// </summary>
        public virtual ICollection<ListaOP> OrdiniProduzione { get; set; } = new List<ListaOP>();
    }
}
