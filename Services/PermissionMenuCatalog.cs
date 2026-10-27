namespace AiDbMaster.Services
{
    /// <summary>
    /// Ordine delle pagine nella gestione permessi.
    /// Ripete il menu laterale: prima l'area, poi le pagine, poi gli eventuali sottogruppi.
    /// </summary>
    public static class PermissionMenuCatalog
    {
        public static IReadOnlyList<PermissionMenuEntry> Entries { get; } = new List<PermissionMenuEntry>
        {
            Page("Home", "Home", "bi-house-door"),

            Group("Interrogazioni DB", "bi-search", "InterrogazioniDB",
                Page("Disponibilita", "Disponibilità", "bi-boxes"),
                Page("AvvisoMercePronta", "Avviso Merce Pronta", "bi-envelope-paper"),
                Page("ListaEmailInviate", "Lista Email Inviate", "bi-envelope-open"),
                Page("ListaEmailInviate", "Log Email Automatiche", "bi-journal-text"),
                Page("Grafici", "Grafici", "bi-graph-up"),
                Page("StoricoMaterialeLiberato", "Storico Mat. Liberato", "bi-clock-history"),
                Page("InterrogazioniAI", "Interrogazioni AI", "bi-robot")),

            Group("Produzione", "bi-gear-wide-connected", "Produzione",
                Page("ListaOPDashboard", "Dashboard", "bi-graph-up"),
                Page("SchedulatoreOP", "Schedulatore OP", "bi-calendar2-check"),
                Page("ListaOP", "Ordini di Produzione", "bi-list-ul"),
                Page("FermiSchedulati", "Fermi Schedulati", "bi-calendar-check"),
                Page("ProductionCalc", "Production Calc", "bi-calculator"),
                Page("ConsumiProduzione", "Consumi Prod. Previsti", "bi-clipboard-data"),
                Page("DurataDelleScorte", "Durata delle Scorte", "bi-hourglass-split")),

            Group("Consegne", "bi-truck", "Consegne",
                Page("ComposizioneViaggi", "Composizione Viaggi", "bi-puzzle"),
                Page("ConsegneKanban", "Calendario Consegne", "bi-calendar3"),
                Page("ViaggiConsegna", "Lista Viaggi Consegna", "bi-calendar-event"),
                Page("OrdiniTestate", "Lista Ordini Clienti", "bi-calendar-event"),
                Page("DashboardConsegne", "Dashboard Consegne", "bi-graph-up"),
                Group("Tabelle e Utilità", "bi-tools", null,
                    Page("Mezzi", "Mezzi Interni", "bi-truck-front"),
                    Page("MezziTrasportoEsterni", "Mezzi Esterni", "bi-truck-flatbed"),
                    Page("TipiTrasporto", "Tipi Trasporto", "bi-signpost-split"),
                    Page("Autisti", "Autisti", "bi-person-badge"),
                    Page("Indisponibilita", "Assenze e Fermi", "bi-calendar-x"))),

            Group("Gestione Cantieri", "bi-building", null,
                Page("GestioneCantieri", "Elenco Cantieri", "bi-list-ul"),
                Page("ContabilitaCantieri", "Contabilità cantiere", "bi-calculator"),
                Page("ImpresePosa", "Imprese di posa", "bi-person-workspace"),
                Page("ReferentiCantiere", "Referenti di cantiere", "bi-person-vcard"),
                Page("CostiArticoliCantiere", "Costi articoli", "bi-cash-coin")),

            Group("Controllo di Gestione", "bi-calculator", null,
                Page("PstreeContoEconomico", "Conto Economico", "bi-graph-up-arrow"),
                Group("Tabelle e Utilità", "bi-tools", null,
                    Page("PstreeStrutturaContoEconomico", "Struttura CE", "bi-diagram-3"),
                    Page("PstreeStrutturaContoEconomico", "Struttura CE (Albero)", "bi-diagram-2"),
                    Page("PstreeListaSaldi", "Lista Saldi", "bi-coin"),
                    Page("PstreeAssociazioniCE", "Associazioni CE", "bi-link-45deg"),
                    Page("PstreeAssociazioniImportSaldiCE", "Ripartizioni Import Saldi", "bi-pie-chart"),
                    Page("PstreeListaRettifiche", "Lista Rettifiche", "bi-arrow-left-right"),
                    Page("PstreeListaFamiglie", "Famiglie", "bi-layers"),
                    Page("PstreeSottoGruppi", "Sotto-Gruppi", "bi-diagram-3"),
                    Page("PstreeListaSedi", "Sedi", "bi-building"),
                    Page("PstreeListaRimanenze", "Rimanenze", "bi-box-seam"),
                    Page("PstreeListaPianoDeiConti", "Piano dei Conti", "bi-book"),
                    Page("PstreePercentualiFamiglie", "Percentuali Famiglie", "bi-percent"))),

            Group("Tabelle", "bi-table", "Tabelle",
                Group("Anagrafiche", "bi-person-vcard", null,
                    Page("AnagraficaArticoli", "Anagrafica Articoli", "bi-table"),
                    Page("AnagraficaClienti", "Anagrafica Clienti", "bi-people"),
                    Page("AnagraficaFornitori", "Anagrafica Fornitori", "bi-truck")),
                Group("Tabelle e Utilità", "bi-tools", null,
                    Page("ArticoliSostitutivi", "Articoli Sostitutivi", "bi-arrow-left-right"),
                    Page("ProgressiviArticoli", "Progressivi Articoli", "bi-boxes"),
                    Page("TabellaAgenti", "Agenti", "bi-person-workspace"),
                    Page("TabellaMagazzini", "Magazzini", "bi-building"),
                    Page("Lavorazioni", "Lavorazioni", "bi-gear-wide"),
                    Page("CentriLavoro", "Centri di Lavoro", "bi-building-gear"),
                    Page("Operatori", "Operatori", "bi-people"),
                    Page("StatiOP", "Stati OP", "bi-flag"),
                    Page("OrdiniTestate", "Gestione Ordini CF", "bi-clipboard-check"),
                    Page("TempiAsciugatura", "Tempi di Asciugatura", "bi-calendar-day"))),

            Group("Amministrazione", "bi-gear", "Amministrazione",
                Page("UserManagement", "Gestione utenti", "bi-people"),
                Page("RoleManagement", "Gestione ruoli", "bi-shield-lock"),
                Page("PermissionManagement", "Gestione Permessi", "bi-shield-lock-fill"),
                Page("AgentiToUser", "Converti Agenti in Utenti", "bi-person-plus-fill"),
                Page("AISettings", "Impostazioni AI", "bi-robot"),
                Page("SyncfusionTest", "Test Syncfusion", "bi-grid-3x3-gap"),
                Page("TabellaOpzioni", "Tabella Opzioni", "bi-sliders"),
                Page("ConsegneProgrammate", "Consegne Programmate", "bi-calendar-event"))
        };

        private static PermissionMenuEntry Page(string resourceName, string displayName, string icon)
        {
            return new PermissionMenuEntry
            {
                ResourceName = resourceName,
                DisplayName = displayName,
                Icon = icon
            };
        }

        private static PermissionMenuEntry Group(string displayName, string icon, string? resourceName, params PermissionMenuEntry[] children)
        {
            return new PermissionMenuEntry
            {
                ResourceName = resourceName,
                DisplayName = displayName,
                Icon = icon,
                IsGroup = true,
                Children = children.ToList()
            };
        }
    }

    public class PermissionMenuEntry
    {
        public string? ResourceName { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string? Icon { get; init; }
        public bool IsGroup { get; init; }
        public List<PermissionMenuEntry> Children { get; init; } = new List<PermissionMenuEntry>();
    }
}
