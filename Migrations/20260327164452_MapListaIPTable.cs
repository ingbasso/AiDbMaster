using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class MapListaIPTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListaIP",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoOrdine = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false),
                    AnnoOrdine = table.Column<short>(type: "smallint", nullable: false),
                    SerieOrdine = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    NumeroOrdine = table.Column<int>(type: "int", nullable: false),
                    RigaOrdine = table.Column<int>(type: "int", nullable: false),
                    RigaImpegno = table.Column<int>(type: "int", nullable: false),
                    CodiceMagazzino = table.Column<short>(type: "smallint", nullable: false),
                    CodiceArticolo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DescrizioneArticolo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DataConsegna = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnitaMisura = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    Quantita = table.Column<decimal>(type: "decimal(27,9)", nullable: false),
                    UnitaMisuraColli = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    NumeroColli = table.Column<decimal>(type: "decimal(27,9)", nullable: false),
                    ColliEvasi = table.Column<decimal>(type: "decimal(27,9)", nullable: false),
                    QuantitaEvasa = table.Column<decimal>(type: "decimal(27,9)", nullable: false),
                    Prezzo = table.Column<decimal>(type: "decimal(24,6)", nullable: false),
                    NoteRiga = table.Column<string>(type: "varchar(max)", nullable: true),
                    ValoreRiga = table.Column<decimal>(type: "money", nullable: false),
                    UltimoAggiornamento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaIP", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListaIP");
        }
    }
}
