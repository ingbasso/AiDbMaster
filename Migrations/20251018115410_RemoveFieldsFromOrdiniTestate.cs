using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldsFromOrdiniTestate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Elimina il foreign key se esiste (cerca dinamicamente il nome)
            migrationBuilder.Sql(@"
                DECLARE @fkName NVARCHAR(200);
                SELECT @fkName = fk.name
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                WHERE fk.parent_object_id = OBJECT_ID(N'OrdiniTestate')
                  AND c.name = 'td_magaz';
                
                IF @fkName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE OrdiniTestate DROP CONSTRAINT [' + @fkName + ']');
                END
            ");

            // Elimina l'indice se esiste
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrdiniTestate_td_magaz' AND object_id = OBJECT_ID(N'OrdiniTestate'))
                BEGIN
                    DROP INDEX [IX_OrdiniTestate_td_magaz] ON [OrdiniTestate];
                END
            ");

            // Elimina le colonne con gestione dei default constraints
            migrationBuilder.Sql(@"
                DECLARE @constraintName NVARCHAR(200);
                
                -- Elimina constraint e colonna TotaleColli
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[OrdiniTestate]') AND c.name = 'TotaleColli';
                
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [OrdiniTestate] DROP CONSTRAINT [' + @constraintName + ']');
                END
                
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrdiniTestate]') AND name = 'TotaleColli')
                BEGIN
                    ALTER TABLE [OrdiniTestate] DROP COLUMN [TotaleColli];
                END
                
                -- Elimina constraint e colonna td_magaz
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[OrdiniTestate]') AND c.name = 'td_magaz';
                
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [OrdiniTestate] DROP CONSTRAINT [' + @constraintName + ']');
                END
                
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrdiniTestate]') AND name = 'td_magaz')
                BEGIN
                    ALTER TABLE [OrdiniTestate] DROP COLUMN [td_magaz];
                END
                
                -- Elimina constraint e colonna td_tipobf
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[OrdiniTestate]') AND c.name = 'td_tipobf';
                
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [OrdiniTestate] DROP CONSTRAINT [' + @constraintName + ']');
                END
                
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrdiniTestate]') AND name = 'td_tipobf')
                BEGIN
                    ALTER TABLE [OrdiniTestate] DROP COLUMN [td_tipobf];
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotaleColli",
                table: "OrdiniTestate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "td_magaz",
                table: "OrdiniTestate",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "td_tipobf",
                table: "OrdiniTestate",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "IX_OrdiniTestate_td_magaz",
                table: "OrdiniTestate",
                column: "td_magaz");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdiniTestate_TabellaMagazzini_td_magaz",
                table: "OrdiniTestate",
                column: "td_magaz",
                principalTable: "TabellaMagazzini",
                principalColumn: "CodiceMagazzino",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
