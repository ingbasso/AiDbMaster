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
    /// Servizio in background che ogni giorno all'ora configurata invia automaticamente
    /// le email "Avviso Merce Pronta" per gli ordini con DataConsegna = prossimo giorno feriale,
    /// se l'opzione EmailAutomatiche è attivata (= 1) in TabellaOpzioni.
    /// 
    /// Opzioni TabellaOpzioni utilizzate:
    /// - EmailAutomatiche: 0 = disattivato, 1 = attivato
    /// - OraInvioEmail: orario invio automatico in formato HH:mm (default 14:00)
    /// - EmailProva: se valorizzata, invia tutto agli indirizzi di prova
    /// - ClienteEscluso: codice cliente da escludere (default 9060650)
    /// - GiorniScadenzaMerce: giorni per calcolo scadenza (default 21)
    /// - Ccn: indirizzi email per copia nascosta (gestito da EmailService)
    /// </summary>
    public class EmailAutomaticoService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailAutomaticoService> _logger;
        private static readonly TimeSpan OraInvioDefault = new(14, 0, 0);

        public EmailAutomaticoService(IServiceProvider serviceProvider, ILogger<EmailAutomaticoService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Legge l'orario di invio dalla TabellaOpzioni (formato "HH:mm").
        /// Se non configurato o formato errato, usa il default 14:00.
        /// </summary>
        private async Task<TimeSpan> GetOraInvioAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var valore = await emailService.GetOpzioneAsync("OraInvioEmail");

            if (!string.IsNullOrEmpty(valore) && TimeSpan.TryParse(valore, out var orario))
            {
                _logger.LogInformation("EmailAutomaticoService: orario invio letto da TabellaOpzioni = {Orario}", valore);
                return orario;
            }

            _logger.LogInformation("EmailAutomaticoService: OraInvioEmail non configurata o formato errato, uso default {Default}",
                OraInvioDefault.ToString(@"hh\:mm"));
            return OraInvioDefault;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailAutomaticoService avviato.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var oraInvio = await GetOraInvioAsync();
                var ora = DateTime.Now;
                var prossimoInvio = DateTime.Today.Add(oraInvio);

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

        private async Task ScriviLogAsync(ApplicationDbContext context, string tipo, string esito, string? motivo,
            short? annoOrdine = null, string? serieOrdine = null, int? numeroOrdine = null, int? rigaOrdine = null,
            int? codiceCliente = null, string? ragioneSociale = null, string? emailDestinatario = null, string? dettagli = null)
        {
            try
            {
                context.LogEmailAutomatico.Add(new Models.LogEmailAutomatico
                {
                    DataOra = DateTime.Now,
                    Tipo = tipo,
                    Esito = esito,
                    Motivo = motivo,
                    AnnoOrdine = annoOrdine,
                    SerieOrdine = serieOrdine,
                    NumeroOrdine = numeroOrdine,
                    RigaOrdine = rigaOrdine,
                    CodiceCliente = codiceCliente,
                    RagioneSociale = ragioneSociale,
                    EmailDestinatario = emailDestinatario,
                    Dettagli = dettagli
                });
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailAutomaticoService: errore scrittura log su tabella.");
            }
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
                    await ScriviLogAsync(context, "Info", "-", $"Invio automatico disattivato (EmailAutomatiche = {emailAutomatiche})");
                    return;
                }

                var oggi = DateTime.Today;
                var dataConsegnaDomani = CalendarioFeriale.ProssimoGiornoFeriale(oggi);

                _logger.LogInformation("EmailAutomaticoService: invio per DataConsegna = {Data}", dataConsegnaDomani.ToString("dd/MM/yyyy"));
                await ScriviLogAsync(context, "Info", "-",
                    $"Elaborazione avviata per DataConsegna = {dataConsegnaDomani:dd/MM/yyyy}");

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
                    await ScriviLogAsync(context, "Info", "-",
                        $"Nessuna riga ordine trovata per DataConsegna = {dataConsegnaDomani:dd/MM/yyyy}");
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
                    await ScriviLogAsync(context, "Info", "-",
                        $"Tutte le {righeOrdine.Count} righe per {dataConsegnaDomani:dd/MM/yyyy} già inviate in precedenza");
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

                    var cliente = await context.AnagraficaClienti
                        .FirstOrDefaultAsync(c => c.CodiceCliente == testata.CodiceCliente, stoppingToken);
                    var ragSociale = cliente?.RagioneSociale ?? "N/D";

                    if (testata.Prenotato == "S")
                    {
                        ordiniSaltati++;
                        await ScriviLogAsync(context, "Saltato", "Saltato", "Ordine prenotato",
                            gruppo.Key.AnnoOrdine, gruppo.Key.SerieOrdine, gruppo.Key.NumeroOrdine,
                            codiceCliente: testata.CodiceCliente, ragioneSociale: ragSociale);
                        continue;
                    }

                    if (testata.CodiceCliente == clienteEscluso)
                    {
                        ordiniSaltati++;
                        await ScriviLogAsync(context, "Saltato", "Saltato", $"Cliente escluso (codice {clienteEscluso})",
                            gruppo.Key.AnnoOrdine, gruppo.Key.SerieOrdine, gruppo.Key.NumeroOrdine,
                            codiceCliente: testata.CodiceCliente, ragioneSociale: ragSociale);
                        continue;
                    }

                    var agente = testata.CodiceAgente.HasValue
                        ? await context.TabellaAgenti
                            .FirstOrDefaultAsync(a => a.CodiceAgente == testata.CodiceAgente.Value, stoppingToken)
                        : null;

                    var emailCliente = cliente?.Email;

                    var isTest = await emailService.IsModalitaTestAsync();
                    if (!isTest && string.IsNullOrWhiteSpace(emailCliente))
                    {
                        ordiniSaltati++;
                        _logger.LogWarning("EmailAutomaticoService: ordine {Anno}/{Numero} - cliente senza email, saltato.",
                            gruppo.Key.AnnoOrdine, gruppo.Key.NumeroOrdine);
                        await ScriviLogAsync(context, "Saltato", "Saltato", "Cliente senza indirizzo email",
                            gruppo.Key.AnnoOrdine, gruppo.Key.SerieOrdine, gruppo.Key.NumeroOrdine,
                            codiceCliente: testata.CodiceCliente, ragioneSociale: ragSociale);
                        continue;
                    }

                    var ordineVm = new OrdineEmailViewModel
                    {
                        TipoOrdine = testata.TipoOrdine,
                        AnnoOrdine = testata.AnnoOrdine,
                        SerieOrdine = testata.SerieOrdine,
                        NumeroOrdine = testata.NumeroOrdine,
                        CodiceCliente = testata.CodiceCliente,
                        RagioneSociale = ragSociale,
                        EmailCliente = emailCliente,
                        CodiceAgente = testata.CodiceAgente ?? 0,
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
                        await ScriviLogAsync(context, "Invio", "OK", $"Email inviata ({righeEmail.Count} righe)",
                            gruppo.Key.AnnoOrdine, gruppo.Key.SerieOrdine, gruppo.Key.NumeroOrdine,
                            codiceCliente: testata.CodiceCliente, ragioneSociale: ragSociale,
                            emailDestinatario: isTest ? $"[TEST] {await emailService.GetOpzioneAsync("EmailProva")}" : emailCliente);
                    }
                    else
                    {
                        emailFallite++;
                        await ScriviLogAsync(context, "Errore", "Fallito", "Invio email fallito (errore SMTP)",
                            gruppo.Key.AnnoOrdine, gruppo.Key.SerieOrdine, gruppo.Key.NumeroOrdine,
                            codiceCliente: testata.CodiceCliente, ragioneSociale: ragSociale,
                            emailDestinatario: emailCliente);
                    }
                }

                _logger.LogInformation(
                    "EmailAutomaticoService: elaborazione completata. Inviate: {Inviate}, Fallite: {Fallite}, Saltate: {Saltate}",
                    emailInviate, emailFallite, ordiniSaltati);

                await ScriviLogAsync(context, "Info", "-",
                    $"Elaborazione completata. Inviate: {emailInviate}, Fallite: {emailFallite}, Saltate: {ordiniSaltati}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailAutomaticoService: errore durante l'elaborazione automatica.");
                await ScriviLogAsync(context, "Errore", "Fallito",
                    "Errore critico durante l'elaborazione", dettagli: ex.Message);
            }
        }
    }
}
