using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("DashboardConsegne", "Dashboard Consegne", Description = "Dashboard analisi consegne", MenuIcon = "bi-graph-up", MenuOrder = 0)]
    [RequirePermission("DashboardConsegne", "View")]
    public class DashboardConsegneController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardConsegneController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string periodo = "mese")
        {
            DateTime dataDa, dataA;
            string periodoLabel;
            var oggi = DateTime.Today;

            switch (periodo)
            {
                case "settimana":
                    var giorno = (int)oggi.DayOfWeek;
                    var lunedi = oggi.AddDays(-(giorno == 0 ? 6 : giorno - 1));
                    dataDa = lunedi;
                    dataA = lunedi.AddDays(6);
                    periodoLabel = $"Settimana {dataDa:dd/MM} - {dataA:dd/MM/yyyy}";
                    break;
                case "mese":
                    dataDa = new DateTime(oggi.Year, oggi.Month, 1);
                    dataA = dataDa.AddMonths(1).AddDays(-1);
                    periodoLabel = oggi.ToString("MMMM yyyy");
                    break;
                case "trimestre":
                    var trimestre = (oggi.Month - 1) / 3;
                    dataDa = new DateTime(oggi.Year, trimestre * 3 + 1, 1);
                    dataA = dataDa.AddMonths(3).AddDays(-1);
                    periodoLabel = $"Q{trimestre + 1} {oggi.Year}";
                    break;
                case "anno":
                    dataDa = new DateTime(oggi.Year, 1, 1);
                    dataA = new DateTime(oggi.Year, 12, 31);
                    periodoLabel = oggi.Year.ToString();
                    break;
                default:
                    dataDa = new DateTime(oggi.Year, oggi.Month, 1);
                    dataA = dataDa.AddMonths(1).AddDays(-1);
                    periodoLabel = oggi.ToString("MMMM yyyy");
                    break;
            }

            var durataPeriodo = (dataA - dataDa).Days + 1;
            var dataDaPrecedente = dataDa.AddDays(-durataPeriodo);
            var dataAPrecedente = dataDa.AddDays(-1);

            var viaggi = await _context.ViaggiConsegna
                .AsNoTracking()
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Autista)
                .Include(v => v.Righe)
                .Where(v => v.DataConsegna.Date >= dataDa && v.DataConsegna.Date <= dataA && v.Stato != "Annullato")
                .ToListAsync();

            var viaggiPrecedenti = await _context.ViaggiConsegna
                .AsNoTracking()
                .Include(v => v.Righe)
                .Where(v => v.DataConsegna.Date >= dataDaPrecedente && v.DataConsegna.Date <= dataAPrecedente && v.Stato != "Annullato")
                .ToListAsync();

            // KPI principali
            var totViaggi = viaggi.Count;
            var totRighe = viaggi.SelectMany(v => v.Righe).Count();
            var pesoTotale = viaggi.SelectMany(v => v.Righe).Sum(r => r.PesoTotaleKgSnapshot);
            var costoTotale = viaggi.Sum(v => v.CostoTrasporto ?? 0);
            var ricavoTotale = viaggi.Sum(v => v.PrezzoVendita ?? 0);

            // Utilizzo medio: peso caricato / portata max del mezzo
            var utilizzoMedio = 0m;
            var viaggiConMezzo = viaggi.Where(v => v.MezzoTrasportoId.HasValue || v.MezzoTrasportoEsternoId.HasValue).ToList();
            if (viaggiConMezzo.Any())
            {
                var listaUtilizzi = new List<decimal>();
                foreach (var v in viaggiConMezzo)
                {
                    var pesoViaggio = v.Righe.Sum(r => r.PesoTotaleKgSnapshot);
                    decimal portataMax = 0;
                    if (v.MezzoTrasportoId.HasValue && v.MezzoTrasporto != null)
                        portataMax = v.MezzoTrasporto.PortataMaxKg;
                    else if (v.MezzoTrasportoEsternoId.HasValue && v.MezzoTrasportoEsterno != null)
                        portataMax = v.MezzoTrasportoEsterno.PortataMax;

                    if (portataMax > 0)
                        listaUtilizzi.Add(Math.Min(100, (pesoViaggio / portataMax) * 100));
                }
                if (listaUtilizzi.Any())
                    utilizzoMedio = Math.Round(listaUtilizzi.Average(), 1);
            }

            // Confronto con periodo precedente
            var viaggiPrec = viaggiPrecedenti.Count;
            var costoPrec = viaggiPrecedenti.Sum(v => v.CostoTrasporto ?? 0);
            var ricavoPrec = viaggiPrecedenti.Sum(v => v.PrezzoVendita ?? 0);

            // Grafico viaggi per giorno (interni vs esterni)
            var giorniLabels = new List<string>();
            var viaggiInterni = new List<int>();
            var viaggiEsterni = new List<int>();
            var pesoPerGiorno = new List<decimal>();
            var marginePerGiorno = new List<decimal>();
            var margineCumulato = new List<decimal>();
            var cumulato = 0m;

            var giorniConViaggi = viaggi
                .GroupBy(v => v.DataConsegna.Date)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var g in giorniConViaggi)
            {
                giorniLabels.Add(g.Key.ToString("dd/MM"));
                viaggiInterni.Add(g.Count(v => v.MezzoTrasportoId.HasValue && !v.MezzoTrasportoEsternoId.HasValue));
                viaggiEsterni.Add(g.Count(v => v.MezzoTrasportoEsternoId.HasValue));
                pesoPerGiorno.Add(Math.Round(g.SelectMany(v => v.Righe).Sum(r => r.PesoTotaleKgSnapshot), 0));
                var margineGiorno = g.Sum(v => (v.PrezzoVendita ?? 0) - (v.CostoTrasporto ?? 0));
                marginePerGiorno.Add(Math.Round(margineGiorno, 2));
                cumulato += margineGiorno;
                margineCumulato.Add(Math.Round(cumulato, 2));
            }

            // Torta distribuzione mezzi
            var tortaLabels = new List<string>();
            var tortaValori = new List<int>();

            var gruppoMezzi = viaggi
                .GroupBy(v =>
                {
                    if (v.MezzoTrasportoId.HasValue && v.MezzoTrasporto != null)
                        return v.MezzoTrasporto.Descrizione;
                    if (v.MezzoTrasportoEsternoId.HasValue && v.MezzoTrasportoEsterno != null)
                        return $"{v.MezzoTrasportoEsterno.NomeVettore} ({v.MezzoTrasportoEsterno.Comune})";
                    return "N/D";
                })
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToList();

            foreach (var g in gruppoMezzi)
            {
                tortaLabels.Add(g.Key);
                tortaValori.Add(g.Count());
            }

            // Top mezzi
            var topMezzi = viaggi
                .Where(v => v.MezzoTrasportoId.HasValue || v.MezzoTrasportoEsternoId.HasValue)
                .GroupBy(v => new
                {
                    Nome = v.MezzoTrasportoId.HasValue && v.MezzoTrasporto != null
                        ? v.MezzoTrasporto.Descrizione
                        : v.MezzoTrasportoEsterno != null
                            ? $"{v.MezzoTrasportoEsterno.NomeVettore} ({v.MezzoTrasportoEsterno.Comune})"
                            : "N/D",
                    Tipo = v.MezzoTrasportoId.HasValue ? "Interno" : "Esterno",
                    PortataMax = v.MezzoTrasportoId.HasValue && v.MezzoTrasporto != null
                        ? v.MezzoTrasporto.PortataMaxKg
                        : v.MezzoTrasportoEsterno != null ? (decimal)v.MezzoTrasportoEsterno.PortataMax : 0
                })
                .Select(g =>
                {
                    var pesoTot = g.SelectMany(v => v.Righe).Sum(r => r.PesoTotaleKgSnapshot);
                    var utilizzi = g.Select(v =>
                    {
                        var pv = v.Righe.Sum(r => r.PesoTotaleKgSnapshot);
                        return g.Key.PortataMax > 0 ? Math.Min(100, (pv / g.Key.PortataMax) * 100) : 0;
                    }).ToList();

                    return new DashboardMezzoDto
                    {
                        Mezzo = g.Key.Nome,
                        Tipo = g.Key.Tipo,
                        Viaggi = g.Count(),
                        PesoTotaleKg = Math.Round(pesoTot, 0),
                        CostoTotale = g.Sum(v => v.CostoTrasporto ?? 0),
                        RicavoTotale = g.Sum(v => v.PrezzoVendita ?? 0),
                        UtilizzoMedioPerc = utilizzi.Any() ? Math.Round(utilizzi.Average(), 1) : 0
                    };
                })
                .OrderByDescending(m => m.Viaggi)
                .Take(10)
                .ToList();

            // Top autisti
            var topAutisti = viaggi
                .Where(v => v.AutistaId.HasValue && v.Autista != null)
                .GroupBy(v => v.Autista!.NomeCompleto)
                .Select(g => new DashboardAutistaDto
                {
                    Autista = g.Key,
                    Viaggi = g.Count(),
                    OreTotali = Math.Round(g.Sum(v => v.DurataStimataMinuti) / 60m, 1),
                    PesoTotaleKg = Math.Round(g.SelectMany(v => v.Righe).Sum(r => r.PesoTotaleKgSnapshot), 0),
                    ViaggiConGru = g.Count(v => v.Gru == true),
                    ViaggiConTrasbordo = g.Count(v => v.Trasbordo == true)
                })
                .OrderByDescending(a => a.Viaggi)
                .Take(10)
                .ToList();

            // Top clienti: serve join con OrdiniRighe -> OrdiniTestate -> AnagraficaClienti
            var righeViaggio = await _context.ViaggioConsegnaRighe
                .AsNoTracking()
                .Include(r => r.ViaggioConsegna)
                .Include(r => r.OrdineRiga)
                .Where(r => r.ViaggioConsegna != null
                    && r.ViaggioConsegna.DataConsegna.Date >= dataDa
                    && r.ViaggioConsegna.DataConsegna.Date <= dataA
                    && r.ViaggioConsegna.Stato != "Annullato")
                .ToListAsync();

            var ordineRigaIds = righeViaggio.Select(r => r.OrdineRigaId).Distinct().ToList();

            var clientiPerOrdine = await (from or in _context.OrdiniRighe.AsNoTracking()
                                          join t in _context.OrdiniTestate.AsNoTracking()
                                              on new { or.TipoOrdine, or.AnnoOrdine, or.SerieOrdine, or.NumeroOrdine }
                                              equals new { t.TipoOrdine, t.AnnoOrdine, t.SerieOrdine, t.NumeroOrdine }
                                          join c in _context.AnagraficaClienti.AsNoTracking()
                                              on t.CodiceCliente equals c.CodiceCliente
                                          where ordineRigaIds.Contains(or.Id)
                                          select new { or.Id, c.RagioneSociale })
                                          .ToDictionaryAsync(x => x.Id, x => x.RagioneSociale);

            var topClienti = righeViaggio
                .GroupBy(r => clientiPerOrdine.TryGetValue(r.OrdineRigaId, out var nome) ? nome : "N/D")
                .Select(g =>
                {
                    var viaggiIds = g.Select(r => r.ViaggioConsegnaId).Distinct().ToList();
                    var viaggiCliente = viaggi.Where(v => viaggiIds.Contains(v.Id)).ToList();

                    return new DashboardClienteDto
                    {
                        Cliente = g.Key,
                        RigheConsegnate = g.Count(),
                        PesoTotaleKg = Math.Round(g.Sum(r => r.PesoTotaleKgSnapshot), 0),
                        Viaggi = viaggiIds.Count,
                        CostoTrasporto = viaggiCliente.Sum(v => v.CostoTrasporto ?? 0),
                        RicavoTrasporto = viaggiCliente.Sum(v => v.PrezzoVendita ?? 0)
                    };
                })
                .OrderByDescending(c => c.PesoTotaleKg)
                .Take(10)
                .ToList();

            // Allerte
            var assegnazioniPerRiga = await _context.ViaggioConsegnaRighe
                .AsNoTracking()
                .Include(x => x.ViaggioConsegna)
                .Where(x => x.ViaggioConsegna != null && x.ViaggioConsegna.Stato != "Annullato")
                .GroupBy(x => x.OrdineRigaId)
                .Select(g => new { OrdineRigaId = g.Key, QuantitaAssegnata = g.Sum(x => x.QuantitaAssegnata) })
                .ToDictionaryAsync(x => x.OrdineRigaId, x => x.QuantitaAssegnata);

            var startDate = DateTime.Today.AddDays(-7).Date;
            var endDate = DateTime.Today.AddDays(15).Date;
            var righeDaPianificare = await _context.OrdiniRighe
                .AsNoTracking()
                .Where(r => r.TipoOrdine == "R"
                    && r.DataConsegna.Date >= startDate
                    && r.DataConsegna.Date <= endDate)
                .ToListAsync();

            var countDaPianificare = righeDaPianificare.Count(r =>
            {
                var assegnata = assegnazioniPerRiga.TryGetValue(r.Id, out var q) ? q : 0;
                return (r.Quantita - r.QuantitaEvasa - assegnata) > 0;
            });

            var viaggiSenzaAutista = viaggi.Count(v => !v.AutistaId.HasValue);
            var viaggiSottoutilizzati = viaggiConMezzo.Count(v =>
            {
                var pesoV = v.Righe.Sum(r => r.PesoTotaleKgSnapshot);
                decimal portata = 0;
                if (v.MezzoTrasportoId.HasValue && v.MezzoTrasporto != null)
                    portata = v.MezzoTrasporto.PortataMaxKg;
                else if (v.MezzoTrasportoEsternoId.HasValue && v.MezzoTrasportoEsterno != null)
                    portata = v.MezzoTrasportoEsterno.PortataMax;
                return portata > 0 && (pesoV / portata) < 0.3m;
            });
            var viaggiSenzaPrezzo = viaggi.Count(v => !v.PrezzoVendita.HasValue || v.PrezzoVendita == 0);

            var vm = new DashboardConsegneViewModel
            {
                PeriodoLabel = periodoLabel,
                DataDa = dataDa,
                DataA = dataA,
                TotaleViaggi = totViaggi,
                TotaleRighe = totRighe,
                PesoTotaleKg = pesoTotale,
                CostoTotale = costoTotale,
                RicavoTotale = ricavoTotale,
                UtilizzoMedioPercentuale = utilizzoMedio,
                ViaggiPeriodoPrecedente = viaggiPrec,
                CostoPeriodoPrecedente = costoPrec,
                MarginePeriodoPrecedente = ricavoPrec - costoPrec,
                RigheDaPianificare = countDaPianificare,
                ViaggiSenzaAutista = viaggiSenzaAutista,
                ViaggiSottoutilizzati = viaggiSottoutilizzati,
                ViaggiSenzaPrezzo = viaggiSenzaPrezzo,
                GraficoGiorniLabels = giorniLabels,
                GraficoViaggiInterni = viaggiInterni,
                GraficoViaggiEsterni = viaggiEsterni,
                GraficoPesoPerGiorno = pesoPerGiorno,
                GraficoMarginePerGiorno = marginePerGiorno,
                GraficoMargineCumulato = margineCumulato,
                TortaMezziLabels = tortaLabels,
                TortaMezziValori = tortaValori,
                TopMezzi = topMezzi,
                TopAutisti = topAutisti,
                TopClienti = topClienti
            };

            ViewBag.PeriodoCorrente = periodo;
            return View(vm);
        }
    }
}
