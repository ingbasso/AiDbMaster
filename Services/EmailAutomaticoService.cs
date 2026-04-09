using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio in background che alle 14:00 di ogni giorno invia automaticamente
    /// le email "Avviso Merce Pronta" per gli ordini con DataConsegna = prossimo giorno feriale,
    /// se l'opzione EmailAutomatiche è attivata (= 1) in TabellaOpzioni.
    /// 
    /// Opzioni TabellaOpzioni utilizzate:
    /// - EmailAutomatiche: 0 = disattivato, 1 = attivato
    /// - EmailProva: se valorizzata, invia tutto agli indirizzi di prova
    /// - ClienteEscluso: codice cliente da escludere (default 9060650)
    /// - GiorniScadenzaMerce: giorni per calcolo scadenza (default 21)
    /// - Ccn: indirizzi email per copia nascosta (gestito da EmailService)
    /// </summary>
    public class EmailAutomaticoService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailAutomaticoService> _logger;
        private static readonly TimeSpan OraInvio = new(14, 0, 0);

        public EmailAutomaticoService(IServiceProvider serviceProvider, ILogger<EmailAutomaticoService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailAutomaticoService avviato.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var ora = DateTime.Now;
                var prossimoInvio = DateTime.Today.Add(OraInvio);

                if (ora >= prossimoInvio)
                    prossimoInvio = prossimoInvio.AddDays(1);

                var attesa = prossimoInvio - ora;
                _logger.LogInformation("EmailAutomaticoService: prossimo check alle {Ora} (tra {Minuti} minuti)",
                    prossimoInvio.ToString("dd/MM/yyyy HH:mm"), attesa.TotalMinutes.ToString("N0"));

                try
                {
                    await Task.Delay(attesa, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await EseguiInvioAutomatico(stoppingToken);
            }

            _logger.LogInformation("EmailAutomaticoService arrestato.");
        }

        private async Task EseguiInvioAutomatico(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailAutomaticoService: inizio elaborazione automatica ore {Ora}", DateTime.Now.ToString("HH:mm:ss"));

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            try
            {
                var emailAutomatiche = await emailService.GetOpzioneIntAsync("EmailAutomatiche", 0);
                if (emailAutomatiche != 1)
                {
                    _logger.LogInformation("EmailAutomaticoService: EmailAutomatiche = {Val}, invio disattivato. Skip.",
                        emailAutomatiche);
                    return;
                }

                var oggi = DateTime.Today;
                var dataConsegnaDomani = CalendarioFeriale.ProssimoGiornoFeriale(oggi);

                _logger.LogInformation("EmailAutomaticoService: invio per DataConsegna = {Data}", dataConsegnaDomani.ToString("dd/MM/yyyy"));

                var clienteEsclusoStr = await emailService.GetOpzioneAsync("ClienteEscluso");
                var clienteEscluso = int.TryParse(clienteEsclusoStr, out var ce) ? ce : 9060650;
                var giorniScadenza = await emailService.GetOpzioneIntAsync("GiorniScadenzaMerce", 21);

                var righeOrdine = await context.OrdiniRighe
                    .Where(r => r.TipoOrdine == "R"
                             && r.DataConsegna.Date == dataConsegnaDomani.Date
                             && (r.Quantita - r.QuantitaEvasa) > 0)
                    .ToListAsync(stoppingToken);

                if (!righeOrdine.Any())
                {
                    _logger.LogInformation("EmailAutomaticoService: nessuna riga trovata per data {Data}.", dataConsegnaDomani.ToString("dd/MM/yyyy"));
                    return;
                }

                var inviiEsistenti = await context.InvioEmail
                    .Where(e => e.TipoOrdine == "R")
                    .Select(e => new { e.TipoOrdine, e.AnnoOrdine, e.SerieOrdine, e.NumeroOrdine, e.RigaOrdine })
                    .ToListAsync(stoppingToken);
                var inviiSet = new HashSet<string>(
                    inviiEsistenti.Select(e => $"{e.TipoOrdine}|{e.AnnoOrdine}|{e.SerieOrdine}|{e.NumeroOrdine}|{e.RigaOrdine}"));

                var righeDaInviare = righeOrdine
                    .Where(r => !inviiSet.Contains($"{r.TipoOrdine}|{r.AnnoOrdine}|{r.SerieOrdine}|{r.NumeroOrdine}|{r.RigaOrdine}"))
                    .ToList();

                if (!righeDaInviare.Any())
                {
                    _logger.LogInformation("EmailAutomaticoService: tutte le righe per data {Data} sono già state inviate.",
                        dataConsegnaDomani.ToString("dd/MM/yyyy"));
                    return;
                }

                var righePerOrdine = righeDaInviare
                    .GroupBy(r => new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine })
                    .ToList();

                int emailInviate = 0;
                int emailFallite = 0;
                int ordiniSaltati = 0;

                foreach (var gruppo in righePerOrdine)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var testata = await context.OrdiniTestate
                        .FirstOrDefaultAsync(t => t.TipoOrdine == gruppo.Key.TipoOrdine
                            && t.AnnoOrdine == gruppo.Key.AnnoOrdine
                            && t.SerieOrdine == gruppo.Key.SerieOrdine
                            && t.NumeroOrdine == gruppo.Key.NumeroOrdine, stoppingToken);

                    if (testata == null) continue;

                    if (testata.Prenotato == "S" || testata.CodiceCliente == clienteEscluso)
                    {
                        ordiniSaltati++;
                        continue;
                    }

                    var cliente = await context.AnagraficaClienti
                        .FirstOrDefaultAsync(c => c.CodiceCliente == testata.CodiceCliente, stoppingToken);
                    var agente = cliente != null
                        ? await context.TabellaAgenti
                            .FirstOrDefaultAsync(a => a.CodiceAgente == cliente.CodiceAgente, stoppingToken)
                        : null;

                    var emailCliente = cliente?.Email;

                    var isTest = await emailService.IsModalitaTestAsync();
                    if (!isTest && string.IsNullOrWhiteSpace(emailCliente))
                    {
                        ordiniSaltati++;
                        _logger.LogWarning("EmailAutomaticoService: ordine {Anno}/{Numero} - cliente senza email, saltato.",
                            gruppo.Key.AnnoOrdine, gruppo.Key.NumeroOrdine);
                        continue;
                    }

                    var ordineVm = new OrdineEmailViewModel
                    {
                        TipoOrdine = testata.TipoOrdine,
                        AnnoOrdine = testata.AnnoOrdine,
                        SerieOrdine = testata.SerieOrdine,
                        NumeroOrdine = testata.NumeroOrdine,
                        CodiceCliente = testata.CodiceCliente,
                        RagioneSociale = cliente?.RagioneSociale ?? "N/D",
                        EmailCliente = emailCliente,
                        CodiceAgente = cliente?.CodiceAgente ?? 0,
                        NomeAgente = agente?.DescrizioneAgente,
                        EmailAgente = agente?.Email,
                        DataOrdine = testata.DataOrdine,
                        RiferimentoOrdine = testata.RiferimentoOrdine,
                    };

                    var righeEmail = gruppo.Select(r => new RigaEmailViewModel
                    {
                        TipoOrdine = r.TipoOrdine,
                        AnnoOrdine = r.AnnoOrdine,
                        SerieOrdine = r.SerieOrdine,
                        NumeroOrdine = r.NumeroOrdine,
                        RigaOrdine = r.RigaOrdine,
                        CodiceArticolo = r.CodiceArticolo,
                        DescrizioneArticolo = r.DescrizioneArticolo,
                        UnitaMisura = r.UnitaMisura,
                        Quantita = r.Quantita,
                        QuantitaEvasa = r.QuantitaEvasa,
                        DataConsegna = r.DataConsegna
                    }).ToList();

                    var oggetto = "Avviso disponibilità merce pronta";
                    var corpo = emailService.GeneraCorpoEmail(ordineVm, righeEmail, giorniScadenza);
                    var emailAgente = agente?.Email;
                    var inviaAdAgente = !string.IsNullOrWhiteSpace(emailAgente);
                    var esito = await emailService.InviaEmailAsync(oggetto, corpo, emailCliente, emailAgente, inviaAdAgente);

                    if (esito)
                    {
                        emailInviate++;
                        foreach (var riga in righeEmail)
                        {
                            await emailService.RegistraInvioAsync(riga, "Automatico");
                        }
                    }
                    else
                    {
                        emailFallite++;
                    }
                }

                _logger.LogInformation(
                    "EmailAutomaticoService: elaborazione completata. Inviate: {Inviate}, Fallite: {Fallite}, Saltate: {Saltate}",
                    emailInviate, emailFallite, ordiniSaltati);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailAutomaticoService: errore durante l'elaborazione automatica.");
            }
        }
    }
}
