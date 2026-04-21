using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la visualizzazione del Conto Economico (Pstree)
    /// </summary>
    [Authorize]
    public class PstreeContoEconomicoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeContoEconomicoController> _logger;

        public PstreeContoEconomicoController(
            ApplicationDbContext context, 
            ILogger<PstreeContoEconomicoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: PstreeContoEconomico
        public async Task<IActionResult> Index(int? anno, List<int>? famiglie, List<int>? mesi, int? sede, string? vista, bool escludiRimanenze = false, bool mostraCodici = false)
        {
            var viewModel = new Pstree_ContoEconomicoViewModel();

            viewModel.TipoVista = vista ?? "mesi";

            viewModel.EscludiRimanenze = escludiRimanenze;
            
            viewModel.MostraCodici = mostraCodici;
            
            // === CONTROLLO CONTI NON ASSOCIATI ===
            var tuttiCodiciPdC = await _context.PstreeListaPianoDeiConti
                .Where(p => p.TipoPdC == "E" && !p.CodicePdC.StartsWith("60"))
                .Select(p => p.CodicePdC)
                .ToListAsync();
            
            var codiciPdCAssociati = await _context.PstreeAssociazioniCE
                .Select(a => a.CodicePdC)
                .Distinct()
                .ToListAsync();
            
            var codiciNonAssociare = await _context.PstreeListaPianoDeiConti
                .Where(p => p.TipoPdC == "E" && p.NonAssociare)
                .Select(p => p.CodicePdC)
                .ToListAsync();
            
            var contiNonAssociati = tuttiCodiciPdC
                .Where(c => !codiciPdCAssociati.Contains(c) && !codiciNonAssociare.Contains(c))
                .Count();
            
            ViewBag.ContiNonAssociati = contiNonAssociati;

            var annoCorrente = DateTime.Now.Year;
            var meseCorrente = DateTime.Now.Month;
            viewModel.Anno = anno ?? annoCorrente;

            var anniSaldi = await _context.PstreeListaSaldi
                .Where(s => s.Anno.HasValue)
                .Select(s => s.Anno!.Value)
                .Distinct()
                .ToListAsync();

            var anniRettifiche = await _context.PstreeListaRettifiche
                .Select(r => r.Anno)
                .Distinct()
                .ToListAsync();

            viewModel.AnniDisponibili = anniSaldi
                .Union(anniRettifiche)
                .Union(new[] { annoCorrente, annoCorrente - 1, annoCorrente + 1 })
                .Distinct()
                .OrderByDescending(a => a)
                .ToList();

            List<int> mesiDefault;
            if (viewModel.Anno == annoCorrente)
            {
                mesiDefault = meseCorrente > 1 ? Enumerable.Range(1, meseCorrente - 1).ToList() : new List<int>();
            }
            else if (viewModel.Anno < annoCorrente)
            {
                mesiDefault = Enumerable.Range(1, 12).ToList();
            }
            else
            {
                mesiDefault = new List<int>();
            }

            var mesiSelezionati = (mesi != null && mesi.Any()) ? mesi : mesiDefault;
            viewModel.MesiSelezionati = mesiSelezionati;

            viewModel.Mesi = Enumerable.Range(1, 12).Select(m => new Pstree_MeseCheckboxItem
            {
                Numero = m,
                Nome = Pstree_ContoEconomicoViewModel.NomiMesi[m],
                Selezionato = mesiSelezionati.Contains(m)
            }).ToList();

            var tutteFamiglie = await _context.PstreeListaFamiglie
                .OrderBy(f => f.Id)
                .ToListAsync();

            var famigliePrincipali = tutteFamiglie.Where(f => f.IdFamigliaPadre == null).ToList();
            var sottoFamiglie = tutteFamiglie.Where(f => f.IdFamigliaPadre != null).ToList();
            
            var mappaFiglie = sottoFamiglie
                .GroupBy(f => f.IdFamigliaPadre!.Value)
                .ToDictionary(g => g.Key, g => g.Select(f => f.Id).ToList());

            var famiglieSelezionate = (famiglie != null && famiglie.Any()) 
                ? famiglie 
                : famigliePrincipali.Where(f => f.Id != 0).Select(f => f.Id).ToList();
            viewModel.FamiglieSelezionate = famiglieSelezionate;

            viewModel.Famiglie = famigliePrincipali
                .Where(f => f.Id != 0)
                .Select(f => new Pstree_FamigliaCheckboxItem
                {
                    Id = f.Id,
                    Nome = f.NomeFamiglia,
                    Selezionato = famiglieSelezionate.Contains(f.Id)
                }).ToList();
            
            var famigliePerCalcolo = new List<int>(famiglieSelezionate);
            foreach (var famId in famiglieSelezionate)
            {
                if (mappaFiglie.TryGetValue(famId, out var figlie))
                {
                    famigliePerCalcolo.AddRange(figlie);
                }
            }

            var tutteSedi = await _context.PstreeListaSedi
                .OrderBy(s => s.Id)
                .ToListAsync();

            viewModel.Sedi = tutteSedi.Select(s => new Pstree_SedeDropdownItem
            {
                Id = s.Id,
                Nome = s.Sede
            }).ToList();

            viewModel.SedeSelezionata = sede;

            var struttura = await _context.PstreeStrutturaContoEconomico
                .OrderBy(s => s.Ordine)
                .ToListAsync();

            var associazioni = await _context.PstreeAssociazioniCE.ToListAsync();
            var associazioniDict = associazioni
                .GroupBy(a => a.IdCodiceConto)
                .ToDictionary(g => g.Key, g => g.Select(a => a.CodicePdC).ToList());

            var saldiQuery = _context.PstreeListaSaldi
                .Where(s => s.Anno == viewModel.Anno);

            if (sede.HasValue)
            {
                saldiQuery = saldiQuery.Where(s => s.IdSede == sede.Value);
            }

            var saldi = await saldiQuery.ToListAsync();

            var rettificheQuery = _context.PstreeListaRettifiche
                .Where(r => r.Anno == viewModel.Anno);

            if (sede.HasValue)
            {
                rettificheQuery = rettificheQuery.Where(r => r.IdSede == sede.Value);
            }

            var rettifiche = await rettificheQuery.ToListAsync();

            // === CARICA RIMANENZE PER CALCOLO DELTA ===
            var rimanenzeQuery = _context.PstreeListaRimanenze
                .Where(r => (r.Anno == viewModel.Anno) || 
                            (r.Anno == viewModel.Anno - 1 && r.Mese == 12));

            if (famigliePerCalcolo.Any())
            {
                rimanenzeQuery = rimanenzeQuery.Where(r => famigliePerCalcolo.Contains(r.IdFamiglia));
            }

            if (sede.HasValue)
            {
                rimanenzeQuery = rimanenzeQuery.Where(r => r.IdSede == sede.Value);
            }

            var rimanenze = await rimanenzeQuery.ToListAsync();
            
            var rimanenzeDict = rimanenze
                .GroupBy(r => (r.IdFamiglia, r.Anno, r.Mese))
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Valore + r.RettificaValore));

            var famiglieDict = await _context.PstreeListaFamiglie
                .ToDictionaryAsync(f => f.Id, f => f.IdCodiceConto);

            var figliDict = struttura
                .GroupBy(s => s.ParentId)
                .ToDictionary(g => g.Key, g => g.Select(s => s.IdCodiceConto).ToList());

            var righe = new List<Pstree_ContoEconomicoRigaViewModel>();
            var valoriCalcolati = new Dictionary<int, decimal[]>();
            
            foreach (var voce in struttura)
            {
                var riga = new Pstree_ContoEconomicoRigaViewModel
                {
                    IdCodiceConto = voce.IdCodiceConto,
                    DescrizioneConto = voce.DescrizioneConto,
                    TipoConto = voce.TipoConto,
                    Livello = voce.Livello,
                    ParentId = voce.ParentId,
                    Ordine = voce.Ordine,
                    HasFigli = figliDict.ContainsKey(voce.IdCodiceConto),
                    VoceRettifica = voce.VoceRettifica,
                    VoceRimanenza = voce.VoceRimanenza,
                    CostiFD = voce.CostiFD
                };

                var valori = new decimal[13];

                if (voce.VoceRettifica)
                {
                    var rettificheVoce = rettifiche.Where(r => r.IdCodiceConto == voce.IdCodiceConto);
                    foreach (var rett in rettificheVoce)
                    {
                        if (rett.Mese >= 1 && rett.Mese <= 12)
                        {
                            valori[rett.Mese] += rett.Saldo * -1;
                        }
                    }
                }
                else if (voce.VoceRimanenza)
                {
                    if (!escludiRimanenze)
                    {
                        foreach (var famId in famigliePerCalcolo)
                        {
                            if (famiglieDict.TryGetValue(famId, out var codiceContoFam))
                            {
                                if (codiceContoFam == voce.IdCodiceConto)
                                {
                                    for (int mese = 1; mese <= 12; mese++)
                                    {
                                        var rimFinale = rimanenzeDict.GetValueOrDefault((famId, viewModel.Anno, mese), 0);
                                        
                                        double rimPrecedente;
                                        if (mese == 1)
                                        {
                                            rimPrecedente = rimanenzeDict.GetValueOrDefault((famId, viewModel.Anno - 1, 12), 0);
                                        }
                                        else
                                        {
                                            rimPrecedente = rimanenzeDict.GetValueOrDefault((famId, viewModel.Anno, mese - 1), 0);
                                        }
                                        
                                        var delta = rimFinale - rimPrecedente;
                                        valori[mese] += (decimal)delta;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (!figliDict.ContainsKey(voce.IdCodiceConto))
                {
                    if (associazioniDict.TryGetValue(voce.IdCodiceConto, out var codiciPdC))
                    {
                        var saldiVoce = saldi.Where(s => codiciPdC.Contains(s.CodicePdC));
                        foreach (var saldo in saldiVoce)
                        {
                            if (saldo.Mese.HasValue && saldo.Mese >= 1 && saldo.Mese <= 12)
                            {
                                valori[saldo.Mese.Value] += saldo.Saldo * -1;
                            }
                        }
                    }
                }

                riga.ValoriMensili = valori;
                valoriCalcolati[voce.IdCodiceConto] = valori;
                righe.Add(riga);
            }

            var figliPerPadre = righe
                .Where(r => r.ParentId > 0)
                .GroupBy(r => r.ParentId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.IdCodiceConto).ToList());
            
            var righeDict = righe.ToDictionary(r => r.IdCodiceConto);
            
            foreach (var riga in righe.Where(r => r.HasFigli))
            {
                if (figliPerPadre.TryGetValue(riga.IdCodiceConto, out var figliIds))
                {
                    riga.HasOnlyTotalChildren = figliIds.All(fId => 
                        righeDict.TryGetValue(fId, out var figlio) && figlio.TipoConto == "T");
                }
            }
            
            var processati = new HashSet<int>();
            
            foreach (var riga in righe.Where(r => !r.HasFigli))
            {
                processati.Add(riga.IdCodiceConto);
            }
            
            bool cambiamenti = true;
            while (cambiamenti)
            {
                cambiamenti = false;
                foreach (var riga in righe.Where(r => r.HasFigli && !processati.Contains(r.IdCodiceConto)))
                {
                    if (figliPerPadre.TryGetValue(riga.IdCodiceConto, out var figliIds) && 
                        figliIds.All(f => processati.Contains(f)))
                    {
                        foreach (var figlioId in figliIds)
                        {
                            if (righeDict.TryGetValue(figlioId, out var figlio))
                            {
                                for (int mese = 1; mese <= 12; mese++)
                                {
                                    riga.ValoriMensili[mese] += figlio.ValoriMensili[mese];
                                }
                            }
                        }
                        processati.Add(riga.IdCodiceConto);
                        cambiamenti = true;
                    }
                }
            }

            // === VISTA FAMIGLIE: Calcola valori per famiglia ===
            if (viewModel.IsVistaFamiglie)
            {
                var percentualiQuery = _context.PstreePercentualiFamiglie
                    .Where(p => p.Anno == viewModel.Anno);
                
                if (sede.HasValue)
                {
                    percentualiQuery = percentualiQuery.Where(p => p.IdSede == sede.Value);
                }
                
                var percentuali = await percentualiQuery.ToListAsync();
                
                var percentualiDict = percentuali
                    .GroupBy(p => (p.IdCodiceConto, p.Mese))
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(p => p.IdFamiglia)
                              .ToDictionary(pg => pg.Key, pg => pg.First().Percentuale)
                    );
                
                var codiceContoToFamiglia = await _context.PstreeListaFamiglie
                    .Where(f => f.IdCodiceConto > 0)
                    .ToDictionaryAsync(f => f.IdCodiceConto, f => f.Id);
                
                var famigliaToFamigliaPrincipale = tutteFamiglie
                    .ToDictionary(f => f.Id, f => f.IdFamigliaPadre ?? f.Id);
                
                foreach (var riga in righe)
                {
                    riga.ValoriFamiglia = new Dictionary<int, decimal>();
                    
                    foreach (var fam in famiglieSelezionate)
                    {
                        riga.ValoriFamiglia[fam] = 0;
                    }
                    
                    decimal totaleCalcolato = 0;
                    decimal totaleMesiSelezionati = 0;
                    bool hasAnyPercentuale = false;
                    
                    if (riga.VoceRimanenza && codiceContoToFamiglia.TryGetValue(riga.IdCodiceConto, out var famigliaRimanenza))
                    {
                        var famigliaPrincipale = famigliaToFamigliaPrincipale.GetValueOrDefault(famigliaRimanenza, famigliaRimanenza);
                        
                        foreach (var mese in mesiSelezionati)
                        {
                            var valoreMese = riga.ValoriMensili[mese];
                            totaleMesiSelezionati += valoreMese;
                            
                            if (famiglieSelezionate.Contains(famigliaPrincipale))
                            {
                                riga.ValoriFamiglia[famigliaPrincipale] += valoreMese;
                                totaleCalcolato += valoreMese;
                            }
                        }
                        hasAnyPercentuale = true;
                    }
                    else
                    {
                        foreach (var mese in mesiSelezionati)
                        {
                            var valoreMese = riga.ValoriMensili[mese];
                            totaleMesiSelezionati += valoreMese;
                            
                            if (percentualiDict.TryGetValue((riga.IdCodiceConto, mese), out var percMese))
                            {
                                hasAnyPercentuale = true;
                                
                                foreach (var fam in famiglieSelezionate)
                                {
                                    if (percMese.TryGetValue(fam, out var perc))
                                    {
                                        var valoreCalcolato = valoreMese * (decimal)(perc / 100.0);
                                        riga.ValoriFamiglia[fam] += valoreCalcolato;
                                        totaleCalcolato += valoreCalcolato;
                                    }
                                }
                            }
                        }
                    }
                    
                    var residuo = totaleMesiSelezionati - totaleCalcolato;
                    
                    if (hasAnyPercentuale && Math.Abs(residuo) > 0 && Math.Abs(residuo) < 20m && famiglieSelezionate.Any())
                    {
                        var ultimaFamiglia = famiglieSelezionate.Last();
                        riga.ValoriFamiglia[ultimaFamiglia] += residuo;
                        riga.ValoreAnalitica = 0;
                        riga.HasPercentualeWarning = false;
                    }
                    else
                    {
                        riga.ValoreAnalitica = residuo;
                        
                        riga.HasPercentualeWarning = Math.Abs(riga.ValoreAnalitica) > 0.01m && hasAnyPercentuale;
                    }
                    
                    if (!hasAnyPercentuale && totaleMesiSelezionati != 0)
                    {
                        riga.ValoreAnalitica = totaleMesiSelezionati;
                        riga.HasPercentualeWarning = true;
                    }
                }
                
                var processatiFamiglia = new HashSet<int>();
                
                foreach (var riga in righe.Where(r => !r.HasFigli))
                {
                    processatiFamiglia.Add(riga.IdCodiceConto);
                }
                
                bool cambiamentiFamiglia = true;
                while (cambiamentiFamiglia)
                {
                    cambiamentiFamiglia = false;
                    foreach (var riga in righe.Where(r => r.HasFigli && !processatiFamiglia.Contains(r.IdCodiceConto)))
                    {
                        if (figliPerPadre.TryGetValue(riga.IdCodiceConto, out var figliIds) && 
                            figliIds.All(f => processatiFamiglia.Contains(f)))
                        {
                            foreach (var fam in famiglieSelezionate)
                            {
                                riga.ValoriFamiglia[fam] = 0;
                            }
                            riga.ValoreAnalitica = 0;
                            riga.HasPercentualeWarning = false;
                            
                            foreach (var figlioId in figliIds)
                            {
                                if (righeDict.TryGetValue(figlioId, out var figlio))
                                {
                                    foreach (var fam in famiglieSelezionate)
                                    {
                                        if (figlio.ValoriFamiglia.ContainsKey(fam))
                                        {
                                            riga.ValoriFamiglia[fam] += figlio.ValoriFamiglia[fam];
                                        }
                                    }
                                    riga.ValoreAnalitica += figlio.ValoreAnalitica;
                                    
                                    if (figlio.HasPercentualeWarning)
                                    {
                                        riga.HasPercentualeWarning = true;
                                    }
                                }
                            }
                            processatiFamiglia.Add(riga.IdCodiceConto);
                            cambiamentiFamiglia = true;
                        }
                    }
                }
            }

            viewModel.Righe = righe;

            // === CALCOLO TOTALI COSTI FISSI/DIRETTI ===
            var righeFoglia = righe.Where(r => !r.HasFigli).ToList();
            
            viewModel.TotaleCostiDiretti = new decimal[13];
            viewModel.TotaleCostiFissi = new decimal[13];
            viewModel.PercentualeFissiDiretti = new decimal[13];
            
            foreach (var riga in righeFoglia)
            {
                if (riga.CostiFD == "D")
                {
                    for (int mese = 1; mese <= 12; mese++)
                    {
                        viewModel.TotaleCostiDiretti[mese] += Math.Abs(riga.ValoriMensili[mese]);
                    }
                }
                else if (riga.CostiFD == "F")
                {
                    for (int mese = 1; mese <= 12; mese++)
                    {
                        viewModel.TotaleCostiFissi[mese] += Math.Abs(riga.ValoriMensili[mese]);
                    }
                }
            }
            
            for (int mese = 1; mese <= 12; mese++)
            {
                if (viewModel.TotaleCostiDiretti[mese] != 0)
                {
                    viewModel.PercentualeFissiDiretti[mese] = Math.Round(
                        viewModel.TotaleCostiFissi[mese] / viewModel.TotaleCostiDiretti[mese] * 100, 2);
                }
            }
            
            // === CALCOLO PER VISTA FAMIGLIE ===
            if (viewModel.IsVistaFamiglie)
            {
                viewModel.TotaleCostiDirettiFamiglia = new Dictionary<int, decimal>();
                viewModel.TotaleCostiFissiFamiglia = new Dictionary<int, decimal>();
                
                foreach (var fam in famiglieSelezionate)
                {
                    viewModel.TotaleCostiDirettiFamiglia[fam] = 0;
                    viewModel.TotaleCostiFissiFamiglia[fam] = 0;
                }
                
                foreach (var riga in righeFoglia)
                {
                    if (riga.CostiFD == "D")
                    {
                        foreach (var fam in famiglieSelezionate)
                        {
                            if (riga.ValoriFamiglia.ContainsKey(fam))
                            {
                                viewModel.TotaleCostiDirettiFamiglia[fam] += Math.Abs(riga.ValoriFamiglia[fam]);
                            }
                        }
                    }
                    else if (riga.CostiFD == "F")
                    {
                        foreach (var fam in famiglieSelezionate)
                        {
                            if (riga.ValoriFamiglia.ContainsKey(fam))
                            {
                                viewModel.TotaleCostiFissiFamiglia[fam] += Math.Abs(riga.ValoriFamiglia[fam]);
                            }
                        }
                    }
                }
            }

            // === DATI PER GRAFICO ANDAMENTO (ultimi 12 mesi) ===
            await CalcolaGraficoAndamento(sede);

            return View(viewModel);
        }

        /// <summary>
        /// Calcola i dati per il grafico di andamento (Fatturato, Risultato Esercizio) per gli ultimi 12 mesi
        /// </summary>
        private async Task CalcolaGraficoAndamento(int? sede)
        {
            const int ID_FATTURATO = 800;
            const int ID_RISULTATO_ESERCIZIO = 11100;
            
            var oggi = DateTime.Now;
            var meseFine = new DateTime(oggi.Year, oggi.Month, 1).AddMonths(-1);
            
            var mesiGrafico = new List<(int Anno, int Mese, string Label)>();
            for (int i = 11; i >= 0; i--)
            {
                var data = meseFine.AddMonths(-i);
                mesiGrafico.Add((data.Year, data.Month, data.ToString("MMM yy", new System.Globalization.CultureInfo("it-IT"))));
            }
            
            var anniCoinvolti = mesiGrafico.Select(m => m.Anno).Distinct().ToList();
            var annoPrimoMese = mesiGrafico.First().Anno;
            if (!anniCoinvolti.Contains(annoPrimoMese - 1))
            {
                anniCoinvolti.Add(annoPrimoMese - 1);
            }
            
            var struttura = await _context.PstreeStrutturaContoEconomico.ToListAsync();
            var associazioni = await _context.PstreeAssociazioniCE.ToListAsync();
            var associazioniDict = associazioni
                .GroupBy(a => a.IdCodiceConto)
                .ToDictionary(g => g.Key, g => g.Select(a => a.CodicePdC).ToList());
            
            var saldiGraficoQuery = _context.PstreeListaSaldi
                .Where(s => s.Anno.HasValue && anniCoinvolti.Contains(s.Anno.Value));
            if (sede.HasValue)
            {
                saldiGraficoQuery = saldiGraficoQuery.Where(s => s.IdSede == sede.Value);
            }
            var saldiGrafico = await saldiGraficoQuery.ToListAsync();
            
            var rettificheGraficoQuery = _context.PstreeListaRettifiche
                .Where(r => anniCoinvolti.Contains(r.Anno));
            if (sede.HasValue)
            {
                rettificheGraficoQuery = rettificheGraficoQuery.Where(r => r.IdSede == sede.Value);
            }
            var rettificheGrafico = await rettificheGraficoQuery.ToListAsync();
            
            var rimanenzeGraficoQuery = _context.PstreeListaRimanenze
                .Where(r => anniCoinvolti.Contains(r.Anno));
            if (sede.HasValue)
            {
                rimanenzeGraficoQuery = rimanenzeGraficoQuery.Where(r => r.IdSede == sede.Value);
            }
            var rimanenzeGrafico = await rimanenzeGraficoQuery.ToListAsync();
            
            var rimanenzeDict = rimanenzeGrafico
                .GroupBy(r => (r.IdFamiglia, r.Anno, r.Mese))
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Valore + r.RettificaValore));
            
            var famiglieDict = await _context.PstreeListaFamiglie
                .ToDictionaryAsync(f => f.Id, f => f.IdCodiceConto);
            
            var tutteFamiglieIds = famiglieDict.Keys.ToList();
            
            var figliDict = struttura
                .GroupBy(s => s.ParentId)
                .ToDictionary(g => g.Key, g => g.Select(s => s.IdCodiceConto).ToList());
            
            var valoriFatturato = new List<decimal>();
            var valoriRisultato = new List<decimal>();
            
            foreach (var (anno, mese, _) in mesiGrafico)
            {
                var saldiMese = saldiGrafico.Where(s => s.Anno == anno && s.Mese == mese).ToList();
                var rettificheMese = rettificheGrafico.Where(r => r.Anno == anno && r.Mese == mese).ToList();
                
                var valoriVoce = new Dictionary<int, decimal>();
                
                foreach (var voce in struttura)
                {
                    decimal valore = 0;
                    
                    if (voce.VoceRettifica)
                    {
                        valore = rettificheMese
                            .Where(r => r.IdCodiceConto == voce.IdCodiceConto)
                            .Sum(r => r.Saldo) * -1;
                    }
                    else if (voce.VoceRimanenza)
                    {
                        foreach (var famId in tutteFamiglieIds)
                        {
                            if (famiglieDict.TryGetValue(famId, out var codiceContoFam) && codiceContoFam == voce.IdCodiceConto)
                            {
                                var rimFinale = rimanenzeDict.GetValueOrDefault((famId, anno, mese), 0);
                                
                                double rimPrecedente;
                                if (mese == 1)
                                {
                                    rimPrecedente = rimanenzeDict.GetValueOrDefault((famId, anno - 1, 12), 0);
                                }
                                else
                                {
                                    rimPrecedente = rimanenzeDict.GetValueOrDefault((famId, anno, mese - 1), 0);
                                }
                                
                                valore += (decimal)(rimFinale - rimPrecedente);
                            }
                        }
                    }
                    else if (!figliDict.ContainsKey(voce.IdCodiceConto))
                    {
                        if (associazioniDict.TryGetValue(voce.IdCodiceConto, out var codiciPdC))
                        {
                            valore = saldiMese
                                .Where(s => codiciPdC.Contains(s.CodicePdC))
                                .Sum(s => s.Saldo) * -1;
                        }
                    }
                    
                    valoriVoce[voce.IdCodiceConto] = valore;
                }
                
                var processati = new HashSet<int>(struttura.Where(v => !figliDict.ContainsKey(v.IdCodiceConto)).Select(v => v.IdCodiceConto));
                bool cambiamenti = true;
                while (cambiamenti)
                {
                    cambiamenti = false;
                    foreach (var voce in struttura.Where(v => figliDict.ContainsKey(v.IdCodiceConto) && !processati.Contains(v.IdCodiceConto)))
                    {
                        var figliIds = figliDict[voce.IdCodiceConto];
                        if (figliIds.All(f => processati.Contains(f)))
                        {
                            valoriVoce[voce.IdCodiceConto] = figliIds.Sum(f => valoriVoce.GetValueOrDefault(f, 0));
                            processati.Add(voce.IdCodiceConto);
                            cambiamenti = true;
                        }
                    }
                }
                
                valoriFatturato.Add(valoriVoce.GetValueOrDefault(ID_FATTURATO, 0));
                valoriRisultato.Add(valoriVoce.GetValueOrDefault(ID_RISULTATO_ESERCIZIO, 0));
            }
            
            ViewBag.GraficoLabels = System.Text.Json.JsonSerializer.Serialize(mesiGrafico.Select(m => m.Label).ToList());
            ViewBag.GraficoFatturato = System.Text.Json.JsonSerializer.Serialize(valoriFatturato.Select(v => (double)v).ToList());
            ViewBag.GraficoRisultato = System.Text.Json.JsonSerializer.Serialize(valoriRisultato.Select(v => (double)v).ToList());
        }

        // GET: PstreeContoEconomico/ExportExcel
        public async Task<IActionResult> ExportExcel(int? anno, List<int>? famiglie, List<int>? mesi, int? sede, string? vista, bool escludiRimanenze = false)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            var viewModel = new Pstree_ContoEconomicoViewModel();
            
            viewModel.Anno = anno ?? DateTime.Now.Year;
            
            var meseCorrente = DateTime.Now.Month;
            var annoCorrente = DateTime.Now.Year;
            
            List<int> mesiDefault;
            if (viewModel.Anno == annoCorrente)
            {
                mesiDefault = meseCorrente > 1 ? Enumerable.Range(1, meseCorrente - 1).ToList() : new List<int>();
            }
            else if (viewModel.Anno < annoCorrente)
            {
                mesiDefault = Enumerable.Range(1, 12).ToList();
            }
            else
            {
                mesiDefault = new List<int>();
            }
            
            var mesiSelezionati = (mesi != null && mesi.Any()) ? mesi : mesiDefault;
            viewModel.MesiSelezionati = mesiSelezionati;
            
            viewModel.TipoVista = vista ?? "mesi";
            
            var tutteFamiglie = await _context.PstreeListaFamiglie.OrderBy(f => f.Id).ToListAsync();
            
            var famigliePrincipali = tutteFamiglie.Where(f => f.IdFamigliaPadre == null).ToList();
            var sottoFamiglie = tutteFamiglie.Where(f => f.IdFamigliaPadre != null).ToList();
            var mappaFiglie = sottoFamiglie
                .GroupBy(f => f.IdFamigliaPadre!.Value)
                .ToDictionary(g => g.Key, g => g.Select(f => f.Id).ToList());
            
            var famiglieSelezionate = (famiglie != null && famiglie.Any()) 
                ? famiglie 
                : famigliePrincipali.Where(f => f.Id != 0).Select(f => f.Id).ToList();
            viewModel.FamiglieSelezionate = famiglieSelezionate;
            
            viewModel.Famiglie = famigliePrincipali
                .Where(f => f.Id != 0)
                .Select(f => new Pstree_FamigliaCheckboxItem
                {
                    Id = f.Id,
                    Nome = f.NomeFamiglia,
                    Selezionato = famiglieSelezionate.Contains(f.Id)
                }).ToList();
            
            var famigliePerCalcolo = new List<int>(famiglieSelezionate);
            foreach (var famId in famiglieSelezionate)
            {
                if (mappaFiglie.TryGetValue(famId, out var figlie))
                {
                    famigliePerCalcolo.AddRange(figlie);
                }
            }
            
            var sedeNome = "";
            if (sede.HasValue)
            {
                var sedeEntity = await _context.PstreeListaSedi.FirstOrDefaultAsync(s => s.Id == sede.Value);
                sedeNome = sedeEntity?.Sede ?? "";
            }
            
            var struttura = await _context.PstreeStrutturaContoEconomico.OrderBy(s => s.Ordine).ToListAsync();
            
            var associazioni = await _context.PstreeAssociazioniCE.ToListAsync();
            var associazioniDict = associazioni
                .GroupBy(a => a.IdCodiceConto)
                .ToDictionary(g => g.Key, g => g.Select(a => a.CodicePdC).ToList());
            
            var saldiQuery = _context.PstreeListaSaldi
                .Where(s => s.Anno == viewModel.Anno && mesiSelezionati.Contains(s.Mese ?? 0));
            if (sede.HasValue)
            {
                saldiQuery = saldiQuery.Where(s => s.IdSede == sede.Value);
            }
            var saldi = await saldiQuery.ToListAsync();
            
            var rettificheQuery = _context.PstreeListaRettifiche.Where(r => r.Anno == viewModel.Anno);
            if (sede.HasValue)
            {
                rettificheQuery = rettificheQuery.Where(r => r.IdSede == sede.Value);
            }
            var rettifiche = await rettificheQuery.ToListAsync();
            
            var rimanenzeQuery = _context.PstreeListaRimanenze
                .Where(r => (r.Anno == viewModel.Anno && mesiSelezionati.Contains(r.Mese)) ||
                            (r.Anno == viewModel.Anno - 1 && r.Mese == 12));
            if (famigliePerCalcolo.Any())
            {
                rimanenzeQuery = rimanenzeQuery.Where(r => famigliePerCalcolo.Contains(r.IdFamiglia));
            }
            if (sede.HasValue)
            {
                rimanenzeQuery = rimanenzeQuery.Where(r => r.IdSede == sede.Value);
            }
            var rimanenze = await rimanenzeQuery.ToListAsync();
            
            var rimanenzeDict = rimanenze
                .GroupBy(r => (r.IdFamiglia, r.Anno, r.Mese))
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Valore + r.RettificaValore));
            
            var famiglieDict = await _context.PstreeListaFamiglie.ToDictionaryAsync(f => f.Id, f => f.IdCodiceConto);
            
            var figliDict = struttura
                .GroupBy(s => s.ParentId)
                .ToDictionary(g => g.Key, g => g.Select(s => s.IdCodiceConto).ToList());
            
            var righe = new List<Pstree_ContoEconomicoRigaViewModel>();
            
            foreach (var voce in struttura)
            {
                var riga = new Pstree_ContoEconomicoRigaViewModel
                {
                    IdCodiceConto = voce.IdCodiceConto,
                    DescrizioneConto = voce.DescrizioneConto,
                    TipoConto = voce.TipoConto,
                    Livello = voce.Livello,
                    ParentId = voce.ParentId,
                    Ordine = voce.Ordine,
                    HasFigli = figliDict.ContainsKey(voce.IdCodiceConto),
                    VoceRettifica = voce.VoceRettifica,
                    VoceRimanenza = voce.VoceRimanenza,
                    CostiFD = voce.CostiFD
                };
                
                var valori = new decimal[13];
                
                if (voce.VoceRettifica)
                {
                    var rettificheVoce = rettifiche.Where(r => r.IdCodiceConto == voce.IdCodiceConto);
                    foreach (var rett in rettificheVoce)
                    {
                        if (rett.Mese >= 1 && rett.Mese <= 12)
                        {
                            valori[rett.Mese] += rett.Saldo * -1;
                        }
                    }
                }
                else if (voce.VoceRimanenza)
                {
                    if (!escludiRimanenze)
                    {
                        foreach (var famId in famigliePerCalcolo)
                        {
                            if (famiglieDict.TryGetValue(famId, out var codiceContoFam))
                            {
                                if (codiceContoFam == voce.IdCodiceConto)
                                {
                                    for (int mese = 1; mese <= 12; mese++)
                                    {
                                        var rimFinale = rimanenzeDict.GetValueOrDefault((famId, viewModel.Anno, mese), 0);
                                        double rimPrecedente;
                                        if (mese == 1)
                                        {
                                            rimPrecedente = rimanenzeDict.GetValueOrDefault((famId, viewModel.Anno - 1, 12), 0);
                                        }
                                        else
                                        {
                                            rimPrecedente = rimanenzeDict.GetValueOrDefault((famId, viewModel.Anno, mese - 1), 0);
                                        }
                                        var delta = rimFinale - rimPrecedente;
                                        valori[mese] += (decimal)delta;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (!figliDict.ContainsKey(voce.IdCodiceConto))
                {
                    if (associazioniDict.TryGetValue(voce.IdCodiceConto, out var codiciPdC))
                    {
                        var saldiVoce = saldi.Where(s => codiciPdC.Contains(s.CodicePdC));
                        foreach (var saldo in saldiVoce)
                        {
                            if (saldo.Mese.HasValue && saldo.Mese >= 1 && saldo.Mese <= 12)
                            {
                                valori[saldo.Mese.Value] += saldo.Saldo * -1;
                            }
                        }
                    }
                }
                
                riga.ValoriMensili = valori;
                righe.Add(riga);
            }
            
            var figliPerPadreExport = righe
                .Where(r => r.ParentId > 0)
                .GroupBy(r => r.ParentId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.IdCodiceConto).ToList());
            
            var righeDictExport = righe.ToDictionary(r => r.IdCodiceConto);
            var processatiExport = new HashSet<int>();
            
            foreach (var riga in righe.Where(r => !r.HasFigli))
            {
                processatiExport.Add(riga.IdCodiceConto);
            }
            
            bool cambiamentiExport = true;
            while (cambiamentiExport)
            {
                cambiamentiExport = false;
                foreach (var riga in righe.Where(r => r.HasFigli && !processatiExport.Contains(r.IdCodiceConto)))
                {
                    if (figliPerPadreExport.TryGetValue(riga.IdCodiceConto, out var figliIds) && 
                        figliIds.All(f => processatiExport.Contains(f)))
                    {
                        foreach (var figlioId in figliIds)
                        {
                            if (righeDictExport.TryGetValue(figlioId, out var figlio))
                            {
                                for (int mese = 1; mese <= 12; mese++)
                                {
                                    riga.ValoriMensili[mese] += figlio.ValoriMensili[mese];
                                }
                            }
                        }
                        processatiExport.Add(riga.IdCodiceConto);
                        cambiamentiExport = true;
                    }
                }
            }
            
            // === CARICA PERCENTUALI PER FAMIGLIA ===
            var percentualiQuery = _context.PstreePercentualiFamiglie
                .Where(p => p.Anno == viewModel.Anno);
            if (sede.HasValue)
            {
                percentualiQuery = percentualiQuery.Where(p => p.IdSede == sede.Value);
            }
            var percentuali = await percentualiQuery.ToListAsync();
            
            var percentualiDict = percentuali
                .GroupBy(p => (p.IdCodiceConto, p.Mese))
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(p => p.IdFamiglia)
                          .ToDictionary(pg => pg.Key, pg => pg.First().Percentuale)
                );
            
            const int ID_ATTREZZERIA = 1;
            const int ID_STAMPAGGIO = 2;
            
            var rigaFatturato = righe.FirstOrDefault(r => r.IdCodiceConto == 800);
            
            var valoriFamigliaPerRiga = new Dictionary<int, Dictionary<int, decimal[]>>();
            
            var codiceContoToFamiglia = tutteFamiglie
                .Where(f => f.IdCodiceConto > 0)
                .ToDictionary(f => f.IdCodiceConto, f => f.Id);
            
            var famigliaToFamigliaPrincipale = tutteFamiglie
                .ToDictionary(f => f.Id, f => f.IdFamigliaPadre ?? f.Id);
            
            foreach (var riga in righe)
            {
                var valoriAttr = new decimal[13];
                var valoriStamp = new decimal[13];
                
                if (riga.VoceRimanenza && codiceContoToFamiglia.TryGetValue(riga.IdCodiceConto, out var famigliaRim))
                {
                    var famPrincipale = famigliaToFamigliaPrincipale.GetValueOrDefault(famigliaRim, famigliaRim);
                    for (int m = 1; m <= 12; m++)
                    {
                        if (famPrincipale == ID_ATTREZZERIA)
                            valoriAttr[m] = riga.ValoriMensili[m];
                        else if (famPrincipale == ID_STAMPAGGIO)
                            valoriStamp[m] = riga.ValoriMensili[m];
                    }
                }
                else if (!riga.HasFigli)
                {
                    for (int m = 1; m <= 12; m++)
                    {
                        var valore = riga.ValoriMensili[m];
                        if (valore != 0 && percentualiDict.TryGetValue((riga.IdCodiceConto, m), out var percMese))
                        {
                            if (percMese.TryGetValue(ID_ATTREZZERIA, out var percAttr))
                                valoriAttr[m] = valore * (decimal)(percAttr / 100.0);
                            if (percMese.TryGetValue(ID_STAMPAGGIO, out var percStamp))
                                valoriStamp[m] = valore * (decimal)(percStamp / 100.0);
                        }
                    }
                }
                
                valoriFamigliaPerRiga[riga.IdCodiceConto] = new Dictionary<int, decimal[]>
                {
                    { ID_ATTREZZERIA, valoriAttr },
                    { ID_STAMPAGGIO, valoriStamp }
                };
            }
            
            processatiExport.Clear();
            foreach (var riga in righe.Where(r => !r.HasFigli))
            {
                processatiExport.Add(riga.IdCodiceConto);
            }
            
            cambiamentiExport = true;
            while (cambiamentiExport)
            {
                cambiamentiExport = false;
                foreach (var riga in righe.Where(r => r.HasFigli && !processatiExport.Contains(r.IdCodiceConto)))
                {
                    if (figliPerPadreExport.TryGetValue(riga.IdCodiceConto, out var figliIds) && 
                        figliIds.All(f => processatiExport.Contains(f)))
                    {
                        var valoriAttr = new decimal[13];
                        var valoriStamp = new decimal[13];
                        
                        foreach (var figlioId in figliIds)
                        {
                            if (valoriFamigliaPerRiga.TryGetValue(figlioId, out var valoriFiglio))
                            {
                                for (int m = 1; m <= 12; m++)
                                {
                                    valoriAttr[m] += valoriFiglio[ID_ATTREZZERIA][m];
                                    valoriStamp[m] += valoriFiglio[ID_STAMPAGGIO][m];
                                }
                            }
                        }
                        
                        valoriFamigliaPerRiga[riga.IdCodiceConto] = new Dictionary<int, decimal[]>
                        {
                            { ID_ATTREZZERIA, valoriAttr },
                            { ID_STAMPAGGIO, valoriStamp }
                        };
                        
                        processatiExport.Add(riga.IdCodiceConto);
                        cambiamentiExport = true;
                    }
                }
            }
            
            // === GENERA EXCEL ===
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Conto Economico");
            
            var colorAttrezzeria = System.Drawing.Color.FromArgb(198, 239, 206);
            var colorStampaggio = System.Drawing.Color.FromArgb(189, 215, 238);
            var colorTotale = System.Drawing.Color.FromArgb(242, 242, 242);
            var colorHeader = System.Drawing.Color.FromArgb(68, 114, 196);
            var colorHeaderMese = System.Drawing.Color.FromArgb(91, 155, 213);
            
            ws.Cells[1, 1].Value = $"CONTO ECONOMICO {viewModel.Anno}";
            ws.Cells[1, 1].Style.Font.Size = 16;
            ws.Cells[1, 1].Style.Font.Bold = true;
            
            ws.Cells[2, 1].Value = $"Generato il: {DateTime.Now:dd/MM/yyyy HH:mm}";
            ws.Cells[2, 1].Style.Font.Italic = true;
            
            if (!string.IsNullOrEmpty(sedeNome))
            {
                ws.Cells[3, 1].Value = $"Sede: {sedeNome}";
            }
            
            // === INTESTAZIONI (2 righe) ===
            int headerRow1 = 5;
            int headerRow2 = 6;
            int startRow = 7;
            int col = 1;
            
            ws.Cells[headerRow1, col, headerRow2, col].Merge = true;
            ws.Cells[headerRow1, col].Value = "Descrizione";
            ws.Cells[headerRow1, col].Style.Font.Bold = true;
            ws.Cells[headerRow1, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow1, col].Style.Fill.BackgroundColor.SetColor(colorHeader);
            ws.Cells[headerRow1, col].Style.Font.Color.SetColor(System.Drawing.Color.White);
            ws.Cells[headerRow1, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[headerRow1, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            col++;
            
            foreach (var mese in mesiSelezionati.OrderBy(m => m))
            {
                ws.Cells[headerRow1, col, headerRow1, col + 5].Merge = true;
                ws.Cells[headerRow1, col].Value = Pstree_ContoEconomicoViewModel.NomiMesi[mese];
                ws.Cells[headerRow1, col].Style.Font.Bold = true;
                ws.Cells[headerRow1, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[headerRow1, col].Style.Fill.BackgroundColor.SetColor(colorHeaderMese);
                ws.Cells[headerRow1, col].Style.Font.Color.SetColor(System.Drawing.Color.White);
                ws.Cells[headerRow1, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                ws.Cells[headerRow2, col].Value = "Attr";
                ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorAttrezzeria);
                ws.Cells[headerRow2, col].Style.Font.Bold = true;
                ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col++;
                
                ws.Cells[headerRow2, col].Value = "%";
                ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorAttrezzeria);
                ws.Cells[headerRow2, col].Style.Font.Bold = true;
                ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col++;
                
                ws.Cells[headerRow2, col].Value = "Stamp";
                ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorStampaggio);
                ws.Cells[headerRow2, col].Style.Font.Bold = true;
                ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col++;
                
                ws.Cells[headerRow2, col].Value = "%";
                ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorStampaggio);
                ws.Cells[headerRow2, col].Style.Font.Bold = true;
                ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col++;
                
                ws.Cells[headerRow2, col].Value = "Totale";
                ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorTotale);
                ws.Cells[headerRow2, col].Style.Font.Bold = true;
                ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col++;
                
                ws.Cells[headerRow2, col].Value = "%";
                ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorTotale);
                ws.Cells[headerRow2, col].Style.Font.Bold = true;
                ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col++;
            }
            
            ws.Cells[headerRow1, col, headerRow1, col + 5].Merge = true;
            ws.Cells[headerRow1, col].Value = "TOTALE ANNO";
            ws.Cells[headerRow1, col].Style.Font.Bold = true;
            ws.Cells[headerRow1, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow1, col].Style.Fill.BackgroundColor.SetColor(colorHeader);
            ws.Cells[headerRow1, col].Style.Font.Color.SetColor(System.Drawing.Color.White);
            ws.Cells[headerRow1, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            ws.Cells[headerRow2, col].Value = "Attr";
            ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorAttrezzeria);
            ws.Cells[headerRow2, col].Style.Font.Bold = true;
            ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            col++;
            
            ws.Cells[headerRow2, col].Value = "%";
            ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorAttrezzeria);
            ws.Cells[headerRow2, col].Style.Font.Bold = true;
            ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            col++;
            
            ws.Cells[headerRow2, col].Value = "Stamp";
            ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorStampaggio);
            ws.Cells[headerRow2, col].Style.Font.Bold = true;
            ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            col++;
            
            ws.Cells[headerRow2, col].Value = "%";
            ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorStampaggio);
            ws.Cells[headerRow2, col].Style.Font.Bold = true;
            ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            col++;
            
            ws.Cells[headerRow2, col].Value = "Totale";
            ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorTotale);
            ws.Cells[headerRow2, col].Style.Font.Bold = true;
            ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            col++;
            
            ws.Cells[headerRow2, col].Value = "%";
            ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorTotale);
            ws.Cells[headerRow2, col].Style.Font.Bold = true;
            ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            col++;
            
            var colorPrevisione = System.Drawing.Color.FromArgb(255, 242, 204);
            
            ws.Cells[headerRow1, col, headerRow1, col + 1].Merge = true;
            ws.Cells[headerRow1, col].Value = "SIMULAZIONE";
            ws.Cells[headerRow1, col].Style.Font.Bold = true;
            ws.Cells[headerRow1, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow1, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 192, 0));
            ws.Cells[headerRow1, col].Style.Font.Color.SetColor(System.Drawing.Color.White);
            ws.Cells[headerRow1, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            ws.Cells[headerRow2, col].Value = "Previsione";
            ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorPrevisione);
            ws.Cells[headerRow2, col].Style.Font.Bold = true;
            ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            int colPrevisione = col;
            col++;
            
            ws.Cells[headerRow2, col].Value = "%Prev";
            ws.Cells[headerRow2, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[headerRow2, col].Style.Fill.BackgroundColor.SetColor(colorPrevisione);
            ws.Cells[headerRow2, col].Style.Font.Bold = true;
            ws.Cells[headerRow2, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            int colPercPrevisione = col;
            
            int lastCol = col;
            
            var idToExcelRow = new Dictionary<int, int>();
            int rigaFatturatoExcel = 0;
            
            // === DATI ===
            int row = startRow;
            foreach (var riga in righe)
            {
                col = 1;
                
                var indentSpaces = Math.Max(0, (riga.Livello - 1) * 2);
                var indent = indentSpaces > 0 ? new string(' ', indentSpaces) : "";
                ws.Cells[row, col].Value = indent + riga.DescrizioneConto;
                
                if (riga.TipoConto == "T" || riga.TipoConto == "S")
                {
                    ws.Cells[row, col].Style.Font.Bold = true;
                }
                col++;
                
                var hasValoriFam = valoriFamigliaPerRiga.TryGetValue(riga.IdCodiceConto, out var valoriFam);
                
                decimal totAnnoAttr = 0, totAnnoStamp = 0, totAnnoTot = 0;
                
                foreach (var mese in mesiSelezionati.OrderBy(m => m))
                {
                    var valAttr = hasValoriFam ? valoriFam![ID_ATTREZZERIA][mese] : 0;
                    var valStamp = hasValoriFam ? valoriFam![ID_STAMPAGGIO][mese] : 0;
                    var valTot = riga.ValoriMensili[mese];
                    
                    var fatturatoMese = rigaFatturato?.ValoriMensili[mese] ?? 0;
                    
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorAttrezzeria);
                    if (valAttr != 0)
                    {
                        ws.Cells[row, col].Value = valAttr;
                        ws.Cells[row, col].Style.Numberformat.Format = "#,##0";
                        if (valAttr < 0) ws.Cells[row, col].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    }
                    col++;
                    
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorAttrezzeria);
                    if (fatturatoMese != 0 && valAttr != 0)
                    {
                        ws.Cells[row, col].Value = valAttr / fatturatoMese;
                        ws.Cells[row, col].Style.Numberformat.Format = "0.0%";
                    }
                    col++;
                    
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorStampaggio);
                    if (valStamp != 0)
                    {
                        ws.Cells[row, col].Value = valStamp;
                        ws.Cells[row, col].Style.Numberformat.Format = "#,##0";
                        if (valStamp < 0) ws.Cells[row, col].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    }
                    col++;
                    
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorStampaggio);
                    if (fatturatoMese != 0 && valStamp != 0)
                    {
                        ws.Cells[row, col].Value = valStamp / fatturatoMese;
                        ws.Cells[row, col].Style.Numberformat.Format = "0.0%";
                    }
                    col++;
                    
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorTotale);
                    if (valTot != 0)
                    {
                        ws.Cells[row, col].Value = valTot;
                        ws.Cells[row, col].Style.Numberformat.Format = "#,##0";
                        ws.Cells[row, col].Style.Font.Bold = true;
                        if (valTot < 0) ws.Cells[row, col].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    }
                    col++;
                    
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorTotale);
                    if (fatturatoMese != 0 && valTot != 0)
                    {
                        ws.Cells[row, col].Value = valTot / fatturatoMese;
                        ws.Cells[row, col].Style.Numberformat.Format = "0.0%";
                    }
                    col++;
                    
                    totAnnoAttr += valAttr;
                    totAnnoStamp += valStamp;
                    totAnnoTot += valTot;
                }
                
                var fatturatoAnno = rigaFatturato != null ? mesiSelezionati.Sum(m => rigaFatturato.ValoriMensili[m]) : 0;
                
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorAttrezzeria);
                if (totAnnoAttr != 0)
                {
                    ws.Cells[row, col].Value = totAnnoAttr;
                    ws.Cells[row, col].Style.Numberformat.Format = "#,##0";
                    ws.Cells[row, col].Style.Font.Bold = true;
                    if (totAnnoAttr < 0) ws.Cells[row, col].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                }
                col++;
                
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorAttrezzeria);
                if (fatturatoAnno != 0 && totAnnoAttr != 0)
                {
                    ws.Cells[row, col].Value = totAnnoAttr / fatturatoAnno;
                    ws.Cells[row, col].Style.Numberformat.Format = "0.0%";
                }
                col++;
                
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorStampaggio);
                if (totAnnoStamp != 0)
                {
                    ws.Cells[row, col].Value = totAnnoStamp;
                    ws.Cells[row, col].Style.Numberformat.Format = "#,##0";
                    ws.Cells[row, col].Style.Font.Bold = true;
                    if (totAnnoStamp < 0) ws.Cells[row, col].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                }
                col++;
                
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorStampaggio);
                if (fatturatoAnno != 0 && totAnnoStamp != 0)
                {
                    ws.Cells[row, col].Value = totAnnoStamp / fatturatoAnno;
                    ws.Cells[row, col].Style.Numberformat.Format = "0.0%";
                }
                col++;
                
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorTotale);
                if (totAnnoTot != 0)
                {
                    ws.Cells[row, col].Value = totAnnoTot;
                    ws.Cells[row, col].Style.Numberformat.Format = "#,##0";
                    ws.Cells[row, col].Style.Font.Bold = true;
                    if (totAnnoTot < 0) ws.Cells[row, col].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                }
                col++;
                
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(colorTotale);
                if (fatturatoAnno != 0 && totAnnoTot != 0)
                {
                    ws.Cells[row, col].Value = totAnnoTot / fatturatoAnno;
                    ws.Cells[row, col].Style.Numberformat.Format = "0.0%";
                }
                
                idToExcelRow[riga.IdCodiceConto] = row;
                
                if (riga.IdCodiceConto == 800)
                {
                    rigaFatturatoExcel = row;
                }
                
                row++;
            }
            
            // === COLONNE PREVISIONE E %PREV ===
            foreach (var riga in righe)
            {
                if (!idToExcelRow.TryGetValue(riga.IdCodiceConto, out var rigaExcel))
                    continue;
                
                ws.Cells[rigaExcel, colPrevisione].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rigaExcel, colPrevisione].Style.Fill.BackgroundColor.SetColor(colorPrevisione);
                ws.Cells[rigaExcel, colPrevisione].Style.Numberformat.Format = "#,##0;[Red]-#,##0";
                
                if (riga.HasFigli)
                {
                    if (figliPerPadreExport.TryGetValue(riga.IdCodiceConto, out var figliIds))
                    {
                        var righeFilgliExcel = figliIds
                            .Where(f => idToExcelRow.ContainsKey(f))
                            .Select(f => idToExcelRow[f])
                            .OrderBy(r => r)
                            .ToList();
                        
                        if (righeFilgliExcel.Any())
                        {
                            var colLetter = GetExcelColumnLetter(colPrevisione);
                            var cellRefs = string.Join(",", righeFilgliExcel.Select(r => $"{colLetter}{r}"));
                            ws.Cells[rigaExcel, colPrevisione].Formula = $"SUM({cellRefs})";
                            ws.Cells[rigaExcel, colPrevisione].Style.Font.Bold = true;
                            ws.Cells[rigaExcel, colPrevisione].Style.Locked = true;
                        }
                    }
                }
                else
                {
                    ws.Cells[rigaExcel, colPrevisione].Value = 0;
                    ws.Cells[rigaExcel, colPrevisione].Style.Locked = false;
                }
                
                ws.Cells[rigaExcel, colPercPrevisione].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rigaExcel, colPercPrevisione].Style.Fill.BackgroundColor.SetColor(colorPrevisione);
                ws.Cells[rigaExcel, colPercPrevisione].Style.Numberformat.Format = "0.0%";
                ws.Cells[rigaExcel, colPercPrevisione].Style.Locked = true;
                
                if (rigaFatturatoExcel > 0)
                {
                    var colLetter = GetExcelColumnLetter(colPrevisione);
                    var cellPrevisione = $"{colLetter}{rigaExcel}";
                    var cellFatturato = $"${colLetter}${rigaFatturatoExcel}";
                    ws.Cells[rigaExcel, colPercPrevisione].Formula = $"IF({cellFatturato}=0,0,{cellPrevisione}/{cellFatturato})";
                }
            }
            
            ws.Protection.IsProtected = true;
            ws.Protection.AllowSelectLockedCells = true;
            ws.Protection.AllowSelectUnlockedCells = true;
            
            if (row > startRow)
            {
                ws.Column(1).Width = 45;
                for (int c = 2; c <= lastCol; c++)
                {
                    ws.Column(c).Width = 11;
                }
                
                var dataRange = ws.Cells[headerRow1, 1, row - 1, lastCol];
                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            
            ws.View.FreezePanes(startRow, 2);
            
            var fileName = $"ContoEconomico_{viewModel.Anno}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var content = package.GetAsByteArray();
            
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Converte un numero di colonna Excel (1-based) nella corrispondente lettera (A, B, ..., Z, AA, AB, ...)
        /// </summary>
        private static string GetExcelColumnLetter(int columnNumber)
        {
            string columnLetter = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnLetter = Convert.ToChar('A' + modulo) + columnLetter;
                columnNumber = (columnNumber - 1) / 26;
            }
            return columnLetter;
        }
    }
}
