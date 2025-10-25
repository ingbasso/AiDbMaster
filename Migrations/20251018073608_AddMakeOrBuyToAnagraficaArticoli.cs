using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddMakeOrBuyToAnagraficaArticoli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MakeOrBuy",
                table: "AnagraficaArticoli",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 18, 9, 36, 7, 121, DateTimeKind.Local).AddTicks(7888));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 23, 9, 36, 7, 121, DateTimeKind.Local).AddTicks(7894));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 28, 9, 36, 7, 121, DateTimeKind.Local).AddTicks(7898));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 3, 9, 36, 7, 121, DateTimeKind.Local).AddTicks(7901));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 8, 9, 36, 7, 121, DateTimeKind.Local).AddTicks(7904));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 13, 9, 36, 7, 121, DateTimeKind.Local).AddTicks(7907));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 29, 9, 36, 7, 121, DateTimeKind.Local).AddTicks(7911));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MakeOrBuy",
                table: "AnagraficaArticoli");

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
    }
}
