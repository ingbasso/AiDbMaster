using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella OrdiniTestate
    /// Rappresenta le testate degli ordini (Fornitori "O" e Clienti "R")
    /// </summary>
    [Table("OrdiniTestate")]
    public class OrdiniTestate
    {
        /// <summary>
        /// Identificativo univoco del record
        /// </summary>
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>
        /// Codice del cliente (relazione con AnagraficaClienti)
        /// </summary>
        [Required(ErrorMessage = "Il codice cliente è obbligatorio")]
        [Display(Name = "Codice Cliente")]
        [Column("CodiceCliente")]
        public int CodiceCliente { get; set; }

        /// <summary>
        /// Tipo ordine: "O" = Ordine Fornitore, "R" = Ordine Cliente
        /// </summary>
        [Required(ErrorMessage = "Il tipo ordine è obbligatorio")]
        [StringLength(1, ErrorMessage = "Il tipo ordine deve essere di 1 carattere")]
        [Display(Name = "Tipo Ordine")]
        [Column("TipoOrdine")]
        public string TipoOrdine { get; set; } = string.Empty;

        /// <summary>
        /// Anno dell'ordine
        /// </summary>
        [Required(ErrorMessage = "L'anno ordine è obbligatorio")]
        [Display(Name = "Anno Ordine")]
        [Column("AnnoOrdine")]
        public short AnnoOrdine { get; set; }

        /// <summary>
        /// Serie dell'ordine
        /// </summary>
        [Required(ErrorMessage = "La serie ordine è obbligatoria")]
        [StringLength(3, ErrorMessage = "La serie ordine non può superare i 3 caratteri")]
        [Display(Name = "Serie Ordine")]
        [Column("SerieOrdine")]
        public string SerieOrdine { get; set; } = string.Empty;

        /// <summary>
        /// Numero dell'ordine
        /// </summary>
        [Required(ErrorMessage = "Il numero ordine è obbligatorio")]
        [Display(Name = "Numero Ordine")]
        [Column("NumeroOrdine")]
        public int NumeroOrdine { get; set; }

        /// <summary>
        /// Data dell'ordine
        /// </summary>
        [Required(ErrorMessage = "La data ordine è obbligatoria")]
        [Display(Name = "Data Ordine")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Column("DataOrdine")]
        public DateTime DataOrdine { get; set; }

        /// <summary>
        /// Riferimento ordine
        /// </summary>
        [StringLength(50, ErrorMessage = "Il riferimento non può superare i 50 caratteri")]
        [Display(Name = "Riferimento")]
        [Column("td_riferim")]
        public string? Riferimento { get; set; }

        /// <summary>
        /// Tipo bolla/fattura
        /// </summary>
        [Required(ErrorMessage = "Il tipo bolla/fattura è obbligatorio")]
        [Display(Name = "Tipo Bolla/Fattura")]
        [Column("td_tipobf")]
        public short TipoBollaFattura { get; set; }

        /// <summary>
        /// Data di consegna prevista
        /// </summary>
        [Display(Name = "Data Consegna")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Column("DataConsegna")]
        public DateTime? DataConsegna { get; set; }

        /// <summary>
        /// Codice agente (relazione con TabellaAgenti)
        /// </summary>
        [Required(ErrorMessage = "Il codice agente è obbligatorio")]
        [Display(Name = "Codice Agente")]
        [Column("CodiceAgente")]
        public short CodiceAgente { get; set; }

        /// <summary>
        /// Codice magazzino (relazione con TabellaMagazzini)
        /// </summary>
        [Required(ErrorMessage = "Il codice magazzino è obbligatorio")]
        [Display(Name = "Codice Magazzino")]
        [Column("td_magaz")]
        public short CodiceMagazzino { get; set; }

        /// <summary>
        /// Totale colli dell'ordine
        /// </summary>
        [Required(ErrorMessage = "Il totale colli è obbligatorio")]
        [Display(Name = "Totale Colli")]
        [Column("TotaleColli")]
        public int TotaleColli { get; set; }

        /// <summary>
        /// Note dell'ordine
        /// </summary>
        [Display(Name = "Note")]
        [Column("td_note")]
        public string? Note { get; set; }

        // Proprietà di navigazione per le relazioni
        /// <summary>
        /// Cliente associato all'ordine
        /// </summary>
        [ForeignKey("CodiceCliente")]
        public virtual AnagraficaClienti? Cliente { get; set; }

        /// <summary>
        /// Agente associato all'ordine
        /// </summary>
        [ForeignKey("CodiceAgente")]
        public virtual TabellaAgenti? Agente { get; set; }

        /// <summary>
        /// Magazzino associato all'ordine
        /// </summary>
        [ForeignKey("CodiceMagazzino")]
        public virtual TabellaMagazzini? Magazzino { get; set; }

        /// <summary>
        /// Righe dell'ordine
        /// </summary>
        public virtual ICollection<OrdiniRighe> Righe { get; set; } = new List<OrdiniRighe>();

        // Proprietà calcolate per la visualizzazione
        /// <summary>
        /// Numero ordine completo per la visualizzazione
        /// </summary>
        [NotMapped]
        public string NumeroOrdineCompleto
        {
            get
            {
                return $"{TipoOrdine}{AnnoOrdine}/{SerieOrdine}/{NumeroOrdine:D6}";
            }
        }

        /// <summary>
        /// Descrizione del tipo ordine
        /// </summary>
        [NotMapped]
        public string DescrizioneTipoOrdine
        {
            get
            {
                return TipoOrdine switch
                {
                    "O" => "Ordine Fornitore",
                    "R" => "Ordine Cliente",
                    _ => $"Tipo {TipoOrdine}"
                };
            }
        }

        /// <summary>
        /// Classe CSS per il badge del tipo ordine
        /// </summary>
        [NotMapped]
        public string TipoOrdineCssClass
        {
            get
            {
                return TipoOrdine switch
                {
                    "O" => "badge bg-warning text-dark",
                    "R" => "badge bg-success text-white",
                    _ => "badge bg-secondary"
                };
            }
        }

        /// <summary>
        /// Icona per il tipo ordine
        /// </summary>
        [NotMapped]
        public string TipoOrdineIcon
        {
            get
            {
                return TipoOrdine switch
                {
                    "O" => "bi-truck",
                    "R" => "bi-person-check",
                    _ => "bi-file-text"
                };
            }
        }

        /// <summary>
        /// Stato dell'ordine basato sulla data di consegna
        /// </summary>
        [NotMapped]
        public string StatoOrdine
        {
            get
            {
                if (!DataConsegna.HasValue)
                    return "Senza Consegna";

                var oggi = DateTime.Today;
                var consegna = DataConsegna.Value.Date;

                if (consegna < oggi)
                    return "Scaduto";
                else if (consegna == oggi)
                    return "In Scadenza";
                else if (consegna <= oggi.AddDays(7))
                    return "Prossima Consegna";
                else
                    return "Programmato";
            }
        }

        /// <summary>
        /// Classe CSS per lo stato dell'ordine
        /// </summary>
        [NotMapped]
        public string StatoOrdineCssClass
        {
            get
            {
                return StatoOrdine switch
                {
                    "Scaduto" => "badge bg-danger",
                    "In Scadenza" => "badge bg-warning text-dark",
                    "Prossima Consegna" => "badge bg-info",
                    "Programmato" => "badge bg-success",
                    "Senza Consegna" => "badge bg-secondary",
                    _ => "badge bg-light text-dark"
                };
            }
        }

        /// <summary>
        /// Descrizione completa dell'ordine
        /// </summary>
        [NotMapped]
        public string DescrizioneCompleta
        {
            get
            {
                var descrizione = $"{DescrizioneTipoOrdine} {NumeroOrdineCompleto}";
                
                if (Cliente != null)
                {
                    descrizione += $" - {Cliente.RagioneSociale}";
                }
                
                descrizione += $" del {DataOrdine:dd/MM/yyyy}";
                
                if (DataConsegna.HasValue)
                {
                    descrizione += $" (Consegna: {DataConsegna:dd/MM/yyyy})";
                }
                
                return descrizione;
            }
        }

        /// <summary>
        /// Informazioni di riepilogo per l'ordine
        /// </summary>
        [NotMapped]
        public string RiepilogoOrdine
        {
            get
            {
                var riepilogo = $"{NumeroOrdineCompleto} - {DescrizioneTipoOrdine}";
                
                if (!string.IsNullOrEmpty(Riferimento))
                {
                    riepilogo += $" (Rif: {Riferimento})";
                }
                
                riepilogo += $" - {TotaleColli} colli";
                
                return riepilogo;
            }
        }

        /// <summary>
        /// Indica se l'ordine ha una data di consegna impostata
        /// </summary>
        [NotMapped]
        public bool HasDataConsegna
        {
            get
            {
                return DataConsegna.HasValue;
            }
        }

        /// <summary>
        /// Giorni rimanenti alla consegna (negativo se scaduto)
        /// </summary>
        [NotMapped]
        public int? GiorniAllaConsegna
        {
            get
            {
                if (!DataConsegna.HasValue)
                    return null;

                return (DataConsegna.Value.Date - DateTime.Today).Days;
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
                var testo = $"{NumeroOrdineCompleto} {DescrizioneTipoOrdine}";
                
                if (!string.IsNullOrEmpty(Riferimento))
                    testo += $" {Riferimento}";
                
                if (Cliente != null)
                    testo += $" {Cliente.RagioneSociale} {Cliente.CodiceFiscale}";
                
                if (!string.IsNullOrEmpty(Note))
                    testo += $" {Note}";
                
                return testo.ToLower();
            }
        }

        /// <summary>
        /// Chiave composita per le relazioni con le righe
        /// </summary>
        [NotMapped]
        public string ChiaveComposita
        {
            get
            {
                return $"{TipoOrdine}|{AnnoOrdine}|{SerieOrdine}|{NumeroOrdine}";
            }
        }
    }
}
