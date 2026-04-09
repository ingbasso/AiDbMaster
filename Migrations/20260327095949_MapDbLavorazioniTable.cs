using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class MapDbLavorazioniTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabella DB_Lavorazioni esiste già nel database.
            // Aggiungiamo solo gli indici e la FK se non esistono ancora.

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DB_Lavorazioni_CodiceCentro' AND object_id = OBJECT_ID('DB_Lavorazioni'))
                    CREATE INDEX [IX_DB_Lavorazioni_CodiceCentro] ON [DB_Lavorazioni] ([CodiceCentro]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DB_Lavorazioni_CodiceDistinta' AND object_id = OBJECT_ID('DB_Lavorazioni'))
                    CREATE INDEX [IX_DB_Lavorazioni_CodiceDistinta] ON [DB_Lavorazioni] ([CodiceDistinta]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DB_Lavorazioni_CodiceLavorazione' AND object_id = OBJECT_ID('DB_Lavorazioni'))
                    CREATE INDEX [IX_DB_Lavorazioni_CodiceLavorazione] ON [DB_Lavorazioni] ([CodiceLavorazione]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DB_Lavorazioni_Distinta_RigaCiclo' AND object_id = OBJECT_ID('DB_Lavorazioni'))
                    CREATE INDEX [IX_DB_Lavorazioni_Distinta_RigaCiclo] ON [DB_Lavorazioni] ([CodiceDistinta], [RigaCiclo]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DB_Lavorazioni_Lavorazioni_CodiceLavorazione')
                    ALTER TABLE [DB_Lavorazioni] ADD CONSTRAINT [FK_DB_Lavorazioni_Lavorazioni_CodiceLavorazione]
                        FOREIGN KEY ([CodiceLavorazione]) REFERENCES [Lavorazioni]([CodiceLavorazione]) ON DELETE NO ACTION;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DB_Lavorazioni");
        }
    }
}
