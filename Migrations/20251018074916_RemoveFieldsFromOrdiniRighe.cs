using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldsFromOrdiniRighe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mo_codiva",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_colpre",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_flevapre",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_prelist",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_preziva",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_prezvalc",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_provv",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_quapre",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_scont1",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_scont2",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "mo_scont3",
                table: "OrdiniRighe");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "mo_codiva",
                table: "OrdiniRighe",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<decimal>(
                name: "mo_colpre",
                table: "OrdiniRighe",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "mo_flevapre",
                table: "OrdiniRighe",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "mo_prelist",
                table: "OrdiniRighe",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "mo_preziva",
                table: "OrdiniRighe",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "mo_prezvalc",
                table: "OrdiniRighe",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "mo_provv",
                table: "OrdiniRighe",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "mo_quapre",
                table: "OrdiniRighe",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "mo_scont1",
                table: "OrdiniRighe",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "mo_scont2",
                table: "OrdiniRighe",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "mo_scont3",
                table: "OrdiniRighe",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

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
    }
}
