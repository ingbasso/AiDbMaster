using System.Drawing;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Esporta in Excel la contabilità già salvata: padri, figli, posa, articoli singoli e sconti.
    /// I totali seguono le stesse regole della griglia a video.
    /// </summary>
    public static class ContabilitaExcel
    {
        private const int Colonne = 10;

        public static byte[] Crea(ContabilitaCantiereViewModel contabilita, string nomeCantiere)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var pacchetto = new ExcelPackage();
            var foglio = pacchetto.Workbook.Worksheets.Add("Contabilità");

            var riga = 1;
            var titolo = foglio.Cells[riga, 1, riga, Colonne];
            titolo.Merge = true;
            titolo.Value = contabilita.Tipo == TipoContabilitaCantiere.Finale
                ? "Contabilità finale"
                : "Contabilità iniziale";
            titolo.Style.Font.Bold = true;
            titolo.Style.Font.Size = 16;

            riga = 3;
            riga = ScriviVoce(foglio, riga, "Nome cantiere", nomeCantiere);
            riga = ScriviVoce(foglio, riga, "Agente", contabilita.NomeAgente);
            riga = ScriviVoce(foglio, riga, "Numero magazzino", contabilita.MagazzinoN);
            riga = ScriviVoce(foglio, riga, "Azienda posa", contabilita.ImpresaPosa);

            riga += 1;
            var rigaIntestazione = riga;
            string[] colonne =
            {
                "Art.", "Descrizione", "UM", "Q.tà", "Ct Un.€", "Tot.Ct.€", "Ric.%", "Ven. €/MQ", "Tot.Ven.€", "Note"
            };
            for (var i = 0; i < colonne.Length; i++)
                foglio.Cells[riga, i + 1].Value = colonne[i];

            using (var intestazione = foglio.Cells[riga, 1, riga, Colonne])
            {
                intestazione.Style.Font.Bold = true;
                intestazione.Style.Fill.PatternType = ExcelFillStyle.Solid;
                intestazione.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(33, 37, 41));
                intestazione.Style.Font.Color.SetColor(Color.White);
                intestazione.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            riga++;
            foreach (var padre in contabilita.Righe.Where(r => !RigaVuota(r)))
            {
                riga = ScriviRiga(foglio, riga, padre, padre: null);
                foreach (var figlio in padre.Figli.Where(f => !RigaVuota(f)))
                    riga = ScriviRiga(foglio, riga, figlio, padre);
            }

            var ultimaRigaDati = riga - 1;
            if (ultimaRigaDati >= rigaIntestazione)
            {
                using var tabella = foglio.Cells[rigaIntestazione, 1, ultimaRigaDati, Colonne];
                tabella.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tabella.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tabella.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tabella.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            riga += 1;
            riga = ScriviVoce(foglio, riga, "Riferimento ordine", contabilita.RiferimentoOrdine);
            riga = ScriviVoce(foglio, riga, "Note", contabilita.Note);

            var acconti = contabilita.Acconti
                .Where(a => !string.IsNullOrWhiteSpace(a.Descrizione) || a.Imponibile != 0)
                .ToList();
            if (acconti.Count == 0)
            {
                ScriviVoce(foglio, riga, "Acconti", "Nessun acconto");
            }
            else
            {
                var etichetta = foglio.Cells[riga, 1];
                etichetta.Value = "Acconti";
                etichetta.Style.Font.Bold = true;
                riga++;
                foreach (var acconto in acconti)
                {
                    foglio.Cells[riga, 1].Value = string.IsNullOrWhiteSpace(acconto.Descrizione)
                        ? "Acconto"
                        : acconto.Descrizione.Trim();
                    var importo = foglio.Cells[riga, 2];
                    importo.Value = acconto.Imponibile;
                    importo.Style.Numberformat.Format = "#,##0.00";
                    riga++;
                }
            }

            foglio.Column(1).Width = 22;
            foglio.Column(2).Width = 52;
            foglio.Column(3).Width = 8;
            for (var colonna = 4; colonna <= 9; colonna++)
                foglio.Column(colonna).Width = 14;
            foglio.Column(10).Width = 28;
            foglio.View.FreezePanes(rigaIntestazione + 1, 1);

            return pacchetto.GetAsByteArray();
        }

        private static int ScriviRiga(
            ExcelWorksheet foglio,
            int riga,
            ContabilitaCantiereRigaViewModel voce,
            ContabilitaCantiereRigaViewModel? padre)
        {
            var padreReale = padre == null && !voce.IsSconto && !voce.IsArticoloSingolo;
            var figlio = padre != null;

            foglio.Cells[riga, 1].Value = voce.CodiceArticolo;
            var descrizione = foglio.Cells[riga, 2];
            descrizione.Value = voce.IsSconto && string.IsNullOrWhiteSpace(voce.Descrizione)
                ? "Sconto"
                : voce.Descrizione;
            if (figlio)
                descrizione.Style.Indent = 1;

            if (!voce.IsSconto)
            {
                foglio.Cells[riga, 3].Value = voce.UnitaMisura;
                ScriviNumero(foglio.Cells[riga, 4], voce.QuantitaPosata, "#,##0.00");
            }

            if (!padreReale && !voce.IsSconto)
            {
                ScriviNumero(foglio.Cells[riga, 5], voce.CostoMaterialeServizio, "#,##0.00");
                var totaleCosto = Arrotonda((voce.QuantitaPosata ?? 0) * (voce.CostoMaterialeServizio ?? 0));
                if (totaleCosto != 0)
                    ScriviNumero(foglio.Cells[riga, 6], totaleCosto, "#,##0.00");
                // Stesso valore della griglia: se la posa non ha un ricarico salvato, a video compare 30.
                ScriviNumero(foglio.Cells[riga, 7], voce.RicaricoEffettivo, "0.##");
            }

            if (voce.IsSconto)
            {
                ScriviNumero(foglio.Cells[riga, 8], voce.PrezzoVenditaCliente.HasValue
                    ? Math.Abs(voce.PrezzoVenditaCliente.Value)
                    : null, "#,##0.00");
            }
            else
            {
                ScriviNumero(foglio.Cells[riga, 8], voce.PrezzoVenditaCliente, "#,##0.00");
            }

            var totaleVendita = TotaleVendita(voce, padre);
            if (totaleVendita is decimal vendita && Arrotonda(vendita) != 0)
                ScriviNumero(foglio.Cells[riga, 9], Arrotonda(vendita), "#,##0.00");

            foglio.Cells[riga, 10].Value = voce.Note;

            if (padreReale || voce.IsArticoloSingolo)
                foglio.Cells[riga, 1, riga, Colonne].Style.Font.Bold = padreReale;

            if (voce.IsPosa)
            {
                foglio.Cells[riga, 1, riga, Colonne].Style.Fill.PatternType = ExcelFillStyle.Solid;
                foglio.Cells[riga, 1, riga, Colonne].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 243, 224));
            }
            else if (voce.IsSconto)
            {
                foglio.Cells[riga, 1, riga, Colonne].Style.Fill.PatternType = ExcelFillStyle.Solid;
                foglio.Cells[riga, 1, riga, Colonne].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 245, 245));
            }

            return riga + 1;
        }

        /// <summary>
        /// Sul figlio (materiale o extra) il totale vendita è quantità del padre per il prezzo al metro quadro.
        /// Sulla posa, sul padre e sull'articolo singolo è quantità della riga per il prezzo.
        /// </summary>
        private static decimal? TotaleVendita(ContabilitaCantiereRigaViewModel voce, ContabilitaCantiereRigaViewModel? padre)
        {
            if (voce.IsSconto)
                return voce.PrezzoVenditaCliente.HasValue ? -Math.Abs(voce.PrezzoVenditaCliente.Value) : null;

            if (voce.PrezzoVenditaCliente is null)
                return null;

            if (padre != null && !voce.IsPosa)
                return (padre.QuantitaPosata ?? 0) * voce.PrezzoVenditaCliente.Value;

            return (voce.QuantitaPosata ?? 0) * voce.PrezzoVenditaCliente.Value;
        }

        private static bool RigaVuota(ContabilitaCantiereRigaViewModel riga)
        {
            return string.IsNullOrWhiteSpace(riga.CodiceArticolo)
                && string.IsNullOrWhiteSpace(riga.Descrizione)
                && riga.QuantitaPosata is null or 0
                && riga.CostoMaterialeServizio is null or 0
                && riga.PrezzoVenditaCliente is null or 0
                && string.IsNullOrWhiteSpace(riga.Note);
        }

        private static int ScriviVoce(ExcelWorksheet foglio, int riga, string etichetta, string? valore)
        {
            var cellaEtichetta = foglio.Cells[riga, 1];
            cellaEtichetta.Value = etichetta;
            cellaEtichetta.Style.Font.Bold = true;

            var cellaValore = foglio.Cells[riga, 2, riga, Colonne];
            cellaValore.Merge = true;
            cellaValore.Value = string.IsNullOrWhiteSpace(valore) ? string.Empty : NormalizzaTesto(valore);
            cellaValore.Style.WrapText = true;
            return riga + 1;
        }

        private static decimal Arrotonda(decimal valore) =>
            Math.Round(valore, 2, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Excel va a capo con il solo carattere di nuova riga.
        /// Il ritorno a capo di Windows, se lasciato, finisce nel file come simbolo strano.
        /// </summary>
        private static string NormalizzaTesto(string valore) =>
            valore.Replace("\r\n", "\n").Replace('\r', '\n').Trim();

        private static void ScriviNumero(ExcelRange cella, decimal? valore, string formato)
        {
            if (valore is null)
                return;

            cella.Value = valore.Value;
            cella.Style.Numberformat.Format = formato;
        }
    }
}
