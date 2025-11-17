using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.ViewModels;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Identity;
using AiDbMaster.Helpers;
using System.Globalization;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class DashboardConsegneController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardConsegneController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardConsegneController(
            ApplicationDbContext context,
            ILogger<DashboardConsegneController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Helper: Ottiene il CodiceAgente dell'utente corrente (se è un agente)
        /// </summary>
        private async Task<short?> GetCodiceAgenteUtenteCorrente()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            return currentUser?.CodiceAgente;
        }

        public async Task<IActionResult> Index(DateTime? dataDa, DateTime? dataA)
        {
            ViewBag.UseFluidContainer = true;

            // Recupera codice agente se loggato come agente
            var codiceAgente = await GetCodiceAgenteUtenteCorrente();
            
            // Default: ultimi 6 mesi
            var oggi = DateTime.Today;
            dataDa ??= oggi.AddMonths(-6);
            dataA ??= oggi.AddMonths(3); // Include previsioni

            var model = new DashboardConsegneViewModel
            {
                DataDa = dataDa.Value,
                DataA = dataA.Value,
                CodiceAgente = codiceAgente
            };

            // Se è un agente, recupera il nome
            if (codiceAgente.HasValue)
            {
                var agente = await _context.TabellaAgenti
                    .FirstOrDefaultAsync(a => a.CodiceAgente == codiceAgente.Value);
                model.NomeAgente = agente?.DescrizioneAgente;
            }

            try
            {
                // Query base ordini clienti nel periodo
                var query = from testata in _context.OrdiniTestate
                            join riga in _context.OrdiniRighe
                                on new { testata.TipoOrdine, testata.AnnoOrdine, testata.SerieOrdine, testata.NumeroOrdine }
                                equals new { riga.TipoOrdine, riga.AnnoOrdine, riga.SerieOrdine, riga.NumeroOrdine }
                            join cliente in _context.AnagraficaClienti
                                on testata.CodiceCliente equals cliente.CodiceCliente
                            join agente in _context.TabellaAgenti
                                on cliente.CodiceAgente equals agente.CodiceAgente into agenteGroup
                            from agente in agenteGroup.DefaultIfEmpty()
                            where testata.TipoOrdine == "R"
                                && riga.DataConsegna >= dataDa
                                && riga.DataConsegna <= dataA
                            select new
                            {
                                Testata = testata,
                                Riga = riga,
                                Cliente = cliente,
                                Agente = agente
                            };

                // Filtro agente se necessario
                if (codiceAgente.HasValue)
                {
                    query = query.Where(x => x.Cliente.CodiceAgente == codiceAgente.Value);
                }

                var ordini = await query.ToListAsync();

                // === KPI CARDS ===
                model.NumeroOrdiniTotali = ordini.Select(x => new { x.Testata.AnnoOrdine, x.Testata.NumeroOrdine }).Distinct().Count();
                model.FatturatoTotale = ordini.Sum(x => x.Riga.ValoreRiga);
                model.FatturatoConsegnato = ordini.Where(x => x.Riga.QuantitaEvasa > 0).Sum(x => (x.Riga.ValoreRiga / x.Riga.Quantita) * x.Riga.QuantitaEvasa);
                model.FatturatoDaConsegnare = model.FatturatoTotale - model.FatturatoConsegnato;
                model.PercentualeEvasione = model.FatturatoTotale > 0 ? Math.Round((model.FatturatoConsegnato / model.FatturatoTotale) * 100, 2) : 0;
                model.ValoreMedioConsegna = model.NumeroOrdiniTotali > 0 ? model.FatturatoTotale / model.NumeroOrdiniTotali : 0;

                // Consegne in ritardo
                var righeInRitardo = ordini.Where(x => x.Riga.DataConsegna < oggi && (x.Riga.Quantita - x.Riga.QuantitaEvasa) > 0).ToList();
                model.NumeroConsegneInRitardo = righeInRitardo.Count;
                model.ValoreConsegneInRitardo = righeInRitardo.Sum(x => (x.Riga.ValoreRiga / x.Riga.Quantita) * (x.Riga.Quantita - x.Riga.QuantitaEvasa));

                // === GRAFICO CONSEGNE PER MESE ===
                var consegnePerMese = ordini
                    .GroupBy(x => new { x.Riga.DataConsegna.Year, x.Riga.DataConsegna.Month })
                    .Select(g => new ConsegnePerMeseDto
                    {
                        Anno = g.Key.Year,
                        Mese = g.Key.Month,
                        MeseNome = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", new CultureInfo("it-IT")),
                        Consegnato = g.Where(x => x.Riga.QuantitaEvasa > 0).Sum(x => (x.Riga.ValoreRiga / x.Riga.Quantita) * x.Riga.QuantitaEvasa),
                        DaConsegnare = g.Sum(x => (x.Riga.ValoreRiga / x.Riga.Quantita) * (x.Riga.Quantita - x.Riga.QuantitaEvasa))
                    })
                    .OrderBy(x => x.Anno).ThenBy(x => x.Mese)
                    .ToList();

                model.ConsegnePerMese = consegnePerMese;

                // === TOP 10 AGENTI ===
                if (!codiceAgente.HasValue) // Solo se admin/manager
                {
                    var topAgenti = ordini
                        .GroupBy(x => new { x.Agente.CodiceAgente, x.Agente.DescrizioneAgente })
                        .Select(g => new ClassificaAgenteDto
                        {
                            CodiceAgente = g.Key.CodiceAgente,
                            NomeAgente = g.Key.DescrizioneAgente ?? $"Agente {g.Key.CodiceAgente}",
                            NumeroOrdini = g.Select(x => new { x.Testata.AnnoOrdine, x.Testata.NumeroOrdine }).Distinct().Count(),
                            Fatturato = g.Sum(x => x.Riga.ValoreRiga),
                            PercentualeEvasione = g.Sum(x => x.Riga.ValoreRiga) > 0 
                                ? Math.Round((g.Where(x => x.Riga.QuantitaEvasa > 0).Sum(x => (x.Riga.ValoreRiga / x.Riga.Quantita) * x.Riga.QuantitaEvasa) / g.Sum(x => x.Riga.ValoreRiga)) * 100, 2)
                                : 0
                        })
                        .OrderByDescending(x => x.Fatturato)
                        .Take(10)
                        .ToList();

                    model.TopAgenti = topAgenti;
                }

                // === TOP 10 PROVINCE ===
                var topProvince = ordini
                    .GroupBy(x => new { x.Cliente.Provincia })
                    .Select(g => new ClassificaProvinciaDto
                    {
                        Provincia = g.Key.Provincia ?? "N/D",
                        Regione = RegioniHelper.GetRegione(g.Key.Provincia),
                        NumeroOrdini = g.Select(x => new { x.Testata.AnnoOrdine, x.Testata.NumeroOrdine }).Distinct().Count(),
                        Fatturato = g.Sum(x => x.Riga.ValoreRiga)
                    })
                    .OrderByDescending(x => x.Fatturato)
                    .Take(10)
                    .ToList();

                model.TopProvince = topProvince;

                // === TOP 10 CLIENTI ===
                var topClienti = ordini
                    .GroupBy(x => new { x.Cliente.CodiceCliente, x.Cliente.RagioneSociale, x.Cliente.Provincia })
                    .Select(g => new TopClienteDto
                    {
                        CodiceCliente = g.Key.CodiceCliente,
                        RagioneSociale = g.Key.RagioneSociale ?? "N/D",
                        Provincia = g.Key.Provincia,
                        NumeroOrdini = g.Select(x => new { x.Testata.AnnoOrdine, x.Testata.NumeroOrdine }).Distinct().Count(),
                        Fatturato = g.Sum(x => x.Riga.ValoreRiga)
                    })
                    .OrderByDescending(x => x.Fatturato)
                    .Take(10)
                    .ToList();

                model.TopClienti = topClienti;

                // === DETTAGLIO CONSEGNE IN RITARDO ===
                var consegneInRitardoDettaglio = righeInRitardo
                    .GroupBy(x => new 
                    { 
                        x.Testata.AnnoOrdine, 
                        x.Testata.NumeroOrdine, 
                        x.Riga.DataConsegna,
                        x.Cliente.CodiceCliente,
                        x.Cliente.RagioneSociale,
                        NomeAgente = x.Agente != null ? x.Agente.DescrizioneAgente : null
                    })
                    .Select(g => new ConsegnaInRitardoDto
                    {
                        AnnoOrdine = g.Key.AnnoOrdine,
                        NumeroOrdine = g.Key.NumeroOrdine,
                        DataConsegna = g.Key.DataConsegna,
                        CodiceCliente = g.Key.CodiceCliente.ToString(),
                        RagioneSociale = g.Key.RagioneSociale ?? "N/D",
                        NomeAgente = g.Key.NomeAgente,
                        ValoreRimanente = g.Sum(x => (x.Riga.ValoreRiga / x.Riga.Quantita) * (x.Riga.Quantita - x.Riga.QuantitaEvasa))
                    })
                    .OrderBy(x => x.DataConsegna)
                    .Take(20)
                    .ToList();

                model.ConsegneInRitardo = consegneInRitardoDettaglio;

                _logger.LogInformation("Dashboard Consegne caricata. Periodo: {DataDa} - {DataA}, Agente: {CodiceAgente}", 
                    dataDa, dataA, codiceAgente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento della Dashboard Consegne");
                ModelState.AddModelError("", "Errore durante il caricamento dei dati.");
            }

            return View(model);
        }
    }
}

