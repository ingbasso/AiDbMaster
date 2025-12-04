using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class CreateDestinazioniDiverseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DestinazioniDiverse",
                columns: table => new
                {
                    CodiceConto = table.Column<int>(type: "int", nullable: false),
                    CodiceDestinazione = table.Column<int>(type: "int", nullable: false),
                    DescrizioneDestinazione = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Indirizzo = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    Cap = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Localita = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Provincia = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    CodiceZona = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinazioniDiverse", x => new { x.CodiceConto, x.CodiceDestinazione });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DestinazioniDiverse");
        }
    }
}
