using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddGiorniAsciugaturaExtraToAnagraficaArticoli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GiorniAsciugaturaExtra",
                table: "AnagraficaArticoli",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DurataDelleScorte",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodMarca = table.Column<short>(type: "smallint", nullable: false),
                    DescrizioneMarca = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    CodFamiglia = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    DescrFamiglia = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    CodiceArticolo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Descrizione = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    UnitàMisura = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    Magazzino = table.Column<short>(type: "smallint", nullable: true),
                    DataUltimoScarico = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Esistenza = table.Column<decimal>(type: "decimal(27,9)", nullable: true),
                    Disponibilità = table.Column<decimal>(type: "decimal(27,9)", nullable: true),
                    ConsumoUltimomese = table.Column<decimal>(type: "decimal(27,9)", nullable: true),
                    ConsumoDueMesifa = table.Column<decimal>(type: "decimal(27,9)", nullable: true),
                    ConsumoTreMesifa = table.Column<decimal>(type: "decimal(27,9)", nullable: true),
                    ConsumoMedioPonderato = table.Column<decimal>(type: "decimal(27,9)", nullable: true),
                    DurataDelleScorte = table.Column<decimal>(type: "decimal(27,9)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DurataDelleScorte", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "LogEmailAutomatico",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataOra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    AnnoOrdine = table.Column<short>(type: "smallint", nullable: true),
                    SerieOrdine = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    NumeroOrdine = table.Column<int>(type: "int", nullable: true),
                    RigaOrdine = table.Column<int>(type: "int", nullable: true),
                    CodiceCliente = table.Column<int>(type: "int", nullable: true),
                    RagioneSociale = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    EmailDestinatario = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Esito = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Motivo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Dettagli = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEmailAutomatico", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StoricoMaterialeLiberato",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataLiberazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoOrdine = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false),
                    AnnoOrdine = table.Column<short>(type: "smallint", nullable: false),
                    SerieOrdine = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    NumeroOrdine = table.Column<int>(type: "int", nullable: false),
                    RigaOrdine = table.Column<int>(type: "int", nullable: false),
                    CodiceCliente = table.Column<int>(type: "int", nullable: false),
                    RagioneSociale = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CodiceArticolo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DescrizioneArticolo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DataConsegna = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnitaMisura = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    Quantita = table.Column<decimal>(type: "decimal(27,9)", nullable: false, defaultValue: 0m),
                    UnitaMisuraColli = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    NumeroColli = table.Column<decimal>(type: "decimal(27,9)", nullable: false, defaultValue: 0m),
                    UltimoAggiornamento = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoricoMaterialeLiberato", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DurataDelleScorte_CodFamiglia",
                table: "DurataDelleScorte",
                column: "CodFamiglia");

            migrationBuilder.CreateIndex(
                name: "IX_DurataDelleScorte_CodiceArticolo",
                table: "DurataDelleScorte",
                column: "CodiceArticolo");

            migrationBuilder.CreateIndex(
                name: "IX_DurataDelleScorte_CodMarca",
                table: "DurataDelleScorte",
                column: "CodMarca");

            migrationBuilder.CreateIndex(
                name: "IX_StoricoMaterialeLiberato_CodiceArticolo",
                table: "StoricoMaterialeLiberato",
                column: "CodiceArticolo");

            migrationBuilder.CreateIndex(
                name: "IX_StoricoMaterialeLiberato_DataLiberazione",
                table: "StoricoMaterialeLiberato",
                column: "DataLiberazione");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DurataDelleScorte");

            migrationBuilder.DropTable(
                name: "LogEmailAutomatico");

            migrationBuilder.DropTable(
                name: "StoricoMaterialeLiberato");

            migrationBuilder.DropColumn(
                name: "GiorniAsciugaturaExtra",
                table: "AnagraficaArticoli");
        }
    }
}
