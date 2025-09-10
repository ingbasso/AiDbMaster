using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la gestione delle lavorazioni
    /// Fornisce metodi per le operazioni CRUD e logica business
    /// </summary>
    public class LavorazioniService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LavorazioniService> _logger;

        public LavorazioniService(
            ApplicationDbContext context,
            ILogger<LavorazioniService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene tutte le lavorazioni con filtri e paginazione
        /// </summary>
        public async Task<LavorazioniIndexViewModel> GetLavorazioniAsync(
            string? search = null,
            bool? attivo = null,
            string sortOrder = "descrizione",
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.Lavorazioni.AsQueryable();

            // Applica filtri
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => 
                    l.DescrizioneLavorazione.Contains(search) ||
                    (l.CodiceLavorazione != null && l.CodiceLavorazione.Contains(search)));
            }

            if (attivo.HasValue)
            {
                query = query.Where(l => l.Attivo == attivo.Value);
            }

            // Conta il totale prima dell'ordinamento e paginazione
            var totalCount = await query.CountAsync();

            // Applica ordinamento
            query = sortOrder switch
            {
                "codice" => query.OrderBy(l => l.CodiceLavorazione),
                "codice_desc" => query.OrderByDescending(l => l.CodiceLavorazione),
                "descrizione" => query.OrderBy(l => l.DescrizioneLavorazione),
                "descrizione_desc" => query.OrderByDescending(l => l.DescrizioneLavorazione),
                "attivo" => query.OrderBy(l => l.Attivo),
                "attivo_desc" => query.OrderByDescending(l => l.Attivo),
                "data" => query.OrderBy(l => l.DataCreazione),
                "data_desc" => query.OrderByDescending(l => l.DataCreazione),
                _ => query.OrderBy(l => l.DescrizioneLavorazione)
            };

            // Applica paginazione
            var lavorazioni = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calcola statistiche
            var stats = await GetStatsAsync();

            return new LavorazioniIndexViewModel
            {
                Lavorazioni = lavorazioni,
                Search = search,
                Attivo = attivo,
                SortOrder = sortOrder,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                CodiceSortParm = sortOrder == "codice" ? "codice_desc" : "codice",
                DescrizioneSortParm = sortOrder == "descrizione" ? "descrizione_desc" : "descrizione",
                AttivoSortParm = sortOrder == "attivo" ? "attivo_desc" : "attivo",
                DataSortParm = sortOrder == "data" ? "data_desc" : "data",
                TotaleLavorazioni = stats.TotaleLavorazioni,
                LavorazioniAttive = stats.LavorazioniAttive,
                LavorazioniInattive = stats.LavorazioniInattive
            };
        }

        /// <summary>
        /// Ottiene una lavorazione per ID
        /// </summary>
        public async Task<Lavorazioni?> GetLavorazioneByIdAsync(int id)
        {
            return await _context.Lavorazioni.FindAsync(id);
        }

        /// <summary>
        /// Ottiene i dettagli di una lavorazione con informazioni aggiuntive
        /// </summary>
        public async Task<LavorazioneDetailsViewModel?> GetLavorazioneDetailsAsync(int id)
        {
            var lavorazione = await GetLavorazioneByIdAsync(id);
            if (lavorazione == null)
                return null;

            // Trova ID precedente e successivo per la navigazione
            var previousId = await _context.Lavorazioni
                .Where(l => l.IdLavorazione < id)
                .OrderByDescending(l => l.IdLavorazione)
                .Select(l => l.IdLavorazione)
                .FirstOrDefaultAsync();

            var nextId = await _context.Lavorazioni
                .Where(l => l.IdLavorazione > id)
                .OrderBy(l => l.IdLavorazione)
                .Select(l => l.IdLavorazione)
                .FirstOrDefaultAsync();

            return new LavorazioneDetailsViewModel
            {
                Lavorazione = lavorazione,
                PreviousId = previousId == 0 ? null : previousId,
                NextId = nextId == 0 ? null : nextId
            };
        }

        /// <summary>
        /// Crea una nuova lavorazione
        /// </summary>
        public async Task<(bool Success, string Message, Lavorazioni? Lavorazione)> CreateLavorazioneAsync(CreateLavorazioneViewModel model)
        {
            try
            {
                // Verifica unicità del codice se specificato
                if (!string.IsNullOrEmpty(model.CodiceLavorazione))
                {
                    var esistente = await _context.Lavorazioni
                        .FirstOrDefaultAsync(l => l.CodiceLavorazione == model.CodiceLavorazione);
                    
                    if (esistente != null)
                    {
                        return (false, "Esiste già una lavorazione con questo codice.", null);
                    }
                }

                var lavorazione = new Lavorazioni
                {
                    CodiceLavorazione = model.CodiceLavorazione?.ToUpper(),
                    DescrizioneLavorazione = model.DescrizioneLavorazione.Trim(),
                    Attivo = model.Attivo,
                    DataCreazione = DateTime.Now
                };

                _context.Lavorazioni.Add(lavorazione);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lavorazione creata con successo - ID: {Id}, Descrizione: {Descrizione}", 
                    lavorazione.IdLavorazione, lavorazione.DescrizioneLavorazione);

                return (true, "Lavorazione creata con successo!", lavorazione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della lavorazione: {Descrizione}", 
                    model.DescrizioneLavorazione);
                return (false, $"Errore durante la creazione: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Aggiorna una lavorazione esistente
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateLavorazioneAsync(EditLavorazioneViewModel model)
        {
            try
            {
                var lavorazione = await GetLavorazioneByIdAsync(model.IdLavorazione);
                if (lavorazione == null)
                {
                    return (false, "Lavorazione non trovata.");
                }

                // Verifica unicità del codice se specificato
                if (!string.IsNullOrEmpty(model.CodiceLavorazione))
                {
                    var esistente = await _context.Lavorazioni
                        .FirstOrDefaultAsync(l => l.CodiceLavorazione == model.CodiceLavorazione && 
                                                 l.IdLavorazione != model.IdLavorazione);
                    
                    if (esistente != null)
                    {
                        return (false, "Esiste già una lavorazione con questo codice.");
                    }
                }

                lavorazione.CodiceLavorazione = model.CodiceLavorazione?.ToUpper();
                lavorazione.DescrizioneLavorazione = model.DescrizioneLavorazione.Trim();
                lavorazione.Attivo = model.Attivo;
                lavorazione.DataUltimaModifica = DateTime.Now;

                _context.Lavorazioni.Update(lavorazione);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lavorazione modificata con successo - ID: {Id}, Descrizione: {Descrizione}", 
                    lavorazione.IdLavorazione, lavorazione.DescrizioneLavorazione);

                return (true, "Lavorazione modificata con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la modifica della lavorazione - ID: {Id}", 
                    model.IdLavorazione);
                return (false, $"Errore durante la modifica: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina una lavorazione
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteLavorazioneAsync(int id)
        {
            try
            {
                var lavorazione = await GetLavorazioneByIdAsync(id);
                if (lavorazione == null)
                {
                    return (false, "Lavorazione non trovata.");
                }

                // TODO: Verificare se ci sono dati collegati prima dell'eliminazione
                // Ad esempio, ordini di produzione che utilizzano questa lavorazione

                _context.Lavorazioni.Remove(lavorazione);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lavorazione eliminata con successo - ID: {Id}, Descrizione: {Descrizione}", 
                    lavorazione.IdLavorazione, lavorazione.DescrizioneLavorazione);

                return (true, "Lavorazione eliminata con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della lavorazione - ID: {Id}", id);
                return (false, $"Errore durante l'eliminazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene le lavorazioni attive per dropdown/selezioni
        /// </summary>
        public async Task<IEnumerable<LavorazioneApiViewModel>> GetLavorazioniAttiveAsync()
        {
            return await _context.Lavorazioni
                .Where(l => l.Attivo)
                .OrderBy(l => l.DescrizioneLavorazione)
                .Select(l => new LavorazioneApiViewModel
                {
                    Id = l.IdLavorazione,
                    Codice = l.CodiceLavorazione,
                    Descrizione = l.DescrizioneLavorazione,
                    Attivo = l.Attivo
                })
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene le statistiche delle lavorazioni
        /// </summary>
        public async Task<LavorazioniStatsViewModel> GetStatsAsync()
        {
            var totaleLavorazioni = await _context.Lavorazioni.CountAsync();
            var lavorazioniAttive = await _context.Lavorazioni.CountAsync(l => l.Attivo);
            var lavorazioniConCodice = await _context.Lavorazioni.CountAsync(l => l.CodiceLavorazione != null);

            var dataUltimaCreazione = await _context.Lavorazioni
                .OrderByDescending(l => l.DataCreazione)
                .Select(l => l.DataCreazione)
                .FirstOrDefaultAsync();

            var dataUltimaModifica = await _context.Lavorazioni
                .Where(l => l.DataUltimaModifica != null)
                .OrderByDescending(l => l.DataUltimaModifica)
                .Select(l => l.DataUltimaModifica)
                .FirstOrDefaultAsync();

            var lavorazioniRecenti = await _context.Lavorazioni
                .OrderByDescending(l => l.DataCreazione)
                .Take(5)
                .Select(l => new LavorazioneFrequencyViewModel
                {
                    IdLavorazione = l.IdLavorazione,
                    CodiceLavorazione = l.CodiceLavorazione,
                    DescrizioneLavorazione = l.DescrizioneLavorazione,
                    UltimoUtilizzo = l.DataCreazione
                })
                .ToListAsync();

            return new LavorazioniStatsViewModel
            {
                TotaleLavorazioni = totaleLavorazioni,
                LavorazioniAttive = lavorazioniAttive,
                LavorazioniInattive = totaleLavorazioni - lavorazioniAttive,
                LavorazioniConCodice = lavorazioniConCodice,
                LavorazioniSenzaCodice = totaleLavorazioni - lavorazioniConCodice,
                DataUltimaCreazione = dataUltimaCreazione,
                DataUltimaModifica = dataUltimaModifica,
                LavorazioniPiuRecenti = lavorazioniRecenti
            };
        }

        /// <summary>
        /// Verifica se una lavorazione può essere eliminata
        /// </summary>
        public Task<(bool CanDelete, string Reason, int RelatedCount)> CanDeleteLavorazioneAsync(int id)
        {
            // TODO: Implementare controlli per dati collegati
            // Ad esempio, verificare se ci sono ordini di produzione che utilizzano questa lavorazione
            
            // Per ora restituisce sempre true
            return Task.FromResult((true, string.Empty, 0));
        }

        /// <summary>
        /// Attiva o disattiva una lavorazione
        /// </summary>
        public async Task<(bool Success, string Message)> ToggleAttivoAsync(int id)
        {
            try
            {
                var lavorazione = await GetLavorazioneByIdAsync(id);
                if (lavorazione == null)
                {
                    return (false, "Lavorazione non trovata.");
                }

                lavorazione.Attivo = !lavorazione.Attivo;
                lavorazione.DataUltimaModifica = DateTime.Now;

                _context.Lavorazioni.Update(lavorazione);
                await _context.SaveChangesAsync();

                var stato = lavorazione.Attivo ? "attivata" : "disattivata";
                _logger.LogInformation("Lavorazione {Stato} - ID: {Id}, Descrizione: {Descrizione}", 
                    stato, lavorazione.IdLavorazione, lavorazione.DescrizioneLavorazione);

                return (true, $"Lavorazione {stato} con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cambio stato della lavorazione - ID: {Id}", id);
                return (false, $"Errore durante il cambio stato: {ex.Message}");
            }
        }
    }
}
