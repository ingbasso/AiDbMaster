using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la gestione degli stati degli ordini di produzione
    /// Fornisce metodi per le operazioni CRUD e logica business
    /// </summary>
    public class StatiOPService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StatiOPService> _logger;

        // Stati di sistema che non possono essere eliminati
        private readonly string[] _statiDiSistema = { "ES", "PR", "CH", "SO", "AN" };

        public StatiOPService(
            ApplicationDbContext context,
            ILogger<StatiOPService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene tutti gli stati OP con filtri e paginazione
        /// </summary>
        public async Task<StatiOPIndexViewModel> GetStatiOPAsync(
            string? search = null,
            bool? attivo = null,
            string? codiceStato = null,
            string sortOrder = "ordine",
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.StatiOP.AsQueryable();

            // Applica filtri
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => 
                    s.CodiceStato.Contains(search) ||
                    s.DescrizioneStato.Contains(search));
            }

            if (attivo.HasValue)
            {
                query = query.Where(s => s.Attivo == attivo.Value);
            }

            if (!string.IsNullOrEmpty(codiceStato))
            {
                query = query.Where(s => s.CodiceStato == codiceStato);
            }

            // Conta il totale prima dell'ordinamento e paginazione
            var totalCount = await query.CountAsync();

            // Applica ordinamento
            query = sortOrder switch
            {
                "codice" => query.OrderBy(s => s.CodiceStato),
                "codice_desc" => query.OrderByDescending(s => s.CodiceStato),
                "descrizione" => query.OrderBy(s => s.DescrizioneStato),
                "descrizione_desc" => query.OrderByDescending(s => s.DescrizioneStato),
                "attivo" => query.OrderBy(s => s.Attivo),
                "attivo_desc" => query.OrderByDescending(s => s.Attivo),
                "ordine" => query.OrderBy(s => s.Ordine).ThenBy(s => s.CodiceStato),
                "ordine_desc" => query.OrderByDescending(s => s.Ordine).ThenByDescending(s => s.CodiceStato),
                _ => query.OrderBy(s => s.Ordine).ThenBy(s => s.CodiceStato)
            };

            // Applica paginazione
            var statiOP = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calcola statistiche
            var stats = await GetStatsAsync();

            return new StatiOPIndexViewModel
            {
                StatiOP = statiOP,
                Search = search,
                Attivo = attivo,
                CodiceStato = codiceStato,
                SortOrder = sortOrder,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                CodiceSortParm = sortOrder == "codice" ? "codice_desc" : "codice",
                DescrizioneSortParm = sortOrder == "descrizione" ? "descrizione_desc" : "descrizione",
                AttivoSortParm = sortOrder == "attivo" ? "attivo_desc" : "attivo",
                OrdineSortParm = sortOrder == "ordine" ? "ordine_desc" : "ordine",
                TotaleStatiOP = stats.TotaleStatiOP,
                StatiOPAttivi = stats.StatiOPAttivi,
                StatiOPInattivi = stats.StatiOPInattivi,
                OrdiniTotali = stats.TotaleOrdiniProduzione
            };
        }

        /// <summary>
        /// Ottiene uno stato OP per ID
        /// </summary>
        public async Task<StatoOP?> GetStatoOPByIdAsync(int id)
        {
            return await _context.StatiOP.FindAsync(id);
        }

        /// <summary>
        /// Ottiene i dettagli di uno stato OP con informazioni aggiuntive
        /// </summary>
        public async Task<StatoOPDetailsViewModel?> GetStatoOPDetailsAsync(int id)
        {
            var statoOP = await _context.StatiOP
                .Include(s => s.OrdiniProduzione)
                .FirstOrDefaultAsync(s => s.IdStato == id);

            if (statoOP == null)
                return null;

            // Trova ID precedente e successivo per la navigazione (basato sull'ordine)
            var previousId = await _context.StatiOP
                .Where(s => s.Ordine < statoOP.Ordine || (s.Ordine == statoOP.Ordine && s.IdStato < id))
                .OrderByDescending(s => s.Ordine)
                .ThenByDescending(s => s.IdStato)
                .Select(s => s.IdStato)
                .FirstOrDefaultAsync();

            var nextId = await _context.StatiOP
                .Where(s => s.Ordine > statoOP.Ordine || (s.Ordine == statoOP.Ordine && s.IdStato > id))
                .OrderBy(s => s.Ordine)
                .ThenBy(s => s.IdStato)
                .Select(s => s.IdStato)
                .FirstOrDefaultAsync();

            // Statistiche ordini di produzione
            var ordiniAssegnati = statoOP.OrdiniProduzione.Count;
            var ordiniAttivi = statoOP.OrdiniProduzione.Count(o => o.IdStato == id);
            var ultimoOrdineAssegnato = statoOP.OrdiniProduzione.Any() ? 
                statoOP.OrdiniProduzione.Max(o => o.DataInizioOP) : (DateTime?)null;
            var primoOrdineAssegnato = statoOP.OrdiniProduzione.Any() ? 
                statoOP.OrdiniProduzione.Min(o => o.DataInizioOP) : (DateTime?)null;

            // Calcola percentuale utilizzo
            var totaleOrdini = await _context.ListaOP.CountAsync();
            var percentualeUtilizzo = totaleOrdini > 0 ? (double)ordiniAssegnati / totaleOrdini * 100 : 0;

            // Ordini ultimo mese e settimana
            var unMeseFa = DateTime.Now.AddMonths(-1);
            var unaSettimanaFa = DateTime.Now.AddDays(-7);
            var ordiniUltimoMese = statoOP.OrdiniProduzione.Count(o => o.DataInizioOP >= unMeseFa);
            var ordiniUltimaSettimana = statoOP.OrdiniProduzione.Count(o => o.DataInizioOP >= unaSettimanaFa);

            return new StatoOPDetailsViewModel
            {
                StatoOP = statoOP,
                PreviousId = previousId == 0 ? null : previousId,
                NextId = nextId == 0 ? null : nextId,
                OrdiniProduzioneAssegnati = ordiniAssegnati,
                OrdiniProduzioneAttivi = ordiniAttivi,
                UltimoOrdineAssegnato = ultimoOrdineAssegnato,
                PrimoOrdineAssegnato = primoOrdineAssegnato,
                PercentualeUtilizzo = percentualeUtilizzo,
                OrdiniUltimoMese = ordiniUltimoMese,
                OrdiniUltimaSettimana = ordiniUltimaSettimana
            };
        }

        /// <summary>
        /// Crea un nuovo stato OP
        /// </summary>
        public async Task<(bool Success, string Message, StatoOP? StatoOP)> CreateStatoOPAsync(CreateStatoOPViewModel model)
        {
            try
            {
                // Verifica unicità del codice stato
                var esistente = await _context.StatiOP
                    .FirstOrDefaultAsync(s => s.CodiceStato == model.CodiceStato);
                
                if (esistente != null)
                {
                    return (false, "Esiste già uno stato con questo codice.", null);
                }

                // Verifica unicità dell'ordine
                var ordineEsistente = await _context.StatiOP
                    .FirstOrDefaultAsync(s => s.Ordine == model.Ordine);
                
                if (ordineEsistente != null)
                {
                    return (false, $"Esiste già uno stato con ordine {model.Ordine}. Scegliere un ordine diverso.", null);
                }

                var statoOP = new StatoOP
                {
                    CodiceStato = model.CodiceStato.ToUpper().Trim(),
                    DescrizioneStato = model.DescrizioneStato.Trim(),
                    Ordine = model.Ordine,
                    Attivo = model.Attivo
                };

                _context.StatiOP.Add(statoOP);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stato OP creato con successo - ID: {Id}, Codice: {Codice}, Descrizione: {Descrizione}", 
                    statoOP.IdStato, statoOP.CodiceStato, statoOP.DescrizioneStato);

                return (true, "Stato OP creato con successo!", statoOP);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dello stato OP: {Codice}", 
                    model.CodiceStato);
                return (false, $"Errore durante la creazione: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Aggiorna uno stato OP esistente
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateStatoOPAsync(EditStatoOPViewModel model)
        {
            try
            {
                var statoOP = await GetStatoOPByIdAsync(model.IdStato);
                if (statoOP == null)
                {
                    return (false, "Stato OP non trovato.");
                }

                // Verifica unicità del codice stato
                var esistente = await _context.StatiOP
                    .FirstOrDefaultAsync(s => s.CodiceStato == model.CodiceStato && 
                                             s.IdStato != model.IdStato);
                
                if (esistente != null)
                {
                    return (false, "Esiste già uno stato con questo codice.");
                }

                // Verifica unicità dell'ordine
                var ordineEsistente = await _context.StatiOP
                    .FirstOrDefaultAsync(s => s.Ordine == model.Ordine && 
                                             s.IdStato != model.IdStato);
                
                if (ordineEsistente != null)
                {
                    return (false, $"Esiste già uno stato con ordine {model.Ordine}.");
                }

                statoOP.CodiceStato = model.CodiceStato.ToUpper().Trim();
                statoOP.DescrizioneStato = model.DescrizioneStato.Trim();
                statoOP.Ordine = model.Ordine;
                statoOP.Attivo = model.Attivo;

                _context.StatiOP.Update(statoOP);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stato OP modificato con successo - ID: {Id}, Codice: {Codice}, Descrizione: {Descrizione}", 
                    statoOP.IdStato, statoOP.CodiceStato, statoOP.DescrizioneStato);

                return (true, "Stato OP modificato con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la modifica dello stato OP - ID: {Id}", 
                    model.IdStato);
                return (false, $"Errore durante la modifica: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina uno stato OP
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteStatoOPAsync(int id)
        {
            try
            {
                var statoOP = await _context.StatiOP
                    .Include(s => s.OrdiniProduzione)
                    .FirstOrDefaultAsync(s => s.IdStato == id);

                if (statoOP == null)
                {
                    return (false, "Stato OP non trovato.");
                }

                // Verifica se è uno stato di sistema
                if (_statiDiSistema.Contains(statoOP.CodiceStato))
                {
                    return (false, $"Impossibile eliminare lo stato '{statoOP.CodiceStato}' perché è uno stato di sistema.");
                }

                // Verifica se ci sono ordini di produzione collegati
                if (statoOP.OrdiniProduzione.Any())
                {
                    return (false, $"Impossibile eliminare lo stato. Ci sono {statoOP.OrdiniProduzione.Count} ordini di produzione collegati.");
                }

                _context.StatiOP.Remove(statoOP);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stato OP eliminato con successo - ID: {Id}, Codice: {Codice}, Descrizione: {Descrizione}", 
                    statoOP.IdStato, statoOP.CodiceStato, statoOP.DescrizioneStato);

                return (true, "Stato OP eliminato con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dello stato OP - ID: {Id}", id);
                return (false, $"Errore durante l'eliminazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene gli stati OP attivi per dropdown/selezioni
        /// </summary>
        public async Task<IEnumerable<StatoOPApiViewModel>> GetStatiOPAttiviAsync()
        {
            return await _context.StatiOP
                .Where(s => s.Attivo)
                .OrderBy(s => s.Ordine)
                .ThenBy(s => s.CodiceStato)
                .Select(s => new StatoOPApiViewModel
                {
                    Id = s.IdStato,
                    Codice = s.CodiceStato,
                    Descrizione = s.DescrizioneStato,
                    Attivo = s.Attivo,
                    Ordine = s.Ordine
                })
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene le statistiche degli stati OP
        /// </summary>
        public async Task<StatiOPStatsViewModel> GetStatsAsync()
        {
            var totaleStatiOP = await _context.StatiOP.CountAsync();
            var statiOPAttivi = await _context.StatiOP.CountAsync(s => s.Attivo);
            var totaleOrdiniProduzione = await _context.ListaOP.CountAsync();

            // Distribuzione ordini per stato
            var distribuzioneOrdini = await _context.StatiOP
                .Include(s => s.OrdiniProduzione)
                .ToDictionaryAsync(s => s.CodiceStato, s => s.OrdiniProduzione.Count);

            var percentualeOrdini = new Dictionary<string, double>();
            if (totaleOrdiniProduzione > 0)
            {
                foreach (var kvp in distribuzioneOrdini)
                {
                    percentualeOrdini[kvp.Key] = (double)kvp.Value / totaleOrdiniProduzione * 100;
                }
            }

            // Stati più utilizzati
            var statiPiuUtilizzati = await _context.StatiOP
                .Include(s => s.OrdiniProduzione)
                .Where(s => s.OrdiniProduzione.Any())
                .OrderByDescending(s => s.OrdiniProduzione.Count)
                .Take(5)
                .Select(s => new StatoOPFrequencyViewModel
                {
                    IdStato = s.IdStato,
                    CodiceStato = s.CodiceStato,
                    DescrizioneStato = s.DescrizioneStato,
                    FrequenzaUtilizzo = s.OrdiniProduzione.Count,
                    UltimoUtilizzo = s.OrdiniProduzione.Any() ? s.OrdiniProduzione.Max(o => o.DataInizioOP) : null,
                    PrimoUtilizzo = s.OrdiniProduzione.Any() ? s.OrdiniProduzione.Min(o => o.DataInizioOP) : null,
                    Ordine = s.Ordine,
                    Attivo = s.Attivo,
                    PercentualeUtilizzo = totaleOrdiniProduzione > 0 ? (double)s.OrdiniProduzione.Count / totaleOrdiniProduzione * 100 : 0
                })
                .ToListAsync();

            // Stati meno utilizzati
            var statiMenoUtilizzati = await _context.StatiOP
                .Include(s => s.OrdiniProduzione)
                .OrderBy(s => s.OrdiniProduzione.Count)
                .Take(5)
                .Select(s => new StatoOPFrequencyViewModel
                {
                    IdStato = s.IdStato,
                    CodiceStato = s.CodiceStato,
                    DescrizioneStato = s.DescrizioneStato,
                    FrequenzaUtilizzo = s.OrdiniProduzione.Count,
                    UltimoUtilizzo = s.OrdiniProduzione.Any() ? s.OrdiniProduzione.Max(o => o.DataInizioOP) : null,
                    PrimoUtilizzo = s.OrdiniProduzione.Any() ? s.OrdiniProduzione.Min(o => o.DataInizioOP) : null,
                    Ordine = s.Ordine,
                    Attivo = s.Attivo,
                    PercentualeUtilizzo = totaleOrdiniProduzione > 0 ? (double)s.OrdiniProduzione.Count / totaleOrdiniProduzione * 100 : 0
                })
                .ToListAsync();

            var dataUltimoUtilizzo = await _context.ListaOP
                .OrderByDescending(o => o.DataInizioOP)
                .Select(o => o.DataInizioOP)
                .FirstOrDefaultAsync();

            var statoPiuUtilizzato = statiPiuUtilizzati.FirstOrDefault()?.CodiceStato;
            var statoMenoUtilizzato = statiMenoUtilizzati.LastOrDefault()?.CodiceStato;

            return new StatiOPStatsViewModel
            {
                TotaleStatiOP = totaleStatiOP,
                StatiOPAttivi = statiOPAttivi,
                StatiOPInattivi = totaleStatiOP - statiOPAttivi,
                TotaleOrdiniProduzione = totaleOrdiniProduzione,
                DistribuzioneOrdiniPerStato = distribuzioneOrdini,
                PercentualeOrdiniPerStato = percentualeOrdini,
                StatiPiuUtilizzati = statiPiuUtilizzati,
                StatiMenoUtilizzati = statiMenoUtilizzati,
                DataUltimoUtilizzo = dataUltimoUtilizzo,
                StatoPiuUtilizzato = statoPiuUtilizzato,
                StatoMenoUtilizzato = statoMenoUtilizzato
            };
        }

        /// <summary>
        /// Verifica se uno stato OP può essere eliminato
        /// </summary>
        public async Task<(bool CanDelete, string Reason, int RelatedCount)> CanDeleteStatoOPAsync(int id)
        {
            var statoOP = await _context.StatiOP.FindAsync(id);
            if (statoOP == null)
            {
                return (false, "Stato OP non trovato", 0);
            }

            // Verifica se è uno stato di sistema
            if (_statiDiSistema.Contains(statoOP.CodiceStato))
            {
                return (false, $"Lo stato '{statoOP.CodiceStato}' è uno stato di sistema e non può essere eliminato", 0);
            }

            var ordiniCount = await _context.StatiOP
                .Where(s => s.IdStato == id)
                .SelectMany(s => s.OrdiniProduzione)
                .CountAsync();

            if (ordiniCount > 0)
            {
                return (false, $"Lo stato ha {ordiniCount} ordini di produzione collegati", ordiniCount);
            }

            return (true, string.Empty, 0);
        }

        /// <summary>
        /// Attiva o disattiva uno stato OP
        /// </summary>
        public async Task<(bool Success, string Message)> ToggleAttivoAsync(int id)
        {
            try
            {
                var statoOP = await GetStatoOPByIdAsync(id);
                if (statoOP == null)
                {
                    return (false, "Stato OP non trovato.");
                }

                statoOP.Attivo = !statoOP.Attivo;

                _context.StatiOP.Update(statoOP);
                await _context.SaveChangesAsync();

                var stato = statoOP.Attivo ? "attivato" : "disattivato";
                _logger.LogInformation("Stato OP {Stato} - ID: {Id}, Codice: {Codice}, Descrizione: {Descrizione}", 
                    stato, statoOP.IdStato, statoOP.CodiceStato, statoOP.DescrizioneStato);

                return (true, $"Stato OP {stato} con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cambio stato dello stato OP - ID: {Id}", id);
                return (false, $"Errore durante il cambio stato: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene il riepilogo stati OP per dashboard
        /// </summary>
        public async Task<StatiOPSummaryViewModel> GetSummaryAsync()
        {
            var totaleStatiOP = await _context.StatiOP.CountAsync();
            var statiOPAttivi = await _context.StatiOP.CountAsync(s => s.Attivo);

            // Conta ordini per stato specifico (assumendo codici standard)
            var ordiniEmessi = await _context.ListaOP.CountAsync(o => o.Stato!.CodiceStato == "ES");
            var ordiniInProduzione = await _context.ListaOP.CountAsync(o => o.Stato!.CodiceStato == "PR");
            var ordiniChiusi = await _context.ListaOP.CountAsync(o => o.Stato!.CodiceStato == "CH");
            var ordiniSospesi = await _context.ListaOP.CountAsync(o => o.Stato!.CodiceStato == "SO");
            var ordiniAnnullati = await _context.ListaOP.CountAsync(o => o.Stato!.CodiceStato == "AN");

            // Calcola variazione ultimo mese (semplificato)
            var unMeseFa = DateTime.Now.AddMonths(-1);
            var ordiniUltimoMese = await _context.ListaOP.CountAsync(o => o.DataInizioOP >= unMeseFa);
            var ordiniMesePrecedente = await _context.ListaOP.CountAsync(o => o.DataInizioOP >= unMeseFa.AddMonths(-1) && o.DataInizioOP < unMeseFa);
            var variazione = ordiniUltimoMese - ordiniMesePrecedente;

            return new StatiOPSummaryViewModel
            {
                TotaleStatiOP = totaleStatiOP,
                StatiOPAttivi = statiOPAttivi,
                OrdiniEmessi = ordiniEmessi,
                OrdiniInProduzione = ordiniInProduzione,
                OrdiniChiusi = ordiniChiusi,
                OrdiniSospesi = ordiniSospesi,
                OrdiniAnnullati = ordiniAnnullati,
                VariazioneOrdiniUltimoMese = variazione
            };
        }

        /// <summary>
        /// Riordina gli stati OP
        /// </summary>
        public async Task<(bool Success, string Message)> ReorderStatiAsync(List<StatoOPOrderItem> stati)
        {
            try
            {
                foreach (var item in stati)
                {
                    var stato = await GetStatoOPByIdAsync(item.IdStato);
                    if (stato != null)
                    {
                        stato.Ordine = item.Ordine;
                        _context.StatiOP.Update(stato);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Riordinamento stati OP completato con successo");
                return (true, "Ordine degli stati aggiornato con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il riordinamento degli stati OP");
                return (false, $"Errore durante il riordinamento: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se un codice stato è di sistema
        /// </summary>
        public bool IsSystemState(string codiceStato)
        {
            return _statiDiSistema.Contains(codiceStato.ToUpper());
        }

        /// <summary>
        /// Ottiene il prossimo ordine disponibile
        /// </summary>
        public async Task<int> GetNextOrderAsync()
        {
            var maxOrder = await _context.StatiOP.MaxAsync(s => (int?)s.Ordine) ?? 0;
            return maxOrder + 1;
        }
    }
}
