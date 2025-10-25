using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCentriLavoroPrimaryKeyToCodiceCentro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // STEP 1: Aggiungi la colonna CodiceCentro a ListaOP (nullable temporaneamente)
            migrationBuilder.AddColumn<string>(
                name: "CodiceCentro",
                table: "ListaOP",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            // STEP 2: Copia i dati da CentriLavoro.CodiceCentro a ListaOP.CodiceCentro
            // basandosi sulla relazione esistente IdCentroLavoro
            migrationBuilder.Sql(@"
                UPDATE ListaOP 
                SET CodiceCentro = c.CodiceCentro
                FROM ListaOP l
                INNER JOIN CentriLavoro c ON l.IdCentroLavoro = c.IdCentroLavoro
                WHERE l.CodiceCentro IS NULL
            ");

            // STEP 3: Rendi CodiceCentro NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "CodiceCentro",
                table: "ListaOP",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            // STEP 4: Elimina la foreign key esistente (cerca dinamicamente il nome)
            migrationBuilder.Sql(@"
                DECLARE @fkName NVARCHAR(200);
                SELECT @fkName = fk.name
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                WHERE fk.parent_object_id = OBJECT_ID(N'ListaOP')
                  AND c.name = 'IdCentroLavoro';
                
                IF @fkName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE ListaOP DROP CONSTRAINT [' + @fkName + ']');
                END
            ");

            // STEP 5: Elimina l'indice esistente (se esiste)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ListaOP_IdCentroLavoro' AND object_id = OBJECT_ID(N'ListaOP'))
                BEGIN
                    DROP INDEX [IX_ListaOP_IdCentroLavoro] ON [ListaOP];
                END
            ");

            // STEP 6: Elimina la vecchia primary key di CentriLavoro
            migrationBuilder.DropPrimaryKey(
                name: "PK_CentriLavoro",
                table: "CentriLavoro");

            // STEP 7: Elimina l'indice su CodiceCentro (se esiste, non più necessario perché diventerà PK)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CentriLavoro_CodiceCentro' AND object_id = OBJECT_ID(N'CentriLavoro'))
                BEGIN
                    DROP INDEX [IX_CentriLavoro_CodiceCentro] ON [CentriLavoro];
                END
            ");

            // STEP 8: Elimina la colonna IdCentroLavoro da ListaOP
            migrationBuilder.DropColumn(
                name: "IdCentroLavoro",
                table: "ListaOP");

            // STEP 9: Elimina la colonna IdCentroLavoro da CentriLavoro
            migrationBuilder.DropColumn(
                name: "IdCentroLavoro",
                table: "CentriLavoro");

            // STEP 10: Rendi CodiceCentro NOT NULL in CentriLavoro (se necessario)
            migrationBuilder.AlterColumn<string>(
                name: "CodiceCentro",
                table: "CentriLavoro",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            // STEP 11: Crea la nuova primary key su CodiceCentro
            migrationBuilder.AddPrimaryKey(
                name: "PK_CentriLavoro",
                table: "CentriLavoro",
                column: "CodiceCentro");

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 1,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 18, 10, 18, 42, 731, DateTimeKind.Local).AddTicks(2634));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 2,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 23, 10, 18, 42, 731, DateTimeKind.Local).AddTicks(2644));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 3,
                column: "DataCreazione",
                value: new DateTime(2025, 9, 28, 10, 18, 42, 731, DateTimeKind.Local).AddTicks(2649));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 4,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 3, 10, 18, 42, 731, DateTimeKind.Local).AddTicks(2653));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 5,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 8, 10, 18, 42, 731, DateTimeKind.Local).AddTicks(2658));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 6,
                column: "DataCreazione",
                value: new DateTime(2025, 10, 13, 10, 18, 42, 731, DateTimeKind.Local).AddTicks(2663));

            migrationBuilder.UpdateData(
                table: "Lavorazioni",
                keyColumn: "IdLavorazione",
                keyValue: 7,
                column: "DataCreazione",
                value: new DateTime(2025, 8, 29, 10, 18, 42, 731, DateTimeKind.Local).AddTicks(2667));

            // STEP 12: Crea l'indice sulla nuova foreign key
            migrationBuilder.CreateIndex(
                name: "IX_ListaOP_CodiceCentro",
                table: "ListaOP",
                column: "CodiceCentro");

            // STEP 13: Crea la nuova foreign key tra ListaOP e CentriLavoro
            migrationBuilder.AddForeignKey(
                name: "FK_ListaOP_CentriLavoro_CodiceCentro",
                table: "ListaOP",
                column: "CodiceCentro",
                principalTable: "CentriLavoro",
                principalColumn: "CodiceCentro",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListaOP_CentriLavoro_CodiceCentro",
                table: "ListaOP");

            migrationBuilder.DropIndex(
                name: "IX_ListaOP_CodiceCentro",
                table: "ListaOP");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CentriLavoro",
                table: "CentriLavoro");

            migrationBuilder.DropColumn(
                name: "CodiceCentro",
                table: "ListaOP");

            migrationBuilder.AddColumn<int>(
                name: "IdCentroLavoro",
                table: "ListaOP",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CodiceCentro",
                table: "CentriLavoro",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<int>(
                name: "IdCentroLavoro",
                table: "CentriLavoro",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CentriLavoro",
                table: "CentriLavoro",
                column: "IdCentroLavoro");

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

            migrationBuilder.CreateIndex(
                name: "IX_ListaOP_IdCentroLavoro",
                table: "ListaOP",
                column: "IdCentroLavoro");

            migrationBuilder.CreateIndex(
                name: "IX_CentriLavoro_CodiceCentro",
                table: "CentriLavoro",
                column: "CodiceCentro",
                unique: true,
                filter: "[CodiceCentro] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ListaOP_CentriLavoro_IdCentroLavoro",
                table: "ListaOP",
                column: "IdCentroLavoro",
                principalTable: "CentriLavoro",
                principalColumn: "IdCentroLavoro",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
