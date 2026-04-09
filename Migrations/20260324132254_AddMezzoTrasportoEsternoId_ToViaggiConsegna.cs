using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddMezzoTrasportoEsternoId_ToViaggiConsegna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MezzoTrasportoId",
                table: "ViaggiConsegna",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MezzoTrasportoEsternoId",
                table: "ViaggiConsegna",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViaggiConsegna_MezzoTrasportoEsternoId",
                table: "ViaggiConsegna",
                column: "MezzoTrasportoEsternoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ViaggiConsegna_MezziTrasportoEsterni_MezzoTrasportoEsternoId",
                table: "ViaggiConsegna",
                column: "MezzoTrasportoEsternoId",
                principalTable: "MezziTrasportoEsterni",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ViaggiConsegna_MezziTrasportoEsterni_MezzoTrasportoEsternoId",
                table: "ViaggiConsegna");

            migrationBuilder.DropIndex(
                name: "IX_ViaggiConsegna_MezzoTrasportoEsternoId",
                table: "ViaggiConsegna");

            migrationBuilder.DropColumn(
                name: "MezzoTrasportoEsternoId",
                table: "ViaggiConsegna");

            migrationBuilder.AlterColumn<int>(
                name: "MezzoTrasportoId",
                table: "ViaggiConsegna",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
