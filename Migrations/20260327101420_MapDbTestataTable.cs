using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class MapDbTestataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabella DB_Testata esiste già nel database.
            // Aggiungiamo solo l'indice univoco se non esiste ancora.

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DB_Testata_CodiceDistinta' AND object_id = OBJECT_ID('DB_Testata'))
                    CREATE UNIQUE INDEX [IX_DB_Testata_CodiceDistinta] ON [DB_Testata] ([CodiceDistinta]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DB_Testata");
        }
    }
}
