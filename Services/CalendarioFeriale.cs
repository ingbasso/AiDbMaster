namespace AiDbMaster.Services
{
    /// <summary>
    /// Helper statico per il calcolo dei giorni feriali italiani.
    /// Gestisce festività nazionali fisse e mobili (Pasqua, Lunedì dell'Angelo).
    /// </summary>
    public static class CalendarioFeriale
    {
        /// <summary>
        /// Verifica se una data è un giorno feriale (lun-ven, non festivo).
        /// </summary>
        public static bool IsGiornoFeriale(DateTime data)
        {
            if (data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
                return false;

            return !IsFestivoNazionale(data);
        }

        /// <summary>
        /// Restituisce il prossimo giorno feriale a partire da oggi (escluso).
        /// </summary>
        public static DateTime ProssimoGiornoFeriale(DateTime oggi)
        {
            var giorno = oggi.AddDays(1);
            while (!IsGiornoFeriale(giorno))
            {
                giorno = giorno.AddDays(1);
            }
            return giorno;
        }

        /// <summary>
        /// Verifica se una data è un giorno festivo nazionale italiano.
        /// Include: festività fisse + Pasqua + Lunedì dell'Angelo.
        /// </summary>
        public static bool IsFestivoNazionale(DateTime data)
        {
            var mese = data.Month;
            var giorno = data.Day;

            // Festività fisse
            if (mese == 1 && giorno == 1) return true;   // Capodanno
            if (mese == 1 && giorno == 6) return true;   // Epifania
            if (mese == 4 && giorno == 25) return true;  // Festa della Liberazione
            if (mese == 5 && giorno == 1) return true;   // Festa dei Lavoratori
            if (mese == 6 && giorno == 2) return true;   // Festa della Repubblica
            if (mese == 8 && giorno == 15) return true;  // Ferragosto
            if (mese == 11 && giorno == 1) return true;  // Ognissanti
            if (mese == 12 && giorno == 8) return true;  // Immacolata Concezione
            if (mese == 12 && giorno == 25) return true;  // Natale
            if (mese == 12 && giorno == 26) return true;  // Santo Stefano

            // Festività mobili: Pasqua e Lunedì dell'Angelo
            var pasqua = CalcolaPasqua(data.Year);
            if (data.Date == pasqua.Date) return true;
            if (data.Date == pasqua.AddDays(1).Date) return true; // Lunedì dell'Angelo

            return false;
        }

        /// <summary>
        /// Calcola la data di Pasqua per un dato anno (algoritmo di Gauss/Meeus).
        /// </summary>
        private static DateTime CalcolaPasqua(int anno)
        {
            int a = anno % 19;
            int b = anno / 100;
            int c = anno % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int mese = (h + l - 7 * m + 114) / 31;
            int giorno = ((h + l - 7 * m + 114) % 31) + 1;

            return new DateTime(anno, mese, giorno);
        }
    }
}
