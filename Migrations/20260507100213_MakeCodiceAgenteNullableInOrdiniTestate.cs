using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class MakeCodiceAgenteNullableInOrdiniTestate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "CodiceAgente",
                table: "OrdiniTestate",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "CodiceAgente",
                table: "OrdiniTestate",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);
        }
    }
}
