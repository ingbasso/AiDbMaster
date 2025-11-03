using System.Collections.Generic;

namespace AiDbMaster.Helpers
{
    /// <summary>
    /// Helper per la mappatura Provincia → Regione italiana
    /// </summary>
    public static class RegioniHelper
    {
        /// <summary>
        /// Dizionario che mappa le sigle delle province italiane alle rispettive regioni
        /// </summary>
        private static readonly Dictionary<string, string> ProvinceRegioni = new()
        {
            // ABRUZZO
            { "AQ", "Abruzzo" }, { "CH", "Abruzzo" }, { "PE", "Abruzzo" }, { "TE", "Abruzzo" },
            
            // BASILICATA
            { "MT", "Basilicata" }, { "PZ", "Basilicata" },
            
            // CALABRIA
            { "CS", "Calabria" }, { "CZ", "Calabria" }, { "KR", "Calabria" }, { "RC", "Calabria" }, { "VV", "Calabria" },
            
            // CAMPANIA
            { "AV", "Campania" }, { "BN", "Campania" }, { "CE", "Campania" }, { "NA", "Campania" }, { "SA", "Campania" },
            
            // EMILIA-ROMAGNA
            { "BO", "Emilia-Romagna" }, { "FC", "Emilia-Romagna" }, { "FE", "Emilia-Romagna" }, { "MO", "Emilia-Romagna" },
            { "PR", "Emilia-Romagna" }, { "PC", "Emilia-Romagna" }, { "RA", "Emilia-Romagna" }, { "RE", "Emilia-Romagna" },
            { "RN", "Emilia-Romagna" },
            
            // FRIULI-VENEZIA GIULIA
            { "GO", "Friuli-Venezia Giulia" }, { "PN", "Friuli-Venezia Giulia" }, { "TS", "Friuli-Venezia Giulia" },
            { "UD", "Friuli-Venezia Giulia" },
            
            // LAZIO
            { "FR", "Lazio" }, { "LT", "Lazio" }, { "RI", "Lazio" }, { "RM", "Lazio" }, { "VT", "Lazio" },
            
            // LIGURIA
            { "GE", "Liguria" }, { "IM", "Liguria" }, { "SP", "Liguria" }, { "SV", "Liguria" },
            
            // LOMBARDIA
            { "BG", "Lombardia" }, { "BS", "Lombardia" }, { "CO", "Lombardia" }, { "CR", "Lombardia" },
            { "LC", "Lombardia" }, { "LO", "Lombardia" }, { "MN", "Lombardia" }, { "MI", "Lombardia" },
            { "MB", "Lombardia" }, { "PV", "Lombardia" }, { "SO", "Lombardia" }, { "VA", "Lombardia" },
            
            // MARCHE
            { "AN", "Marche" }, { "AP", "Marche" }, { "FM", "Marche" }, { "MC", "Marche" }, { "PU", "Marche" },
            
            // MOLISE
            { "CB", "Molise" }, { "IS", "Molise" },
            
            // PIEMONTE
            { "AL", "Piemonte" }, { "AT", "Piemonte" }, { "BI", "Piemonte" }, { "CN", "Piemonte" },
            { "NO", "Piemonte" }, { "TO", "Piemonte" }, { "VB", "Piemonte" }, { "VC", "Piemonte" },
            
            // PUGLIA
            { "BA", "Puglia" }, { "BT", "Puglia" }, { "BR", "Puglia" }, { "FG", "Puglia" },
            { "LE", "Puglia" }, { "TA", "Puglia" },
            
            // SARDEGNA
            { "CA", "Sardegna" }, { "CI", "Sardegna" }, { "NU", "Sardegna" }, { "OR", "Sardegna" },
            { "OT", "Sardegna" }, { "SS", "Sardegna" }, { "SU", "Sardegna" }, { "VS", "Sardegna" },
            
            // SICILIA
            { "AG", "Sicilia" }, { "CL", "Sicilia" }, { "CT", "Sicilia" }, { "EN", "Sicilia" },
            { "ME", "Sicilia" }, { "PA", "Sicilia" }, { "RG", "Sicilia" }, { "SR", "Sicilia" }, { "TP", "Sicilia" },
            
            // TOSCANA
            { "AR", "Toscana" }, { "FI", "Toscana" }, { "GR", "Toscana" }, { "LI", "Toscana" },
            { "LU", "Toscana" }, { "MS", "Toscana" }, { "PI", "Toscana" }, { "PT", "Toscana" },
            { "PO", "Toscana" }, { "SI", "Toscana" },
            
            // TRENTINO-ALTO ADIGE
            { "BZ", "Trentino-Alto Adige" }, { "TN", "Trentino-Alto Adige" },
            
            // UMBRIA
            { "PG", "Umbria" }, { "TR", "Umbria" },
            
            // VALLE D'AOSTA
            { "AO", "Valle d'Aosta" },
            
            // VENETO
            { "BL", "Veneto" }, { "PD", "Veneto" }, { "RO", "Veneto" }, { "TV", "Veneto" },
            { "VE", "Veneto" }, { "VI", "Veneto" }, { "VR", "Veneto" }
        };

        /// <summary>
        /// Restituisce la regione italiana corrispondente alla sigla della provincia
        /// </summary>
        /// <param name="siglaProvincia">Sigla della provincia (es. "MI", "RM", "NA")</param>
        /// <returns>Nome della regione o null se la provincia non è trovata</returns>
        public static string? GetRegione(string? siglaProvincia)
        {
            if (string.IsNullOrWhiteSpace(siglaProvincia))
                return null;

            var siglaUpper = siglaProvincia.Trim().ToUpper();
            return ProvinceRegioni.TryGetValue(siglaUpper, out var regione) ? regione : null;
        }

        /// <summary>
        /// Restituisce tutte le regioni italiane in ordine alfabetico
        /// </summary>
        /// <returns>Lista di nomi di regioni</returns>
        public static List<string> GetTutteLeRegioni()
        {
            var regioni = ProvinceRegioni.Values.Distinct().OrderBy(r => r).ToList();
            return regioni;
        }

        /// <summary>
        /// Restituisce tutte le province di una specifica regione
        /// </summary>
        /// <param name="nomeRegione">Nome della regione</param>
        /// <returns>Lista di sigle delle province</returns>
        public static List<string> GetProvincePerRegione(string nomeRegione)
        {
            if (string.IsNullOrWhiteSpace(nomeRegione))
                return new List<string>();

            var province = ProvinceRegioni
                .Where(kvp => kvp.Value.Equals(nomeRegione, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .OrderBy(p => p)
                .ToList();

            return province;
        }

        /// <summary>
        /// Verifica se una sigla di provincia è valida
        /// </summary>
        /// <param name="siglaProvincia">Sigla della provincia da verificare</param>
        /// <returns>True se la provincia esiste, False altrimenti</returns>
        public static bool IsProvinciaValida(string? siglaProvincia)
        {
            if (string.IsNullOrWhiteSpace(siglaProvincia))
                return false;

            return ProvinceRegioni.ContainsKey(siglaProvincia.Trim().ToUpper());
        }
    }
}

