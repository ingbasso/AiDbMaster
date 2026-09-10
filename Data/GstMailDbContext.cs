using AiDbMaster.Models;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Data
{
    /// <summary>
    /// DbContext del database esterno GSTMAIL_FAVARO1 (istanza SVRGEST).
    /// È un database già esistente: non va migrato da questa applicazione.
    /// </summary>
    public class GstMailDbContext : DbContext
    {
        public GstMailDbContext(DbContextOptions<GstMailDbContext> options)
            : base(options)
        {
        }

        public DbSet<SendMailModello> SendMailModelli { get; set; }
        public DbSet<SendMailStorico> SendMailStorico { get; set; }
        public DbSet<SendMailTempInvio> SendMailTempInvio { get; set; }

        public DbSet<V2050NegativiMagazzino> NegativiMagazzino { get; set; }
        public DbSet<V2040OrdiniProduzioneScaduti> OrdiniProduzioneScaduti { get; set; }
        public DbSet<V2030GiacenzeMagazzino2> GiacenzeMagazzino2 { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<V2050NegativiMagazzino>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V2050_NegativiMagazzino");
            });

            modelBuilder.Entity<V2040OrdiniProduzioneScaduti>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V2040_OrdiniProduzioneScaduti");
            });

            modelBuilder.Entity<V2030GiacenzeMagazzino2>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V2030_Giacenze_Magazzino_2");
            });
        }
    }
}
