using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddViaggioConsegnaDestinazioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViaggioConsegnaDestinazioni",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViaggioConsegnaId = table.Column<int>(type: "int", nullable: false),
                    CodiceCliente = table.Column<int>(type: "int", nullable: false),
                    CodiceDestinazione = table.Column<int>(type: "int", nullable: true),
                    Gru = table.Column<bool>(type: "bit", nullable: false),
                    Trasbordo = table.Column<bool>(type: "bit", nullable: false),
                    OrdineConsegna = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViaggioConsegnaDestinazioni", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ViaggioConsegnaDestinazioni_ViaggiConsegna_ViaggioConsegnaId",
                        column: x => x.ViaggioConsegnaId,
                        principalTable: "ViaggiConsegna",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViaggioConsegnaDestinazioni_Viaggio_Cliente_Dest",
                table: "ViaggioConsegnaDestinazioni",
                columns: new[] { "ViaggioConsegnaId", "CodiceCliente", "CodiceDestinazione" },
                unique: true,
                filter: "[CodiceDestinazione] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViaggioConsegnaDestinazioni");
        }
    }
}
