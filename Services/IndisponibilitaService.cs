using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio centralizzato per la gestione delle indisponibilità (assenze autisti, fermi mezzi).
    /// Usato sia per la validazione in fase di creazione/modifica viaggio,
    /// sia per mostrare le indisponibilità nel calendario consegne.
    /// </summary>
    public class IndisponibilitaService
    {
        private readonly ApplicationDbContext _context;

        public IndisponibilitaService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verifica se un mezzo interno e/o un autista risultano indisponibili
        /// nella data e fascia oraria indicate.
        /// Restituisce un messaggio di errore descrittivo oppure null se tutto ok.
        /// </summary>
        public async Task<string?> ControllaIndisponibilitaAsync(
            DateTime data,
            TimeSpan oraPartenza,
            TimeSpan oraArrivo,
            int? mezzoInternoId,
            int? autistaId)
        {
            var giorno = data.Date;

            // Carica le indisponibilità che coprono la data richiesta per i soggetti coinvolti.
            var candidate = await _context.Indisponibilita
                .AsNoTracking()
                .Include(i => i.Autista)
                .Include(i => i.MezzoTrasporto)
                .Where(i => i.DataInizio.Date <= giorno && i.DataFine.Date >= giorno
                    && ((mezzoInternoId.HasValue && i.Tipo == TipoIndisponibilita.Mezzo && i.MezzoTrasportoId == mezzoInternoId.Value)
                        || (autistaId.HasValue && i.Tipo == TipoIndisponibilita.Autista && i.AutistaId == autistaId.Value)))
                .ToListAsync();

            foreach (var ind in candidate)
            {
                if (!SovrapponeFascia(ind, oraPartenza, oraArrivo))
                    continue;

                var fascia = DescriviFascia(ind);

                if (ind.Tipo == TipoIndisponibilita.Mezzo)
                {
                    var nome = ind.MezzoTrasporto?.Descrizione ?? "selezionato";
                    return $"Il mezzo '{nome}' non è disponibile il {giorno:dd/MM/yyyy} ({ind.Causale}{fascia}).";
                }
                else
                {
                    var nome = ind.Autista != null ? $"{ind.Autista.Cognome} {ind.Autista.Nome}" : "selezionato";
                    return $"L'autista '{nome}' è assente il {giorno:dd/MM/yyyy} ({ind.Causale}{fascia}).";
                }
            }

            return null;
        }

        /// <summary>
        /// Restituisce tutte le indisponibilità che intersecano l'intervallo di date indicato,
        /// utili per la visualizzazione nel calendario.
        /// </summary>
        public async Task<List<Indisponibilita>> GetIndisponibilitaPerPeriodoAsync(DateTime dataInizio, DateTime dataFine)
        {
            var da = dataInizio.Date;
            var a = dataFine.Date;

            return await _context.Indisponibilita
                .AsNoTracking()
                .Include(i => i.Autista)
                .Include(i => i.MezzoTrasporto)
                .Where(i => i.DataInizio.Date <= a && i.DataFine.Date >= da)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se l'indisponibilità (giorno intero o fascia oraria) si sovrappone
        /// alla fascia oraria del viaggio.
        /// </summary>
        private static bool SovrapponeFascia(Indisponibilita ind, TimeSpan oraPartenza, TimeSpan oraArrivo)
        {
            if (ind.GiornoIntero || !ind.OraInizio.HasValue || !ind.OraFine.HasValue)
                return true;

            return oraPartenza < ind.OraFine.Value && ind.OraInizio.Value < oraArrivo;
        }

        private static string DescriviFascia(Indisponibilita ind)
        {
            if (ind.GiornoIntero || !ind.OraInizio.HasValue || !ind.OraFine.HasValue)
                return "";
            return $", {ind.OraInizio.Value:hh\\:mm}-{ind.OraFine.Value:hh\\:mm}";
        }
    }
}
