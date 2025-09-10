using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella TabellaAgenti
    /// Rappresenta gli agenti di vendita nel sistema
    /// </summary>
    [Table("TabellaAgenti")]
    public class TabellaAgenti
    {
        /// <summary>
        /// Codice identificativo dell'agente
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Il codice agente è obbligatorio")]
        [Display(Name = "Codice Agente")]
        [Column("CodiceAgente")]
        public short CodiceAgente { get; set; }

        /// <summary>
        /// Nome/descrizione dell'agente
        /// </summary>
        [StringLength(50, ErrorMessage = "La descrizione agente non può superare i 50 caratteri")]
        [Display(Name = "Nome Agente")]
        [Column("DescrizioneAgente")]
        public string? DescrizioneAgente { get; set; }

        /// <summary>
        /// Indirizzo dell'agente
        /// </summary>
        [StringLength(70, ErrorMessage = "L'indirizzo non può superare i 70 caratteri")]
        [Display(Name = "Indirizzo")]
        [Column("IndirizzoAgente")]
        public string? IndirizzoAgente { get; set; }

        /// <summary>
        /// Codice di avviamento postale dell'agente
        /// </summary>
        [StringLength(10, ErrorMessage = "Il CAP non può superare i 10 caratteri")]
        [Display(Name = "CAP")]
        [Column("CAPAgente")]
        public string? CapAgente { get; set; }

        /// <summary>
        /// Città dell'agente
        /// </summary>
        [StringLength(50, ErrorMessage = "La città non può superare i 50 caratteri")]
        [Display(Name = "Città")]
        [Column("CittaAgente")]
        public string? CittaAgente { get; set; }

        /// <summary>
        /// Provincia dell'agente (sigla di 2 caratteri)
        /// </summary>
        [StringLength(2, ErrorMessage = "La provincia deve essere di 2 caratteri")]
        [Display(Name = "Provincia")]
        [Column("ProvinciaAgente")]
        public string? ProvinciaAgente { get; set; }

        /// <summary>
        /// Indica se l'agente è attivo
        /// </summary>
        [Required(ErrorMessage = "Lo stato attivo è obbligatorio")]
        [Display(Name = "Attivo")]
        [Column("Attivo")]
        public bool Attivo { get; set; }

        /// <summary>
        /// Indirizzo completo formattato per la visualizzazione
        /// </summary>
        [NotMapped]
        public string IndirizzoCompleto
        {
            get
            {
                var parti = new List<string>();
                
                if (!string.IsNullOrEmpty(IndirizzoAgente))
                    parti.Add(IndirizzoAgente);
                
                if (!string.IsNullOrEmpty(CapAgente) || !string.IsNullOrEmpty(CittaAgente))
                {
                    var cittaCompleta = new List<string>();
                    if (!string.IsNullOrEmpty(CapAgente))
                        cittaCompleta.Add(CapAgente);
                    if (!string.IsNullOrEmpty(CittaAgente))
                        cittaCompleta.Add(CittaAgente);
                    if (!string.IsNullOrEmpty(ProvinciaAgente))
                        cittaCompleta.Add($"({ProvinciaAgente})");
                    
                    parti.Add(string.Join(" ", cittaCompleta));
                }
                
                return string.Join(", ", parti);
            }
        }

        /// <summary>
        /// Nome completo dell'agente per la visualizzazione
        /// </summary>
        [NotMapped]
        public string NomeCompleto
        {
            get
            {
                if (!string.IsNullOrEmpty(DescrizioneAgente))
                {
                    return $"{CodiceAgente} - {DescrizioneAgente}";
                }
                return CodiceAgente.ToString();
            }
        }

        /// <summary>
        /// Stato dell'agente per la visualizzazione
        /// </summary>
        [NotMapped]
        public string StatoAgente
        {
            get
            {
                return Attivo ? "Attivo" : "Inattivo";
            }
        }

        /// <summary>
        /// Classe CSS per lo stato dell'agente
        /// </summary>
        [NotMapped]
        public string StatoAgenteCssClass
        {
            get
            {
                return Attivo ? "badge bg-success" : "badge bg-secondary";
            }
        }

        /// <summary>
        /// Descrizione completa per la ricerca e visualizzazione
        /// </summary>
        [NotMapped]
        public string DescrizioneCompleta
        {
            get
            {
                var descrizione = $"Agente {CodiceAgente}";
                
                if (!string.IsNullOrEmpty(DescrizioneAgente))
                {
                    descrizione += $" - {DescrizioneAgente}";
                }
                
                if (!string.IsNullOrEmpty(CittaAgente))
                {
                    descrizione += $" ({CittaAgente}";
                    if (!string.IsNullOrEmpty(ProvinciaAgente))
                    {
                        descrizione += $" - {ProvinciaAgente}";
                    }
                    descrizione += ")";
                }
                
                descrizione += $" - {StatoAgente}";
                
                return descrizione;
            }
        }

        /// <summary>
        /// Informazioni di contatto e localizzazione
        /// </summary>
        [NotMapped]
        public string InfoLocalizzazione
        {
            get
            {
                var info = new List<string>();
                
                if (!string.IsNullOrEmpty(CittaAgente))
                    info.Add(CittaAgente);
                
                if (!string.IsNullOrEmpty(ProvinciaAgente))
                    info.Add(ProvinciaAgente);
                
                return string.Join(" - ", info);
            }
        }

        /// <summary>
        /// Indica se l'agente ha informazioni di indirizzo complete
        /// </summary>
        [NotMapped]
        public bool HasIndirizzoCompleto
        {
            get
            {
                return !string.IsNullOrEmpty(IndirizzoAgente) && 
                       !string.IsNullOrEmpty(CittaAgente);
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
                var testo = $"{CodiceAgente}";
                
                if (!string.IsNullOrEmpty(DescrizioneAgente))
                    testo += $" {DescrizioneAgente}";
                
                if (!string.IsNullOrEmpty(CittaAgente))
                    testo += $" {CittaAgente}";
                
                if (!string.IsNullOrEmpty(ProvinciaAgente))
                    testo += $" {ProvinciaAgente}";
                
                if (!string.IsNullOrEmpty(IndirizzoAgente))
                    testo += $" {IndirizzoAgente}";
                
                testo += $" {StatoAgente}";
                
                return testo.ToLower();
            }
        }
    }
}
