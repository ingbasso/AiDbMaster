using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la gestione delle classi provvigioni.
    /// Fornisce metodi per le operazioni CRUD e logica business.
    /// </summary>
    public class ClassiProvvigioniService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClassiProvvigioniService> _logger;

        public ClassiProvvigioniService(
            ApplicationDbContext context,
            ILogger<ClassiProvvigioniService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene tutte le classi provvigioni con filtri e paginazione
        /// </summary>
        public async Task<ClassiProvvigioniIndexViewModel> GetClassiProvvigioniAsync(
            string? search = null,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.ClassiProvvigioni.AsQueryable();

            // Applica filtri di ricerca
            if (!string.IsNullOrEmpty(search))
            {
                // Prova a parsare come numero per cercare nel CodiceClasse
                if (short.TryParse(search, out short codiceSearch))
                {
                    query = query.Where(c =>
                        c.CodiceClasse == codiceSearch ||
                        (c.DescrizioneClasse != null && c.DescrizioneClasse.Contains(search)));
                }
                else
                {
                    query = query.Where(c =>
                        c.DescrizioneClasse != null && c.DescrizioneClasse.Contains(search));
                }
            }

            // Conta il totale prima dell'ordinamento e paginazione
            var totalCount = await query.CountAsync();

            // Applica ordinamento
            query = sortOrder switch
            {
                "codice" => query.OrderBy(c => c.CodiceClasse),
                "codice_desc" => query.OrderByDescending(c => c.CodiceClasse),
                "descrizione" => query.OrderBy(c => c.DescrizioneClasse),
                "descrizione_desc" => query.OrderByDescending(c => c.DescrizioneClasse),
                "perc_sconto" => query.OrderBy(c => c.Perc_Sconto),
                "perc_sconto_desc" => query.OrderByDescending(c => c.Perc_Sconto),
                "data" => query.OrderBy(c => c.UltimoAggiornamento),
                "data_desc" => query.OrderByDescending(c => c.UltimoAggiornamento),
                _ => query.OrderBy(c => c.CodiceClasse)
            };

            // Applica paginazione
            var classiProvvigioni = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calcola statistiche
            var stats = await GetStatsAsync();

            return new ClassiProvvigioniIndexViewModel
            {
                ClassiProvvigioni = classiProvvigioni,
                Search = search,
                SortOrder = sortOrder,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                CodiceSortParm = sortOrder == "codice" ? "codice_desc" : "codice",
                DescrizioneSortParm = sortOrder == "descrizione" ? "descrizione_desc" : "descrizione",
                PercScontoSortParm = sortOrder == "perc_sconto" ? "perc_sconto_desc" : "perc_sconto",
                DataSortParm = sortOrder == "data" ? "data_desc" : "data",
                TotaleClassi = stats.TotaleClassi,
                PercScontoMedia = stats.PercScontoMedia,
                PercScontoMin = stats.PercScontoMin,
                PercScontoMax = stats.PercScontoMax
            };
        }

        /// <summary>
        /// Ottiene una classe provvigione per ID
        /// </summary>
        public async Task<ClasseProvvigione?> GetClasseProvvigioneByIdAsync(int id)
        {
            return await _context.ClassiProvvigioni.FirstOrDefaultAsync(c => c.ID == id);
        }

        /// <summary>
        /// Ottiene i dettagli di una classe provvigione con navigazione precedente/successiva
        /// </summary>
        public async Task<ClasseProvvigioneDetailsViewModel?> GetClasseProvvigioneDetailsAsync(int id)
        {
            var classeProvvigione = await _context.ClassiProvvigioni
                .FirstOrDefaultAsync(c => c.ID == id);

            if (classeProvvigione == null)
                return null;

            // Trova ID precedente e successivo per la navigazione
            var previousId = await _context.ClassiProvvigioni
                .Where(c => c.CodiceClasse < classeProvvigione.CodiceClasse ||
                           (c.CodiceClasse == classeProvvigione.CodiceClasse && c.ID < id))
                .OrderByDescending(c => c.CodiceClasse)
                .ThenByDescending(c => c.ID)
                .Select(c => (int?)c.ID)
                .FirstOrDefaultAsync();

            var nextId = await _context.ClassiProvvigioni
                .Where(c => c.CodiceClasse > classeProvvigione.CodiceClasse ||
                           (c.CodiceClasse == classeProvvigione.CodiceClasse && c.ID > id))
                .OrderBy(c => c.CodiceClasse)
                .ThenBy(c => c.ID)
                .Select(c => (int?)c.ID)
                .FirstOrDefaultAsync();

            return new ClasseProvvigioneDetailsViewModel
            {
                ClasseProvvigione = classeProvvigione,
                PreviousId = previousId,
                NextId = nextId
            };
        }

        /// <summary>
        /// Crea una nuova classe provvigione
        /// </summary>
        public async Task<(bool Success, string Message, ClasseProvvigione? ClasseProvvigione)> CreateClasseProvvigioneAsync(CreateClasseProvvigioneViewModel model)
        {
            try
            {
                // Verifica unicità del CodiceClasse
                var esistente = await _context.ClassiProvvigioni
                    .FirstOrDefaultAsync(c => c.CodiceClasse == model.CodiceClasse);

                if (esistente != null)
                {
                    return (false, $"Esiste già una classe provvigione con codice {model.CodiceClasse}.", null);
                }

                var classeProvvigione = new ClasseProvvigione
                {
                    CodiceClasse = model.CodiceClasse,
                    DescrizioneClasse = model.DescrizioneClasse?.Trim(),
                    Perc_Sconto = model.Perc_Sconto,
                    UltimoAggiornamento = DateTime.Now
                };

                _context.ClassiProvvigioni.Add(classeProvvigione);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Classe provvigione creata con successo - ID: {ID}, CodiceClasse: {Codice}, Descrizione: {Descrizione}",
                    classeProvvigione.ID, classeProvvigione.CodiceClasse, classeProvvigione.DescrizioneClasse);

                return (true, "Classe provvigione creata con successo!", classeProvvigione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della classe provvigione - CodiceClasse: {Codice}",
                    model.CodiceClasse);
                return (false, $"Errore durante la creazione: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Aggiorna una classe provvigione esistente
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateClasseProvvigioneAsync(EditClasseProvvigioneViewModel model)
        {
            try
            {
                var classeProvvigione = await GetClasseProvvigioneByIdAsync(model.ID);
                if (classeProvvigione == null)
                {
                    return (false, "Classe provvigione non trovata.");
                }

                // Verifica unicità del CodiceClasse (escluso l'elemento corrente)
                var esistente = await _context.ClassiProvvigioni
                    .FirstOrDefaultAsync(c => c.CodiceClasse == model.CodiceClasse && c.ID != model.ID);

                if (esistente != null)
                {
                    return (false, $"Esiste già un'altra classe provvigione con codice {model.CodiceClasse}.");
                }

                classeProvvigione.CodiceClasse = model.CodiceClasse;
                classeProvvigione.DescrizioneClasse = model.DescrizioneClasse?.Trim();
                classeProvvigione.Perc_Sconto = model.Perc_Sconto;
                classeProvvigione.UltimoAggiornamento = DateTime.Now;

                _context.ClassiProvvigioni.Update(classeProvvigione);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Classe provvigione modificata con successo - ID: {ID}, CodiceClasse: {Codice}",
                    classeProvvigione.ID, classeProvvigione.CodiceClasse);

                return (true, "Classe provvigione modificata con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la modifica della classe provvigione - ID: {ID}", model.ID);
                return (false, $"Errore durante la modifica: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina una classe provvigione
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteClasseProvvigioneAsync(int id)
        {
            try
            {
                var classeProvvigione = await _context.ClassiProvvigioni
                    .FirstOrDefaultAsync(c => c.ID == id);

                if (classeProvvigione == null)
                {
                    return (false, "Classe provvigione non trovata.");
                }

                _context.ClassiProvvigioni.Remove(classeProvvigione);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Classe provvigione eliminata con successo - ID: {ID}, CodiceClasse: {Codice}, Descrizione: {Descrizione}",
                    classeProvvigione.ID, classeProvvigione.CodiceClasse, classeProvvigione.DescrizioneClasse);

                return (true, "Classe provvigione eliminata con successo!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della classe provvigione - ID: {Id}", id);
                return (false, $"Errore durante l'eliminazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene tutte le classi provvigioni per dropdown/selezioni API
        /// </summary>
        public async Task<IEnumerable<ClasseProvvigioneApiViewModel>> GetClassiProvvigioniApiAsync()
        {
            return await _context.ClassiProvvigioni
                .OrderBy(c => c.CodiceClasse)
                .Select(c => new ClasseProvvigioneApiViewModel
                {
                    ID = c.ID,
                    CodiceClasse = c.CodiceClasse,
                    DescrizioneClasse = c.DescrizioneClasse,
                    Perc_Sconto = c.Perc_Sconto
                })
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene le statistiche delle classi provvigioni
        /// </summary>
        public async Task<ClassiProvvigioniStatsViewModel> GetStatsAsync()
        {
            var totaleClassi = await _context.ClassiProvvigioni.CountAsync();

            decimal? percScontoMedia = null;
            decimal? percScontoMin = null;
            decimal? percScontoMax = null;
            DateTime? dataUltimoAggiornamento = null;

            if (totaleClassi > 0)
            {
                percScontoMedia = await _context.ClassiProvvigioni.AverageAsync(c => c.Perc_Sconto);
                percScontoMin = await _context.ClassiProvvigioni.MinAsync(c => c.Perc_Sconto);
                percScontoMax = await _context.ClassiProvvigioni.MaxAsync(c => c.Perc_Sconto);
                dataUltimoAggiornamento = await _context.ClassiProvvigioni
                    .OrderByDescending(c => c.UltimoAggiornamento)
                    .Select(c => c.UltimoAggiornamento)
                    .FirstOrDefaultAsync();
            }

            return new ClassiProvvigioniStatsViewModel
            {
                TotaleClassi = totaleClassi,
                PercScontoMedia = percScontoMedia,
                PercScontoMin = percScontoMin,
                PercScontoMax = percScontoMax,
                DataUltimoAggiornamento = dataUltimoAggiornamento
            };
        }

        /// <summary>
        /// Verifica se una classe provvigione può essere eliminata
        /// (per ora non ci sono vincoli, ma il metodo è pronto per futuri controlli)
        /// </summary>
        public async Task<(bool CanDelete, string Reason)> CanDeleteClasseProvvigioneAsync(int id)
        {
            var classe = await _context.ClassiProvvigioni.FirstOrDefaultAsync(c => c.ID == id);
            if (classe == null)
            {
                return (false, "Classe provvigione non trovata.");
            }

            // In futuro, qui si potranno aggiungere controlli su relazioni con altre tabelle
            // Esempio: verificare se ci sono agenti/clienti collegati a questa classe

            return (true, string.Empty);
        }
    }
}
