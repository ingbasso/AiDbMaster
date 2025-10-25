using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddPercentualeInclusioneToOrdiniRighe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PercentualeInclusione",
                table: "OrdiniRighe",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 18, 8, 36, 0, 230, DateTimeKind.Local).AddTicks(47));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 23, 8, 36, 0, 230, DateTimeKind.Local).AddTicks(56));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 28, 8, 36, 0, 230, DateTimeKind.Local).AddTicks(60));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 3, 8, 36, 0, 230, DateTimeKind.Local).AddTicks(64));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 8, 8, 36, 0, 230, DateTimeKind.Local).AddTicks(68));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 13, 8, 36, 0, 230, DateTimeKind.Local).AddTicks(73));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 29, 8, 36, 0, 230, DateTimeKind.Local).AddTicks(76));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PercentualeInclusione",
                table: "OrdiniRighe");

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 16, 14, 29, 29, 179, DateTimeKind.Local).AddTicks(1790));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 21, 14, 29, 29, 179, DateTimeKind.Local).AddTicks(1802));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 26, 14, 29, 29, 179, DateTimeKind.Local).AddTicks(1811));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 31, 14, 29, 29, 179, DateTimeKind.Local).AddTicks(1820));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 5, 14, 29, 29, 179, DateTimeKind.Local).AddTicks(1823));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 10, 14, 29, 29, 179, DateTimeKind.Local).AddTicks(1829));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 7, 27, 14, 29, 29, 179, DateTimeKind.Local).AddTicks(1835));
        }
    }
}
