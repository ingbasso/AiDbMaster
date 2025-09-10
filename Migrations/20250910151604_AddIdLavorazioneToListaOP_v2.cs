using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddIdLavorazioneToListaOP_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdLavorazione",
                table: "ListaOP",
                type: "int",
                nullable: true);

            // Imposta un valore di default per i record esistenti
            // Usa la prima lavorazione attiva disponibile
            migrationBuilder.Sql(@"
                UPDATE ListaOP 
                SET IdLavorazione = (
                    SELECT TOP 1 IdLavorazione 
                    FROM Lavorazioni 
                    WHERE Attivo = 1 
                    ORDER BY IdLavorazione
                )
                WHERE IdLavorazione IS NULL
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ListaOP_IdLavorazione",
                table: "ListaOP",
                column: "IdLavorazione");

            migrationBuilder.AddForeignKey(
                name: "FK_ListaOP_Lavorazioni_IdLavorazione",
                table: "ListaOP",
                column: "IdLavorazione",
                principalTable: "Lavorazioni",
                principalColumn: "IdLavorazione",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListaOP_Lavorazioni_IdLavorazione",
                table: "ListaOP");

            migrationBuilder.DropIndex(
                name: "IX_ListaOP_IdLavorazione",
                table: "ListaOP");

            migrationBuilder.DropColumn(
                name: "IdLavorazione",
                table: "ListaOP");
        }
    }
}
