using System.Globalization;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la composizione di una ripartizione di import saldi:
    /// un conto del Piano dei Conti, una sede e un periodo, ripartito su piu' voci
    /// di Conto Economico con le rispettive percentuali.
    /// </summary>
    public class RipartizioneImportSaldiCreateViewModel
    {
        public string CodicePdC { get; set; } = string.Empty;

        public int IdSede { get; set; }

        public int Anno { get; set; }

        public int Mese { get; set; }

        public List<RipartizioneVoceItem> Voci { get; set; } = new();
    }

    /// <summary>
    /// Singola voce di ripartizione (voce di CE + percentuale).
    /// La percentuale e' una stringa per gestire in modo robusto la cultura
    /// (virgola/punto) come gia' avviene per le percentuali per famiglia.
    /// </summary>
    public class RipartizioneVoceItem
    {
        public int IdCodiceConto { get; set; }

        public string? Percentuale { get; set; }

        public double GetPercentuale()
        {
            if (string.IsNullOrWhiteSpace(Percentuale))
                return 0;

            var normalizzato = Percentuale.Replace(',', '.').Trim();
            return double.TryParse(normalizzato, NumberStyles.Any, CultureInfo.InvariantCulture, out var valore)
                ? valore
                : 0;
        }
    }
}
