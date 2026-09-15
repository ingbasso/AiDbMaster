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

        [StringLength(500)]
        [Display(Name = "Cartella documenti")]
        public string? PercorsoDocumenti { get; set; }

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

        [Display(Name = "Imponibile acconto")]
        public decimal ImponibileAcconto { get; set; }

        [StringLength(150)]
        [Display(Name = "Riferimento ordine")]
        public string? RiferimentoOrdine { get; set; }

        [Display(Name = "Note")]
        public string? Note { get; set; }

        public List<ContabilitaCantiereRigaViewModel> Righe { get; set; } = new();

        public decimal TotaleVendita => Righe.Sum(r => r.TotaleVendita);
        public decimal TotaleVenditaPosa => Righe.SelectMany(r => r.Figli).Sum(f => f.TotaleVendita);
        public decimal TotaleAcquistoPosa => Righe.SelectMany(r => r.Figli).Sum(f => f.TotaleCosto);
        public decimal ImponibileSaldo => TotaleVendita - ImponibileAcconto;
        public decimal TotaleConIva => Math.Round(ImponibileSaldo * (1 + AliquotaIva / 100m), 2);
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
        public decimal? PrezzoVenditaCliente { get; set; }

        [StringLength(200)]
        public string? Note { get; set; }

        public List<ContabilitaCantiereRigaViewModel> Figli { get; set; } = new();

        public decimal TotaleCosto => (QuantitaPosata ?? 0) * (CostoMaterialeServizio ?? 0);
        public decimal TotaleVendita => (QuantitaPosata ?? 0) * (PrezzoVenditaCliente ?? 0);
    }
}
