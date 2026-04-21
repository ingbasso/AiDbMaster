using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddStatoArticoli_CodiceAgente2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "CodiceAgente2",
                table: "OrdiniTestate",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "CodiceAgente2",
                table: "DestinazioniDiverse",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "CodiceAgente2",
                table: "AnagraficaClienti",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatoArticoli",
                table: "AnagraficaArticoli",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdiniTestate_CodiceAgente2",
                table: "OrdiniTestate",
                column: "CodiceAgente2");

            migrationBuilder.CreateIndex(
                name: "IX_DestinazioniDiverse_CodiceAgente2",
                table: "DestinazioniDiverse",
                column: "CodiceAgente2");

            migrationBuilder.CreateIndex(
                name: "IX_AnagraficaClienti_CodiceAgente2",
                table: "AnagraficaClienti",
                column: "CodiceAgente2");

            migrationBuilder.AddForeignKey(
                name: "FK_AnagraficaClienti_TabellaAgenti_CodiceAgente2",
                table: "AnagraficaClienti",
                column: "CodiceAgente2",
                principalTable: "TabellaAgenti",
                principalColumn: "CodiceAgente",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DestinazioniDiverse_TabellaAgenti_CodiceAgente2",
                table: "DestinazioniDiverse",
                column: "CodiceAgente2",
                principalTable: "TabellaAgenti",
                principalColumn: "CodiceAgente",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdiniTestate_TabellaAgenti_CodiceAgente2",
                table: "OrdiniTestate",
                column: "CodiceAgente2",
                principalTable: "TabellaAgenti",
                principalColumn: "CodiceAgente",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnagraficaClienti_TabellaAgenti_CodiceAgente2",
                table: "AnagraficaClienti");

            migrationBuilder.DropForeignKey(
                name: "FK_DestinazioniDiverse_TabellaAgenti_CodiceAgente2",
                table: "DestinazioniDiverse");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdiniTestate_TabellaAgenti_CodiceAgente2",
                table: "OrdiniTestate");

            migrationBuilder.DropIndex(
                name: "IX_OrdiniTestate_CodiceAgente2",
                table: "OrdiniTestate");

            migrationBuilder.DropIndex(
                name: "IX_DestinazioniDiverse_CodiceAgente2",
                table: "DestinazioniDiverse");

            migrationBuilder.DropIndex(
                name: "IX_AnagraficaClienti_CodiceAgente2",
                table: "AnagraficaClienti");

            migrationBuilder.DropColumn(
                name: "CodiceAgente2",
                table: "OrdiniTestate");

            migrationBuilder.DropColumn(
                name: "CodiceAgente2",
                table: "DestinazioniDiverse");

            migrationBuilder.DropColumn(
                name: "CodiceAgente2",
                table: "AnagraficaClienti");

            migrationBuilder.DropColumn(
                name: "StatoArticoli",
                table: "AnagraficaArticoli");
        }
    }
}
