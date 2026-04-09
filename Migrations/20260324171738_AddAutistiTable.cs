using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddAutistiTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AutistaId",
                table: "ViaggiConsegna",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AutistaDefaultId",
                table: "MezziTrasportoInterni",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Autisti",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cognome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Attivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autisti", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViaggiConsegna_AutistaId",
                table: "ViaggiConsegna",
                column: "AutistaId");

            migrationBuilder.CreateIndex(
                name: "IX_MezziTrasportoInterni_AutistaDefaultId",
                table: "MezziTrasportoInterni",
                column: "AutistaDefaultId");

            migrationBuilder.CreateIndex(
                name: "IX_Autisti_Attivo",
                table: "Autisti",
                column: "Attivo");

            migrationBuilder.CreateIndex(
                name: "IX_Autisti_CognomeNome",
                table: "Autisti",
                columns: new[] { "Cognome", "Nome" });

            migrationBuilder.AddForeignKey(
                name: "FK_MezziTrasportoInterni_Autisti_AutistaDefaultId",
                table: "MezziTrasportoInterni",
                column: "AutistaDefaultId",
                principalTable: "Autisti",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ViaggiConsegna_Autisti_AutistaId",
                table: "ViaggiConsegna",
                column: "AutistaId",
                principalTable: "Autisti",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MezziTrasportoInterni_Autisti_AutistaDefaultId",
                table: "MezziTrasportoInterni");

            migrationBuilder.DropForeignKey(
                name: "FK_ViaggiConsegna_Autisti_AutistaId",
                table: "ViaggiConsegna");

            migrationBuilder.DropTable(
                name: "Autisti");

            migrationBuilder.DropIndex(
                name: "IX_ViaggiConsegna_AutistaId",
                table: "ViaggiConsegna");

            migrationBuilder.DropIndex(
                name: "IX_MezziTrasportoInterni_AutistaDefaultId",
                table: "MezziTrasportoInterni");

            migrationBuilder.DropColumn(
                name: "AutistaId",
                table: "ViaggiConsegna");

            migrationBuilder.DropColumn(
                name: "AutistaDefaultId",
                table: "MezziTrasportoInterni");
        }
    }
}
