namespace AiDbMaster.ViewModels
{
    public class Pstree_PercentualiFamiglieIndexViewModel
    {
        public int Anno { get; set; }
        public int? SedeSelezionata { get; set; }
        public List<int> AnniDisponibili { get; set; } = new();
        public List<Pstree_SedeDropdownItem> Sedi { get; set; } = new();
        public List<Pstree_VoceCEPercentualeStatus> VociCE { get; set; } = new();
    }

    public class Pstree_VoceCEPercentualeStatus
    {
        public int IdCodiceConto { get; set; }
        public string DescrizioneConto { get; set; } = string.Empty;
        public string TipoConto { get; set; } = string.Empty;
        public int MesiCompleti { get; set; }
        public int MesiIncompleti { get; set; }
        public bool HasPercentuali => MesiCompleti > 0 || MesiIncompleti > 0;

        public string StatoTesto => HasPercentuali
            ? (MesiIncompleti > 0 ? $"{MesiCompleti}/12 OK" : "12/12 OK")
            : "Non configurato";

        public string StatoIcona => HasPercentuali
            ? (MesiIncompleti > 0 ? "fa-exclamation-triangle text-warning" : "fa-check-circle text-success")
            : "fa-times-circle text-secondary";
    }

    public class Pstree_PercentualiFamiglieEditViewModel
    {
        public int IdCodiceConto { get; set; }
        public string DescrizioneConto { get; set; } = string.Empty;
        public int Anno { get; set; }
        public int IdSede { get; set; }
        public string NomeSede { get; set; } = string.Empty;
        public List<int> AnniDisponibili { get; set; } = new();
        public List<Pstree_SedeDropdownItem> Sedi { get; set; } = new();
        public List<Pstree_VoceCEDropdownItem> VociCE { get; set; } = new();
        public List<Pstree_FamigliaColonna> Famiglie { get; set; } = new();
        public List<Pstree_PercentualeRigaMese> RigheMesi { get; set; } = new();

        public static readonly string[] NomiMesi = new[]
        {
            "", "Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno",
            "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre"
        };
    }

    public class Pstree_VoceCEDropdownItem
    {
        public int IdCodiceConto { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    public class Pstree_FamigliaColonna
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class Pstree_PercentualeRigaMese
    {
        public int Mese { get; set; }
        public string NomeMese { get; set; } = string.Empty;
        public Dictionary<int, double> Percentuali { get; set; } = new();
        public double Totale => Percentuali.Values.Sum();
        public string RowClass => Math.Abs(Totale - 100.0) < 0.01 ? "" : (Totale > 0 ? "table-warning" : "");
    }

    public class Pstree_PercentualiFamiglieSaveModel
    {
        public int IdCodiceConto { get; set; }
        public int Anno { get; set; }
        public int IdSede { get; set; }
        public List<Pstree_PercentualeItem> Percentuali { get; set; } = new();
    }

    public class Pstree_PercentualeItem
    {
        public int Mese { get; set; }
        public int IdFamiglia { get; set; }
        public string Percentuale { get; set; } = "0";

        public double GetPercentualeValue()
        {
            var value = Percentuale?.Replace(",", ".") ?? "0";
            return double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
        }
    }

    public class Pstree_RimanenzeWarning
    {
        public int IdFamiglia { get; set; }
        public string NomeFamiglia { get; set; } = string.Empty;
        public int Mese { get; set; }
        public string NomeMese { get; set; } = string.Empty;
        public string Messaggio { get; set; } = string.Empty;
        public string Tipo { get; set; } = "warning";
        public string Icona { get; set; } = "bi-exclamation-triangle";
        public int Sede { get; set; }
        public int Anno { get; set; }
    }
}
