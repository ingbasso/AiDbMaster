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
        public DbSet<ArticoliSostitutivi> ArticoliSostitutivi { get; set; }
        public DbSet<ProgressiviArticoli> ProgressiviArticoli { get; set; }
        public DbSet<TabellaAgenti> TabellaAgenti { get; set; }
        public DbSet<TabellaMagazzini> TabellaMagazzini { get; set; }
        public DbSet<OrdiniTestate> OrdiniTestate { get; set; }
        public DbSet<OrdiniRighe> OrdiniRighe { get; set; }
        
        // Tabelle Ordini di Produzione
        public DbSet<StatoOP> StatiOP { get; set; }
        public DbSet<Operatore> Operatori { get; set; }
        public DbSet<CentroLavoro> CentriLavoro { get; set; }
        public DbSet<ListaOP> ListaOP { get; set; }
        public DbSet<Lavorazioni> Lavorazioni { get; set; }

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

            builder.Entity<OrdiniTestate>()
                .HasOne(o => o.Magazzino)
                .WithMany()
                .HasForeignKey(o => o.CodiceMagazzino)
                .HasPrincipalKey(m => m.CodiceMagazzino)
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
                .Property(r => r.ColliPrenotati)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.QuantitaPrenotata)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.Prezzo)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.Sconto1)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.Sconto2)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.Sconto3)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.Provvigione)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.PrezzoConIva)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.PrezzoValuta)
                .HasColumnType("decimal(18,4)");

            builder.Entity<OrdiniRighe>()
                .Property(r => r.PrezzoListino)
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
                .HasIndex(c => c.CodiceCentro)
                .IsUnique()
                .HasDatabaseName("IX_CentriLavoro_CodiceCentro");

            builder.Entity<CentroLavoro>()
                .HasIndex(c => c.DescrizioneCentro)
                .HasDatabaseName("IX_CentriLavoro_DescrizioneCentro");

            builder.Entity<CentroLavoro>()
                .HasIndex(c => c.Attivo)
                .HasDatabaseName("IX_CentriLavoro_Attivo");

            // Configurazione Lavorazioni
            builder.Entity<Lavorazioni>()
                .HasIndex(l => l.CodiceLavorazione)
                .IsUnique()
                .HasDatabaseName("IX_Lavorazioni_CodiceLavorazione")
                .HasFilter("[CodiceLavorazione] IS NOT NULL");

            builder.Entity<Lavorazioni>()
                .HasIndex(l => l.Attivo)
                .HasDatabaseName("IX_Lavorazioni_Attivo");

            builder.Entity<Lavorazioni>()
                .HasIndex(l => l.DescrizioneLavorazione)
                .HasDatabaseName("IX_Lavorazioni_DescrizioneLavorazione");

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
                .HasIndex(l => l.IdCentroLavoro)
                .HasDatabaseName("IX_ListaOP_IdCentroLavoro");

            // Indice per operatore
            builder.Entity<ListaOP>()
                .HasIndex(l => l.IdOperatore)
                .HasDatabaseName("IX_ListaOP_IdOperatore");

            // Indice per lavorazione
            builder.Entity<ListaOP>()
                .HasIndex(l => l.IdLavorazione)
                .HasDatabaseName("IX_ListaOP_IdLavorazione");

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
                .HasForeignKey(l => l.IdCentroLavoro)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ListaOP>()
                .HasOne(l => l.Lavorazione)
                .WithMany(lav => lav.OrdiniProduzione)
                .HasForeignKey(l => l.IdLavorazione)
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

            // Seed data per Lavorazioni
            builder.Entity<Lavorazioni>().HasData(
                new Lavorazioni 
                { 
                    IdLavorazione = 1, 
                    CodiceLavorazione = "T", 
                    DescrizioneLavorazione = "Taglio", 
                    Attivo = true, 
                    DataCreazione = DateTime.Now.AddDays(-30) 
                },
                new Lavorazioni 
                { 
                    IdLavorazione = 2, 
                    CodiceLavorazione = "F", 
                    DescrizioneLavorazione = "Fresatura", 
                    Attivo = true, 
                    DataCreazione = DateTime.Now.AddDays(-25) 
                },
                new Lavorazioni 
                { 
                    IdLavorazione = 3, 
                    CodiceLavorazione = "T", 
                    DescrizioneLavorazione = "Tornitura", 
                    Attivo = true, 
                    DataCreazione = DateTime.Now.AddDays(-20) 
                },
                new Lavorazioni 
                { 
                    IdLavorazione = 4, 
                    CodiceLavorazione = "S", 
                    DescrizioneLavorazione = "Saldatura", 
                    Attivo = true, 
                    DataCreazione = DateTime.Now.AddDays(-15) 
                },
                new Lavorazioni 
                { 
                    IdLavorazione = 5, 
                    CodiceLavorazione = "A", 
                    DescrizioneLavorazione = "Assemblaggio", 
                    Attivo = true, 
                    DataCreazione = DateTime.Now.AddDays(-10) 
                },
                new Lavorazioni 
                { 
                    IdLavorazione = 6, 
                    CodiceLavorazione = null, 
                    DescrizioneLavorazione = "Controllo Qualità", 
                    Attivo = true, 
                    DataCreazione = DateTime.Now.AddDays(-5) 
                },
                new Lavorazioni 
                { 
                    IdLavorazione = 7, 
                    CodiceLavorazione = "P", 
                    DescrizioneLavorazione = "Verniciatura", 
                    Attivo = false, 
                    DataCreazione = DateTime.Now.AddDays(-50) 
                }
            );
        }
    }
} 