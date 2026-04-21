namespace AiDbMaster.ViewModels
{
    public class Pstree_ContoEconomicoRigaViewModel
    {
        public int IdCodiceConto { get; set; }
        public string DescrizioneConto { get; set; } = string.Empty;
        public string TipoConto { get; set; } = string.Empty;
        public int Livello { get; set; }
        public int ParentId { get; set; }
        public int Ordine { get; set; }
        public bool HasFigli { get; set; }
        public bool HasOnlyTotalChildren { get; set; }
        public bool VoceRettifica { get; set; }
        public bool VoceRimanenza { get; set; }
        public string CostiFD { get; set; } = "N";
        public decimal[] ValoriMensili { get; set; } = new decimal[13];
        public Dictionary<int, decimal> ValoriFamiglia { get; set; } = new();
        public decimal ValoreAnalitica { get; set; }
        public bool HasPercentualeWarning { get; set; }
        public decimal Totale => ValoriMensili.Skip(1).Sum();

        public string TipoContoDescrizione => TipoConto switch
        {
            "F" => "Foglia", "R" => "Ricavo", "C" => "Costo",
            "T" => "Totale", "S" => "Sottototale", _ => TipoConto
        };

        public string CssClass => TipoConto switch
        {
            "T" => "fw-bold bg-light", "S" => "fw-bold",
            "R" => "text-success", "C" => "text-danger", _ => ""
        };

        public string IndentStyle => $"padding-left: {(Livello - 1) * 20}px;";
    }

    public class Pstree_ContoEconomicoViewModel
    {
        public int Anno { get; set; }
        public List<int> FamiglieSelezionate { get; set; } = new();
        public List<int> MesiSelezionati { get; set; } = new();
        public int? SedeSelezionata { get; set; }
        public string TipoVista { get; set; } = "mesi";
        public bool EscludiRimanenze { get; set; } = false;
        public bool MostraCodici { get; set; } = false;
        public bool IsVistaFamiglie => TipoVista == "famiglie";
        public bool IsVistaMesi => TipoVista == "mesi";
        public List<Pstree_FamigliaCheckboxItem> Famiglie { get; set; } = new();
        public List<Pstree_MeseCheckboxItem> Mesi { get; set; } = new();
        public List<Pstree_SedeDropdownItem> Sedi { get; set; } = new();
        public List<int> AnniDisponibili { get; set; } = new();
        public List<Pstree_ContoEconomicoRigaViewModel> Righe { get; set; } = new();
        public decimal[] TotaleCostiDiretti { get; set; } = new decimal[13];
        public decimal[] TotaleCostiFissi { get; set; } = new decimal[13];
        public decimal[] PercentualeFissiDiretti { get; set; } = new decimal[13];
        public decimal TotaleCostiDirettiAnno => TotaleCostiDiretti.Skip(1).Sum();
        public decimal TotaleCostiFissiAnno => TotaleCostiFissi.Skip(1).Sum();
        public decimal PercentualeFissiDirettiAnno => TotaleCostiDirettiAnno != 0
            ? Math.Round(TotaleCostiFissiAnno / TotaleCostiDirettiAnno * 100, 2) : 0;
        public Dictionary<int, decimal> TotaleCostiDirettiFamiglia { get; set; } = new();
        public Dictionary<int, decimal> TotaleCostiFissiFamiglia { get; set; } = new();

        public static readonly string[] NomiMesi = new[]
        {
            "", "Gen", "Feb", "Mar", "Apr", "Mag", "Giu",
            "Lug", "Ago", "Set", "Ott", "Nov", "Dic"
        };
    }

    public class Pstree_MeseCheckboxItem
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Selezionato { get; set; }
    }

    public class Pstree_FamigliaCheckboxItem
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Selezionato { get; set; }
    }

    public class Pstree_SedeDropdownItem
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
