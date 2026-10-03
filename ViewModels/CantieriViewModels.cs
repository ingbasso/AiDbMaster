using System.ComponentModel.DataAnnotations;
using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    public class CantiereElencoItemViewModel
    {
        public int Id { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public int CodiceCliente { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string? Localita { get; set; }
        public string? Provincia { get; set; }
        public string? ImpresaPosa { get; set; }
        public DateTime? DataInizioPrevista { get; set; }
        public DateTime? DataFinePrevista { get; set; }
        public CantiereStato Stato { get; set; }
        public int NumeroOrdini { get; set; }
    }

    public class CantiereSchedaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Codice")]
        public string? Codice { get; set; }

        [Required(ErrorMessage = "Il nome del cantiere è obbligatorio")]
        [StringLength(150)]
        [Display(Name = "Nome cantiere")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il cliente è obbligatorio")]
        [Display(Name = "Cliente")]
        public int? CodiceCliente { get; set; }

        public string? ClienteNome { get; set; }

        [Display(Name = "Destinazione")]
        public int? CodiceDestinazione { get; set; }

        [StringLength(70)]
        [Display(Name = "Indirizzo")]
        public string? Indirizzo { get; set; }

        [StringLength(10)]
        [Display(Name = "CAP")]
        public string? Cap { get; set; }

        [StringLength(50)]
        [Display(Name = "Località")]
        public string? Localita { get; set; }

        [StringLength(2)]
        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }

        [StringLength(18)]
        [Display(Name = "Telefono")]
        public string? Telefono { get; set; }

        public List<CantiereReferenteItemViewModel> Referenti { get; set; } = new();

        [Display(Name = "Impresa di posa")]
        public int? ImpresaPosaId { get; set; }

        [Display(Name = "Inizio previsto")]
        [DataType(DataType.Date)]
        public DateTime? DataInizioPrevista { get; set; }

        [Display(Name = "Fine prevista")]
        [DataType(DataType.Date)]
        public DateTime? DataFinePrevista { get; set; }

        [Display(Name = "Stato")]
        public CantiereStato Stato { get; set; } = CantiereStato.Preventivo;

        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "Agente")]
        public string? NomeAgente { get; set; }

        [StringLength(50)]
        [Display(Name = "Trasporto")]
        public string? TipoTrasporto { get; set; }

        [Display(Name = "Pulizia cantiere")]
        public bool PuliziaCantiere { get; set; }

        [Display(Name = "Check affidabilità cliente")]
        public bool CheckAffidabilitaCliente { get; set; }

        [StringLength(100)]
        [Display(Name = "Pagamento")]
        public string? Pagamento { get; set; }

        [Display(Name = "Conferma firmata")]
        public bool ConfermaFirmata { get; set; }

        [Display(Name = "Trasformato in Favaro1")]
        public bool TrasformatoInFavaro1 { get; set; }

        [StringLength(20)]
        [Display(Name = "Magazzino N.")]
        public string? MagazzinoN { get; set; }

        [Display(Name = "Accredito acconto")]
        public bool AccreditoAcconto { get; set; }

        [Display(Name = "Anagrafica SDI/PEC")]
        public bool AnagraficaSdiPec { get; set; }

        [StringLength(100)]
        [Display(Name = "Banca")]
        public string? Banca { get; set; }

        [Display(Name = "Aliquota IVA %")]
        public decimal? AliquotaIva { get; set; }

        [Display(Name = "Doc. agevolazione IVA")]
        public bool DocAgevolazioneIva { get; set; }

        [Display(Name = "Contratto posatore firmato")]
        public bool ContrattoPosatoreFirmato { get; set; }

        [Display(Name = "PSC")]
        public bool Psc { get; set; }

        [Display(Name = "POS Favaro1")]
        public bool PosFavaro1 { get; set; }

        [Display(Name = "Fine posa firmato")]
        public bool FinePosaFirmato { get; set; }

        [Display(Name = "Fine posa il")]
        [DataType(DataType.Date)]
        public DateTime? FinePosaData { get; set; }

        [Display(Name = "Contabilità di cantiere")]
        public bool ContabilitaCantiere { get; set; }

        [Display(Name = "Fattura saldo attivo")]
        public bool FatturaSaldoAttivo { get; set; }

        [Display(Name = "Fattura saldo passiva")]
        public bool FatturaSaldoPassivo { get; set; }

        public List<CantiereOrdineItemViewModel> Ordini { get; set; } = new();

        public string? CartellaDocumenti { get; set; }
        public string? ErroreDocumenti { get; set; }
        public List<CantiereDocumentoItemViewModel> Documenti { get; set; } = new();
    }

    public class CartellaCantiereInfo
    {
        public string? Percorso { get; set; }
        public string? Errore { get; set; }
        public List<CantiereDocumentoItemViewModel> File { get; set; } = new();
    }

    public class CantiereDocumentoItemViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public long Dimensione { get; set; }
        public DateTime DataModifica { get; set; }
    }

    public class CantiereOrdineItemViewModel
    {
        public int Id { get; set; }
        public int OrdineTestataId { get; set; }
        public string NumeroOrdine { get; set; } = string.Empty;
        public DateTime DataOrdine { get; set; }
        public string? Riferimento { get; set; }
        public string StatoEvasione { get; set; } = string.Empty;
        public string StatoEvasioneCss { get; set; } = "badge bg-secondary";
        public bool TrasportoPosa { get; set; }
    }

    public class CantiereReferenteItemViewModel
    {
        public int Id { get; set; }
        public int ReferenteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Ruolo { get; set; }
    }

    public class ContabilitaCantiereViewModel
    {
        public int CantiereId { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string? ClienteNome { get; set; }
        public string? ImpresaPosa { get; set; }
        public string? NomeAgente { get; set; }
        public string? MagazzinoN { get; set; }
        public TipoContabilitaCantiere Tipo { get; set; } = TipoContabilitaCantiere.Iniziale;
        public bool HaContabilitaIniziale { get; set; }
        public bool HaContabilitaFinale { get; set; }
        public int NumeroOrdiniCollegati { get; set; }

        [Display(Name = "Aliquota IVA %")]
        public decimal AliquotaIva { get; set; } = 22;

        public List<ContabilitaAccontoViewModel> Acconti { get; set; } = new();

        [Display(Name = "Imponibile acconto")]
        public decimal ImponibileAcconto => Acconti.Sum(a => a.Imponibile);

        [StringLength(150)]
        [Display(Name = "Riferimento ordine")]
        public string? RiferimentoOrdine { get; set; }

        [Display(Name = "Note")]
        public string? Note { get; set; }

        public List<ContabilitaCantiereRigaViewModel> Righe { get; set; } = new();

        /// <summary>
        /// Costi e prezzi medi letti dalla tabella CostiArticoliCantiere.
        /// Servono solo a proporre il costo e a mostrare «PM» sul padre. Non vengono salvati con la contabilità.
        /// </summary>
        public List<CostoArticoloCantiereVoce> CostiArticoli { get; set; } = new();

        /// <summary>Solo sulla contabilità finale: esistono righe iniziali da confrontare.</summary>
        public bool ConfrontaIniziale { get; set; }

        public decimal TotaleCostiIniziale { get; set; }
        public decimal TotaleVenditaIniziale { get; set; }

        /// <summary>Padri presenti nell'iniziale e assenti nella finale. Non vengono salvati.</summary>
        public List<ContabilitaCantiereRigaViewModel> RigheSoloIniziale { get; set; } = new();

        public decimal TotaleVendita => Righe.Sum(r => r.TotaleVendita);
        public decimal TotaleVenditaPosa => Righe.SelectMany(r => r.Figli).Where(f => f.IsPosa).Sum(f => f.TotaleVendita);
        public decimal TotaleAcquistoPosa => Righe.SelectMany(r => r.Figli).Where(f => f.IsPosa).Sum(f => f.TotaleCosto);
        public decimal ImponibileSaldo => TotaleVendita - ImponibileAcconto;
        public decimal TotaleConIva => Math.Round(ImponibileSaldo * (1 + AliquotaIva / 100m), 2);
    }

    public class ContabilitaAccontoViewModel
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string? Descrizione { get; set; }

        public decimal Imponibile { get; set; }
    }

    public class ContabilitaCantiereRigaViewModel
    {
        public int Id { get; set; }
        public int? PadreId { get; set; }

        [StringLength(50)]
        public string? CodiceArticolo { get; set; }

        [StringLength(255)]
        public string Descrizione { get; set; } = string.Empty;

        [StringLength(10)]
        public string? UnitaMisura { get; set; }

        public decimal? QuantitaPosata { get; set; }
        public decimal? CostoMaterialeServizio { get; set; }
        public decimal? RicaricoPercentuale { get; set; }
        public decimal? PrezzoVenditaCliente { get; set; }

        /// <summary>
        /// Vero se in questa riga l'utente ha scritto il prezzo di vendita
        /// e il ricarico va ricavato da quello. Non è una colonna del database:
        /// serve solo durante il salvataggio.
        /// </summary>
        public bool PrezzoDaPrezzo { get; set; }

        public decimal? RicaricoEffettivo
        {
            get
            {
                if (IsSconto || IsArticoloSingolo)
                    return RicaricoPercentuale;
                if (RicaricoPercentuale.HasValue)
                    return RicaricoPercentuale;
                if (!IsPosa && CostoMaterialeServizio is > 0 && PrezzoVenditaCliente.HasValue)
                    return Math.Round((PrezzoVenditaCliente.Value / CostoMaterialeServizio.Value - 1m) * 100m, 4);
                return 30m;
            }
        }

        [StringLength(200)]
        public string? Note { get; set; }

        public bool IsPosa { get; set; }

        /// <summary>
        /// Riga materiale creata insieme al padre: stesso articolo, quantità e codice.
        /// Serve solo a riconoscerla nel salvataggio, non è una colonna del database.
        /// </summary>
        public bool IsArticoloMateriale { get; set; }

        /// <summary>Riga di sconto: l'importo in cella è positivo e il totale vendita è negativo.</summary>
        public bool IsSconto { get; set; }

        /// <summary>Articolo singolo, senza figli e senza posa.</summary>
        public bool IsArticoloSingolo { get; set; }

        /// <summary>
        /// Se è vero, questa riga compare nella proforma. Vale per il padre e per l'articolo singolo.
        /// </summary>
        public bool MostraInProforma { get; set; } = true;

        public bool HaConfronto { get; set; }
        public bool SoloFinale { get; set; }
        public decimal? InizialeQuantita { get; set; }
        public decimal? InizialeCostoUnitario { get; set; }
        public decimal? InizialePrezzoUnitario { get; set; }

        public bool HaConfrontoPosa { get; set; }
        public decimal? InizialePosaQuantita { get; set; }
        public decimal? InizialePosaCostoUnitario { get; set; }
        public decimal? InizialePosaPrezzoUnitario { get; set; }

        /// <summary>Figli dell'iniziale non trovati sotto questo padre nella finale. Non vengono salvati.</summary>
        public List<ContabilitaCantiereRigaViewModel> Mancanti { get; set; } = new();

        public List<ContabilitaCantiereRigaViewModel> Figli { get; set; } = new();

        public decimal TotaleCosto => IsSconto ? 0 : (QuantitaPosata ?? 0) * (CostoMaterialeServizio ?? 0);

        public decimal TotaleVendita => IsSconto
            ? -Math.Abs(PrezzoVenditaCliente ?? 0)
            : (QuantitaPosata ?? 0) * (PrezzoVenditaCliente ?? 0);
    }

    /// <summary>Voce letta da CostiArticoliCantiere e passata alla griglia.</summary>
    public class CostoArticoloCantiereVoce
    {
        public string Codice { get; set; } = string.Empty;
        public decimal Costo { get; set; }
        public decimal PrezzoMedio { get; set; }
    }

    /// <summary>Maschera di inserimento e modifica di un costo articolo.</summary>
    public class CostoArticoloCantiereForm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Il codice articolo è obbligatorio.")]
        [StringLength(50, ErrorMessage = "Il codice può avere al massimo 50 caratteri.")]
        [Display(Name = "Codice articolo")]
        public string CodiceArticolo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descrizione è obbligatoria.")]
        [StringLength(255, ErrorMessage = "La descrizione può avere al massimo 255 caratteri.")]
        [Display(Name = "Descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il costo unitario è obbligatorio.")]
        [Display(Name = "Costo unitario")]
        public string? CostoUnitario { get; set; }

        [Required(ErrorMessage = "Il prezzo medio di vendita è obbligatorio.")]
        [Display(Name = "Prezzo medio di vendita")]
        public string? PrezzoMedioVendita { get; set; }
    }
}
