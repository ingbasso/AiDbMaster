using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella ParametriChiave
    /// Contiene i parametri di configurazione globali dell'applicazione.
    /// La tabella conterrà tipicamente una sola riga.
    /// I dati sono gestiti da procedure esterne.
    /// </summary>
    [Table("ParametriChiave")]
    public class ParametriChiave
    {
        /// <summary>
        /// Identificativo univoco del record
        /// </summary>
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>
        /// Numero di giorni da sommare a una data per calcolare l'impegno.
        /// Utilizzato per determinare la finestra temporale delle operazioni.
        /// </summary>
        [Required(ErrorMessage = "I giorni impegno sono obbligatori")]
        [Display(Name = "Giorni Impegno")]
        [Column("GiorniImpegno")]
        public int GiorniImpegno { get; set; }
    }
}
