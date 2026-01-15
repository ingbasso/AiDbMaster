using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddProntoToProgressiviArticoli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Colonna già creata manualmente nel database
            // Questa migrazione serve solo per allineare lo snapshot EF Core
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Non fare nulla - la colonna è gestita esternamente
        }
    }
}
