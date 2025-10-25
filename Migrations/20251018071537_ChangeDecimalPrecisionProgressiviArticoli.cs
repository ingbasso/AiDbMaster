using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDecimalPrecisionProgressiviArticoli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OrdinatoFornitoriDataOdierna",
                table: "ProgressiviArticoli",
                type: "decimal(27,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Esistenza",
                table: "ProgressiviArticoli",
                type: "decimal(27,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 18, 9, 15, 37, 61, DateTimeKind.Local).AddTicks(2894));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 23, 9, 15, 37, 61, DateTimeKind.Local).AddTicks(2900));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 28, 9, 15, 37, 61, DateTimeKind.Local).AddTicks(2903));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 3, 9, 15, 37, 61, DateTimeKind.Local).AddTicks(2906));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 8, 9, 15, 37, 61, DateTimeKind.Local).AddTicks(2909));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 13, 9, 15, 37, 61, DateTimeKind.Local).AddTicks(2912));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 29, 9, 15, 37, 61, DateTimeKind.Local).AddTicks(2914));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OrdinatoFornitoriDataOdierna",
                table: "ProgressiviArticoli",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(27,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Esistenza",
                table: "ProgressiviArticoli",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(27,9)");

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 18, 9, 7, 23, 652, DateTimeKind.Local).AddTicks(7560));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 23, 9, 7, 23, 652, DateTimeKind.Local).AddTicks(7572));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 28, 9, 7, 23, 652, DateTimeKind.Local).AddTicks(7575));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 3, 9, 7, 23, 652, DateTimeKind.Local).AddTicks(7578));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 8, 9, 7, 23, 652, DateTimeKind.Local).AddTicks(7581));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 13, 9, 7, 23, 652, DateTimeKind.Local).AddTicks(7583));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 29, 9, 7, 23, 652, DateTimeKind.Local).AddTicks(7586));
        }
    }
}
