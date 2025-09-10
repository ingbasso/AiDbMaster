using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la gestione degli operatori
    /// Fornisce metodi per le operazioni CRUD e logica business
    /// </summary>
    public class OperatoriService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OperatoriService> _logger;

        public OperatoriService(
            ApplicationDbContext context,
            ILogger<OperatoriService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene tutti gli operatori con filtri e paginazione
        /// </summary>
        public async Task<OperatoriIndexViewModel> GetOperatoriAsync(
            string? search = null,
            bool? attivo = null,
            int? livelloCompetenza = null,
            bool? hasEmail = null,
            bool? hasTelefono = null,
            DateTime? dataAssunzioneDa = null,
            DateTime? dataAssunzioneA = null,
            string sortOrder = "cognome",
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.Operatori.AsQueryable();

            // Applica filtri
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => 
                    o.CodiceOperatore.Contains(search) ||
                    o.Nome.Contains(search) ||
                    o.Cognome.Contains(search) ||
                    (o.Email != null && o.Email.Contains(search)) ||
                    (o.Telefono != null && o.Telefono.Contains(search)) ||
                    (o.Note != null && o.Note.Contains(search)));
            }

            if (attivo.HasValue)
            {
                query = query.Where(o => o.Attivo == attivo.Value);
            }

            if (livelloCompetenza.HasValue)
            {
                query = query.Where(o => o.LivelloCompetenza == livelloCompetenza.Value);
            }

            if (hasEmail.HasValue)
            {
                if (hasEmail.Value)
                    query = query.Where(o => !string.IsNullOrEmpty(o.Email));
                else
                    query = query.Where(o => string.IsNullOrEmpty(o.Email));
            }

            if (hasTelefono.HasValue)
            {
                if (hasTelefono.Value)
                    query = query.Where(o => !string.IsNullOrEmpty(o.Telefono));
                else
                    query = query.Where(o => string.IsNullOrEmpty(o.Telefono));
            }

            if (dataAssunzioneDa.HasValue)
            {
                query = query.Where(o => o.DataAssunzione >= dataAssunzioneDa.Value);
            }

            if (dataAssunzioneA.HasValue)
            {
                query = query.Where(o => o.DataAssunzione <= dataAssunzioneA.Value);
            }

            // Conta il totale prima dell'ordinamento e paginazione
            var totalCount = await query.CountAsync();

            // Applica ordinamento
            query = sortOrder switch
            {
                "codice" => query.OrderBy(o => o.CodiceOperatore),
                "codice_desc" => query.OrderByDescending(o => o.CodiceOperatore),
                "nome" => query.OrderBy(o => o.Nome).ThenBy(o => o.Cognome),
                "nome_desc" => query.OrderByDescending(o => o.Nome).ThenByDescending(o => o.Cognome),
                "cognome" => query.OrderBy(o => o.Cognome).ThenBy(o => o.Nome),
                "cognome_desc" => query.OrderByDescending(o => o.Cognome).ThenByDescending(o => o.Nome),
                "email" => query.OrderBy(o => o.Email),
                "email_desc" => query.OrderByDescending(o => o.Email),
                "attivo" => query.OrderBy(o => o.Attivo),
                "attivo_desc" => query.OrderByDescending(o => o.Attivo),
                "livello" => query.OrderBy(o => o.LivelloCompetenza),
                "livello_desc" => query.OrderByDescending(o => o.LivelloCompetenza),
                "data_assunzione" => query.OrderBy(o => o.DataAssunzione),
                "data_assunzione_desc" => query.OrderByDescending(o => o.DataAssunzione),
                _ => query.OrderBy(o => o.Cognome).ThenBy(o => o.Nome)
            };

            // Applica paginazione
            var operatori = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calcola statistiche
            var stats = await GetStatsAsync();

            return new OperatoriIndexViewModel
            {
                Operatori = operatori,
                Search = search,
                Attivo = attivo,
                LivelloCompetenza = livelloCompetenza,
                HasEmail = hasEmail,
                HasTelefono = hasTelefono,
                DataAssunzioneDa = dataAssunzioneDa,
                DataAssunzioneA = dataAssunzioneA,
                SortOrder = sortOrder,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                CodiceSortParm = sortOrder == "codice" ? "codice_desc" : "codice",
                NomeSortParm = sortOrder == "nome" ? "nome_desc" : "nome",
                CognomeSortParm = sortOrder == "cognome" ? "cognome_desc" : "cognome",
                EmailSortParm = sortOrder == "email" ? "email_desc" : "email",
                AttivoSortParm = sortOrder == "attivo" ? "attivo_desc" : "attivo",
                LivelloSortParm = sortOrder == "livello" ? "livello_desc" : "livello",
                DataAssunzioneSortParm = sortOrder == "data_assunzione" ? "data_assunzione_desc" : "data_assunzione",
                TotaleOperatori = stats.TotaleOperatori,
                OperatoriAttivi = stats.OperatoriAttivi,
                OperatoriInattivi = stats.OperatoriInattivi,
                OperatoriConEmail = stats.OperatoriConEmail,
                OperatoriConTelefono = stats.OperatoriConTelefono,
                LivelloCompetenzaMedia = stats.LivelloCompetenzaMedia
            };
        }

        /// <summary>
        /// Ottiene un operatore per ID
        /// </summary>
        public async Task<Operatore?> GetOperatoreByIdAsync(int id)
        {
            return await _context.Operatori.FindAsync(id);
        }

        /// <summary>
        /// Ottiene i dettagli di un operatore con informazioni aggiuntive
        /// </summary>
        public async Task<OperatoreDetailsViewModel?> GetOperatoreDetailsAsync(int id)
        {
            var operatore = await _context.Operatori
                .Include(o => o.OrdiniProduzione)
                .FirstOrDefaultAsync(o => o.IdOperatore == id);

            if (operatore == null)
                return null;

            // Trova ID precedente e successivo per la navigazione
            var previousId = await _context.Operatori
                .Where(o => o.IdOperatore < id)
                .OrderByDescending(o => o.IdOperatore)
                .Select(o => o.IdOperatore)
                .FirstOrDefaultAsync();

            var nextId = await _context.Operatori
                .Where(o => o.IdOperatore > id)
                .OrderBy(o => o.IdOperatore)
                .Select(o => o.IdOperatore)
                .FirstOrDefaultAsync();

            // Statistiche ordini di produzione
            var ordiniAssegnati = operatore.OrdiniProduzione.Count;
            var ordiniAttivi = operatore.OrdiniProduzione.Count(o => o.IdStato == 2); // Stato "Produzione"
            var ordiniCompletati = operatore.OrdiniProduzione.Count(o => o.IdStato == 3); // Stato "Chiuso"
            var ultimoOrdineAssegnato = operatore.OrdiniProduzione.Any() ? 
                operatore.OrdiniProduzione.Max(o => o.DataInizioOP) : (DateTime?)null;

            return new OperatoreDetailsViewModel
            {
                Operatore = operatore,
                PreviousId = previousId == 0 ? null : previousId,
                NextId = nextId == 0 ? null : nextId,
                OrdiniProduzioneAssegnati = ordiniAssegnati,
                OrdiniProduzioneAttivi = ordiniAttivi,
                OrdiniProduzioneCompletati = ordiniCompletati,
                UltimoOrdineAssegnato = ultimoOrdineAssegnato
            };
        }

        /// <summary>
        /// Crea un nuovo operatore
        /// </summary>
        public async Task<(bool Success, string Message, Operatore? Operatore)> CreateOperatoreAsync(CreateOperatoreViewModel model)
        {
            try
            {
                // Verifica unicità del codice operatore
                var esistente = await _context.Operatori
                    .FirstOrDefaultAsync(o => o.CodiceOperatore == model.CodiceOperatore);
                
                if (esistente != null)
                {
                    return (false, "Esiste già un operatore con questo codice.", null);
                }

                // Verifica unicità dell'email se specificata
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var emailEsistente = await _context.Operatori
                        .FirstOrDefaultAsync(o => o.Email == model.Email);
                    
                    if (emailEsistente != null)
                    {
                        return (false, "Esiste già un operatore con questa email.", null);
                    }
                }

                var operatore = new Operatore
                {
                    CodiceOperatore = model.CodiceOperatore.ToUpper().Trim(),
                    Nome = model.Nome.Trim(),
                    Cognome = model.Cognome.Trim(),
                    Email = model.Email?.Trim().ToLower(),
                    Telefono = model.Telefono?.Trim(),
                    DataAssunzione = model.DataAssunzione,
                    LivelloCompetenza = model.LivelloCompetenza,
                    Note = model.Note?.Trim(),
                    Attivo = model.Attivo
                };

                _context.Operatori.Add(operatore);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Operatore creato con successo - ID: {Id}, Codice: {Codice}, Nome: {Nome}", 
                    operatore.IdOperatore, operatore.CodiceOperatore, operatore.NomeCompleto);

                return (true, "Operatore creato con successo!", operatore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'operatore: {Codice}", 
                    model.CodiceOperatore);
                return (false, $"Errore durante la creazione: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Aggiorna un operatore esistente
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateOperatoreAsync(EditOperatoreViewModel model)
        {
            try
            {
                var operatore = await GetOperatoreByIdAsync(model.IdOperatore);
                if (operatore == null)
                {
                    return (false, "Operatore non trovato.");
                }

                // Verifica unicità del codice operatore
                var esistente = await _context.Operatori
                    .FirstOrDefaultAsync(o => o.CodiceOperatore == model.CodiceOperatore && 
                                             o.IdOperatore != model.IdOperatore);
                
                if (esistente != null)
                {
                    return (false, "Esiste già un operatore con questo codice.");
                }

                // Verifica unicità dell'email se specificata
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var emailEsistente = await _context.Operatori
                        .FirstOrDefaultAsync(o => o.Email == model.Email && 
                                                 o.IdOperatore != model.IdOperatore);
                    
                    if (emailEsistente != null)
                    {
                        return (false, "Esiste già un operatore con questa email.");
                    }
                }

                operatore.CodiceOperatore = model.CodiceOperatore.ToUpper().Trim();
                operatore.Nome = model.Nome.Trim();
                operatore.Cognome = model.Cognome.Trim();
                operatore.Email = model.Email?.Trim().ToLower();
                operatore.Telefono = model.Telefono?.Trim();
                operatore.DataAssunzione = model.DataAssunzione;
                operatore.LivelloCompetenza = model.LivelloCompetenza;
                operatore.Note = model.Note?.Trim();
                operatore.Attivo = model.Attivo;

                _context.Operatori.Update(operatore);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Operatore modificato con successo - ID: {Id}, Codice: {Codice}, Nome: {Nome}", 
                    operatore.IdOperatore, operatore.CodiceOperatore, operatore.NomeCompleto);

                return (true, "Operatore modificato con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la modifica dell'operatore - ID: {Id}", 
                    model.IdOperatore);
                return (false, $"Errore durante la modifica: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina un operatore
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteOperatoreAsync(int id)
        {
            try
            {
                var operatore = await _context.Operatori
                    .Include(o => o.OrdiniProduzione)
                    .FirstOrDefaultAsync(o => o.IdOperatore == id);

                if (operatore == null)
                {
                    return (false, "Operatore non trovato.");
                }

                // Verifica se ci sono ordini di produzione collegati
                if (operatore.OrdiniProduzione.Any())
                {
                    return (false, $"Impossibile eliminare l'operatore. Ci sono {operatore.OrdiniProduzione.Count} ordini di produzione collegati.");
                }

                _context.Operatori.Remove(operatore);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Operatore eliminato con successo - ID: {Id}, Codice: {Codice}, Nome: {Nome}", 
                    operatore.IdOperatore, operatore.CodiceOperatore, operatore.NomeCompleto);

                return (true, "Operatore eliminato con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'operatore - ID: {Id}", id);
                return (false, $"Errore durante l'eliminazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene gli operatori attivi per dropdown/selezioni
        /// </summary>
        public async Task<IEnumerable<OperatoreApiViewModel>> GetOperatoriAttiviAsync()
        {
            return await _context.Operatori
                .Where(o => o.Attivo)
                .OrderBy(o => o.Cognome)
                .ThenBy(o => o.Nome)
                .Select(o => new OperatoreApiViewModel
                {
                    Id = o.IdOperatore,
                    Codice = o.CodiceOperatore,
                    Nome = o.Nome,
                    Cognome = o.Cognome,
                    Attivo = o.Attivo,
                    LivelloCompetenza = o.LivelloCompetenza,
                    Email = o.Email,
                    Telefono = o.Telefono
                })
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene le statistiche degli operatori
        /// </summary>
        public async Task<OperatoriStatsViewModel> GetStatsAsync()
        {
            var totaleOperatori = await _context.Operatori.CountAsync();
            var operatoriAttivi = await _context.Operatori.CountAsync(o => o.Attivo);
            var operatoriConEmail = await _context.Operatori.CountAsync(o => !string.IsNullOrEmpty(o.Email));
            var operatoriConTelefono = await _context.Operatori.CountAsync(o => !string.IsNullOrEmpty(o.Telefono));
            var operatoriConDataAssunzione = await _context.Operatori.CountAsync(o => o.DataAssunzione.HasValue);

            var livelloCompetenzaMedia = await _context.Operatori
                .Where(o => o.LivelloCompetenza.HasValue)
                .AverageAsync(o => (double?)o.LivelloCompetenza) ?? 0;

            var livelloCompetenzaMinimo = await _context.Operatori
                .Where(o => o.LivelloCompetenza.HasValue)
                .MinAsync(o => (int?)o.LivelloCompetenza) ?? 0;

            var livelloCompetenzaMassimo = await _context.Operatori
                .Where(o => o.LivelloCompetenza.HasValue)
                .MaxAsync(o => (int?)o.LivelloCompetenza) ?? 0;

            var dataAssunzionePiuRecente = await _context.Operatori
                .Where(o => o.DataAssunzione.HasValue)
                .MaxAsync(o => o.DataAssunzione);

            var dataAssunzionePiuAntica = await _context.Operatori
                .Where(o => o.DataAssunzione.HasValue)
                .MinAsync(o => o.DataAssunzione);

            // Calcola anni di servizio medio
            var anniServizioMedio = 0.0;
            if (operatoriConDataAssunzione > 0)
            {
                var operatoriConAnni = await _context.Operatori
                    .Where(o => o.DataAssunzione.HasValue)
                    .Select(o => DateTime.Now.Year - o.DataAssunzione!.Value.Year)
                    .ToListAsync();
                anniServizioMedio = operatoriConAnni.Average();
            }

            // Distribuzione per livello di competenza
            var distribuzioneLivelli = await _context.Operatori
                .Where(o => o.LivelloCompetenza.HasValue)
                .GroupBy(o => o.LivelloCompetenza!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            // Operatori più recenti
            var operatoriPiuRecenti = await _context.Operatori
                .Where(o => o.DataAssunzione.HasValue)
                .OrderByDescending(o => o.DataAssunzione)
                .Take(5)
                .Select(o => new OperatoreFrequencyViewModel
                {
                    IdOperatore = o.IdOperatore,
                    CodiceOperatore = o.CodiceOperatore,
                    Nome = o.Nome,
                    Cognome = o.Cognome,
                    LivelloCompetenza = o.LivelloCompetenza,
                    DataAssunzione = o.DataAssunzione,
                    UltimoUtilizzo = o.DataAssunzione
                })
                .ToListAsync();

            // Operatori più esperti (livello competenza più alto)
            var operatoriPiuEsperti = await _context.Operatori
                .Where(o => o.LivelloCompetenza.HasValue)
                .OrderByDescending(o => o.LivelloCompetenza)
                .ThenBy(o => o.DataAssunzione)
                .Take(5)
                .Select(o => new OperatoreFrequencyViewModel
                {
                    IdOperatore = o.IdOperatore,
                    CodiceOperatore = o.CodiceOperatore,
                    Nome = o.Nome,
                    Cognome = o.Cognome,
                    LivelloCompetenza = o.LivelloCompetenza,
                    DataAssunzione = o.DataAssunzione
                })
                .ToListAsync();

            // Operatori più attivi (basato sul numero di ordini di produzione)
            var operatoriPiuAttivi = await _context.Operatori
                .Include(o => o.OrdiniProduzione)
                .Where(o => o.OrdiniProduzione.Any())
                .OrderByDescending(o => o.OrdiniProduzione.Count)
                .Take(5)
                .Select(o => new OperatoreFrequencyViewModel
                {
                    IdOperatore = o.IdOperatore,
                    CodiceOperatore = o.CodiceOperatore,
                    Nome = o.Nome,
                    Cognome = o.Cognome,
                    FrequenzaUtilizzo = o.OrdiniProduzione.Count,
                    UltimoUtilizzo = o.OrdiniProduzione.Any() ? o.OrdiniProduzione.Max(op => op.DataInizioOP) : o.DataAssunzione,
                    LivelloCompetenza = o.LivelloCompetenza,
                    DataAssunzione = o.DataAssunzione
                })
                .ToListAsync();

            return new OperatoriStatsViewModel
            {
                TotaleOperatori = totaleOperatori,
                OperatoriAttivi = operatoriAttivi,
                OperatoriInattivi = totaleOperatori - operatoriAttivi,
                OperatoriConEmail = operatoriConEmail,
                OperatoriConTelefono = operatoriConTelefono,
                OperatoriConDataAssunzione = operatoriConDataAssunzione,
                LivelloCompetenzaMedia = livelloCompetenzaMedia,
                LivelloCompetenzaMinimo = livelloCompetenzaMinimo,
                LivelloCompetenzaMassimo = livelloCompetenzaMassimo,
                AnniServizioMedio = anniServizioMedio,
                DataAssunzionePiuRecente = dataAssunzionePiuRecente,
                DataAssunzionePiuAntica = dataAssunzionePiuAntica,
                DistribuzioneLivelli = distribuzioneLivelli,
                OperatoriPiuRecenti = operatoriPiuRecenti,
                OperatoriPiuEsperti = operatoriPiuEsperti,
                OperatoriPiuAttivi = operatoriPiuAttivi
            };
        }

        /// <summary>
        /// Verifica se un operatore può essere eliminato
        /// </summary>
        public async Task<(bool CanDelete, string Reason, int RelatedCount)> CanDeleteOperatoreAsync(int id)
        {
            var ordiniCount = await _context.Operatori
                .Where(o => o.IdOperatore == id)
                .SelectMany(o => o.OrdiniProduzione)
                .CountAsync();

            if (ordiniCount > 0)
            {
                return (false, $"L'operatore ha {ordiniCount} ordini di produzione collegati", ordiniCount);
            }

            return (true, string.Empty, 0);
        }

        /// <summary>
        /// Attiva o disattiva un operatore
        /// </summary>
        public async Task<(bool Success, string Message)> ToggleAttivoAsync(int id)
        {
            try
            {
                var operatore = await GetOperatoreByIdAsync(id);
                if (operatore == null)
                {
                    return (false, "Operatore non trovato.");
                }

                operatore.Attivo = !operatore.Attivo;

                _context.Operatori.Update(operatore);
                await _context.SaveChangesAsync();

                var stato = operatore.Attivo ? "attivato" : "disattivato";
                _logger.LogInformation("Operatore {Stato} - ID: {Id}, Codice: {Codice}, Nome: {Nome}", 
                    stato, operatore.IdOperatore, operatore.CodiceOperatore, operatore.NomeCompleto);

                return (true, $"Operatore {stato} con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cambio stato dell'operatore - ID: {Id}", id);
                return (false, $"Errore durante il cambio stato: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene il riepilogo operatori per dashboard
        /// </summary>
        public async Task<OperatoriSummaryViewModel> GetSummaryAsync()
        {
            var totaleOperatori = await _context.Operatori.CountAsync();
            var operatoriAttivi = await _context.Operatori.CountAsync(o => o.Attivo);
            
            var nuoviOperatoriUltimoMese = await _context.Operatori
                .CountAsync(o => o.DataAssunzione >= DateTime.Now.AddMonths(-1));

            var livelloCompetenzaMedia = await _context.Operatori
                .Where(o => o.LivelloCompetenza.HasValue)
                .AverageAsync(o => (double?)o.LivelloCompetenza) ?? 0;

            var operatoriLivelloEsperto = await _context.Operatori
                .CountAsync(o => o.LivelloCompetenza >= 4);

            var operatoriInFormazioneOrdini = await _context.Operatori
                .Include(o => o.OrdiniProduzione)
                .CountAsync(o => o.OrdiniProduzione.Any(op => op.IdStato == 2)); // Stato "Produzione"

            return new OperatoriSummaryViewModel
            {
                TotaleOperatori = totaleOperatori,
                OperatoriAttivi = operatoriAttivi,
                NuoviOperatoriUltimoMese = nuoviOperatoriUltimoMese,
                LivelloCompetenzaMedia = livelloCompetenzaMedia,
                OperatoriLivelloEsperto = operatoriLivelloEsperto,
                OperatoriInFormazioneOrdini = operatoriInFormazioneOrdini
            };
        }
    }
}
