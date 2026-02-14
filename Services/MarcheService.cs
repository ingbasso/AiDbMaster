using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la gestione delle marche.
    /// Fornisce metodi per le operazioni CRUD e logica business.
    /// </summary>
    public class MarcheService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MarcheService> _logger;

        public MarcheService(
            ApplicationDbContext context,
            ILogger<MarcheService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene tutte le marche con filtri e paginazione
        /// </summary>
        public async Task<MarcheIndexViewModel> GetMarcheAsync(
            string? search = null,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.Marche.AsQueryable();

            // Applica filtri di ricerca
            if (!string.IsNullOrEmpty(search))
            {
                if (short.TryParse(search, out short codiceSearch))
                {
                    query = query.Where(m =>
                        m.CodiceMarca == codiceSearch ||
                        (m.DescrizioneMarca != null && m.DescrizioneMarca.Contains(search)));
                }
                else
                {
                    query = query.Where(m =>
                        m.DescrizioneMarca != null && m.DescrizioneMarca.Contains(search));
                }
            }

            // Conta il totale prima dell'ordinamento e paginazione
            var totalCount = await query.CountAsync();

            // Applica ordinamento
            query = sortOrder switch
            {
                "codice" => query.OrderBy(m => m.CodiceMarca),
                "codice_desc" => query.OrderByDescending(m => m.CodiceMarca),
                "descrizione" => query.OrderBy(m => m.DescrizioneMarca),
                "descrizione_desc" => query.OrderByDescending(m => m.DescrizioneMarca),
                "data" => query.OrderBy(m => m.UltimoAggiornamento),
                "data_desc" => query.OrderByDescending(m => m.UltimoAggiornamento),
                _ => query.OrderBy(m => m.CodiceMarca)
            };

            // Applica paginazione
            var marche = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calcola statistiche
            var stats = await GetStatsAsync();

            return new MarcheIndexViewModel
            {
                Marche = marche,
                Search = search,
                SortOrder = sortOrder,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                CodiceSortParm = sortOrder == "codice" ? "codice_desc" : "codice",
                DescrizioneSortParm = sortOrder == "descrizione" ? "descrizione_desc" : "descrizione",
                DataSortParm = sortOrder == "data" ? "data_desc" : "data",
                TotaleMarche = stats.TotaleMarche
            };
        }

        /// <summary>
        /// Ottiene una marca per ID
        /// </summary>
        public async Task<Marca?> GetMarcaByIdAsync(int id)
        {
            return await _context.Marche.FirstOrDefaultAsync(m => m.ID == id);
        }

        /// <summary>
        /// Ottiene i dettagli di una marca con navigazione precedente/successiva
        /// </summary>
        public async Task<MarcaDetailsViewModel?> GetMarcaDetailsAsync(int id)
        {
            var marca = await _context.Marche
                .FirstOrDefaultAsync(m => m.ID == id);

            if (marca == null)
                return null;

            var previousId = await _context.Marche
                .Where(m => m.CodiceMarca < marca.CodiceMarca ||
                           (m.CodiceMarca == marca.CodiceMarca && m.ID < id))
                .OrderByDescending(m => m.CodiceMarca)
                .ThenByDescending(m => m.ID)
                .Select(m => (int?)m.ID)
                .FirstOrDefaultAsync();

            var nextId = await _context.Marche
                .Where(m => m.CodiceMarca > marca.CodiceMarca ||
                           (m.CodiceMarca == marca.CodiceMarca && m.ID > id))
                .OrderBy(m => m.CodiceMarca)
                .ThenBy(m => m.ID)
                .Select(m => (int?)m.ID)
                .FirstOrDefaultAsync();

            return new MarcaDetailsViewModel
            {
                Marca = marca,
                PreviousId = previousId,
                NextId = nextId
            };
        }

        /// <summary>
        /// Crea una nuova marca
        /// </summary>
        public async Task<(bool Success, string Message, Marca? Marca)> CreateMarcaAsync(CreateMarcaViewModel model)
        {
            try
            {
                // Verifica unicità del CodiceMarca
                var esistente = await _context.Marche
                    .FirstOrDefaultAsync(m => m.CodiceMarca == model.CodiceMarca);

                if (esistente != null)
                {
                    return (false, $"Esiste già una marca con codice {model.CodiceMarca}.", null);
                }

                var marca = new Marca
                {
                    CodiceMarca = model.CodiceMarca,
                    DescrizioneMarca = model.DescrizioneMarca?.Trim(),
                    UltimoAggiornamento = DateTime.Now
                };

                _context.Marche.Add(marca);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Marca creata con successo - ID: {ID}, Codice: {Codice}, Descrizione: {Descrizione}",
                    marca.ID, marca.CodiceMarca, marca.DescrizioneMarca);

                return (true, "Marca creata con successo!", marca);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della marca - Codice: {Codice}",
                    model.CodiceMarca);
                return (false, $"Errore durante la creazione: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Aggiorna una marca esistente
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateMarcaAsync(EditMarcaViewModel model)
        {
            try
            {
                var marca = await GetMarcaByIdAsync(model.ID);
                if (marca == null)
                {
                    return (false, "Marca non trovata.");
                }

                // Verifica unicità del CodiceMarca (escluso l'elemento corrente)
                var esistente = await _context.Marche
                    .FirstOrDefaultAsync(m => m.CodiceMarca == model.CodiceMarca && m.ID != model.ID);

                if (esistente != null)
                {
                    return (false, $"Esiste già un'altra marca con codice {model.CodiceMarca}.");
                }

                marca.CodiceMarca = model.CodiceMarca;
                marca.DescrizioneMarca = model.DescrizioneMarca?.Trim();
                marca.UltimoAggiornamento = DateTime.Now;

                _context.Marche.Update(marca);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Marca modificata con successo - ID: {ID}, Codice: {Codice}",
                    marca.ID, marca.CodiceMarca);

                return (true, "Marca modificata con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la modifica della marca - ID: {ID}", model.ID);
                return (false, $"Errore durante la modifica: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina una marca
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteMarcaAsync(int id)
        {
            try
            {
                var marca = await _context.Marche
                    .FirstOrDefaultAsync(m => m.ID == id);

                if (marca == null)
                {
                    return (false, "Marca non trovata.");
                }

                _context.Marche.Remove(marca);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Marca eliminata con successo - ID: {ID}, Codice: {Codice}, Descrizione: {Descrizione}",
                    marca.ID, marca.CodiceMarca, marca.DescrizioneMarca);

                return (true, "Marca eliminata con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della marca - ID: {Id}", id);
                return (false, $"Errore durante l'eliminazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene tutte le marche per dropdown/selezioni API
        /// </summary>
        public async Task<IEnumerable<MarcaApiViewModel>> GetMarcheApiAsync()
        {
            return await _context.Marche
                .OrderBy(m => m.CodiceMarca)
                .Select(m => new MarcaApiViewModel
                {
                    ID = m.ID,
                    CodiceMarca = m.CodiceMarca,
                    DescrizioneMarca = m.DescrizioneMarca
                })
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene le statistiche delle marche
        /// </summary>
        public async Task<MarcheStatsViewModel> GetStatsAsync()
        {
            var totaleMarche = await _context.Marche.CountAsync();

            DateTime? dataUltimoAggiornamento = null;

            if (totaleMarche > 0)
            {
                dataUltimoAggiornamento = await _context.Marche
                    .OrderByDescending(m => m.UltimoAggiornamento)
                    .Select(m => m.UltimoAggiornamento)
                    .FirstOrDefaultAsync();
            }

            return new MarcheStatsViewModel
            {
                TotaleMarche = totaleMarche,
                DataUltimoAggiornamento = dataUltimoAggiornamento
            };
        }

        /// <summary>
        /// Verifica se una marca può essere eliminata
        /// (predisposto per futuri vincoli con altre tabelle)
        /// </summary>
        public async Task<(bool CanDelete, string Reason)> CanDeleteMarcaAsync(int id)
        {
            var marca = await _context.Marche.FirstOrDefaultAsync(m => m.ID == id);
            if (marca == null)
            {
                return (false, "Marca non trovata.");
            }

            // In futuro, qui si potranno aggiungere controlli su relazioni con altre tabelle
            // Esempio: verificare se ci sono articoli collegati a questa marca

            return (true, string.Empty);
        }
    }
}
