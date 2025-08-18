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

            builder.Entity<Document>()
                .HasOne(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 