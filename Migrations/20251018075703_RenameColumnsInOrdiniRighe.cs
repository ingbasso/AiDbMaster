using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsInOrdiniRighe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "mo_quaeva",
                table: "OrdiniRighe",
                newName: "QuantitaEvasa");

            migrationBuilder.RenameColumn(
                name: "mo_coleva",
                table: "OrdiniRighe",
                newName: "ColliEvasi");

            migrationBuilder.RenameColumn(
                name: "mo_note",
                table: "OrdiniRighe",
                newName: "NoteRiga");

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 18, 9, 57, 2, 440, DateTimeKind.Local).AddTicks(225));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 23, 9, 57, 2, 440, DateTimeKind.Local).AddTicks(232));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 28, 9, 57, 2, 440, DateTimeKind.Local).AddTicks(235));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 3, 9, 57, 2, 440, DateTimeKind.Local).AddTicks(238));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 8, 9, 57, 2, 440, DateTimeKind.Local).AddTicks(242));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 13, 9, 57, 2, 440, DateTimeKind.Local).AddTicks(245));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 29, 9, 57, 2, 440, DateTimeKind.Local).AddTicks(248));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuantitaEvasa",
                table: "OrdiniRighe",
                newName: "mo_quaeva");

            migrationBuilder.RenameColumn(
                name: "ColliEvasi",
                table: "OrdiniRighe",
                newName: "mo_coleva");

            migrationBuilder.RenameColumn(
                name: "NoteRiga",
                table: "OrdiniRighe",
                newName: "mo_note");

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 18, 9, 49, 15, 637, DateTimeKind.Local).AddTicks(2748));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 23, 9, 49, 15, 637, DateTimeKind.Local).AddTicks(2756));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 28, 9, 49, 15, 637, DateTimeKind.Local).AddTicks(2768));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 3, 9, 49, 15, 637, DateTimeKind.Local).AddTicks(2781));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 8, 9, 49, 15, 637, DateTimeKind.Local).AddTicks(2784));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 13, 9, 49, 15, 637, DateTimeKind.Local).AddTicks(2792));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 29, 9, 49, 15, 637, DateTimeKind.Local).AddTicks(2801));
        }
    }
}
