using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la gestione dei centri di lavoro
    /// Fornisce metodi per le operazioni CRUD e logica business
    /// </summary>
    public class CentriLavoroService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CentriLavoroService> _logger;

        public CentriLavoroService(
            ApplicationDbContext context,
            ILogger<CentriLavoroService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene tutti i centri di lavoro con filtri e paginazione
        /// </summary>
        public async Task<CentriLavoroIndexViewModel> GetCentriLavoroAsync(
            string? search = null,
            bool? attivo = null,
            bool? hasCapacita = null,
            bool? hasCosto = null,
            string sortOrder = "descrizione",
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.CentriLavoro.AsQueryable();

            // Applica filtri
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => 
                    c.DescrizioneCentro.Contains(search) ||
                    (c.CodiceCentro != null && c.CodiceCentro.Contains(search)) ||
                    (c.Note != null && c.Note.Contains(search)));
            }

            if (attivo.HasValue)
            {
                query = query.Where(c => c.Attivo == attivo.Value);
            }

            if (hasCapacita.HasValue)
            {
                if (hasCapacita.Value)
                    query = query.Where(c => c.CapacitaOraria.HasValue && c.CapacitaOraria > 0);
                else
                    query = query.Where(c => !c.CapacitaOraria.HasValue || c.CapacitaOraria == 0);
            }

            if (hasCosto.HasValue)
            {
                if (hasCosto.Value)
                    query = query.Where(c => c.CostoOrarioStandard.HasValue && c.CostoOrarioStandard > 0);
                else
                    query = query.Where(c => !c.CostoOrarioStandard.HasValue || c.CostoOrarioStandard == 0);
            }

            // Conta il totale prima dell'ordinamento e paginazione
            var totalCount = await query.CountAsync();

            // Applica ordinamento
            query = sortOrder switch
            {
                "codice" => query.OrderBy(c => c.CodiceCentro),
                "codice_desc" => query.OrderByDescending(c => c.CodiceCentro),
                "descrizione" => query.OrderBy(c => c.DescrizioneCentro),
                "descrizione_desc" => query.OrderByDescending(c => c.DescrizioneCentro),
                "attivo" => query.OrderBy(c => c.Attivo),
                "attivo_desc" => query.OrderByDescending(c => c.Attivo),
                "capacita" => query.OrderBy(c => c.CapacitaOraria),
                "capacita_desc" => query.OrderByDescending(c => c.CapacitaOraria),
                "costo" => query.OrderBy(c => c.CostoOrarioStandard),
                "costo_desc" => query.OrderByDescending(c => c.CostoOrarioStandard),
                "data" => query.OrderBy(c => c.DataCreazione),
                "data_desc" => query.OrderByDescending(c => c.DataCreazione),
                _ => query.OrderBy(c => c.DescrizioneCentro)
            };

            // Applica paginazione
            var centriLavoro = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calcola statistiche
            var stats = await GetStatsAsync();

            return new CentriLavoroIndexViewModel
            {
                CentriLavoro = centriLavoro,
                Search = search,
                Attivo = attivo,
                HasCapacita = hasCapacita,
                HasCosto = hasCosto,
                SortOrder = sortOrder,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                CodiceSortParm = sortOrder == "codice" ? "codice_desc" : "codice",
                DescrizioneSortParm = sortOrder == "descrizione" ? "descrizione_desc" : "descrizione",
                AttivoSortParm = sortOrder == "attivo" ? "attivo_desc" : "attivo",
                CapacitaSortParm = sortOrder == "capacita" ? "capacita_desc" : "capacita",
                CostoSortParm = sortOrder == "costo" ? "costo_desc" : "costo",
                DataSortParm = sortOrder == "data" ? "data_desc" : "data",
                TotaleCentriLavoro = stats.TotaleCentriLavoro,
                CentriLavoroAttivi = stats.CentriLavoroAttivi,
                CentriLavoroInattivi = stats.CentriLavoroInattivi,
                CentriConCapacita = stats.CentriConCapacita,
                CentriConCosto = stats.CentriConCosto
            };
        }

        /// <summary>
        /// Ottiene un centro di lavoro per ID
        /// </summary>
        public async Task<CentroLavoro?> GetCentroLavoroByIdAsync(int id)
        {
            return await _context.CentriLavoro.FindAsync(id);
        }

        /// <summary>
        /// Ottiene i dettagli di un centro di lavoro con informazioni aggiuntive
        /// </summary>
        public async Task<CentroLavoroDetailsViewModel?> GetCentroLavoroDetailsAsync(int id)
        {
            var centroLavoro = await _context.CentriLavoro
                .Include(c => c.OrdiniProduzione)
                .FirstOrDefaultAsync(c => c.IdCentroLavoro == id);

            if (centroLavoro == null)
                return null;

            // Trova ID precedente e successivo per la navigazione
            var previousId = await _context.CentriLavoro
                .Where(c => c.IdCentroLavoro < id)
                .OrderByDescending(c => c.IdCentroLavoro)
                .Select(c => c.IdCentroLavoro)
                .FirstOrDefaultAsync();

            var nextId = await _context.CentriLavoro
                .Where(c => c.IdCentroLavoro > id)
                .OrderBy(c => c.IdCentroLavoro)
                .Select(c => c.IdCentroLavoro)
                .FirstOrDefaultAsync();

            // Statistiche ordini di produzione
            var ordiniAssegnati = centroLavoro.OrdiniProduzione.Count;
            var ordiniAttivi = centroLavoro.OrdiniProduzione.Count(o => o.IdStato == 2); // Stato "Produzione"
            var ordiniCompletati = centroLavoro.OrdiniProduzione.Count(o => o.IdStato == 3); // Stato "Chiuso"

            return new CentroLavoroDetailsViewModel
            {
                CentroLavoro = centroLavoro,
                PreviousId = previousId == 0 ? null : previousId,
                NextId = nextId == 0 ? null : nextId,
                OrdiniProduzioneAssegnati = ordiniAssegnati,
                OrdiniProduzioneAttivi = ordiniAttivi,
                OrdiniProduzioneCompletati = ordiniCompletati
            };
        }

        /// <summary>
        /// Crea un nuovo centro di lavoro
        /// </summary>
        public async Task<(bool Success, string Message, CentroLavoro? CentroLavoro)> CreateCentroLavoroAsync(CreateCentroLavoroViewModel model)
        {
            try
            {
                // Verifica unicità del codice se specificato
                if (!string.IsNullOrEmpty(model.CodiceCentro))
                {
                    var esistente = await _context.CentriLavoro
                        .FirstOrDefaultAsync(c => c.CodiceCentro == model.CodiceCentro);
                    
                    if (esistente != null)
                    {
                        return (false, "Esiste già un centro di lavoro con questo codice.", null);
                    }
                }

                var centroLavoro = new CentroLavoro
                {
                    CodiceCentro = model.CodiceCentro?.ToUpper(),
                    DescrizioneCentro = model.DescrizioneCentro.Trim(),
                    CapacitaOraria = model.CapacitaOraria,
                    CostoOrarioStandard = model.CostoOrarioStandard,
                    Note = model.Note?.Trim(),
                    Attivo = model.Attivo,
                    DataCreazione = DateTime.Now
                };

                _context.CentriLavoro.Add(centroLavoro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Centro di lavoro creato con successo - ID: {Id}, Descrizione: {Descrizione}", 
                    centroLavoro.IdCentroLavoro, centroLavoro.DescrizioneCentro);

                return (true, "Centro di lavoro creato con successo!", centroLavoro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del centro di lavoro: {Descrizione}", 
                    model.DescrizioneCentro);
                return (false, $"Errore durante la creazione: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Aggiorna un centro di lavoro esistente
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateCentroLavoroAsync(EditCentroLavoroViewModel model)
        {
            try
            {
                var centroLavoro = await GetCentroLavoroByIdAsync(model.IdCentroLavoro);
                if (centroLavoro == null)
                {
                    return (false, "Centro di lavoro non trovato.");
                }

                // Verifica unicità del codice se specificato
                if (!string.IsNullOrEmpty(model.CodiceCentro))
                {
                    var esistente = await _context.CentriLavoro
                        .FirstOrDefaultAsync(c => c.CodiceCentro == model.CodiceCentro && 
                                                 c.IdCentroLavoro != model.IdCentroLavoro);
                    
                    if (esistente != null)
                    {
                        return (false, "Esiste già un centro di lavoro con questo codice.");
                    }
                }

                centroLavoro.CodiceCentro = model.CodiceCentro?.ToUpper();
                centroLavoro.DescrizioneCentro = model.DescrizioneCentro.Trim();
                centroLavoro.CapacitaOraria = model.CapacitaOraria;
                centroLavoro.CostoOrarioStandard = model.CostoOrarioStandard;
                centroLavoro.Note = model.Note?.Trim();
                centroLavoro.Attivo = model.Attivo;
                centroLavoro.DataUltimaModifica = DateTime.Now;

                _context.CentriLavoro.Update(centroLavoro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Centro di lavoro modificato con successo - ID: {Id}, Descrizione: {Descrizione}", 
                    centroLavoro.IdCentroLavoro, centroLavoro.DescrizioneCentro);

                return (true, "Centro di lavoro modificato con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la modifica del centro di lavoro - ID: {Id}", 
                    model.IdCentroLavoro);
                return (false, $"Errore durante la modifica: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina un centro di lavoro
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteCentroLavoroAsync(int id)
        {
            try
            {
                var centroLavoro = await _context.CentriLavoro
                    .Include(c => c.OrdiniProduzione)
                    .FirstOrDefaultAsync(c => c.IdCentroLavoro == id);

                if (centroLavoro == null)
                {
                    return (false, "Centro di lavoro non trovato.");
                }

                // Verifica se ci sono ordini di produzione collegati
                if (centroLavoro.OrdiniProduzione.Any())
                {
                    return (false, $"Impossibile eliminare il centro di lavoro. Ci sono {centroLavoro.OrdiniProduzione.Count} ordini di produzione collegati.");
                }

                _context.CentriLavoro.Remove(centroLavoro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Centro di lavoro eliminato con successo - ID: {Id}, Descrizione: {Descrizione}", 
                    centroLavoro.IdCentroLavoro, centroLavoro.DescrizioneCentro);

                return (true, "Centro di lavoro eliminato con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del centro di lavoro - ID: {Id}", id);
                return (false, $"Errore durante l'eliminazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene i centri di lavoro attivi per dropdown/selezioni
        /// </summary>
        public async Task<IEnumerable<CentroLavoroApiViewModel>> GetCentriLavoroAttiviAsync()
        {
            return await _context.CentriLavoro
                .Where(c => c.Attivo)
                .OrderBy(c => c.DescrizioneCentro)
                .Select(c => new CentroLavoroApiViewModel
                {
                    Id = c.IdCentroLavoro,
                    Codice = c.CodiceCentro,
                    Descrizione = c.DescrizioneCentro,
                    Attivo = c.Attivo,
                    CapacitaOraria = c.CapacitaOraria,
                    CostoOrarioStandard = c.CostoOrarioStandard
                })
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene le statistiche dei centri di lavoro
        /// </summary>
        public async Task<CentriLavoroStatsViewModel> GetStatsAsync()
        {
            var totaleCentriLavoro = await _context.CentriLavoro.CountAsync();
            var centriLavoroAttivi = await _context.CentriLavoro.CountAsync(c => c.Attivo);
            var centriConCapacita = await _context.CentriLavoro.CountAsync(c => c.CapacitaOraria.HasValue && c.CapacitaOraria > 0);
            var centriConCosto = await _context.CentriLavoro.CountAsync(c => c.CostoOrarioStandard.HasValue && c.CostoOrarioStandard > 0);
            var centriConCodice = await _context.CentriLavoro.CountAsync(c => c.CodiceCentro != null);

            var capacitaMediaOraria = await _context.CentriLavoro
                .Where(c => c.CapacitaOraria.HasValue && c.CapacitaOraria > 0)
                .AverageAsync(c => (decimal?)c.CapacitaOraria);

            var costoMedioOrario = await _context.CentriLavoro
                .Where(c => c.CostoOrarioStandard.HasValue && c.CostoOrarioStandard > 0)
                .AverageAsync(c => c.CostoOrarioStandard);

            var capacitaTotaleOraria = await _context.CentriLavoro
                .Where(c => c.CapacitaOraria.HasValue && c.CapacitaOraria > 0)
                .SumAsync(c => (decimal?)c.CapacitaOraria);

            var dataUltimaCreazione = await _context.CentriLavoro
                .OrderByDescending(c => c.DataCreazione)
                .Select(c => c.DataCreazione)
                .FirstOrDefaultAsync();

            var dataUltimaModifica = await _context.CentriLavoro
                .Where(c => c.DataUltimaModifica != null)
                .OrderByDescending(c => c.DataUltimaModifica)
                .Select(c => c.DataUltimaModifica)
                .FirstOrDefaultAsync();

            var centriPiuRecenti = await _context.CentriLavoro
                .OrderByDescending(c => c.DataCreazione)
                .Take(5)
                .Select(c => new CentroLavoroFrequencyViewModel
                {
                    IdCentroLavoro = c.IdCentroLavoro,
                    CodiceCentro = c.CodiceCentro,
                    DescrizioneCentro = c.DescrizioneCentro,
                    UltimoUtilizzo = c.DataCreazione,
                    CapacitaOraria = c.CapacitaOraria,
                    CostoOrarioStandard = c.CostoOrarioStandard
                })
                .ToListAsync();

            // Centri più utilizzati (basato sul numero di ordini di produzione)
            var centriPiuUtilizzati = await _context.CentriLavoro
                .Include(c => c.OrdiniProduzione)
                .Where(c => c.OrdiniProduzione.Any())
                .OrderByDescending(c => c.OrdiniProduzione.Count)
                .Take(5)
                .Select(c => new CentroLavoroFrequencyViewModel
                {
                    IdCentroLavoro = c.IdCentroLavoro,
                    CodiceCentro = c.CodiceCentro,
                    DescrizioneCentro = c.DescrizioneCentro,
                    FrequenzaUtilizzo = c.OrdiniProduzione.Count,
                    UltimoUtilizzo = c.OrdiniProduzione.Any() ? c.OrdiniProduzione.Max(o => o.DataInizioOP) : c.DataCreazione,
                    CapacitaOraria = c.CapacitaOraria,
                    CostoOrarioStandard = c.CostoOrarioStandard
                })
                .ToListAsync();

            return new CentriLavoroStatsViewModel
            {
                TotaleCentriLavoro = totaleCentriLavoro,
                CentriLavoroAttivi = centriLavoroAttivi,
                CentriLavoroInattivi = totaleCentriLavoro - centriLavoroAttivi,
                CentriConCapacita = centriConCapacita,
                CentriConCosto = centriConCosto,
                CentriConCodice = centriConCodice,
                CapacitaMediaOraria = capacitaMediaOraria,
                CostoMedioOrario = costoMedioOrario,
                CapacitaTotaleOraria = capacitaTotaleOraria,
                DataUltimaCreazione = dataUltimaCreazione,
                DataUltimaModifica = dataUltimaModifica,
                CentriPiuUtilizzati = centriPiuUtilizzati,
                CentriPiuRecenti = centriPiuRecenti
            };
        }

        /// <summary>
        /// Verifica se un centro di lavoro può essere eliminato
        /// </summary>
        public async Task<(bool CanDelete, string Reason, int RelatedCount)> CanDeleteCentroLavoroAsync(int id)
        {
            var ordiniCount = await _context.CentriLavoro
                .Where(c => c.IdCentroLavoro == id)
                .SelectMany(c => c.OrdiniProduzione)
                .CountAsync();

            if (ordiniCount > 0)
            {
                return (false, $"Il centro di lavoro ha {ordiniCount} ordini di produzione collegati", ordiniCount);
            }

            return (true, string.Empty, 0);
        }

        /// <summary>
        /// Attiva o disattiva un centro di lavoro
        /// </summary>
        public async Task<(bool Success, string Message)> ToggleAttivoAsync(int id)
        {
            try
            {
                var centroLavoro = await GetCentroLavoroByIdAsync(id);
                if (centroLavoro == null)
                {
                    return (false, "Centro di lavoro non trovato.");
                }

                centroLavoro.Attivo = !centroLavoro.Attivo;
                centroLavoro.DataUltimaModifica = DateTime.Now;

                _context.CentriLavoro.Update(centroLavoro);
                await _context.SaveChangesAsync();

                var stato = centroLavoro.Attivo ? "attivato" : "disattivato";
                _logger.LogInformation("Centro di lavoro {Stato} - ID: {Id}, Descrizione: {Descrizione}", 
                    stato, centroLavoro.IdCentroLavoro, centroLavoro.DescrizioneCentro);

                return (true, $"Centro di lavoro {stato} con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cambio stato del centro di lavoro - ID: {Id}", id);
                return (false, $"Errore durante il cambio stato: {ex.Message}");
            }
        }
    }
}
