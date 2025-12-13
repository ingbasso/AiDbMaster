using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella DestinazioniDiverse
    /// Rappresenta le destinazioni diverse associate ai conti clienti
    /// </summary>
    [Table("DestinazioniDiverse")]
    public class DestinazioniDiverse
    {
        /// <summary>
        /// Codice conto (parte della chiave composta)
        /// </summary>
        [Required(ErrorMessage = "Il codice conto è obbligatorio")]
        [Display(Name = "Codice Conto")]
        [Column("CodiceConto", Order = 0)]
        public int CodiceConto { get; set; }

        /// <summary>
        /// Codice destinazione (parte della chiave composta)
        /// </summary>
        [Required(ErrorMessage = "Il codice destinazione è obbligatorio")]
        [Display(Name = "Codice Destinazione")]
        [Column("CodiceDestinazione", Order = 1)]
        public int CodiceDestinazione { get; set; }

        /// <summary>
        /// Descrizione della destinazione
        /// </summary>
        [StringLength(50, ErrorMessage = "La descrizione non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Destinazione")]
        [Column("DescrizioneDestinazione")]
        public string? DescrizioneDestinazione { get; set; }

        /// <summary>
        /// Indirizzo della destinazione
        /// </summary>
        [StringLength(70, ErrorMessage = "L'indirizzo non può superare i 70 caratteri")]
        [Display(Name = "Indirizzo")]
        [Column("Indirizzo")]
        public string? Indirizzo { get; set; }

        /// <summary>
        /// CAP della destinazione
        /// </summary>
        [StringLength(10, ErrorMessage = "Il CAP non può superare i 10 caratteri")]
        [Display(Name = "CAP")]
        [Column("Cap")]
        public string? Cap { get; set; }

        /// <summary>
        /// Località della destinazione
        /// </summary>
        [StringLength(50, ErrorMessage = "La località non può superare i 50 caratteri")]
        [Display(Name = "Località")]
        [Column("Localita")]
        public string? Localita { get; set; }

        /// <summary>
        /// Provincia della destinazione
        /// </summary>
        [StringLength(2, ErrorMessage = "La provincia deve essere di 2 caratteri")]
        [Display(Name = "Provincia")]
        [Column("Provincia")]
        public string? Provincia { get; set; }

        /// <summary>
        /// Telefono della destinazione
        /// </summary>
        [StringLength(18, ErrorMessage = "Il telefono non può superare i 18 caratteri")]
        [Display(Name = "Telefono")]
        [Column("Telefono")]
        public string? Telefono { get; set; }

        /// <summary>
        /// Codice zona
        /// </summary>
        [Display(Name = "Codice Zona")]
        [Column("CodiceZona")]
        public short? CodiceZona { get; set; }

        /// <summary>
        /// Data e ora dell'ultimo aggiornamento del record
        /// </summary>
        [Display(Name = "Ultimo Aggiornamento")]
        [Column("UltimoAggiornamento")]
        public DateTime? UltimoAggiornamento { get; set; }

        // Proprietà calcolate per la visualizzazione
        /// <summary>
        /// Indirizzo completo per la visualizzazione
        /// </summary>
        [NotMapped]
        public string IndirizzoCompleto
        {
            get
            {
                var parti = new List<string>();

                if (!string.IsNullOrEmpty(Indirizzo))
                    parti.Add(Indirizzo);

                if (!string.IsNullOrEmpty(Cap))
                    parti.Add(Cap);

                if (!string.IsNullOrEmpty(Localita))
                    parti.Add(Localita);

                if (!string.IsNullOrEmpty(Provincia))
                    parti.Add($"({Provincia})");

                return parti.Count > 0 ? string.Join(", ", parti) : "N/D";
            }
        }

        /// <summary>
        /// Descrizione completa della destinazione
        /// </summary>
        [NotMapped]
        public string DescrizioneCompleta
        {
            get
            {
                var descrizione = !string.IsNullOrEmpty(DescrizioneDestinazione)
                    ? DescrizioneDestinazione
                    : $"Destinazione {CodiceDestinazione}";

                return $"{descrizione} - {IndirizzoCompleto}";
            }
        }

        /// <summary>
        /// Chiave composita per identificare univocamente il record
        /// </summary>
        [NotMapped]
        public string ChiaveComposita
        {
            get
            {
                return $"{CodiceConto}|{CodiceDestinazione}";
            }
        }

        /// <summary>
        /// Testo per la ricerca (tutti i campi concatenati)
        /// </summary>
        [NotMapped]
        public string TestoRicerca
        {
            get
            {
                var testo = $"{CodiceConto} {CodiceDestinazione}";

                if (!string.IsNullOrEmpty(DescrizioneDestinazione))
                    testo += $" {DescrizioneDestinazione}";

                if (!string.IsNullOrEmpty(Indirizzo))
                    testo += $" {Indirizzo}";

                if (!string.IsNullOrEmpty(Localita))
                    testo += $" {Localita}";

                if (!string.IsNullOrEmpty(Provincia))
                    testo += $" {Provincia}";

                return testo.ToLower();
            }
        }

        /// <summary>
        /// Indica se la destinazione ha un indirizzo completo
        /// </summary>
        [NotMapped]
        public bool HasIndirizzoCompleto
        {
            get
            {
                return !string.IsNullOrEmpty(Indirizzo) &&
                       !string.IsNullOrEmpty(Cap) &&
                       !string.IsNullOrEmpty(Localita);
            }
        }
    }
}

