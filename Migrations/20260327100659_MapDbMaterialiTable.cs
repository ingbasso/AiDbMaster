using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class MapDbMaterialiTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabella DB_Materiali esiste già nel database.
            // Aggiungiamo solo gli indici se non esistono ancora.

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DB_Materiali_CodiceDistinta' AND object_id = OBJECT_ID('DB_Materiali'))
                    CREATE INDEX [IX_DB_Materiali_CodiceDistinta] ON [DB_Materiali] ([CodiceDistinta]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DB_Materiali_CodiceFiglio' AND object_id = OBJECT_ID('DB_Materiali'))
                    CREATE INDEX [IX_DB_Materiali_CodiceFiglio] ON [DB_Materiali] ([CodiceFiglio]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DB_Materiali_Distinta_RigaDistinta' AND object_id = OBJECT_ID('DB_Materiali'))
                    CREATE INDEX [IX_DB_Materiali_Distinta_RigaDistinta] ON [DB_Materiali] ([CodiceDistinta], [RigaDistinta]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DB_Materiali");
        }
    }
}
