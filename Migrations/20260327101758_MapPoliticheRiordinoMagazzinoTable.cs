using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class MapPoliticheRiordinoMagazzinoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabella PoliticheRiordinoMagazzino esiste già nel database.
            // Aggiungiamo solo gli indici se non esistono ancora.

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PoliticheRiordino_CodiceArticolo' AND object_id = OBJECT_ID('PoliticheRiordinoMagazzino'))
                    CREATE INDEX [IX_PoliticheRiordino_CodiceArticolo] ON [PoliticheRiordinoMagazzino] ([CodiceArticolo]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PoliticheRiordino_Articolo_Magazzino' AND object_id = OBJECT_ID('PoliticheRiordinoMagazzino'))
                    CREATE UNIQUE INDEX [IX_PoliticheRiordino_Articolo_Magazzino] ON [PoliticheRiordinoMagazzino] ([CodiceArticolo], [CodiceMagazzino]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoliticheRiordinoMagazzino");
        }
    }
}
