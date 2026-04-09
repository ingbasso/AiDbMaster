using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddMezziTrasportoEsterni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ValoreOpzione",
                table: "TabellaOpzioni",
                type: "varchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(max)");

            migrationBuilder.CreateTable(
                name: "MezziTrasportoEsterni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comune = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Provincia = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Regione = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    NomeVettore = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    TipoMezzo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Costo = table.Column<double>(type: "float", nullable: false),
                    Note = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MezziTrasportoEsterni", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MezziTrasportoEsterni_Comune",
                table: "MezziTrasportoEsterni",
                column: "Comune");

            migrationBuilder.CreateIndex(
                name: "IX_MezziTrasportoEsterni_NomeVettore",
                table: "MezziTrasportoEsterni",
                column: "NomeVettore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MezziTrasportoEsterni");

            migrationBuilder.AlterColumn<string>(
                name: "ValoreOpzione",
                table: "TabellaOpzioni",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldNullable: true);
        }
    }
}
