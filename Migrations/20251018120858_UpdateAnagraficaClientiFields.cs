using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnagraficaClientiFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "an_faxtlx",
                table: "AnagraficaClienti");

            migrationBuilder.RenameColumn(
                name: "an_pariva",
                table: "AnagraficaClienti",
                newName: "PartitaIva");

            migrationBuilder.RenameColumn(
                name: "an_codfis",
                table: "AnagraficaClienti",
                newName: "CodiceFiscale");

            migrationBuilder.RenameColumn(
                name: "an_tipo",
                table: "AnagraficaClienti",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "an_descr2",
                table: "AnagraficaClienti",
                newName: "DescrizioneUlteriore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PartitaIva",
                table: "AnagraficaClienti",
                newName: "an_pariva");

            migrationBuilder.RenameColumn(
                name: "CodiceFiscale",
                table: "AnagraficaClienti",
                newName: "an_codfis");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "AnagraficaClienti",
                newName: "an_tipo");

            migrationBuilder.RenameColumn(
                name: "DescrizioneUlteriore",
                table: "AnagraficaClienti",
                newName: "an_descr2");

            migrationBuilder.AddColumn<string>(
                name: "an_faxtlx",
                table: "AnagraficaClienti",
                type: "nvarchar(18)",
                maxLength: 18,
                nullable: true);
        }
    }
}
