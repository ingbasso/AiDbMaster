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
        [Display(Name = "Riferimento Ordine")]
        [Column("RiferimentoOrdine")]
        public string? RiferimentoOrdine { get; set; }

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
        /// Codice destinazione
        /// </summary>
        [Display(Name = "Codice Destinazione")]
        [Column("CodiceDestinazione")]
        public int? CodiceDestinazione { get; set; }

        /// <summary>
        /// Note della testata ordine
        /// </summary>
        [Display(Name = "Note Testata")]
        [Column("NoteTestata")]
        public string? NoteTestata { get; set; }

        /// <summary>
        /// Indica se l'ordine è prenotato: "S" = Sì, "N" = No
        /// </summary>
        [StringLength(1)]
        [Display(Name = "Prenotato")]
        [Column("Prenotato")]
        public string? Prenotato { get; set; }

        /// <summary>
        /// Indica se serve motrice con gru: "S" = Sì, "N" = No
        /// </summary>
        [StringLength(1)]
        [Display(Name = "Motrice Gru")]
        [Column("MotriceGru")]
        public string? MotriceGru { get; set; }

        /// <summary>
        /// Indica se serve autotreno con gru: "S" = Sì, "N" = No
        /// </summary>
        [StringLength(1)]
        [Display(Name = "Autotreno Gru")]
        [Column("AutotrenoGru")]
        public string? AutotrenoGru { get; set; }

        /// <summary>
        /// Indica se è necessario il trasbordo: "S" = Sì, "N" = No
        /// </summary>
        [StringLength(1)]
        [Display(Name = "Trasbordo")]
        [Column("Trasbordo")]
        public string? Trasbordo { get; set; }

        /// <summary>
        /// Autotreno abbinato: "S" = Sì, "N" = No
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Autotreno Abbinato")]
        [Column("AutotrenoAbbinato")]
        public string AutotrenoAbbinato { get; set; } = "N";

        /// <summary>
        /// Autotreno senza gru: "S" = Sì, "N" = No
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Autotreno No Gru")]
        [Column("AutotrenoNoGru")]
        public string AutotrenoNoGru { get; set; } = "N";

        /// <summary>
        /// Bilico: "S" = Sì, "N" = No
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Bilico")]
        [Column("Bilico")]
        public string Bilico { get; set; } = "N";

        /// <summary>
        /// Bilico in abbinamento: "S" = Sì, "N" = No
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Bilico In Abbinamento")]
        [Column("BilicoInAbbinamento")]
        public string BilicoInAbbinamento { get; set; } = "N";

        /// <summary>
        /// Motrice in abbinamento: "S" = Sì, "N" = No
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Motrice In Abbinamento")]
        [Column("MotriceInAbbinamento")]
        public string MotriceInAbbinamento { get; set; } = "N";

        /// <summary>
        /// Trasporto: "S" = Sì, "N" = No
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Trasporto")]
        [Column("Trasporto")]
        public string Trasporto { get; set; } = "N";

        /// <summary>
        /// Trasporto e posa: "S" = Sì, "N" = No
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Trasporto Posa")]
        [Column("TrasportoPosa")]
        public string TrasportoPosa { get; set; } = "N";

        /// <summary>
        /// Peso in Kg dell'ordine
        /// </summary>
        [Required]
        [Display(Name = "Peso Kg")]
        [Column("PesoKg", TypeName = "decimal(27,9)")]
        public decimal PesoKg { get; set; } = 0;

        /// <summary>
        /// Stato evasione ordine: "A" = Aperto, "P" = Parzialmente Evaso, "E" = Evaso
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Stato Evasione")]
        [Column("StatoEvasione")]
        public string StatoEvasione { get; set; } = "A";

        /// <summary>
        /// Descrizione dello stato evasione
        /// </summary>
        [NotMapped]
        public string DescrizioneStatoEvasione => StatoEvasione switch
        {
            "A" => "Aperto",
            "P" => "Parzialmente Evaso",
            "E" => "Evaso",
            _ => "Sconosciuto"
        };

        /// <summary>
        /// Classe CSS per il badge dello stato evasione
        /// </summary>
        [NotMapped]
        public string StatoEvasioneCssClass => StatoEvasione switch
        {
            "A" => "badge bg-primary",
            "P" => "badge bg-warning text-dark",
            "E" => "badge bg-success",
            _ => "badge bg-secondary"
        };

        /// <summary>
        /// Codice porto: "1" = Porto Franco, "2" = Porto Assegnato.
        /// Nel database è un varchar, non un smallint.
        /// Nullable: potrebbe non essere valorizzato.
        /// </summary>
        [Display(Name = "Porto")]
        [Column("Porto")]
        public string? Porto { get; set; }

        /// <summary>
        /// Descrizione del porto in base al codice
        /// </summary>
        [NotMapped]
        public string DescrizionePorto
        {
            get
            {
                return Porto?.Trim() switch
                {
                    "1" => "Porto Franco",
                    "2" => "Porto Assegnato",
                    _ => !string.IsNullOrEmpty(Porto) ? $"Porto {Porto}" : "Non specificato"
                };
            }
        }

        /// <summary>
        /// Classe CSS per il badge del porto
        /// </summary>
        [NotMapped]
        public string PortoCssClass
        {
            get
            {
                return Porto?.Trim() switch
                {
                    "1" => "badge bg-success",       // Porto Franco = verde
                    "2" => "badge bg-warning text-dark", // Porto Assegnato = giallo
                    _ => "badge bg-secondary"
                };
            }
        }

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
                
                if (!string.IsNullOrEmpty(RiferimentoOrdine))
                {
                    riepilogo += $" (Rif: {RiferimentoOrdine})";
                }
                
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
                
                if (!string.IsNullOrEmpty(RiferimentoOrdine))
                    testo += $" {RiferimentoOrdine}";
                
                if (Cliente != null)
                    testo += $" {Cliente.RagioneSociale} {Cliente.CodiceFiscale}";
                
                if (!string.IsNullOrEmpty(NoteTestata))
                    testo += $" {NoteTestata}";
                
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
