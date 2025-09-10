using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddIdLavorazioneToListaOP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdLavorazione",
                table: "ListaOP",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
