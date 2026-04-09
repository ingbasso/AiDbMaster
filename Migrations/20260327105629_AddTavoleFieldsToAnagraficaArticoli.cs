using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddTavoleFieldsToAnagraficaArticoli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Le colonne esistono già nel database (aggiunte manualmente).
            // Questa migration serve solo ad aggiornare lo snapshot EF.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QtàUMPPerPallet",
                table: "AnagraficaArticoli");

            migrationBuilder.DropColumn(
                name: "QtàUMPPerTavola",
                table: "AnagraficaArticoli");

            migrationBuilder.DropColumn(
                name: "TavolePerPallet",
                table: "AnagraficaArticoli");
        }
    }
}
