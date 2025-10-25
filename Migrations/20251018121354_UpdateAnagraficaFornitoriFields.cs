using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnagraficaFornitoriFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "an_faxtlx",
                table: "AnagraficaFornitori");

            migrationBuilder.RenameColumn(
                name: "an_pariva",
                table: "AnagraficaFornitori",
                newName: "PartitaIva");

            migrationBuilder.RenameColumn(
                name: "an_codfis",
                table: "AnagraficaFornitori",
                newName: "CodiceFiscale");

            migrationBuilder.RenameColumn(
                name: "an_tipo",
                table: "AnagraficaFornitori",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "an_descr2",
                table: "AnagraficaFornitori",
                newName: "DescrizioneUlteriore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PartitaIva",
                table: "AnagraficaFornitori",
                newName: "an_pariva");

            migrationBuilder.RenameColumn(
                name: "CodiceFiscale",
                table: "AnagraficaFornitori",
                newName: "an_codfis");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "AnagraficaFornitori",
                newName: "an_tipo");

            migrationBuilder.RenameColumn(
                name: "DescrizioneUlteriore",
                table: "AnagraficaFornitori",
                newName: "an_descr2");

            migrationBuilder.AddColumn<string>(
                name: "an_faxtlx",
                table: "AnagraficaFornitori",
                type: "nvarchar(18)",
                maxLength: 18,
                nullable: true);
        }
    }
}
