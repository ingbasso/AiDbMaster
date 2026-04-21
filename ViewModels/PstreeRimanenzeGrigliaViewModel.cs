namespace AiDbMaster.ViewModels
{
    public class Pstree_RimanenzeGrigliaViewModel
    {
        public int Anno { get; set; }
        public int Mese { get; set; }
        public int IdSede { get; set; }
        public string NomeSede { get; set; } = string.Empty;
        public string NomeMese { get; set; } = string.Empty;
        public List<Pstree_RimanenzaRigaViewModel> Righe { get; set; } = new();
        public double TotaleRimanenzaIniziale => Righe.Sum(r => r.RimanenzaIniziale);
        public double TotaleRimanenzaFinale => Righe.Sum(r => r.RimanenzaFinale);
        public double TotaleVariazione => Righe.Sum(r => r.Variazione);
        public List<int> AnniDisponibili { get; set; } = new();
        public List<Pstree_SedeDropdownItem> Sedi { get; set; } = new();
        public bool RecordEsistenti { get; set; }
        public bool IsGennaioSenzaDicembre { get; set; }

        public static readonly string[] NomiMesi = new[]
        {
            "", "Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno",
            "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre"
        };

        public string MessaggioRimanenzaIniziale => IsGennaioSenzaDicembre
            ? $"Non esistono rimanenze per Dicembre {Anno - 1}. Inserisci le rimanenze iniziali che verranno salvate come Dicembre {Anno - 1}."
            : "";
    }

    public class Pstree_RimanenzaRigaViewModel
    {
        public int Id { get; set; }
        public int IdFamiglia { get; set; }
        public string NomeFamiglia { get; set; } = string.Empty;
        public double RimanenzaIniziale { get; set; }
        public double RimanenzaFinale { get; set; }
        public double RettificaValore { get; set; }
        public string? NoteRettifica { get; set; }
        public double ValoreEffettivo => RimanenzaFinale + RettificaValore;
        public bool HasRettifica => RettificaValore != 0;
        public double Variazione => ValoreEffettivo - RimanenzaIniziale;
        public string VariazioneClass => Variazione > 0 ? "text-success" : Variazione < 0 ? "text-danger" : "";
    }

    public class Pstree_RimanenzeSelezionaViewModel
    {
        public int Anno { get; set; }
        public int Mese { get; set; }
        public int IdSede { get; set; }
        public List<int> AnniDisponibili { get; set; } = new();
        public List<Pstree_SedeDropdownItem> Sedi { get; set; } = new();
        public static readonly string[] NomiMesi = Pstree_RimanenzeGrigliaViewModel.NomiMesi;
    }

    public class Pstree_RimanenzeSaveModel
    {
        public int Anno { get; set; }
        public int Mese { get; set; }
        public int IdSede { get; set; }
        public List<Pstree_RimanenzaItemSave> Rimanenze { get; set; } = new();
    }

    public class Pstree_RimanenzaItemSave
    {
        public int Id { get; set; }
        public int IdFamiglia { get; set; }
        public string RimanenzaFinale { get; set; } = "0";
        public string RimanenzaIniziale { get; set; } = "0";
        public string RettificaValore { get; set; } = "0";
        public string? NoteRettifica { get; set; }

        public double GetRimanenzaFinaleValue()
        {
            if (string.IsNullOrEmpty(RimanenzaFinale)) return 0;
            var normalized = RimanenzaFinale.Replace(",", ".");
            if (double.TryParse(normalized, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result))
                return result;
            return 0;
        }

        public double GetRimanenzaInizialeValue()
        {
            if (string.IsNullOrEmpty(RimanenzaIniziale)) return 0;
            var normalized = RimanenzaIniziale.Replace(",", ".");
            if (double.TryParse(normalized, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result))
                return result;
            return 0;
        }

        public double GetRettificaValoreValue()
        {
            if (string.IsNullOrEmpty(RettificaValore)) return 0;
            var normalized = RettificaValore.Replace(",", ".");
            if (double.TryParse(normalized, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result))
                return result;
            return 0;
        }
    }
}
