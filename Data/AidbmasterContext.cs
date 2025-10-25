using System;
using System.Collections.Generic;
using AiDbMaster.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Data;

public partial class AidbmasterContext : DbContext
{
    public AidbmasterContext(DbContextOptions<AidbmasterContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ProgressiviArticoli> ProgressiviArticolis { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProgressiviArticoli>(entity =>
        {
            entity.ToTable("ProgressiviArticoli");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CodiceArticolo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("Codice identificativo dell’articolo");
            entity.Property(e => e.CodiceMagazzino).HasComment("Codice identificativo del magazzino");
            entity.Property(e => e.Esistenza)
                .HasComment("Esistenza del codice articolo nel magazzino")
                .HasColumnType("decimal(27, 9)");
            entity.Property(e => e.ImpegnatoDataOdierna)
                .HasComment("Impegnato del codice articolo nel magazzino")
                .HasColumnType("decimal(27, 9)");
            entity.Property(e => e.ImpegnatoTotale)
                .HasComment("Impegnato del codice articolo nel magazzino")
                .HasColumnType("decimal(27, 9)");
            entity.Property(e => e.Ordinato)
                .HasComment("Ordinato del codice articolo nel magazzino")
                .HasColumnType("decimal(27, 9)");
            entity.Property(e => e.OrdinatoFornitoriDataOdierna).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Prenotato)
                .HasComment("Prenotato del codice articolo nel magazzino")
                .HasColumnType("decimal(27, 9)");
            entity.Property(e => e.UltimoAggiornamento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
