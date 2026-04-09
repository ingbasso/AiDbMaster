using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddMezziTrasportoEsterni_PortataGruTrasbordo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Gru",
                table: "MezziTrasportoEsterni",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PortataMax",
                table: "MezziTrasportoEsterni",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Trasbordo",
                table: "MezziTrasportoEsterni",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gru",
                table: "MezziTrasportoEsterni");

            migrationBuilder.DropColumn(
                name: "PortataMax",
                table: "MezziTrasportoEsterni");

            migrationBuilder.DropColumn(
                name: "Trasbordo",
                table: "MezziTrasportoEsterni");
        }
    }
}
