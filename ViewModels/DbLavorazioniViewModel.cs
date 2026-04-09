using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    public class DbLavorazioniIndexViewModel
    {
        public List<DbLavorazione> Lavorazioni { get; set; } = new();

        // Filtri
        public string? FiltroCodiceDistinta { get; set; }
        public short? FiltroCodiceLavorazione { get; set; }
        public short? FiltroCodiceReparto { get; set; }
        public short? FiltroCodiceCentro { get; set; }

        // Dropdown per i filtri
        public List<Lavorazioni> LavorazioniDisponibili { get; set; } = new();
        public List<short> RepartiDisponibili { get; set; } = new();
        public List<short> CentriDisponibili { get; set; } = new();

        // Paginazione
        public int PaginaCorrente { get; set; } = 1;
        public int RighePerPagina { get; set; } = 20;
        public int TotaleRecord { get; set; }
        public int TotalePagine => (int)Math.Ceiling((double)TotaleRecord / RighePerPagina);
        public bool HaPaginaPrecedente => PaginaCorrente > 1;
        public bool HaPaginaSuccessiva => PaginaCorrente < TotalePagine;
    }

    public class DbLavorazioneEditViewModel
    {
        public DbLavorazione Lavorazione { get; set; } = new();

        // Dropdown per la selezione
        public List<Lavorazioni> LavorazioniDisponibili { get; set; } = new();
    }
}
