using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per l'invio di email di avviso merce pronta.
    /// Legge parametri SMTP e configurazione da TabellaOpzioni.
    /// Registra gli invii nella tabella InvioEmail per evitare duplicati.
    /// </summary>
    public class EmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(ApplicationDbContext context, ILogger<EmailService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Legge un'opzione dalla TabellaOpzioni per nome.
        /// </summary>
        public async Task<string?> GetOpzioneAsync(string nomeOpzione)
        {
            var opzione = await _context.Opzioni
                .FirstOrDefaultAsync(o => o.NomeOpzione == nomeOpzione);
            return opzione?.ValoreOpzione;
        }

        /// <summary>
        /// Legge un'opzione e la converte in int. Ritorna il default se non trovata.
        /// </summary>
        public async Task<int> GetOpzioneIntAsync(string nomeOpzione, int defaultValue = 0)
        {
            var valore = await GetOpzioneAsync(nomeOpzione);
            return int.TryParse(valore, out var risultato) ? risultato : defaultValue;
        }

        /// <summary>
        /// Invia un'email usando i parametri SMTP configurati in TabellaOpzioni.
        /// In modalità test, invia agli indirizzi di prova (EmailProva).
        /// </summary>
        /// <param name="oggetto">Oggetto dell'email</param>
        /// <param name="corpoHtml">Corpo HTML dell'email</param>
        /// <returns>True se l'invio è riuscito</returns>
        public async Task<bool> InviaEmailAsync(string oggetto, string corpoHtml)
        {
            try
            {
                // Leggi parametri SMTP da TabellaOpzioni
                var smtpServer = await GetOpzioneAsync("SmtpServer");
                var smtpPortStr = await GetOpzioneAsync("SmtpPort");
                var smtpUsername = await GetOpzioneAsync("SmtpUsername");
                var smtpPassword = await GetOpzioneAsync("SmtpPassword");
                var smtpSender = await GetOpzioneAsync("SmtpSender");
                var emailProva = await GetOpzioneAsync("EmailProva");

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) ||
                    string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(smtpSender))
                {
                    _logger.LogError("Parametri SMTP mancanti in TabellaOpzioni.");
                    return false;
                }

                if (string.IsNullOrEmpty(emailProva))
                {
                    _logger.LogError("EmailProva non configurata in TabellaOpzioni.");
                    return false;
                }

                var port = int.TryParse(smtpPortStr, out var p) ? p : 587;

                // Crea il messaggio
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Uff. Commerciale Favaro1", smtpSender));

                // Destinatari: in modalità test, usa EmailProva (indirizzi separati da ';')
                var indirizzi = emailProva.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var indirizzo in indirizzi)
                {
                    var email = indirizzo.Trim();
                    if (!string.IsNullOrEmpty(email))
                    {
                        message.To.Add(MailboxAddress.Parse(email));
                    }
                }

                message.Subject = oggetto;

                // Corpo HTML
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = corpoHtml
                };
                message.Body = bodyBuilder.ToMessageBody();

                // Invio tramite SMTP
                using var client = new SmtpClient();

                // Accetta il certificato SSL anche se il nome host non corrisponde
                // (il server mail.favaro1.com usa un certificato intestato a cl-137.noamweb.net)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Connessione con STARTTLS sulla porta 587
                await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email inviata con successo. Oggetto: {Oggetto}, Destinatari: {Dest}",
                    oggetto, string.Join(", ", indirizzi));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio dell'email. Oggetto: {Oggetto}", oggetto);
                return false;
            }
        }

        /// <summary>
        /// Genera il corpo HTML dell'email per un ordine con le sue righe selezionate.
        /// </summary>
        public string GeneraCorpoEmail(OrdineEmailViewModel ordine, List<RigaEmailViewModel> righeSelezionate, int giorniScadenza)
        {
            var dataDisponibilita = DateTime.Today;
            var dataScadenza = dataDisponibilita.AddDays(giorniScadenza);

            // Tabella articoli
            var righeHtml = "";
            foreach (var riga in righeSelezionate)
            {
                righeHtml += $@"
                <tr>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{riga.CodiceArticolo}</td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{riga.DescrizioneArticolo}</td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{riga.UnitaMisura}</td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{riga.QuantitaRimanente:N2}</td>
                </tr>";
            }

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; font-size: 14px; color: #333; }}
        table {{ border-collapse: collapse; width: 100%; margin: 15px 0; }}
        th {{ background-color: #4472C4; color: white; padding: 10px 8px; border: 1px solid #ddd; text-align: left; }}
        td {{ padding: 8px; border: 1px solid #ddd; }}
        .highlight {{ color: #c00000; font-weight: bold; }}
        .footer {{ margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <p>Gentile cliente,</p>
    <p>con la presente siamo ad avvisare che la merce da Lei ordinata relativa al nostro ordine cliente <strong>{ordine.AnnoOrdine}/{ordine.NumeroOrdine:D4}</strong>:</p>

    <table>
        <thead>
            <tr>
                <th>Articolo</th>
                <th>Descrizione</th>
                <th style='text-align: center;'>U.M.</th>
                <th style='text-align: right;'>Quantit&agrave;</th>
            </tr>
        </thead>
        <tbody>
            {righeHtml}
        </tbody>
    </table>

    <p>&Egrave; disponibile presso il nostro magazzino dal giorno <strong>{dataDisponibilita:dd-MM-yyyy}</strong>.</p>
    <p>La merce sar&agrave; disponibile fino al giorno <span class='highlight'>{dataScadenza:dd-MM-yyyy}</span>.</p>
    <p>Trascorsa tale data il materiale potrebbe non essere pi&ugrave; disponibile.</p>
    <p>La invitiamo a verificare il ritiro o la consegna entro il <strong>{dataScadenza:dd-MM-yyyy}</strong>, 
       se tale attivit&agrave; non fosse possibile La invitiamo a contattare i nostri uffici per riprogrammare 
       una nuova data di produzione.</p>

    <p>Grazie,<br>
    <strong>Uff. Commerciale Favaro1</strong></p>

    <div class='footer'>
        <p><em>Questa email &egrave; stata generata automaticamente dal sistema AiDbMaster.</em></p>
    </div>
</body>
</html>";

            return html;
        }

        /// <summary>
        /// Registra l'invio email nella tabella InvioEmail per evitare duplicati.
        /// </summary>
        public async Task RegistraInvioAsync(RigaEmailViewModel riga)
        {
            var invio = new InvioEmail
            {
                TipoOrdine = riga.TipoOrdine,
                AnnoOrdine = riga.AnnoOrdine,
                SerieOrdine = riga.SerieOrdine,
                NumeroOrdine = riga.NumeroOrdine,
                RigaOrdine = riga.RigaOrdine,
                DataInvio = DateTime.Now,
                Contabilizzato = "N"
            };

            _context.InvioEmail.Add(invio);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Registrato invio email per riga: {Tipo}{Anno}/{Serie}/{Numero} Riga {Riga}",
                riga.TipoOrdine, riga.AnnoOrdine, riga.SerieOrdine, riga.NumeroOrdine, riga.RigaOrdine);
        }

        /// <summary>
        /// Verifica se l'email è già stata inviata per una riga ordine.
        /// </summary>
        public async Task<bool> EmailGiaInviataAsync(string tipoOrdine, short annoOrdine, string serieOrdine, int numeroOrdine, int rigaOrdine)
        {
            return await _context.InvioEmail
                .AnyAsync(e => e.TipoOrdine == tipoOrdine &&
                               e.AnnoOrdine == annoOrdine &&
                               e.SerieOrdine == serieOrdine &&
                               e.NumeroOrdine == numeroOrdine &&
                               e.RigaOrdine == rigaOrdine);
        }

        /// <summary>
        /// Recupera i dati di invio email per una riga ordine (se esiste).
        /// </summary>
        public async Task<InvioEmail?> GetInvioEmailAsync(string tipoOrdine, short annoOrdine, string serieOrdine, int numeroOrdine, int rigaOrdine)
        {
            return await _context.InvioEmail
                .FirstOrDefaultAsync(e => e.TipoOrdine == tipoOrdine &&
                                          e.AnnoOrdine == annoOrdine &&
                                          e.SerieOrdine == serieOrdine &&
                                          e.NumeroOrdine == numeroOrdine &&
                                          e.RigaOrdine == rigaOrdine);
        }
    }
}
