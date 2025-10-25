using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class RemoveImpegnatoTotaleAndPrenotatoFromProgressiviArticoli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Script per eliminare colonne con i loro constraint
            migrationBuilder.Sql(@"
                -- Elimina constraint e colonna ImpegnatoTotale
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND c.name = 'ImpegnatoTotale';
                
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @constraintName + ']');
                END
                
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND name = 'ImpegnatoTotale')
                BEGIN
                    ALTER TABLE [ProgressiviArticoli] DROP COLUMN [ImpegnatoTotale];
                END
            ");

            migrationBuilder.Sql(@"
                -- Elimina constraint e colonna Prenotato
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND c.name = 'Prenotato';
                
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @constraintName + ']');
                END
                
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND name = 'Prenotato')
                BEGIN
                    ALTER TABLE [ProgressiviArticoli] DROP COLUMN [Prenotato];
                END
            ");

            migrationBuilder.Sql(@"
                -- Elimina constraint e colonna Impegnato
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND c.name = 'Impegnato';
                
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @constraintName + ']');
                END
                
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND name = 'Impegnato')
                BEGIN
                    ALTER TABLE [ProgressiviArticoli] DROP COLUMN [Impegnato];
                END
            ");

            migrationBuilder.Sql(@"
                -- Elimina constraint e colonna Ordinato
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND c.name = 'Ordinato';
                
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @constraintName + ']');
                END
                
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND name = 'Ordinato')
                BEGIN
                    ALTER TABLE [ProgressiviArticoli] DROP COLUMN [Ordinato];
                END
            ");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 18, 8, 57, 7, 419, DateTimeKind.Local).AddTicks(106));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 23, 8, 57, 7, 419, DateTimeKind.Local).AddTicks(112));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 28, 8, 57, 7, 419, DateTimeKind.Local).AddTicks(115));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 3, 8, 57, 7, 419, DateTimeKind.Local).AddTicks(118));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 8, 8, 57, 7, 419, DateTimeKind.Local).AddTicks(121));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 13, 8, 57, 7, 419, DateTimeKind.Local).AddTicks(124));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 29, 8, 57, 7, 419, DateTimeKind.Local).AddTicks(127));
        }
    }
}
