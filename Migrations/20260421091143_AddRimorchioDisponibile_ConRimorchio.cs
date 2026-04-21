using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddRimorchioDisponibile_ConRimorchio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConRimorchio",
                table: "ViaggiConsegna",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PortataMaxConRimorchioKg",
                table: "MezziTrasportoInterni",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RimorchioDisponibile",
                table: "MezziTrasportoInterni",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConRimorchio",
                table: "ViaggiConsegna");

            migrationBuilder.DropColumn(
                name: "PortataMaxConRimorchioKg",
                table: "MezziTrasportoInterni");

            migrationBuilder.DropColumn(
                name: "RimorchioDisponibile",
                table: "MezziTrasportoInterni");
        }
    }
}
