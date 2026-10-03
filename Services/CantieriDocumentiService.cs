using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Cartella documenti cantieri sul file server.
    /// Ogni cantiere ha una sottocartella col proprio codice (es. CAN-00001).
    /// </summary>
    public class CantieriDocumentiService
    {
        public const long DimensioneMassimaFile = 50L * 1024 * 1024;

        private static readonly HashSet<string> EstensioniBloccate = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".bat", ".cmd", ".com", ".msi", ".scr", ".ps1", ".vbs", ".js", ".dll"
        };

        private readonly ApplicationDbContext _context;
        private readonly CantieriOptions _options;
        private readonly ILogger<CantieriDocumentiService> _logger;

        public CantieriDocumentiService(
            ApplicationDbContext context,
            IOptions<CantieriOptions> options,
            ILogger<CantieriDocumentiService> logger)
        {
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string?> GetCartellaDocumentiAsync()
        {
            var daOpzioni = await _context.Opzioni
                .AsNoTracking()
                .Where(o => o.NomeOpzione == CantieriOptions.NomeOpzioneCartella)
                .Select(o => o.ValoreOpzione)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(daOpzioni))
                return daOpzioni.Trim();

            return string.IsNullOrWhiteSpace(_options.CartellaDocumenti)
                ? null
                : _options.CartellaDocumenti.Trim();
        }

        public async Task<CartellaCantiereInfo> CaricaCartellaAsync(string codiceCantiere)
        {
            var info = new CartellaCantiereInfo();
            var root = await GetCartellaDocumentiAsync();
            if (string.IsNullOrWhiteSpace(root))
            {
                info.Errore = "Percorso documenti non configurato. Imposta Cantieri.CartellaDocumenti in Tabella Opzioni.";
                return info;
            }

            try
            {
                var cartella = PercorsoCartellaCantiere(root, codiceCantiere);
                info.Percorso = cartella;
                Directory.CreateDirectory(cartella);
                info.File = ElencaFile(cartella);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Accesso negato alla cartella documenti {Percorso}", info.Percorso ?? root);
                info.Errore = "Accesso negato a " + (info.Percorso ?? root) +
                              ". L'utente Windows con cui gira l'applicazione non ha Modifica sulla cartella.";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossibile accedere alla cartella documenti del cantiere {Codice}", codiceCantiere);
                info.Errore = "Impossibile accedere a " + (info.Percorso ?? root) + ": " + MessaggioUtente(ex);
            }

            return info;
        }

        public async Task<(bool Ok, string Messaggio, string? NomeSalvato)> SalvaFileAsync(
            string codiceCantiere,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "Nessun file selezionato.", null);

            if (file.Length > DimensioneMassimaFile)
                return (false, $"Il file supera i {DimensioneMassimaFile / (1024 * 1024)} MB.", null);

            var nome = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(nome))
                return (false, "Nome file non valido.", null);

            var estensione = Path.GetExtension(nome);
            if (EstensioniBloccate.Contains(estensione))
                return (false, "Questo tipo di file non è consentito.", null);

            var info = await CaricaCartellaAsync(codiceCantiere);
            if (!string.IsNullOrWhiteSpace(info.Errore) || string.IsNullOrWhiteSpace(info.Percorso))
                return (false, info.Errore ?? "Cartella non disponibile.", null);

            var destinazione = PercorsoFileSicuro(info.Percorso, nome);
            destinazione = NomeUnivoco(destinazione);

            await using var stream = new FileStream(destinazione, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream);

            return (true, "File caricato.", Path.GetFileName(destinazione));
        }

        public async Task<(bool Ok, string? Percorso, string? Errore)> PercorsoDownloadAsync(
            string codiceCantiere,
            string nomeFile)
        {
            var info = await CaricaCartellaAsync(codiceCantiere);
            if (!string.IsNullOrWhiteSpace(info.Errore) || string.IsNullOrWhiteSpace(info.Percorso))
                return (false, null, info.Errore ?? "Cartella non disponibile.");

            var percorso = PercorsoFileSicuro(info.Percorso, nomeFile);
            if (!System.IO.File.Exists(percorso))
                return (false, null, "File non trovato.");

            return (true, percorso, null);
        }

        public async Task<(bool Ok, string Messaggio)> EliminaFileAsync(string codiceCantiere, string nomeFile)
        {
            var download = await PercorsoDownloadAsync(codiceCantiere, nomeFile);
            if (!download.Ok || string.IsNullOrWhiteSpace(download.Percorso))
                return (false, download.Errore ?? "File non trovato.");

            System.IO.File.Delete(download.Percorso);
            return (true, "File eliminato.");
        }

        private static List<CantiereDocumentoItemViewModel> ElencaFile(string cartella)
        {
            return Directory.GetFiles(cartella)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => new CantiereDocumentoItemViewModel
                {
                    Nome = f.Name,
                    Dimensione = f.Length,
                    DataModifica = f.LastWriteTime
                })
                .ToList();
        }

        private static string PercorsoCartellaCantiere(string root, string codiceCantiere)
        {
            var codice = SanificaNome(codiceCantiere);
            if (string.IsNullOrWhiteSpace(codice))
                throw new InvalidOperationException("Codice cantiere non valido per la cartella documenti.");

            return Path.Combine(root, codice);
        }

        private static string PercorsoFileSicuro(string cartella, string nomeFile)
        {
            var nome = SanificaNome(Path.GetFileName(nomeFile));
            if (string.IsNullOrWhiteSpace(nome))
                throw new InvalidOperationException("Nome file non valido.");

            var cartellaPiena = Path.GetFullPath(cartella);
            var filePieno = Path.GetFullPath(Path.Combine(cartellaPiena, nome));
            if (!filePieno.StartsWith(cartellaPiena, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Percorso file non valido.");

            return filePieno;
        }

        private static string NomeUnivoco(string percorso)
        {
            if (!System.IO.File.Exists(percorso))
                return percorso;

            var cartella = Path.GetDirectoryName(percorso)!;
            var nome = Path.GetFileNameWithoutExtension(percorso);
            var ext = Path.GetExtension(percorso);
            for (var i = 1; i < 1000; i++)
            {
                var candidato = Path.Combine(cartella, $"{nome} ({i}){ext}");
                if (!System.IO.File.Exists(candidato))
                    return candidato;
            }

            return Path.Combine(cartella, $"{nome}-{DateTime.Now:yyyyMMddHHmmss}{ext}");
        }

        private static string MessaggioUtente(Exception ex)
        {
            if (ex is DirectoryNotFoundException or DriveNotFoundException)
                return "cartella non trovata (controlla nome server e condivisione).";
            if (ex is IOException)
                return ex.Message;
            return ex.Message;
        }

        private static string SanificaNome(string? valore)
        {
            if (string.IsNullOrWhiteSpace(valore))
                return string.Empty;

            var pulito = valore.Trim();
            foreach (var c in Path.GetInvalidFileNameChars())
                pulito = pulito.Replace(c, '_');

            return pulito;
        }
    }
}
