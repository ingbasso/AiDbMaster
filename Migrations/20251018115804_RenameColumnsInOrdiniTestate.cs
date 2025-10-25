using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsInOrdiniTestate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "td_riferim",
                table: "OrdiniTestate",
                newName: "RiferimentoOrdine");

            migrationBuilder.RenameColumn(
                name: "td_note",
                table: "OrdiniTestate",
                newName: "NoteTestata");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RiferimentoOrdine",
                table: "OrdiniTestate",
                newName: "td_riferim");

            migrationBuilder.RenameColumn(
                name: "NoteTestata",
                table: "OrdiniTestate",
                newName: "td_note");
        }
    }
}
