using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class SeedTempiAsciugatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Inserimento dei 12 mesi dell'anno con valori di default per GiorniAsciugatura
            migrationBuilder.InsertData(
                table: "TempiAsciugatura",
                columns: new[] { "IdMese", "Mese", "GiorniAsciugatura" },
                values: new object[,]
                {
                    { 1, "Gennaio", 0 },
                    { 2, "Febbraio", 0 },
                    { 3, "Marzo", 0 },
                    { 4, "Aprile", 0 },
                    { 5, "Maggio", 0 },
                    { 6, "Giugno", 0 },
                    { 7, "Luglio", 0 },
                    { 8, "Agosto", 0 },
                    { 9, "Settembre", 0 },
                    { 10, "Ottobre", 0 },
                    { 11, "Novembre", 0 },
                    { 12, "Dicembre", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rimozione dei dati dei mesi inseriti
            migrationBuilder.DeleteData(
                table: "TempiAsciugatura",
                keyColumn: "IdMese",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
        }
    }
}
