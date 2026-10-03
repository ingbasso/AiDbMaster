using System.Globalization;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Stampa la fattura proforma della contabilità, sul modello del PDF di esempio.
    /// Entra solo quello che è già stato salvato: padri, articoli singoli e sconti.
    /// </summary>
    public static class ProformaContabilitaPdf
    {
        private static readonly CultureInfo Italiano = CultureInfo.GetCultureInfo("it-IT");
        private static readonly Border Bordo = new SolidBorder(ColorConstants.BLACK, 0.6f);
        private const string FraseLegale =
            "Il presente documento non costituisce fattura valida ai fini del DPR 633/72. La fattura definitiva verrà inviata tramite SDI.";

        public static byte[] Crea(
            ContabilitaCantiereViewModel contabilita,
            Cantiere cantiere,
            AnagraficaClienti? cliente,
            string? percorsoIntestazione,
            DateTime dataDocumento)
        {
            var normale = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var grassetto = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            using var memoria = new MemoryStream();
            using var scrittore = new PdfWriter(memoria);
            scrittore.SetCloseStream(false);
            using var pdf = new PdfDocument(scrittore);
            using var documento = new iText.Layout.Document(pdf, PageSize.A4);
            documento.SetMargins(18, 28, 18, 28);

            var foglio = new Div().SetBorder(Bordo).SetPadding(8);
            foglio.Add(Intestazione(cantiere, cliente, percorsoIntestazione, normale, grassetto));
            foglio.Add(Titolo(dataDocumento, grassetto));
            foglio.Add(RigaPagamento(cantiere.Pagamento, normale, grassetto));
            foglio.Add(TabellaRighe(contabilita, normale, grassetto));
            foglio.Add(RigaAcconti(contabilita, normale));
            foglio.Add(Totali(contabilita, grassetto));
            foglio.Add(new Paragraph(FraseLegale)
                .SetFont(normale)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(8)
                .SetMarginBottom(2));

            documento.Add(foglio);
            documento.Close();
            return memoria.ToArray();
        }

        private static Table Intestazione(
            Cantiere cantiere,
            AnagraficaClienti? cliente,
            string? percorsoIntestazione,
            PdfFont normale,
            PdfFont grassetto)
        {
            var tabella = new Table(UnitValue.CreatePercentArray(new[] { 56f, 44f }))
                .UseAllAvailableWidth()
                .SetMargin(0)
                .SetBorder(Border.NO_BORDER);

            var sinistra = new Cell().SetBorder(Border.NO_BORDER).SetPadding(2).SetVerticalAlignment(VerticalAlignment.TOP);
            if (!string.IsNullOrWhiteSpace(percorsoIntestazione) && File.Exists(percorsoIntestazione))
                AggiungiLogo(sinistra, percorsoIntestazione, normale);
            sinistra.Add(DatiSocieta(normale));
            tabella.AddCell(sinistra);

            var destra = new Cell().SetBorder(Border.NO_BORDER).SetPadding(2).SetVerticalAlignment(VerticalAlignment.TOP);
            destra.Add(RiquadroCliente(cliente, cantiere.CodiceCliente, normale, grassetto));
            destra.Add(RiquadroDestinazione(cantiere, normale, grassetto));
            tabella.AddCell(destra);
            return tabella;
        }

        /// <summary>
        /// ARCHITECTURAL SURFACES sta subito sotto il logo e viene
        /// allargata fino a coprire tutta la scritta FAVARO1.
        /// </summary>
        private static void AggiungiLogo(Cell cella, string percorsoLogo, PdfFont font)
        {
            const float larghezzaLogo = 210f;
            const string payOff = "ARCHITECTURAL SURFACES";
            const float dimensionePayOff = 6.2f;

            var logo = new Image(ImageDataFactory.Create(percorsoLogo))
                .SetWidth(larghezzaLogo)
                .SetHorizontalAlignment(HorizontalAlignment.LEFT)
                .SetMarginTop(0)
                .SetMarginBottom(1);
            cella.Add(logo);

            var larghezzaTesto = font.GetWidth(payOff, dimensionePayOff);
            var intervalli = payOff.Length - 1;
            var spaziatura = intervalli > 0 ? (larghezzaLogo - larghezzaTesto) / intervalli : 0f;

            cella.Add(new Paragraph(payOff)
                .SetFont(font)
                .SetFontSize(dimensionePayOff)
                .SetFontColor(new DeviceRgb(70, 70, 70))
                .SetCharacterSpacing(spaziatura)
                .SetMargin(0)
                .SetMarginBottom(5)
                .SetMultipliedLeading(1f));
        }

        private static Table DatiSocieta(PdfFont font)
        {
            var tabella = new Table(UnitValue.CreatePercentArray(new[] { 58f, 42f }))
                .UseAllAvailableWidth()
                .SetMargin(0);

            tabella.AddCell(LineeSenzaBordo(new[]
            {
                "Favaro1 S.r.l. società con socio unico,",
                "soggetta a direzione e coordinamento di",
                "ZEROPIU' S.R.L.",
                "Via Noalese, 79",
                "31059 Zero Branco",
                "Treviso, Italy",
                "Tel  +39 0422 4868",
                "Fax amm. +39 0422 97508",
                "Fax comm. +39 0422 487263"
            }, font, 6.5f));

            tabella.AddCell(LineeSenzaBordo(new[]
            {
                "www.favaro1.com",
                "info@favaro1.com",
                "Reg.Impr.TV",
                "C.Fisc. / P.Iva IT 00199020264",
                "R.E.A. N. 97182",
                "Cap. Soc. € 1.000.000"
            }, font, 6.5f));

            return tabella;
        }

        private static Table RiquadroCliente(AnagraficaClienti? cliente, int codiceCliente, PdfFont normale, PdfFont grassetto)
        {
            var nome = cliente == null
                ? string.Empty
                : string.IsNullOrWhiteSpace(cliente.DescrizioneUlteriore)
                    ? cliente.RagioneSociale
                    : $"{cliente.RagioneSociale} {cliente.DescrizioneUlteriore}".Trim();

            var righe = new List<string>();
            if (!string.IsNullOrWhiteSpace(nome))
                righe.Add(nome.Trim());
            if (!string.IsNullOrWhiteSpace(cliente?.Indirizzo))
                righe.Add(cliente.Indirizzo.Trim());
            var luogo = ComponiLuogo(cliente?.Cap, cliente?.Citta, cliente?.Provincia);
            if (!string.IsNullOrWhiteSpace(luogo))
                righe.Add(luogo);

            return Riquadro("Spett.le Ditta :", codiceCliente.ToString(), righe, normale, grassetto);
        }

        private static Table RiquadroDestinazione(Cantiere cantiere, PdfFont normale, PdfFont grassetto)
        {
            var righe = new List<string>();
            if (!string.IsNullOrWhiteSpace(cantiere.Nome))
                righe.Add(cantiere.Nome.Trim());
            if (!string.IsNullOrWhiteSpace(cantiere.Indirizzo))
                righe.Add(cantiere.Indirizzo.Trim());
            var luogo = ComponiLuogo(cantiere.Cap, cantiere.Localita, cantiere.Provincia);
            if (!string.IsNullOrWhiteSpace(luogo))
                righe.Add(luogo);

            return Riquadro("Luogo di destinazione :", null, righe, normale, grassetto);
        }

        private static Table Riquadro(string titolo, string? codice, List<string> righe, PdfFont normale, PdfFont grassetto)
        {
            var box = new Table(1).UseAllAvailableWidth().SetMargin(0).SetMarginBottom(6);
            box.AddCell(new Cell()
                .Add(new Paragraph(titolo).SetFont(grassetto).SetFontSize(8).SetMargin(0))
                .SetBorder(Bordo)
                .SetBorderBottom(Border.NO_BORDER)
                .SetPadding(3));

            var corpo = new Cell().SetBorder(Bordo).SetBorderTop(Border.NO_BORDER).SetPadding(3).SetMinHeight(42);
            if (!string.IsNullOrWhiteSpace(codice))
            {
                var rigaCodice = new Table(UnitValue.CreatePercentArray(new[] { 28f, 72f })).UseAllAvailableWidth();
                rigaCodice.AddCell(TestoSenzaBordo(codice, normale, 8));
                rigaCodice.AddCell(LineeSenzaBordo(righe, normale, 8));
                corpo.Add(rigaCodice);
            }
            else
            {
                corpo.Add(LineeSenzaBordo(righe, normale, 8));
            }

            box.AddCell(corpo);
            return box;
        }

        private static Paragraph Titolo(DateTime dataDocumento, PdfFont grassetto)
        {
            return new Paragraph($"FATTURA PROFORMA DEL {dataDocumento:dd/MM/yyyy}")
                .SetFont(grassetto)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMargin(0)
                .SetPaddingTop(4)
                .SetPaddingBottom(4)
                .SetBorderTop(Bordo)
                .SetBorderBottom(Bordo);
        }

        private static Table RigaPagamento(string? pagamento, PdfFont normale, PdfFont grassetto)
        {
            var testo = string.IsNullOrWhiteSpace(pagamento) ? string.Empty : pagamento.Trim();
            var tabella = new Table(UnitValue.CreatePercentArray(new[] { 38f, 34f, 28f }))
                .UseAllAvailableWidth()
                .SetMargin(0);

            tabella.AddCell(CellaMista("PAGAMENTO: ", testo, grassetto, normale));
            tabella.AddCell(CellaMista("SCADENZA: ", testo, grassetto, normale));
            tabella.AddCell(CellaMista("CIG: ", "---    CUP: ---", grassetto, normale));
            return tabella;
        }

        private static Table TabellaRighe(ContabilitaCantiereViewModel contabilita, PdfFont normale, PdfFont grassetto)
        {
            var tabella = new Table(UnitValue.CreatePercentArray(new[] { 48f, 8f, 10f, 12f, 14f, 8f }))
                .UseAllAvailableWidth()
                .SetMargin(0);

            tabella.AddHeaderCell(IntestazioneColonna("DESCRIZIONE", grassetto));
            tabella.AddHeaderCell(IntestazioneColonna("U.M.", grassetto));
            tabella.AddHeaderCell(IntestazioneColonna("Q.TA'", grassetto));
            tabella.AddHeaderCell(IntestazioneColonna("PREZZO", grassetto));
            tabella.AddHeaderCell(IntestazioneColonna("IMPORTO", grassetto));
            tabella.AddHeaderCell(IntestazioneColonna("IVA", grassetto));

            var iva = TestoIva(contabilita.AliquotaIva);
            var stampate = 0;
            foreach (var riga in contabilita.Righe.Where(VaInStampa))
            {
                stampate++;
                var importo = riga.TotaleVendita;
                tabella.AddCell(Cella(riga.Descrizione, normale, 8, TextAlignment.LEFT));
                tabella.AddCell(Cella(riga.UnitaMisura?.Trim() ?? string.Empty, normale, 8, TextAlignment.CENTER));
                tabella.AddCell(Cella(riga.IsSconto ? string.Empty : TestoQuantita(riga.QuantitaPosata), normale, 8, TextAlignment.RIGHT));
                tabella.AddCell(Cella(riga.IsSconto ? string.Empty : TestoPrezzo(riga.PrezzoVenditaCliente), normale, 8, TextAlignment.RIGHT));
                tabella.AddCell(Cella(Euro(importo), normale, 8, TextAlignment.RIGHT));
                tabella.AddCell(Cella(iva, normale, 8, TextAlignment.CENTER));
            }

            if (stampate == 0)
            {
                tabella.AddCell(Cella(string.Empty, normale, 8, TextAlignment.LEFT).SetMinHeight(28));
                for (var i = 0; i < 5; i++)
                    tabella.AddCell(Cella(string.Empty, normale, 8, TextAlignment.CENTER));
            }

            var nota = (contabilita.Note ?? string.Empty).Trim();
            var riferimento = (contabilita.RiferimentoOrdine ?? string.Empty).Trim();
            var cellaNota = new Cell()
                .SetBorder(Bordo)
                .SetPadding(6)
                .SetMinHeight(stampate < 8 ? 90 : 36)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetTextAlignment(TextAlignment.CENTER);
            if (nota.Length > 0)
            {
                cellaNota.Add(new Paragraph(nota)
                    .SetFont(grassetto)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMargin(0));
            }
            if (riferimento.Length > 0)
            {
                cellaNota.Add(new Paragraph(riferimento)
                    .SetFont(grassetto)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMargin(0)
                    .SetMarginTop(nota.Length > 0 ? 6 : 0));
            }
            if (nota.Length == 0 && riferimento.Length == 0)
                cellaNota.Add(new Paragraph(" ").SetFont(normale).SetFontSize(8).SetMargin(0));

            tabella.AddCell(cellaNota);
            for (var i = 0; i < 5; i++)
                tabella.AddCell(Cella(string.Empty, normale, 8, TextAlignment.CENTER));

            return tabella;
        }

        private static bool VaInStampa(ContabilitaCantiereRigaViewModel riga)
        {
            if (riga.IsPosa || !riga.MostraInProforma)
                return false;

            var vuota = string.IsNullOrWhiteSpace(riga.CodiceArticolo)
                && string.IsNullOrWhiteSpace(riga.Descrizione)
                && riga.QuantitaPosata is null or 0
                && riga.PrezzoVenditaCliente is null or 0;
            return !vuota;
        }

        private static Paragraph RigaAcconti(ContabilitaCantiereViewModel contabilita, PdfFont normale)
        {
            if (contabilita.ImponibileAcconto == 0)
                return new Paragraph().SetMargin(0).SetFontSize(1);

            return new Paragraph($"Acconti già ricevuti: {Euro(contabilita.ImponibileAcconto)}")
                .SetFont(normale)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMargin(0)
                .SetPaddingTop(3)
                .SetPaddingBottom(2);
        }

        private static Table Totali(ContabilitaCantiereViewModel contabilita, PdfFont grassetto)
        {
            var imponibile = contabilita.ImponibileSaldo;
            var totale = contabilita.TotaleConIva;
            var iva = totale - imponibile;

            var tabella = new Table(UnitValue.CreatePercentArray(new[] { 34f, 33f, 33f }))
                .UseAllAvailableWidth()
                .SetMargin(0)
                .SetMarginTop(2);

            tabella.AddCell(CellaTotale("IMPONIBILE", Euro(imponibile), grassetto));
            tabella.AddCell(CellaTotale("IVA", Euro(iva), grassetto));
            tabella.AddCell(CellaTotale("TOTALE EURO", Euro(totale), grassetto));
            return tabella;
        }

        private static Cell CellaTotale(string etichetta, string valore, PdfFont grassetto)
        {
            var paragrafo = new Paragraph()
                .SetMargin(0)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMultipliedLeading(1.15f);
            paragrafo.Add(new Text(etichetta + "\n").SetFont(grassetto).SetFontSize(8));
            paragrafo.Add(new Text(valore).SetFont(grassetto).SetFontSize(11));
            return new Cell()
                .Add(paragrafo)
                .SetBorder(Bordo)
                .SetPadding(4)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        private static Cell IntestazioneColonna(string testo, PdfFont grassetto)
        {
            return Cella(testo, grassetto, 8, TextAlignment.CENTER);
        }

        private static Cell Cella(string testo, PdfFont font, float size, TextAlignment align)
        {
            return new Cell()
                .Add(new Paragraph(testo ?? string.Empty).SetFont(font).SetFontSize(size).SetMargin(0).SetMultipliedLeading(1.1f))
                .SetBorder(Bordo)
                .SetPadding(3)
                .SetTextAlignment(align)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        private static Cell CellaMista(string etichetta, string valore, PdfFont grassetto, PdfFont normale)
        {
            var paragrafo = new Paragraph().SetMargin(0).SetMultipliedLeading(1.1f);
            paragrafo.Add(new Text(etichetta).SetFont(grassetto).SetFontSize(8));
            paragrafo.Add(new Text(valore).SetFont(normale).SetFontSize(8));
            return new Cell()
                .Add(paragrafo)
                .SetBorder(Bordo)
                .SetPadding(3)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        private static Cell TestoSenzaBordo(string testo, PdfFont font, float size)
        {
            return new Cell()
                .Add(new Paragraph(testo).SetFont(font).SetFontSize(size).SetMargin(0))
                .SetBorder(Border.NO_BORDER)
                .SetPadding(1);
        }

        private static Cell LineeSenzaBordo(IEnumerable<string> righe, PdfFont font, float size)
        {
            var cella = new Cell().SetBorder(Border.NO_BORDER).SetPadding(1);
            var scritte = righe.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
            if (scritte.Count == 0)
                scritte.Add(" ");

            foreach (var riga in scritte)
            {
                cella.Add(new Paragraph(riga.Trim())
                    .SetFont(font)
                    .SetFontSize(size)
                    .SetMargin(0)
                    .SetMultipliedLeading(1.15f));
            }

            return cella;
        }

        private static string ComponiLuogo(string? cap, string? citta, string? provincia)
        {
            var luogo = $"{cap} {citta}".Trim();
            var sigla = (provincia ?? string.Empty).Trim();
            if (sigla.Length == 0)
                return luogo;
            return luogo.Length == 0 ? $"({sigla})" : $"{luogo} ({sigla})";
        }

        private static string Euro(decimal valore) => valore.ToString("N2", Italiano) + " \u20AC";

        private static string TestoQuantita(decimal? quantita) =>
            quantita.HasValue ? quantita.Value.ToString("N2", Italiano) : string.Empty;

        private static string TestoPrezzo(decimal? prezzo) =>
            prezzo.HasValue ? prezzo.Value.ToString("N2", Italiano) + " \u20AC" : string.Empty;

        private static string TestoIva(decimal aliquota)
        {
            if (aliquota == decimal.Truncate(aliquota))
                return aliquota.ToString("0", Italiano) + "%";
            return aliquota.ToString("0.##", Italiano) + "%";
        }
    }
}
