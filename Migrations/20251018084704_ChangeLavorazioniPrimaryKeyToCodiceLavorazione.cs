using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLavorazioniPrimaryKeyToCodiceLavorazione : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IMPORTANTE: Prima di eseguire questa migrazione, assicurarsi che tutti i record in Lavorazioni
            // abbiano un CodiceLavorazione univoco e NON NULL

            // STEP 1: Assegna codici univoci alle lavorazioni in modo progressivo
            // Usa caratteri alfanumerici (0-9, A-Z) per massimizzare le possibilità
            migrationBuilder.Sql(@"
                -- Crea una tabella temporanea con codici univoci
                DECLARE @Chars VARCHAR(36) = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
                
                WITH RankedLavorazioni AS (
                    SELECT IdLavorazione, 
                           CodiceLavorazione,
                           ROW_NUMBER() OVER (ORDER BY IdLavorazione) - 1 AS RowNum
                    FROM Lavorazioni
                ),
                NewCodes AS (
                    SELECT IdLavorazione,
                           CodiceLavorazione,
                           CASE 
                               WHEN CodiceLavorazione IS NOT NULL AND CodiceLavorazione != '' 
                                    AND LEN(CodiceLavorazione) = 1
                               THEN CodiceLavorazione
                               ELSE SUBSTRING(@Chars, (RowNum % 36) + 1, 1)
                           END AS NewCode
                    FROM RankedLavorazioni
                )
                UPDATE L
                SET L.CodiceLavorazione = NC.NewCode
                FROM Lavorazioni L
                INNER JOIN NewCodes NC ON L.IdLavorazione = NC.IdLavorazione
                WHERE L.CodiceLavorazione IS NULL 
                   OR L.CodiceLavorazione = '' 
                   OR LEN(L.CodiceLavorazione) != 1
            ");

            // STEP 2: Gestisci eventuali duplicati di CodiceLavorazione rimasti
            // Assegna codici progressivi ai duplicati
            migrationBuilder.Sql(@"
                DECLARE @Chars VARCHAR(36) = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
                
                WITH Duplicates AS (
                    SELECT IdLavorazione,
                           CodiceLavorazione, 
                           ROW_NUMBER() OVER (PARTITION BY CodiceLavorazione ORDER BY IdLavorazione) AS RowNum,
                           COUNT(*) OVER (PARTITION BY CodiceLavorazione) AS DupCount
                    FROM Lavorazioni
                )
                UPDATE L
                SET L.CodiceLavorazione = SUBSTRING(@Chars, ((L.IdLavorazione - 1) % 36) + 1, 1)
                FROM Lavorazioni L
                INNER JOIN Duplicates D ON L.IdLavorazione = D.IdLavorazione
                WHERE D.DupCount > 1 AND D.RowNum > 1
            ");

            // STEP 3: Aggiungi la colonna CodiceLavorazione a ListaOP (nullable temporaneamente)
            migrationBuilder.AddColumn<string>(
                name: "CodiceLavorazione",
                table: "ListaOP",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true);

            // STEP 4: Copia i dati da Lavorazioni.CodiceLavorazione a ListaOP.CodiceLavorazione
            // basandosi sulla relazione esistente IdLavorazione
            // Usa LEFT() per garantire che sia solo 1 carattere
            migrationBuilder.Sql(@"
                UPDATE ListaOP 
                SET CodiceLavorazione = LEFT(ISNULL(l.CodiceLavorazione, CAST(l.IdLavorazione AS VARCHAR(1))), 1)
                FROM ListaOP lo
                INNER JOIN Lavorazioni l ON lo.IdLavorazione = l.IdLavorazione
                WHERE lo.CodiceLavorazione IS NULL
            ");

            // STEP 5: Rendi CodiceLavorazione NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "CodiceLavorazione",
                table: "ListaOP",
                type: "varchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            // STEP 6: Elimina la foreign key esistente (cerca dinamicamente il nome)
            migrationBuilder.Sql(@"
                DECLARE @fkName NVARCHAR(200);
                SELECT @fkName = fk.name
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                WHERE fk.parent_object_id = OBJECT_ID(N'ListaOP')
                  AND c.name = 'IdLavorazione';
                
                IF @fkName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE ListaOP DROP CONSTRAINT [' + @fkName + ']');
                END
            ");

            // STEP 7: Elimina l'indice esistente (se esiste)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ListaOP_IdLavorazione' AND object_id = OBJECT_ID(N'ListaOP'))
                BEGIN
                    DROP INDEX [IX_ListaOP_IdLavorazione] ON [ListaOP];
                END
            ");

            // STEP 8: Elimina la vecchia primary key di Lavorazioni
            migrationBuilder.DropPrimaryKey(
                name: "PK_Lavorazioni",
                table: "Lavorazioni");

            // STEP 9: Elimina l'indice su CodiceLavorazione (se esiste, non più necessario perché diventerà PK)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Lavorazioni_CodiceLavorazione' AND object_id = OBJECT_ID(N'Lavorazioni'))
                BEGIN
                    DROP INDEX [IX_Lavorazioni_CodiceLavorazione] ON [Lavorazioni];
                END
            ");

            // STEP 10: Elimina la colonna IdLavorazione da ListaOP
            migrationBuilder.DropColumn(
                name: "IdLavorazione",
                table: "ListaOP");

            // STEP 11: Elimina la colonna IdLavorazione da Lavorazioni
            migrationBuilder.DropColumn(
                name: "IdLavorazione",
                table: "Lavorazioni");

            // STEP 11.5: Tronca tutti i CodiceLavorazione a 1 carattere (caso edge per dati legacy)
            migrationBuilder.Sql(@"
                UPDATE Lavorazioni
                SET CodiceLavorazione = LEFT(CodiceLavorazione, 1)
                WHERE LEN(CodiceLavorazione) > 1 OR CodiceLavorazione IS NULL
            ");

            // STEP 12: Rendi CodiceLavorazione NOT NULL in Lavorazioni (se necessario)
            migrationBuilder.AlterColumn<string>(
                name: "CodiceLavorazione",
                table: "Lavorazioni",
                type: "varchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            // STEP 13: Crea la nuova primary key su CodiceLavorazione
            migrationBuilder.AddPrimaryKey(
                name: "PK_Lavorazioni",
                table: "Lavorazioni",
                column: "CodiceLavorazione");

            // STEP 14: Crea l'indice sulla nuova foreign key
            migrationBuilder.CreateIndex(
                name: "IX_ListaOP_CodiceLavorazione",
                table: "ListaOP",
                column: "CodiceLavorazione");

            // STEP 15: Crea la nuova foreign key tra ListaOP e Lavorazioni
            migrationBuilder.AddForeignKey(
                name: "FK_ListaOP_Lavorazioni_CodiceLavorazione",
                table: "ListaOP",
                column: "CodiceLavorazione",
                principalTable: "Lavorazioni",
                principalColumn: "CodiceLavorazione",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nota: Il rollback di questa migrazione potrebbe causare perdita di dati
            // perché richiede di ricreare IdLavorazione come identity
            
            migrationBuilder.DropForeignKey(
                name: "FK_ListaOP_Lavorazioni_CodiceLavorazione",
                table: "ListaOP");

            migrationBuilder.DropIndex(
                name: "IX_ListaOP_CodiceLavorazione",
                table: "ListaOP");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lavorazioni",
                table: "Lavorazioni");

            migrationBuilder.DropColumn(
                name: "CodiceLavorazione",
                table: "ListaOP");

            migrationBuilder.AddColumn<int>(
                name: "IdLavorazione",
                table: "ListaOP",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CodiceLavorazione",
                table: "Lavorazioni",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AddColumn<int>(
                name: "IdLavorazione",
                table: "Lavorazioni",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lavorazioni",
                table: "Lavorazioni",
                column: "IdLavorazione");

            migrationBuilder.CreateIndex(
                name: "IX_ListaOP_IdLavorazione",
                table: "ListaOP",
                column: "IdLavorazione");

            migrationBuilder.CreateIndex(
                name: "IX_Lavorazioni_CodiceLavorazione",
                table: "Lavorazioni",
                column: "CodiceLavorazione",
                unique: true,
                filter: "[CodiceLavorazione] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ListaOP_Lavorazioni_IdLavorazione",
                table: "ListaOP",
                column: "IdLavorazione",
                principalTable: "Lavorazioni",
                principalColumn: "IdLavorazione",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
