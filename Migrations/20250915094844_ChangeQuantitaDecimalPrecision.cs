using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class ChangeQuantitaDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Modifica la precisione dei campi decimal da (27,9) a (10,3)
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantita",
                table: "ListaOP",
                type: "decimal(10,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(27,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantitaProdotta",
                table: "ListaOP",
                type: "decimal(10,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(27,9)");
            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 16, 11, 48, 42, 395, DateTimeKind.Local).AddTicks(91));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 21, 11, 48, 42, 395, DateTimeKind.Local).AddTicks(97));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 26, 11, 48, 42, 395, DateTimeKind.Local).AddTicks(100));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 31, 11, 48, 42, 395, DateTimeKind.Local).AddTicks(103));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 5, 11, 48, 42, 395, DateTimeKind.Local).AddTicks(106));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 10, 11, 48, 42, 395, DateTimeKind.Local).AddTicks(111));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 7, 27, 11, 48, 42, 395, DateTimeKind.Local).AddTicks(113));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ripristina la precisione originale dei campi decimal da (10,3) a (27,9)
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantita",
                table: "ListaOP",
                type: "decimal(27,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantitaProdotta",
                table: "ListaOP",
                type: "decimal(27,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)");
            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 11, 17, 23, 50, 453, DateTimeKind.Local).AddTicks(1098));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 16, 17, 23, 50, 453, DateTimeKind.Local).AddTicks(1106));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 21, 17, 23, 50, 453, DateTimeKind.Local).AddTicks(1109));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 26, 17, 23, 50, 453, DateTimeKind.Local).AddTicks(1113));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 31, 17, 23, 50, 453, DateTimeKind.Local).AddTicks(1116));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 5, 17, 23, 50, 453, DateTimeKind.Local).AddTicks(1120));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 7, 22, 17, 23, 50, 453, DateTimeKind.Local).AddTicks(1123));
        }
    }
}
