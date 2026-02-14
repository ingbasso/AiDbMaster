using System;
using System.Collections.Generic;

namespace AiDbMaster.ViewModels
{
    public class DashboardConsegneViewModel
    {
        // Filtri
        public DateTime? DataDa { get; set; }
        public DateTime? DataA { get; set; }
        public short? CodiceAgente { get; set; }
        public string? NomeAgente { get; set; }

        // KPI Cards
        public int NumeroOrdiniTotali { get; set; }
        public decimal FatturatoTotale { get; set; }
        public decimal FatturatoConsegnato { get; set; }
        public decimal FatturatoDaConsegnare { get; set; }
        public int NumeroConsegneInRitardo { get; set; }
        public decimal ValoreConsegneInRitardo { get; set; }
        public decimal PercentualeEvasione { get; set; }
        public decimal ValoreMedioConsegna { get; set; }

        // Grafici
        public List<ConsegnePerMeseDto> ConsegnePerMese { get; set; } = new();

        // Classifiche
        public List<ClassificaAgenteDto> TopAgenti { get; set; } = new();
        public List<ClassificaProvinciaDto> TopProvince { get; set; } = new();
        public List<TopClienteDto> TopClienti { get; set; } = new();

        // Consegne in Ritardo
        public List<ConsegnaInRitardoDto> ConsegneInRitardo { get; set; } = new();
    }

    public class ConsegnePerMeseDto
    {
        public int Anno { get; set; }
        public int Mese { get; set; }
        public string MeseNome { get; set; } = "";
        public decimal Consegnato { get; set; }
        public decimal DaConsegnare { get; set; }
    }

    public class ClassificaAgenteDto
    {
        public short CodiceAgente { get; set; }
        public string NomeAgente { get; set; } = "";
        public int NumeroOrdini { get; set; }
        public decimal Fatturato { get; set; }
        public decimal PercentualeEvasione { get; set; }
    }

    public class ClassificaProvinciaDto
    {
        public string Provincia { get; set; } = "";
        public string? Regione { get; set; }
        public int NumeroOrdini { get; set; }
        public decimal Fatturato { get; set; }
    }

    public class TopClienteDto
    {
        public int CodiceCliente { get; set; }
        public string RagioneSociale { get; set; } = "";
        public string? Provincia { get; set; }
        public int NumeroOrdini { get; set; }
        public decimal Fatturato { get; set; }
    }

    public class ConsegnaInRitardoDto
    {
        public int AnnoOrdine { get; set; }
        public string SerieOrdine { get; set; } = "";
        public int NumeroOrdine { get; set; }
        public string NumeroOrdineCompleto => $"{AnnoOrdine}/{NumeroOrdine:D4}";
        public DateTime DataConsegna { get; set; }
        public int GiorniRitardo => (DateTime.Today - DataConsegna).Days;
        public string CodiceCliente { get; set; } = "";
        public string RagioneSociale { get; set; } = "";
        public string? NomeAgente { get; set; }
        public decimal ValoreRimanente { get; set; }

        /// <summary>
        /// Indica se almeno una riga di questo ordine ha già ricevuto una notifica email.
        /// </summary>
        public bool HasEmailInviata { get; set; }

        /// <summary>
        /// Data dell'ultimo invio email per questo ordine (se presente).
        /// </summary>
        public DateTime? DataEmailInviata { get; set; }
    }
}

