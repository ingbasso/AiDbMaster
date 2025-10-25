using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class CreateCalendarioFermiCentriLavoro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarioFermiCentriLavoro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodiceCentro = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DataInizioFermo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFineFermo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoFermo = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPianificato = table.Column<bool>(type: "bit", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataUltimaModifica = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarioFermiCentriLavoro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarioFermiCentriLavoro_CentriLavoro_CodiceCentro",
                        column: x => x.CodiceCentro,
                        principalTable: "CentriLavoro",
                        principalColumn: "CodiceCentro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarioFermiCentriLavoro_CodiceCentro",
                table: "CalendarioFermiCentriLavoro",
                column: "CodiceCentro");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarioFermiCentriLavoro_DataFineFermo",
                table: "CalendarioFermiCentriLavoro",
                column: "DataFineFermo");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarioFermiCentriLavoro_DataInizioFermo",
                table: "CalendarioFermiCentriLavoro",
                column: "DataInizioFermo");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarioFermiCentriLavoro_TipoFermo",
                table: "CalendarioFermiCentriLavoro",
                column: "TipoFermo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarioFermiCentriLavoro");
        }
    }
}
