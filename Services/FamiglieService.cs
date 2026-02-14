using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la gestione delle famiglie articoli.
    /// Fornisce metodi per le operazioni CRUD e logica business.
    /// </summary>
    public class FamiglieService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FamiglieService> _logger;

        public FamiglieService(
            ApplicationDbContext context,
            ILogger<FamiglieService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene tutte le famiglie con filtri e paginazione
        /// </summary>
        public async Task<FamiglieIndexViewModel> GetFamiglieAsync(
            string? search = null,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.Famiglie.AsQueryable();

            // Applica filtri di ricerca
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f =>
                    f.CodiceFamiglia.Contains(search) ||
                    (f.DescrizioneFamiglia != null && f.DescrizioneFamiglia.Contains(search)));
            }

            // Conta il totale prima dell'ordinamento e paginazione
            var totalCount = await query.CountAsync();

            // Applica ordinamento
            query = sortOrder switch
            {
                "codice" => query.OrderBy(f => f.CodiceFamiglia),
                "codice_desc" => query.OrderByDescending(f => f.CodiceFamiglia),
                "descrizione" => query.OrderBy(f => f.DescrizioneFamiglia),
                "descrizione_desc" => query.OrderByDescending(f => f.DescrizioneFamiglia),
                "data" => query.OrderBy(f => f.UltimoAggiornamento),
                "data_desc" => query.OrderByDescending(f => f.UltimoAggiornamento),
                _ => query.OrderBy(f => f.CodiceFamiglia)
            };

            // Applica paginazione
            var famiglie = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calcola statistiche
            var stats = await GetStatsAsync();

            return new FamiglieIndexViewModel
            {
                Famiglie = famiglie,
                Search = search,
                SortOrder = sortOrder,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                CodiceSortParm = sortOrder == "codice" ? "codice_desc" : "codice",
                DescrizioneSortParm = sortOrder == "descrizione" ? "descrizione_desc" : "descrizione",
                DataSortParm = sortOrder == "data" ? "data_desc" : "data",
                TotaleFamiglie = stats.TotaleFamiglie
            };
        }

        /// <summary>
        /// Ottiene una famiglia per ID
        /// </summary>
        public async Task<Famiglia?> GetFamigliaByIdAsync(int id)
        {
            return await _context.Famiglie.FirstOrDefaultAsync(f => f.ID == id);
        }

        /// <summary>
        /// Ottiene i dettagli di una famiglia con navigazione precedente/successiva
        /// </summary>
        public async Task<FamigliaDetailsViewModel?> GetFamigliaDetailsAsync(int id)
        {
            var famiglia = await _context.Famiglie
                .FirstOrDefaultAsync(f => f.ID == id);

            if (famiglia == null)
                return null;

            // Trova ID precedente e successivo per la navigazione
            var previousId = await _context.Famiglie
                .Where(f => string.Compare(f.CodiceFamiglia, famiglia.CodiceFamiglia) < 0 ||
                           (f.CodiceFamiglia == famiglia.CodiceFamiglia && f.ID < id))
                .OrderByDescending(f => f.CodiceFamiglia)
                .ThenByDescending(f => f.ID)
                .Select(f => (int?)f.ID)
                .FirstOrDefaultAsync();

            var nextId = await _context.Famiglie
                .Where(f => string.Compare(f.CodiceFamiglia, famiglia.CodiceFamiglia) > 0 ||
                           (f.CodiceFamiglia == famiglia.CodiceFamiglia && f.ID > id))
                .OrderBy(f => f.CodiceFamiglia)
                .ThenBy(f => f.ID)
                .Select(f => (int?)f.ID)
                .FirstOrDefaultAsync();

            return new FamigliaDetailsViewModel
            {
                Famiglia = famiglia,
                PreviousId = previousId,
                NextId = nextId
            };
        }

        /// <summary>
        /// Crea una nuova famiglia
        /// </summary>
        public async Task<(bool Success, string Message, Famiglia? Famiglia)> CreateFamigliaAsync(CreateFamigliaViewModel model)
        {
            try
            {
                // Verifica unicità del CodiceFamiglia
                var esistente = await _context.Famiglie
                    .FirstOrDefaultAsync(f => f.CodiceFamiglia == model.CodiceFamiglia.Trim().ToUpper());

                if (esistente != null)
                {
                    return (false, $"Esiste già una famiglia con codice '{model.CodiceFamiglia}'.", null);
                }

                var famiglia = new Famiglia
                {
                    CodiceFamiglia = model.CodiceFamiglia.Trim().ToUpper(),
                    DescrizioneFamiglia = model.DescrizioneFamiglia?.Trim(),
                    UltimoAggiornamento = DateTime.Now
                };

                _context.Famiglie.Add(famiglia);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Famiglia creata con successo - ID: {ID}, Codice: {Codice}, Descrizione: {Descrizione}",
                    famiglia.ID, famiglia.CodiceFamiglia, famiglia.DescrizioneFamiglia);

                return (true, "Famiglia creata con successo!", famiglia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della famiglia - Codice: {Codice}",
                    model.CodiceFamiglia);
                return (false, $"Errore durante la creazione: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Aggiorna una famiglia esistente
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateFamigliaAsync(EditFamigliaViewModel model)
        {
            try
            {
                var famiglia = await GetFamigliaByIdAsync(model.ID);
                if (famiglia == null)
                {
                    return (false, "Famiglia non trovata.");
                }

                // Verifica unicità del CodiceFamiglia (escluso l'elemento corrente)
                var esistente = await _context.Famiglie
                    .FirstOrDefaultAsync(f => f.CodiceFamiglia == model.CodiceFamiglia.Trim().ToUpper() && f.ID != model.ID);

                if (esistente != null)
                {
                    return (false, $"Esiste già un'altra famiglia con codice '{model.CodiceFamiglia}'.");
                }

                famiglia.CodiceFamiglia = model.CodiceFamiglia.Trim().ToUpper();
                famiglia.DescrizioneFamiglia = model.DescrizioneFamiglia?.Trim();
                famiglia.UltimoAggiornamento = DateTime.Now;

                _context.Famiglie.Update(famiglia);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Famiglia modificata con successo - ID: {ID}, Codice: {Codice}",
                    famiglia.ID, famiglia.CodiceFamiglia);

                return (true, "Famiglia modificata con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la modifica della famiglia - ID: {ID}", model.ID);
                return (false, $"Errore durante la modifica: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina una famiglia
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteFamigliaAsync(int id)
        {
            try
            {
                var famiglia = await _context.Famiglie
                    .FirstOrDefaultAsync(f => f.ID == id);

                if (famiglia == null)
                {
                    return (false, "Famiglia non trovata.");
                }

                _context.Famiglie.Remove(famiglia);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Famiglia eliminata con successo - ID: {ID}, Codice: {Codice}, Descrizione: {Descrizione}",
                    famiglia.ID, famiglia.CodiceFamiglia, famiglia.DescrizioneFamiglia);

                return (true, "Famiglia eliminata con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della famiglia - ID: {Id}", id);
                return (false, $"Errore durante l'eliminazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene tutte le famiglie per dropdown/selezioni API
        /// </summary>
        public async Task<IEnumerable<FamigliaApiViewModel>> GetFamiglieApiAsync()
        {
            return await _context.Famiglie
                .OrderBy(f => f.CodiceFamiglia)
                .Select(f => new FamigliaApiViewModel
                {
                    ID = f.ID,
                    CodiceFamiglia = f.CodiceFamiglia,
                    DescrizioneFamiglia = f.DescrizioneFamiglia
                })
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene le statistiche delle famiglie
        /// </summary>
        public async Task<FamiglieStatsViewModel> GetStatsAsync()
        {
            var totaleFamiglie = await _context.Famiglie.CountAsync();

            DateTime? dataUltimoAggiornamento = null;

            if (totaleFamiglie > 0)
            {
                dataUltimoAggiornamento = await _context.Famiglie
                    .OrderByDescending(f => f.UltimoAggiornamento)
                    .Select(f => f.UltimoAggiornamento)
                    .FirstOrDefaultAsync();
            }

            return new FamiglieStatsViewModel
            {
                TotaleFamiglie = totaleFamiglie,
                DataUltimoAggiornamento = dataUltimoAggiornamento
            };
        }

        /// <summary>
        /// Verifica se una famiglia può essere eliminata
        /// (predisposto per futuri vincoli con altre tabelle)
        /// </summary>
        public async Task<(bool CanDelete, string Reason)> CanDeleteFamigliaAsync(int id)
        {
            var famiglia = await _context.Famiglie.FirstOrDefaultAsync(f => f.ID == id);
            if (famiglia == null)
            {
                return (false, "Famiglia non trovata.");
            }

            // In futuro, qui si potranno aggiungere controlli su relazioni con altre tabelle
            // Esempio: verificare se ci sono articoli collegati a questa famiglia

            return (true, string.Empty);
        }
    }
}
