using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddLavorazioniTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lavorazioni",
                columns: table => new
                {
                    IdLavorazione = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodiceLavorazione = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false),
                    DescrizioneLavorazione = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataUltimaModifica = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lavorazioni", x => x.IdLavorazione);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lavorazioni_Attivo",
                table: "Lavorazioni",
                column: "Attivo");

            migrationBuilder.CreateIndex(
                name: "IX_Lavorazioni_CodiceLavorazione",
                table: "Lavorazioni",
                column: "CodiceLavorazione",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lavorazioni");
        }
    }
}
