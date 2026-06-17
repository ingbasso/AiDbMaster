using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddIndisponibilita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Indisponibilita",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AutistaId = table.Column<int>(type: "int", nullable: true),
                    MezzoTrasportoId = table.Column<int>(type: "int", nullable: true),
                    DataInizio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFine = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GiornoIntero = table.Column<bool>(type: "bit", nullable: false),
                    OraInizio = table.Column<TimeSpan>(type: "time", nullable: true),
                    OraFine = table.Column<TimeSpan>(type: "time", nullable: true),
                    Causale = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatoDa = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indisponibilita", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Indisponibilita_Autisti_AutistaId",
                        column: x => x.AutistaId,
                        principalTable: "Autisti",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Indisponibilita_MezziTrasportoInterni_MezzoTrasportoId",
                        column: x => x.MezzoTrasportoId,
                        principalTable: "MezziTrasportoInterni",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Indisponibilita_AutistaId",
                table: "Indisponibilita",
                column: "AutistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Indisponibilita_Date",
                table: "Indisponibilita",
                columns: new[] { "DataInizio", "DataFine" });

            migrationBuilder.CreateIndex(
                name: "IX_Indisponibilita_MezzoTrasportoId",
                table: "Indisponibilita",
                column: "MezzoTrasportoId");

            migrationBuilder.CreateIndex(
                name: "IX_Indisponibilita_Tipo",
                table: "Indisponibilita",
                column: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Indisponibilita");
        }
    }
}
