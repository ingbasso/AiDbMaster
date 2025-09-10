using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella AnagraficaFornitori
    /// Rappresenta l'anagrafica dei fornitori nel sistema
    /// </summary>
    [Table("AnagraficaFornitori")]
    public class AnagraficaFornitori
    {
        /// <summary>
        /// Identificativo univoco del fornitore
        /// </summary>
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>
        /// Codice fornitore numerico
        /// </summary>
        [Required(ErrorMessage = "Il codice fornitore è obbligatorio")]
        [Display(Name = "Codice Fornitore")]
        [Column("CodiceFornitore")]
        public int CodiceFornitore { get; set; }

        /// <summary>
        /// Tipo anagrafica (codice di 1 carattere)
        /// </summary>
        [Required(ErrorMessage = "Il tipo anagrafica è obbligatorio")]
        [StringLength(1, ErrorMessage = "Il tipo anagrafica deve essere di 1 carattere")]
        [Display(Name = "Tipo Anagrafica")]
        [Column("an_tipo")]
        public string TipoAnagrafica { get; set; } = string.Empty;

        /// <summary>
        /// Ragione sociale del fornitore
        /// </summary>
        [Required(ErrorMessage = "La ragione sociale è obbligatoria")]
        [StringLength(50, ErrorMessage = "La ragione sociale non può superare i 50 caratteri")]
        [Display(Name = "Ragione Sociale")]
        [Column("RagioneSociale")]
        public string RagioneSociale { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione aggiuntiva del fornitore
        /// </summary>
        [StringLength(50, ErrorMessage = "La descrizione aggiuntiva non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Aggiuntiva")]
        [Column("an_descr2")]
        public string? DescrizioneAggiuntiva { get; set; }

        /// <summary>
        /// Indirizzo del fornitore
        /// </summary>
        [StringLength(70, ErrorMessage = "L'indirizzo non può superare i 70 caratteri")]
        [Display(Name = "Indirizzo")]
        [Column("Indirizzo")]
        public string? Indirizzo { get; set; }

        /// <summary>
        /// Codice di avviamento postale
        /// </summary>
        [StringLength(10, ErrorMessage = "Il CAP non può superare i 10 caratteri")]
        [Display(Name = "CAP")]
        [Column("CAP")]
        public string? Cap { get; set; }

        /// <summary>
        /// Città del fornitore
        /// </summary>
        [StringLength(50, ErrorMessage = "La città non può superare i 50 caratteri")]
        [Display(Name = "Città")]
        [Column("Citta")]
        public string? Citta { get; set; }

        /// <summary>
        /// Provincia del fornitore (sigla di 2 caratteri)
        /// </summary>
        [StringLength(2, ErrorMessage = "La provincia deve essere di 2 caratteri")]
        [Display(Name = "Provincia")]
        [Column("Provincia")]
        public string? Provincia { get; set; }

        /// <summary>
        /// Codice fiscale del fornitore
        /// </summary>
        [StringLength(16, ErrorMessage = "Il codice fiscale non può superare i 16 caratteri")]
        [Display(Name = "Codice Fiscale")]
        [Column("an_codfis")]
        public string? CodiceFiscale { get; set; }

        /// <summary>
        /// Partita IVA del fornitore
        /// </summary>
        [StringLength(11, ErrorMessage = "La partita IVA non può superare gli 11 caratteri")]
        [Display(Name = "Partita IVA")]
        [Column("an_pariva")]
        public string? PartitaIva { get; set; }

        /// <summary>
        /// Numero di telefono del fornitore
        /// </summary>
        [StringLength(18, ErrorMessage = "Il telefono non può superare i 18 caratteri")]
        [Display(Name = "Telefono")]
        [Column("Telefono")]
        public string? Telefono { get; set; }

        /// <summary>
        /// Numero di fax/telex del fornitore
        /// </summary>
        [StringLength(18, ErrorMessage = "Il fax non può superare i 18 caratteri")]
        [Display(Name = "Fax/Telex")]
        [Column("an_faxtlx")]
        public string? FaxTelex { get; set; }

        /// <summary>
        /// Indirizzo completo formattato per la visualizzazione
        /// </summary>
        [NotMapped]
        public string IndirizzoCompleto
        {
            get
            {
                var parti = new List<string>();
                
                if (!string.IsNullOrEmpty(Indirizzo))
                    parti.Add(Indirizzo);
                
                if (!string.IsNullOrEmpty(Cap) || !string.IsNullOrEmpty(Citta))
                {
                    var cittaCompleta = new List<string>();
                    if (!string.IsNullOrEmpty(Cap))
                        cittaCompleta.Add(Cap);
                    if (!string.IsNullOrEmpty(Citta))
                        cittaCompleta.Add(Citta);
                    if (!string.IsNullOrEmpty(Provincia))
                        cittaCompleta.Add($"({Provincia})");
                    
                    parti.Add(string.Join(" ", cittaCompleta));
                }
                
                return string.Join(", ", parti);
            }
        }

        /// <summary>
        /// Nome completo del fornitore per la visualizzazione
        /// </summary>
        [NotMapped]
        public string NomeCompleto
        {
            get
            {
                var parti = new List<string> { RagioneSociale };
                
                if (!string.IsNullOrEmpty(DescrizioneAggiuntiva))
                    parti.Add($"- {DescrizioneAggiuntiva}");
                
                return string.Join(" ", parti);
            }
        }

        /// <summary>
        /// Informazioni di contatto formattate
        /// </summary>
        [NotMapped]
        public string ContattiCompleti
        {
            get
            {
                var contatti = new List<string>();
                
                if (!string.IsNullOrEmpty(Telefono))
                    contatti.Add($"Tel: {Telefono}");
                
                if (!string.IsNullOrEmpty(FaxTelex))
                    contatti.Add($"Fax: {FaxTelex}");
                
                return string.Join(" | ", contatti);
            }
        }
    }
}
