using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione degli ordini di produzione
    /// </summary>
    [Authorize]
    public class ListaOPController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ListaOPController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Visualizza la lista degli ordini di produzione con filtri
        /// </summary>
        public async Task<IActionResult> Index(string? filtroStato, string? filtroCentro, string? filtroArticolo)
        {
            ViewBag.Title = "Gestione Ordini di Produzione";
            
            // Prepara i dati per i filtri
            await PrepareFilterData();
            
            // Query base con tutte le relazioni
            var query = _context.ListaOP
                .Include(l => l.Stato)
                .Include(l => l.Operatore)
                .Include(l => l.CentroLavoro)
                .Include(l => l.Lavorazione)
                .AsQueryable();

            // Applica filtri
            if (!string.IsNullOrEmpty(filtroStato))
            {
                query = query.Where(l => l.Stato!.CodiceStato == filtroStato);
                ViewBag.FiltroStato = filtroStato;
            }

            if (!string.IsNullOrEmpty(filtroCentro))
            {
                query = query.Where(l => l.CodiceCentro == filtroCentro);
                ViewBag.FiltroCentro = filtroCentro;
            }

            if (!string.IsNullOrEmpty(filtroArticolo))
            {
                query = query.Where(l => l.CodiceArticolo == filtroArticolo);
                ViewBag.FiltroArticolo = filtroArticolo;
            }

            // Ordina per data inizio (più recenti prima)
            var ordini = await query
                .OrderByDescending(l => l.DataInizioOP)
                .ToListAsync();

            return View(ordini);
        }

        /// <summary>
        /// Dashboard con statistiche degli ordini
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.Title = "Dashboard Ordini di Produzione";

            var statistiche = new
            {
                TotaleOrdini = await _context.ListaOP.CountAsync(),
                OrdiniEmessi = await _context.ListaOP.CountAsync(l => l.Stato!.CodiceStato == "EM"),
                OrdiniInProduzione = await _context.ListaOP.CountAsync(l => l.Stato!.CodiceStato == "PR"),
                OrdiniSospesi = await _context.ListaOP.CountAsync(l => l.Stato!.CodiceStato == "SO"),
                OrdiniChiusi = await _context.ListaOP.CountAsync(l => l.Stato!.CodiceStato == "CH"),
                
                // Statistiche per centro di lavoro
                OrdiniPerCentro = await _context.ListaOP
                    .Include(l => l.CentroLavoro)
                    .GroupBy(l => l.CentroLavoro!.DescrizioneCentro)
                    .Select(g => new { Centro = g.Key, Conteggio = g.Count() })
                    .ToListAsync(),
                
                // Ordini urgenti
                OrdiniUrgenti = await _context.ListaOP
                    .Include(l => l.Stato)
                    .Include(l => l.CentroLavoro)
                    .Where(l => l.Priorita >= 4 && l.Stato!.CodiceStato != "CH")
                    .OrderByDescending(l => l.Priorita)
                    .Take(10)
                    .ToListAsync(),
                
                // Ordini in ritardo (data fine prevista passata)
                OrdiniInRitardo = await _context.ListaOP
                    .Include(l => l.Stato)
                    .Include(l => l.CentroLavoro)
                    .Where(l => l.DataFinePrevista < DateTime.Now && l.Stato!.CodiceStato != "CH")
                    .OrderBy(l => l.DataFinePrevista)
                    .Take(10)
                    .ToListAsync()
            };

            return View(statistiche);
        }

        /// <summary>
        /// Visualizza i dettagli di un ordine
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context.ListaOP
                .Include(l => l.Stato)
                .Include(l => l.Operatore)
                .Include(l => l.CentroLavoro)
                .FirstOrDefaultAsync(m => m.IdListaOP == id);

            if (ordine == null)
            {
                return NotFound();
            }

            ViewBag.Title = $"Dettagli Ordine {ordine.IdentificativoCompleto}";
            return View(ordine);
        }

        /// <summary>
        /// Form per creare un nuovo ordine
        /// </summary>
        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Nuovo Ordine di Produzione";
            await PrepareSelectLists();
            
            // Imposta valori predefiniti
            var nuovoOrdine = new ListaOP
            {
                DataInizioOP = DateTime.Now,
                AnnoOrdine = (short)DateTime.Now.Year,
                IdStato = 1, // Emesso
                Priorita = 2, // Normale
                QuantitaProdotta = 0
            };
            
            return View(nuovoOrdine);
        }

        /// <summary>
        /// Salva un nuovo ordine
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipoOrdine,AnnoOrdine,SerieOrdine,NumeroOrdine,RigaOrdine,DescrOrdine,CodiceArticolo,DescrizioneArticolo,UnitaMisura,Quantita,QuantitaProdotta,DataInizioOP,TempoCiclo,DataInizioSetup,TempoSetup,IdStato,CodiceCentro,CodiceLavorazione,Note,DataFineOP,DataFinePrevista,Priorita,IdOperatore,CostoOrario,TempoEffettivo,Modificato")] ListaOP listaOP)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listaOP);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ordine di produzione creato con successo!";
                return RedirectToAction(nameof(Index));
            }
            
            await PrepareSelectLists(listaOP);
            ViewBag.Title = "Nuovo Ordine di Produzione";
            return View(listaOP);
        }

        /// <summary>
        /// Form per modificare un ordine
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context.ListaOP.FindAsync(id);
            if (ordine == null)
            {
                return NotFound();
            }

            await PrepareSelectLists(ordine);
            ViewBag.Title = $"Modifica Ordine {ordine.IdentificativoCompleto}";
            return View(ordine);
        }

        /// <summary>
        /// Salva le modifiche di un ordine
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdListaOP,TipoOrdine,AnnoOrdine,SerieOrdine,NumeroOrdine,RigaOrdine,DescrOrdine,CodiceArticolo,DescrizioneArticolo,UnitaMisura,Quantita,QuantitaProdotta,DataInizioOP,TempoCiclo,DataInizioSetup,TempoSetup,IdStato,CodiceCentro,CodiceLavorazione,Note,DataFineOP,DataFinePrevista,Priorita,IdOperatore,CostoOrario,TempoEffettivo,Modificato")] ListaOP listaOP)
        {
            if (id != listaOP.IdListaOP)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listaOP);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ordine di produzione aggiornato con successo!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListaOPExists(listaOP.IdListaOP))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            await PrepareSelectLists(listaOP);
            ViewBag.Title = $"Modifica Ordine {listaOP.IdentificativoCompleto}";
            return View(listaOP);
        }

        /// <summary>
        /// Conferma eliminazione ordine
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context.ListaOP
                .Include(l => l.Stato)
                .Include(l => l.Operatore)
                .Include(l => l.CentroLavoro)
                .Include(l => l.Lavorazione)
                .FirstOrDefaultAsync(m => m.IdListaOP == id);

            if (ordine == null)
            {
                return NotFound();
            }

            ViewBag.Title = $"Elimina Ordine {ordine.IdentificativoCompleto}";
            return View(ordine);
        }

        /// <summary>
        /// Elimina un ordine
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ordine = await _context.ListaOP.FindAsync(id);
            if (ordine != null)
            {
                _context.ListaOP.Remove(ordine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ordine di produzione eliminato con successo!";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Aggiorna rapidamente lo stato di un ordine
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateStato(int id, int nuovoStato)
        {
            var ordine = await _context.ListaOP.FindAsync(id);
            if (ordine == null)
            {
                return Json(new { success = false, message = "Ordine non trovato" });
            }

            ordine.IdStato = nuovoStato;
            
            // Se viene chiuso, imposta la data fine
            if (nuovoStato == 3) // Chiuso
            {
                ordine.DataFineOP = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            
            var stato = await _context.StatiOP.FindAsync(nuovoStato);
            return Json(new { success = true, message = $"Stato aggiornato a: {stato?.DescrizioneStato}" });
        }

        /// <summary>
        /// Esporta gli ordini di produzione in Excel, raggruppati per centro di lavoro
        /// </summary>
        public async Task<IActionResult> ExportExcel(string? filtroStato, string? filtroCentro, string? filtroArticolo)
        {
            // Configura EPPlus per uso non commerciale
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Query base con tutte le relazioni
            var query = _context.ListaOP
                .Include(l => l.Stato)
                .Include(l => l.Operatore)
                .Include(l => l.CentroLavoro)
                .Include(l => l.Lavorazione)
                .AsQueryable();

            // Applica gli stessi filtri della pagina Index
            if (!string.IsNullOrEmpty(filtroStato))
            {
                query = query.Where(l => l.Stato!.CodiceStato == filtroStato);
            }

            if (!string.IsNullOrEmpty(filtroCentro))
            {
                query = query.Where(l => l.CodiceCentro == filtroCentro);
            }

            if (!string.IsNullOrEmpty(filtroArticolo))
            {
                query = query.Where(l => l.CodiceArticolo == filtroArticolo);
            }

            // Raggruppa per centro di lavoro e ordina per data inizio
            var ordiniPerCentro = await query
                .OrderBy(l => l.CentroLavoro!.DescrizioneCentro)
                .ThenBy(l => l.DataInizioOP)
                .ToListAsync();

            // Crea il file Excel
            using (var package = new ExcelPackage())
            {
                // Raggruppa gli ordini per centro di lavoro
                var gruppi = ordiniPerCentro
                    .GroupBy(o => o.CentroLavoro?.DescrizioneCentro ?? "Non assegnato")
                    .OrderBy(g => g.Key);

                foreach (var gruppo in gruppi)
                {
                    // Crea un foglio per ogni centro di lavoro
                    var nomeSheet = gruppo.Key.Length > 31 ? gruppo.Key.Substring(0, 31) : gruppo.Key;
                    // Rimuovi caratteri non validi per il nome del foglio
                    nomeSheet = new string(nomeSheet.Where(c => !new[] { '\\', '/', '*', '[', ']', ':', '?' }.Contains(c)).ToArray());
                    
                    var worksheet = package.Workbook.Worksheets.Add(nomeSheet);

                    // Intestazioni
                    var headers = new[] { "Ordine", "Articolo", "Descrizione", "Quantità", "Prodotto", "%", "Stato", "Data Inizio", "Data Fine Prev.", "Priorità", "Note" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                    }

                    // Stile intestazioni
                    using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                    }

                    // Dati
                    int row = 2;
                    foreach (var ordine in gruppo.OrderBy(o => o.DataInizioOP))
                    {
                        worksheet.Cells[row, 1].Value = ordine.IdentificativoCompleto;
                        worksheet.Cells[row, 2].Value = ordine.CodiceArticolo;
                        worksheet.Cells[row, 3].Value = ordine.DescrizioneArticolo;
                        worksheet.Cells[row, 4].Value = ordine.Quantita;
                        worksheet.Cells[row, 5].Value = ordine.QuantitaProdotta;
                        worksheet.Cells[row, 6].Value = ordine.PercentualeCompletamento;
                        worksheet.Cells[row, 7].Value = ordine.Stato?.DescrizioneStato ?? "N/D";
                        worksheet.Cells[row, 8].Value = ordine.DataInizioOP;
                        worksheet.Cells[row, 9].Value = ordine.DataFinePrevista;
                        worksheet.Cells[row, 10].Value = ordine.PrioritaDescrizione;
                        worksheet.Cells[row, 11].Value = ordine.Note;

                        // Formatta date con ore e minuti
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                        worksheet.Cells[row, 9].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                        
                        // Formatta numeri
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00\"%\"";

                        // Colora riga alternata
                        if (row % 2 == 0)
                        {
                            using (var rowRange = worksheet.Cells[row, 1, row, headers.Length])
                            {
                                rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                rowRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                            }
                        }

                        row++;
                    }

                    // Auto-fit colonne
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    
                    // Imposta larghezze minime per alcune colonne
                    worksheet.Column(1).Width = 20;  // Ordine
                    worksheet.Column(3).Width = 30;  // Descrizione
                    worksheet.Column(8).Width = 16;  // Data Inizio
                    worksheet.Column(9).Width = 16;  // Data Fine Prev.
                    worksheet.Column(11).Width = 40; // Note
                }

                // ===== FOGLI GANTT =====
                // Determina il range di date per il Gantt
                var ordiniConDate = ordiniPerCentro
                    .Where(o => o.DataFinePrevista.HasValue)
                    .ToList();

                if (ordiniConDate.Any())
                {
                    var dataMinima = ordiniConDate.Min(o => o.DataInizioOP).Date;
                    var dataMassima = ordiniConDate.Max(o => o.DataFinePrevista!.Value).Date;
                    
                    // Limita a max 90 giorni per evitare fogli troppo larghi
                    if ((dataMassima - dataMinima).Days > 90)
                    {
                        dataMassima = dataMinima.AddDays(90);
                    }

                    var giorniTotali = (dataMassima - dataMinima).Days + 1;

                    // Colori per gli stati
                    var coloriStato = new Dictionary<string, Color>
                    {
                        { "EM", Color.FromArgb(146, 208, 80) },   // Emesso - Verde chiaro
                        { "PR", Color.FromArgb(0, 176, 240) },    // In Produzione - Azzurro
                        { "SO", Color.FromArgb(255, 192, 0) },    // Sospeso - Arancione
                        { "CH", Color.FromArgb(128, 128, 128) },  // Chiuso - Grigio
                    };
                    var coloreDefault = Color.FromArgb(180, 180, 180);

                    foreach (var gruppo in gruppi)
                    {
                        var ordiniGruppoConDate = gruppo
                            .Where(o => o.DataFinePrevista.HasValue)
                            .OrderBy(o => o.DataInizioOP)
                            .ToList();

                        if (!ordiniGruppoConDate.Any()) continue;

                        // Nome foglio Gantt
                        var nomeSheetGantt = gruppo.Key.Length > 25 ? gruppo.Key.Substring(0, 25) : gruppo.Key;
                        nomeSheetGantt = new string(nomeSheetGantt.Where(c => !new[] { '\\', '/', '*', '[', ']', ':', '?' }.Contains(c)).ToArray());
                        nomeSheetGantt = $"G_{nomeSheetGantt}";

                        var ganttSheet = package.Workbook.Worksheets.Add(nomeSheetGantt);

                        // Intestazioni fisse
                        ganttSheet.Cells[1, 1].Value = "Ordine";
                        ganttSheet.Cells[1, 2].Value = "Articolo";
                        ganttSheet.Cells[1, 3].Value = "Stato";

                        // Intestazioni date (giorni)
                        for (int d = 0; d < giorniTotali; d++)
                        {
                            var data = dataMinima.AddDays(d);
                            ganttSheet.Cells[1, 4 + d].Value = data;
                            ganttSheet.Cells[1, 4 + d].Style.Numberformat.Format = "dd/MM";
                            ganttSheet.Cells[1, 4 + d].Style.TextRotation = 90;
                            ganttSheet.Column(4 + d).Width = 4;

                            // Evidenzia weekend
                            if (data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
                            {
                                ganttSheet.Cells[1, 4 + d].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ganttSheet.Cells[1, 4 + d].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 230, 230));
                            }
                        }

                        // Stile intestazioni
                        using (var headerRange = ganttSheet.Cells[1, 1, 1, 3 + giorniTotali])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Font.Size = 9;
                            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                        }
                        using (var headerColRange = ganttSheet.Cells[1, 1, 1, 3])
                        {
                            headerColRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerColRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                            headerColRange.Style.Font.Color.SetColor(Color.White);
                        }

                        ganttSheet.Row(1).Height = 50;

                        // Dati ordini
                        int ganttRow = 2;
                        foreach (var ordine in ordiniGruppoConDate)
                        {
                            ganttSheet.Cells[ganttRow, 1].Value = ordine.IdentificativoCompleto;
                            ganttSheet.Cells[ganttRow, 2].Value = ordine.CodiceArticolo;
                            ganttSheet.Cells[ganttRow, 3].Value = ordine.Stato?.CodiceStato ?? "N/D";

                            // Calcola giorni di inizio e fine relativi al range
                            var inizioRelativo = Math.Max(0, (ordine.DataInizioOP.Date - dataMinima).Days);
                            var fineRelativa = Math.Min(giorniTotali - 1, (ordine.DataFinePrevista!.Value.Date - dataMinima).Days);

                            // Ottieni colore in base allo stato
                            var codiceStato = ordine.Stato?.CodiceStato ?? "";
                            var colore = coloriStato.ContainsKey(codiceStato) ? coloriStato[codiceStato] : coloreDefault;

                            // Colora le celle per la durata dell'ordine
                            for (int d = inizioRelativo; d <= fineRelativa; d++)
                            {
                                var cell = ganttSheet.Cells[ganttRow, 4 + d];
                                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(colore);
                                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                cell.Style.Border.Left.Color.SetColor(Color.White);
                                cell.Style.Border.Right.Color.SetColor(Color.White);
                            }

                            // Bordi per le celle dati
                            ganttSheet.Cells[ganttRow, 1, ganttRow, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            ganttSheet.Cells[ganttRow, 1, ganttRow, 3].Style.Border.Bottom.Color.SetColor(Color.LightGray);

                            ganttRow++;
                        }

                        // Larghezze colonne fisse
                        ganttSheet.Column(1).Width = 18;
                        ganttSheet.Column(2).Width = 15;
                        ganttSheet.Column(3).Width = 6;

                        // Freeze panes: blocca le prime 3 colonne e la prima riga
                        ganttSheet.View.FreezePanes(2, 4);

                        // Legenda in fondo
                        int legendaRow = ganttRow + 2;
                        ganttSheet.Cells[legendaRow, 1].Value = "Legenda:";
                        ganttSheet.Cells[legendaRow, 1].Style.Font.Bold = true;
                        
                        int legendaCol = 2;
                        foreach (var stato in coloriStato)
                        {
                            var statoDB = _context.StatiOP.FirstOrDefault(s => s.CodiceStato == stato.Key);
                            ganttSheet.Cells[legendaRow, legendaCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ganttSheet.Cells[legendaRow, legendaCol].Style.Fill.BackgroundColor.SetColor(stato.Value);
                            ganttSheet.Cells[legendaRow, legendaCol + 1].Value = statoDB?.DescrizioneStato ?? stato.Key;
                            legendaCol += 2;
                        }
                    }
                }

                // Restituisci il file
                var fileName = $"OrdiniProduzione_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fileContent = package.GetAsByteArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        #region Metodi Helper

        /// <summary>
        /// Prepara le SelectList per i dropdown
        /// </summary>
        private async Task PrepareSelectLists(ListaOP? ordine = null)
        {
            ViewData["IdStato"] = new SelectList(
                await _context.StatiOP.Where(s => s.Attivo).OrderBy(s => s.Ordine).ToListAsync(),
                "IdStato", "DescrizioneStato", ordine?.IdStato);

            ViewData["IdOperatore"] = new SelectList(
                await _context.Operatori.Where(o => o.Attivo).OrderBy(o => o.Nome).ThenBy(o => o.Cognome).ToListAsync(),
                "IdOperatore", "NomeCompleto", ordine?.IdOperatore);

            ViewData["CodiceCentro"] = new SelectList(
                await _context.CentriLavoro.Where(c => c.Attivo).OrderBy(c => c.DescrizioneCentro).ToListAsync(),
                "CodiceCentro", "DescrizioneCentro", ordine?.CodiceCentro);

            ViewData["CodiceLavorazione"] = new SelectList(
                await _context.Lavorazioni.Where(l => l.Attivo).OrderBy(l => l.DescrizioneLavorazione).ToListAsync(),
                "CodiceLavorazione", "DescrizioneLavorazione", ordine?.CodiceLavorazione);

            ViewData["Priorita"] = new SelectList(new[]
            {
                new { Value = 1, Text = "1 - Bassa" },
                new { Value = 2, Text = "2 - Normale" },
                new { Value = 3, Text = "3 - Media" },
                new { Value = 4, Text = "4 - Alta" },
                new { Value = 5, Text = "5 - Urgente" }
            }, "Value", "Text", ordine?.Priorita);
        }

        /// <summary>
        /// Prepara i dati per i filtri
        /// </summary>
        private async Task PrepareFilterData()
        {
            ViewBag.Stati = await _context.StatiOP
                .Where(s => s.Attivo)
                .OrderBy(s => s.Ordine)
                .Select(s => new { s.CodiceStato, s.DescrizioneStato })
                .ToListAsync();

            ViewBag.CentriLavoro = await _context.CentriLavoro
                .Where(c => c.Attivo)
                .OrderBy(c => c.DescrizioneCentro)
                .Select(c => new { c.CodiceCentro, c.DescrizioneCentro })
                .ToListAsync();

            // Articoli distinti dalla tabella ListaOP
            ViewBag.Articoli = await _context.ListaOP
                .Select(l => new { l.CodiceArticolo, l.DescrizioneArticolo })
                .Distinct()
                .OrderBy(a => a.CodiceArticolo)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se un ordine esiste
        /// </summary>
        private bool ListaOPExists(int id)
        {
            return _context.ListaOP.Any(e => e.IdListaOP == id);
        }

        #endregion
    }
}
