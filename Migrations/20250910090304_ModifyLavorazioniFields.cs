using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class ModifyLavorazioniFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lavorazioni_CodiceLavorazione",
                table: "Lavorazioni");

            migrationBuilder.AlterColumn<string>(
                name: "DescrizioneLavorazione",
                table: "Lavorazioni",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodiceLavorazione",
                table: "Lavorazioni",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldMaxLength: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Lavorazioni_CodiceLavorazione",
                table: "Lavorazioni",
                column: "CodiceLavorazione",
                unique: true,
                filter: "[CodiceLavorazione] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lavorazioni_DescrizioneLavorazione",
                table: "Lavorazioni",
                column: "DescrizioneLavorazione");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lavorazioni_CodiceLavorazione",
                table: "Lavorazioni");

            migrationBuilder.DropIndex(
                name: "IX_Lavorazioni_DescrizioneLavorazione",
                table: "Lavorazioni");

            migrationBuilder.AlterColumn<string>(
                name: "DescrizioneLavorazione",
                table: "Lavorazioni",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CodiceLavorazione",
                table: "Lavorazioni",
                type: "varchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lavorazioni_CodiceLavorazione",
                table: "Lavorazioni",
                column: "CodiceLavorazione",
                unique: true);
        }
    }
}
