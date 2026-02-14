using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class FixPortoColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La colonna Porto nel database è già varchar.
            // Questa migration serve solo per allineare lo snapshot EF Core
            // dal tipo errato (smallint) al tipo corretto (varchar).
            // Non è necessaria alcuna modifica al database.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nessuna operazione necessaria
        }
    }
}
