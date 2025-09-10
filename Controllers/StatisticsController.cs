using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Ottieni statistiche sui tipi di file
            var fileTypeStats = await _context.Documents
                .GroupBy(d => d.FileType)
                .Select(g => new
                {
                    FileType = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Prepara i dati per il grafico a torta
            var pieChartData = new
            {
                Labels = fileTypeStats.Select(x => x.FileType.ToString()).ToArray(),
                Data = fileTypeStats.Select(x => x.Count).ToArray(),
                Colors = fileTypeStats.Select(x => GetColorForDocumentType(x.FileType)).ToArray()
            };

            // Prepara i dati per l'istogramma
            var barChartData = new
            {
                Labels = fileTypeStats.Select(x => x.FileType.ToString()).ToArray(),
                Data = fileTypeStats.Select(x => x.Count).ToArray(),
                Colors = fileTypeStats.Select(x => GetColorForDocumentType(x.FileType)).ToArray()
            };

            // Ottieni statistiche sulle categorie
            var categoryStats = await _context.Documents
                .Include(d => d.Category)
                .GroupBy(d => d.Category!.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Prepara i dati per il grafico delle categorie
            var categoryChartData = new
            {
                Labels = categoryStats.Select(x => x.CategoryName).ToArray(),
                Data = categoryStats.Select(x => x.Count).ToArray()
            };

            // Ottieni statistiche sui documenti confidenziali
            var confidentialCount = await _context.Documents.CountAsync(d => d.IsConfidential);
            var nonConfidentialCount = await _context.Documents.CountAsync(d => !d.IsConfidential);

            var confidentialChartData = new
            {
                Labels = new[] { "Confidenziale", "Non Confidenziale" },
                Data = new[] { confidentialCount, nonConfidentialCount },
                Colors = new[] { "#dc3545", "#28a745" }
            };

            // Passa i dati alla vista
            ViewBag.PieChartData = pieChartData;
            ViewBag.BarChartData = barChartData;
            ViewBag.CategoryChartData = categoryChartData;
            ViewBag.ConfidentialChartData = confidentialChartData;
            ViewBag.TotalDocuments = await _context.Documents.CountAsync();

            return View();
        }

        private string GetColorForDocumentType(DocumentType type)
        {
            return type switch
            {
                DocumentType.PDF => "#dc3545",     // Rosso
                DocumentType.DOCX => "#0d6efd",    // Blu
                DocumentType.TXT => "#6c757d",     // Grigio
                DocumentType.EMAIL => "#198754",   // Verde
                DocumentType.EXCEL => "#20c997",   // Verde acqua
                DocumentType.OTHER => "#fd7e14",   // Arancione
                _ => "#6610f2"                     // Viola
            };
        }
    }
} 