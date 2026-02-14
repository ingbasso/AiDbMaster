using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddTabellaOpzioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== TABELLA TabellaOpzioni =====
            // Crea la tabella solo se non esiste già
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TabellaOpzioni')
                BEGIN
                    CREATE TABLE [TabellaOpzioni] (
                        [ID] int IDENTITY(1,1) NOT NULL,
                        [NomeOpzione] varchar(255) NOT NULL,
                        [ValoreOpzione] varchar(max) NOT NULL,
                        CONSTRAINT [PK_TabellaOpzioni] PRIMARY KEY CLUSTERED ([ID] ASC)
                    );
                END
            ");

            // ===== INDICE UNIVOCO su NomeOpzione =====
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name = 'IX_TabellaOpzioni_NomeOpzione' AND object_id = OBJECT_ID('TabellaOpzioni')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_TabellaOpzioni_NomeOpzione] 
                    ON [TabellaOpzioni] ([NomeOpzione]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TabellaOpzioni");
        }
    }
}
