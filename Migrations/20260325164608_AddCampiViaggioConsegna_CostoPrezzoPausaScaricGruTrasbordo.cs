using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddCampiViaggioConsegna_CostoPrezzoPausaScaricGruTrasbordo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoTrasporto",
                table: "ViaggiConsegna",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Gru",
                table: "ViaggiConsegna",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrezzoVendita",
                table: "ViaggiConsegna",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TempoPausa",
                table: "ViaggiConsegna",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TempoScarico",
                table: "ViaggiConsegna",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Trasbordo",
                table: "ViaggiConsegna",
                type: "bit",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE [ViaggiConsegna] SET
                    [CostoTrasporto] = 0,
                    [PrezzoVendita] = 0,
                    [TempoPausa] = 0,
                    [TempoScarico] = 0,
                    [Gru] = 0,
                    [Trasbordo] = 0
                WHERE [CostoTrasporto] IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoTrasporto",
                table: "ViaggiConsegna");

            migrationBuilder.DropColumn(
                name: "Gru",
                table: "ViaggiConsegna");

            migrationBuilder.DropColumn(
                name: "PrezzoVendita",
                table: "ViaggiConsegna");

            migrationBuilder.DropColumn(
                name: "TempoPausa",
                table: "ViaggiConsegna");

            migrationBuilder.DropColumn(
                name: "TempoScarico",
                table: "ViaggiConsegna");

            migrationBuilder.DropColumn(
                name: "Trasbordo",
                table: "ViaggiConsegna");
        }
    }
}
