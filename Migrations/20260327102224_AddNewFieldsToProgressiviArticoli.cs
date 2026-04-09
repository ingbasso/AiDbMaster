using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsToProgressiviArticoli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Le colonne esistono già nel database (aggiunte manualmente).
            // Questa migration serve solo ad aggiornare lo snapshot EF.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataUltimoCarico",
                table: "ProgressiviArticoli");

            migrationBuilder.DropColumn(
                name: "DataUltimoScarico",
                table: "ProgressiviArticoli");

            migrationBuilder.DropColumn(
                name: "DurataDelleScorte",
                table: "ProgressiviArticoli");

            migrationBuilder.DropColumn(
                name: "Prenotato",
                table: "ProgressiviArticoli");

            migrationBuilder.DropColumn(
                name: "UltimoAggiornamento",
                table: "ProgressiviArticoli");
        }
    }
}
