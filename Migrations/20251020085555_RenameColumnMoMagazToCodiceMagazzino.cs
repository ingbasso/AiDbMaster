using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnMoMagazToCodiceMagazzino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Usa sp_rename per rinominare la colonna - gestisce automaticamente FK e indici
            migrationBuilder.Sql("EXEC sp_rename 'OrdiniRighe.mo_magaz', 'CodiceMagazzino', 'COLUMN'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: rinomina la colonna al nome originale
            migrationBuilder.Sql("EXEC sp_rename 'OrdiniRighe.CodiceMagazzino', 'mo_magaz', 'COLUMN'");
        }
    }
}
