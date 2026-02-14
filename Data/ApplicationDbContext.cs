using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Models;

namespace AiDbMaster.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentCategory> DocumentCategories { get; set; }
        public DbSet<DocumentPermission> DocumentPermissions { get; set; }
        public DbSet<AnagraficaArticoli> AnagraficaArticoli { get; set; }
        public DbSet<AnagraficaClienti> AnagraficaClienti { get; set; }
        public DbSet<AnagraficaFornitori> AnagraficaFornitori { get; set; }
        public DbSet<DestinazioniDiverse> DestinazioniDiverse { get; set; }
        public DbSet<ArticoliSostitutivi> ArticoliSostitutivi { get; set; }
        public DbSet<ProgressiviArticoli> ProgressiviArticoli { get; set; }
        public DbSet<TabellaAgenti> TabellaAgenti { get; set; }
        public DbSet<TabellaMagazzini> TabellaMagazzini { get; set; }
        public DbSet<ParametriChiave> ParametriChiave { get; set; }
        public DbSet<OrdiniTestate> OrdiniTestate { get; set; }
        public DbSet<OrdiniRighe> OrdiniRighe { get; set; }
        
        // Tabelle Ordini di Produzione
        public DbSet<StatoOP> StatiOP { get; set; }
        public DbSet<Operatore> Operatori { get; set; }
        public DbSet<CentroLavoro> CentriLavoro { get; set; }
        public DbSet<ListaOP> ListaOP { get; set; }
        public DbSet<Lavorazioni> Lavorazioni { get; set; }
        public DbSet<CalendarioFermiCentriLavoro> CalendarioFermiCentriLavoro { get; set; }
        public DbSet<TempiAsciugatura> TempiAsciugatura { get; set; }
        
        // Tabelle Anagrafiche / Provvigioni
        public DbSet<ClasseProvvigione> ClassiProvvigioni { get; set; }
        public DbSet<Famiglia> Famiglie { get; set; }
        public DbSet<Marca> Marche { get; set; }
        
        // Tabella Log Invio Email
        public DbSet<InvioEmail> InvioEmail { get; set; }
        
        // Tabella Opzioni di Sistema
        public DbSet<Opzione> Opzioni { get; set; }
        
        // Tabelle Sistema Permessi
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserDataFilter> UserDataFilters { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Personalizzazioni aggiuntive del modello possono essere aggiunte qui

            // Configurazione delle relazioni per evitare cicli di cancellazione
            builder.Entity<DocumentPermission>()
                .HasOne(dp => dp.Document)
                .WithMany()
                .HasForeignKey(dp => dp.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DocumentPermission>()
                .HasOne(dp => dp.User)
                .WithMany()
                .HasForeignKey(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DocumentPermission>()
                .HasOne(dp => dp.GrantedBy)
                .WithMany()
                .HasForeignKey(dp => dp.GrantedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Document>()
                .HasOne(d => d.UploadedBy)
                .WithMany()
                .HasForeignKey(d => d.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurazione chiave composta per ArticoliSostitutivi
            builder.Entity<ArticoliSostitutivi>()
                .HasKey(a => new { a.CodiceArticolo, a.CodiceArticoloSostitutivo });

            // Configurazione chiave composta per DestinazioniDiverse
            builder.Entity<DestinazioniDiverse>()
                .HasKey(d => new { d.CodiceConto, d.CodiceDestinazione });

            // Configurazione delle relazioni per OrdiniTestate
            builder.Entity<OrdiniTestate>()
                .HasOne(o => o.Cliente)
                .WithMany()
                .HasForeignKey(o => o.CodiceCliente)
                .HasPrincipalKey(c => c.CodiceCliente)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrdiniTestate>()
                .HasOne(o => o.Agente)
                .WithMany()
                .HasForeignKey(o => o.CodiceAgente)
                .HasPrincipalKey(a => a.CodiceAgente)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurazione delle relazioni per OrdiniRighe
            builder.Entity<OrdiniRighe>()
                .HasOne(r => r.Articolo)
                .WithMany()
                .HasForeignKey(r => r.CodiceArticolo)
                .HasPrincipalKey(a => a.CodiceArticolo)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrdiniRighe>()
                .HasOne(r => r.Magazzino)
                .WithMany()
                .HasForeignKey(r => r.CodiceMagazzino)
                .HasPrincipalKey(m => m.CodiceMagazzino)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurazione della relazione complessa tra OrdiniTestate e OrdiniRighe
            // Basata sui campi TipoOrdine, AnnoOrdine, SerieOrdine, NumeroOrdine
            builder.Entity<OrdiniRighe>()
                .HasOne(r => r.Testata)
                .WithMany(t => t.Righe)
                .HasForeignKey(r => new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine })
                .HasPrincipalKey(t => new { t.TipoOrdine, t.AnnoOrdine, t.SerieOrdine, t.NumeroOrdine })
                .OnDelete(DeleteBehavior.Cascade);

            // Configurazione degli indici per le performance
            builder.Entity<OrdiniTestate>()
                .HasIndex(o => new { o.TipoOrdine, o.AnnoOrdine, o.SerieOrdine, o.NumeroOrdine })
                .IsUnique()
                .HasDatabaseName("IX_OrdiniTestate_ChiaveComposita");

            builder.Entity<OrdiniTestate>()
                .HasIndex(o => o.DataOrdine)
                .HasDatabaseName("IX_OrdiniTestate_DataOrdine");

            builder.Entity<OrdiniTestate>()
                .HasIndex(o => o.CodiceCliente)
                .HasDatabaseName("IX_OrdiniTestate_CodiceCliente");

            builder.Entity<OrdiniRighe>()
                .HasIndex(r => new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine })
                .HasDatabaseName("IX_OrdiniRighe_ChiaveComposita");

            builder.Entity<OrdiniRighe>()
                .HasIndex(r => r.CodiceArticolo)
                .HasDatabaseName("IX_OrdiniRighe_CodiceArticolo");

            builder.Entity<OrdiniRighe>()
                .HasIndex(r => r.DataConsegna)
                .HasDatabaseName("IX_OrdiniRighe_DataConsegna");

            // Configurazione dei tipi di dati per i campi decimal
            builder.Entity<OrdiniRighe>()
                .Property(r => r.Quantita)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.NumeroColli)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.ColliEvasi)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.QuantitaEvasa)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.Prezzo)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.ValoreRiga)
                .HasColumnType("money");

            builder.Entity<Document>()
                .HasOne(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== CONFIGURAZIONI ORDINI DI PRODUZIONE =====

            // Configurazione StatoOP
            builder.Entity<StatoOP>()
                .HasIndex(s => s.CodiceStato)
                .IsUnique()
                .HasDatabaseName("IX_StatiOP_CodiceStato");

            builder.Entity<StatoOP>()
                .HasIndex(s => s.Ordine)
                .HasDatabaseName("IX_StatiOP_Ordine");

            // Configurazione Operatore
            builder.Entity<Operatore>()
                .HasIndex(o => o.CodiceOperatore)
                .IsUnique()
                .HasDatabaseName("IX_Operatori_CodiceOperatore");

            builder.Entity<Operatore>()
                .HasIndex(o => new { o.Nome, o.Cognome })
                .HasDatabaseName("IX_Operatori_NomeCognome");

            builder.Entity<Operatore>()
                .HasIndex(o => o.Email)
                .HasDatabaseName("IX_Operatori_Email");

            // Configurazione CentroLavoro
            builder.Entity<CentroLavoro>()
                .HasIndex(c => c.DescrizioneCentro)
                .HasDatabaseName("IX_CentriLavoro_DescrizioneCentro");

            builder.Entity<CentroLavoro>()
                .HasIndex(c => c.Attivo)
                .HasDatabaseName("IX_CentriLavoro_Attivo");

            // Configurazione Lavorazioni
            builder.Entity<Lavorazioni>()
                .HasIndex(l => l.Attivo)
                .HasDatabaseName("IX_Lavorazioni_Attivo");

            builder.Entity<Lavorazioni>()
                .HasIndex(l => l.DescrizioneLavorazione)
                .HasDatabaseName("IX_Lavorazioni_DescrizioneLavorazione");

            // Configurazione CalendarioFermiCentriLavoro
            builder.Entity<CalendarioFermiCentriLavoro>()
                .HasOne(cf => cf.CentroLavoro)
                .WithMany()
                .HasForeignKey(cf => cf.CodiceCentro)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CalendarioFermiCentriLavoro>()
                .HasIndex(cf => cf.CodiceCentro)
                .HasDatabaseName("IX_CalendarioFermiCentriLavoro_CodiceCentro");

            builder.Entity<CalendarioFermiCentriLavoro>()
                .HasIndex(cf => cf.DataInizioFermo)
                .HasDatabaseName("IX_CalendarioFermiCentriLavoro_DataInizioFermo");

            builder.Entity<CalendarioFermiCentriLavoro>()
                .HasIndex(cf => cf.DataFineFermo)
                .HasDatabaseName("IX_CalendarioFermiCentriLavoro_DataFineFermo");

            builder.Entity<CalendarioFermiCentriLavoro>()
                .HasIndex(cf => cf.TipoFermo)
                .HasDatabaseName("IX_CalendarioFermiCentriLavoro_TipoFermo");

            // Configurazione ListaOP
            // Indice composito per identificazione ordine
            builder.Entity<ListaOP>()
                .HasIndex(l => new { l.TipoOrdine, l.AnnoOrdine, l.SerieOrdine, l.NumeroOrdine })
                .HasDatabaseName("IX_ListaOP_ChiaveComposita");

            // Indice per stato OP (per filtrare rapidamente)
            builder.Entity<ListaOP>()
                .HasIndex(l => l.IdStato)
                .HasDatabaseName("IX_ListaOP_IdStato");

            // Indice per data inizio OP (per ordinamenti temporali)
            builder.Entity<ListaOP>()
                .HasIndex(l => l.DataInizioOP)
                .HasDatabaseName("IX_ListaOP_DataInizioOP");

            // Indice per centro di lavoro
            builder.Entity<ListaOP>()
                .HasIndex(l => l.CodiceCentro)
                .HasDatabaseName("IX_ListaOP_CodiceCentro");

            // Indice per operatore
            builder.Entity<ListaOP>()
                .HasIndex(l => l.IdOperatore)
                .HasDatabaseName("IX_ListaOP_IdOperatore");

            // Indice per lavorazione
            builder.Entity<ListaOP>()
                .HasIndex(l => l.CodiceLavorazione)
                .HasDatabaseName("IX_ListaOP_CodiceLavorazione");

            // Indice per priorità
            builder.Entity<ListaOP>()
                .HasIndex(l => l.Priorita)
                .HasDatabaseName("IX_ListaOP_Priorita");

            // Indice per codice articolo
            builder.Entity<ListaOP>()
                .HasIndex(l => l.CodiceArticolo)
                .HasDatabaseName("IX_ListaOP_CodiceArticolo");

            // Configurazione delle relazioni per ListaOP
            builder.Entity<ListaOP>()
                .HasOne(l => l.Stato)
                .WithMany(s => s.OrdiniProduzione)
                .HasForeignKey(l => l.IdStato)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ListaOP>()
                .HasOne(l => l.Operatore)
                .WithMany(o => o.OrdiniProduzione)
                .HasForeignKey(l => l.IdOperatore)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ListaOP>()
                .HasOne(l => l.CentroLavoro)
                .WithMany(c => c.OrdiniProduzione)
                .HasForeignKey(l => l.CodiceCentro)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ListaOP>()
                .HasOne(l => l.Lavorazione)
                .WithMany(lav => lav.OrdiniProduzione)
                .HasForeignKey(l => l.CodiceLavorazione)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurazione dei tipi di dati per i campi decimal di ListaOP
            builder.Entity<ListaOP>()
                .Property(l => l.Quantita)
                .HasColumnType("decimal(10,3)");

            builder.Entity<ListaOP>()
                .Property(l => l.QuantitaProdotta)
                .HasColumnType("decimal(10,3)");

            builder.Entity<ListaOP>()
                .Property(l => l.CostoOrario)
                .HasColumnType("decimal(10,2)");

            // Configurazione dei tipi di dati per CentroLavoro
            builder.Entity<CentroLavoro>()
                .Property(c => c.CostoOrarioStandard)
                .HasColumnType("decimal(10,2)");

            // Seed data per StatiOP
            builder.Entity<StatoOP>().HasData(
                new StatoOP { IdStato = 1, CodiceStato = "EM", DescrizioneStato = "Emesso", Attivo = true, Ordine = 1 },
                new StatoOP { IdStato = 2, CodiceStato = "PR", DescrizioneStato = "Produzione", Attivo = true, Ordine = 2 },
                new StatoOP { IdStato = 3, CodiceStato = "CH", DescrizioneStato = "Chiuso", Attivo = true, Ordine = 4 },
                new StatoOP { IdStato = 4, CodiceStato = "SO", DescrizioneStato = "Sospeso", Attivo = true, Ordine = 3 }
            );

            // ===== CONFIGURAZIONI ANAGRAFICA ARTICOLI - NUOVI CAMPI =====

            // Configurazione tipi di dato per i nuovi campi
            builder.Entity<AnagraficaArticoli>()
                .Property(a => a.Famiglia)
                .HasColumnType("varchar(4)");

            builder.Entity<AnagraficaArticoli>()
                .Property(a => a.Outlet)
                .HasColumnType("varchar(1)")
                .HasDefaultValue("N");

            builder.Entity<AnagraficaArticoli>()
                .Property(a => a.FuoriProduzione)
                .HasColumnType("varchar(1)")
                .HasDefaultValue("N");

            // FK: AnagraficaArticoli.Marca --> TabellaMarche.CodiceMarca
            builder.Entity<AnagraficaArticoli>()
                .HasOne(a => a.MarcaNavigation)
                .WithMany()
                .HasForeignKey(a => a.Marca)
                .HasPrincipalKey(m => m.CodiceMarca)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: AnagraficaArticoli.Famiglia --> TabellaFamiglie.CodiceFamiglia
            builder.Entity<AnagraficaArticoli>()
                .HasOne(a => a.FamigliaNavigation)
                .WithMany()
                .HasForeignKey(a => a.Famiglia)
                .HasPrincipalKey(f => f.CodiceFamiglia)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: AnagraficaArticoli.ClasseProvvigione --> TabellaClassiProvvigioni.CodiceClasse
            builder.Entity<AnagraficaArticoli>()
                .HasOne(a => a.ClasseProvvigioneNavigation)
                .WithMany()
                .HasForeignKey(a => a.ClasseProvvigione)
                .HasPrincipalKey(c => c.CodiceClasse)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== CONFIGURAZIONI CLASSI PROVVIGIONI =====

            // Indice univoco sul CodiceClasse
            builder.Entity<ClasseProvvigione>()
                .HasIndex(c => c.CodiceClasse)
                .IsUnique()
                .HasDatabaseName("IX_TabellaClassiProvvigioni_CodiceClasse");

            // Configurazione del tipo di dato per Perc_Sconto
            builder.Entity<ClasseProvvigione>()
                .Property(c => c.Perc_Sconto)
                .HasColumnType("decimal(27,9)");

            // Default value per UltimoAggiornamento
            builder.Entity<ClasseProvvigione>()
                .Property(c => c.UltimoAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ===== CONFIGURAZIONI FAMIGLIE =====

            // Indice univoco sul CodiceFamiglia
            builder.Entity<Famiglia>()
                .HasIndex(f => f.CodiceFamiglia)
                .IsUnique()
                .HasDatabaseName("IX_TabellaFamiglie_CodiceFamiglia");

            // Configurazione dei tipi di dato varchar (non nvarchar)
            builder.Entity<Famiglia>()
                .Property(f => f.CodiceFamiglia)
                .HasColumnType("varchar(4)");

            builder.Entity<Famiglia>()
                .Property(f => f.DescrizioneFamiglia)
                .HasColumnType("varchar(50)");

            // Default value per UltimoAggiornamento
            builder.Entity<Famiglia>()
                .Property(f => f.UltimoAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ===== CONFIGURAZIONI MARCHE =====

            // Indice univoco sul CodiceMarca
            builder.Entity<Marca>()
                .HasIndex(m => m.CodiceMarca)
                .IsUnique()
                .HasDatabaseName("IX_TabellaMarche_CodiceMarca");

            // Configurazione del tipo di dato varchar per DescrizioneMarca
            builder.Entity<Marca>()
                .Property(m => m.DescrizioneMarca)
                .HasColumnType("varchar(50)");

            // Default value per UltimoAggiornamento
            builder.Entity<Marca>()
                .Property(m => m.UltimoAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ===== CONFIGURAZIONE CAMPO PORTO (OrdiniTestate) =====
            // Il campo Porto nel database è un varchar, non smallint
            builder.Entity<OrdiniTestate>()
                .Property(o => o.Porto)
                .HasColumnType("varchar(10)");

            // ===== CONFIGURAZIONI TABELLA OPZIONI =====

            // Configurazione dei tipi di dato varchar (non nvarchar)
            builder.Entity<Opzione>()
                .Property(o => o.NomeOpzione)
                .HasColumnType("varchar(255)");

            builder.Entity<Opzione>()
                .Property(o => o.ValoreOpzione)
                .HasColumnType("varchar(max)");

            // Indice univoco sul NomeOpzione (ogni opzione deve avere un nome univoco)
            builder.Entity<Opzione>()
                .HasIndex(o => o.NomeOpzione)
                .IsUnique()
                .HasDatabaseName("IX_TabellaOpzioni_NomeOpzione");

            // ===== CONFIGURAZIONI INVIO EMAIL =====

            // Configurazione dei tipi di dato varchar (non nvarchar)
            builder.Entity<InvioEmail>()
                .Property(e => e.TipoOrdine)
                .HasColumnType("varchar(1)");

            builder.Entity<InvioEmail>()
                .Property(e => e.SerieOrdine)
                .HasColumnType("varchar(3)");

            builder.Entity<InvioEmail>()
                .Property(e => e.Contabilizzato)
                .HasColumnType("varchar(1)")
                .HasDefaultValue("N");

            // Indice univoco sulla combinazione ordine+riga per evitare invii duplicati
            builder.Entity<InvioEmail>()
                .HasIndex(e => new { e.TipoOrdine, e.AnnoOrdine, e.SerieOrdine, e.NumeroOrdine, e.RigaOrdine })
                .IsUnique()
                .HasDatabaseName("IX_InvioEmail_OrdineRiga");

            // Indice sulla DataInvio per query temporali
            builder.Entity<InvioEmail>()
                .HasIndex(e => e.DataInvio)
                .HasDatabaseName("IX_InvioEmail_DataInvio");

            // ===== CONFIGURAZIONI SISTEMA PERMESSI =====

            // Configurazione Resource (auto-referenza per gerarchia)
            builder.Entity<Resource>()
                .HasOne(r => r.Parent)
                .WithMany(r => r.Children)
                .HasForeignKey(r => r.ParentResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indice univoco sul nome risorsa
            builder.Entity<Resource>()
                .HasIndex(r => r.Name)
                .IsUnique()
                .HasDatabaseName("IX_Resources_Name");

            // Configurazione Permission
            builder.Entity<Permission>()
                .HasOne(p => p.Role)
                .WithMany()
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Permission>()
                .HasOne(p => p.Resource)
                .WithMany(r => r.Permissions)
                .HasForeignKey(p => p.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indice univoco: un ruolo può avere un solo permesso per risorsa
            builder.Entity<Permission>()
                .HasIndex(p => new { p.RoleId, p.ResourceId })
                .IsUnique()
                .HasDatabaseName("IX_Permissions_RoleId_ResourceId");

            // Configurazione UserDataFilter
            builder.Entity<UserDataFilter>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserDataFilter>()
                .HasIndex(f => new { f.UserId, f.ResourceName })
                .HasDatabaseName("IX_UserDataFilters_UserId_ResourceName");

            // Seed data per Lavorazioni
            // Nota: I dati di seed vengono ora gestiti tramite la migrazione per preservare i dati esistenti
        }
    }
} 