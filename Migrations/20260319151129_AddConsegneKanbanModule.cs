using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddConsegneKanbanModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('AnagraficaArticoli', 'PesoUnitarioKg') IS NULL
                BEGIN
                    ALTER TABLE [AnagraficaArticoli] ADD [PesoUnitarioKg] decimal(18,3) NULL;
                END
            ");

            migrationBuilder.CreateTable(
                name: "MezziTrasporto",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodiceMezzo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Targa = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Descrizione = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PortataMaxKg = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MezziTrasporto", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TipiTrasporto",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codice = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipiTrasporto", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ViaggiConsegna",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataConsegna = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoTrasportoId = table.Column<int>(type: "int", nullable: false),
                    MezzoTrasportoId = table.Column<int>(type: "int", nullable: false),
                    OraPartenza = table.Column<TimeSpan>(type: "time", nullable: false),
                    OraArrivo = table.Column<TimeSpan>(type: "time", nullable: true),
                    DurataStimataMinuti = table.Column<int>(type: "int", nullable: false),
                    Stato = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatoDa = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViaggiConsegna", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ViaggiConsegna_MezziTrasporto_MezzoTrasportoId",
                        column: x => x.MezzoTrasportoId,
                        principalTable: "MezziTrasporto",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViaggiConsegna_TipiTrasporto_TipoTrasportoId",
                        column: x => x.TipoTrasportoId,
                        principalTable: "TipiTrasporto",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ViaggioConsegnaRighe",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViaggioConsegnaId = table.Column<int>(type: "int", nullable: false),
                    OrdineRigaId = table.Column<int>(type: "int", nullable: false),
                    QuantitaAssegnata = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PesoUnitarioKgSnapshot = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PesoTotaleKgSnapshot = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    NoteRiga = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViaggioConsegnaRighe", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ViaggioConsegnaRighe_OrdiniRighe_OrdineRigaId",
                        column: x => x.OrdineRigaId,
                        principalTable: "OrdiniRighe",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViaggioConsegnaRighe_ViaggiConsegna_ViaggioConsegnaId",
                        column: x => x.ViaggioConsegnaId,
                        principalTable: "ViaggiConsegna",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MezziTrasporto_Attivo",
                table: "MezziTrasporto",
                column: "Attivo");

            migrationBuilder.CreateIndex(
                name: "IX_MezziTrasporto_CodiceMezzo",
                table: "MezziTrasporto",
                column: "CodiceMezzo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MezziTrasporto_Targa",
                table: "MezziTrasporto",
                column: "Targa");

            migrationBuilder.CreateIndex(
                name: "IX_TipiTrasporto_Attivo",
                table: "TipiTrasporto",
                column: "Attivo");

            migrationBuilder.CreateIndex(
                name: "IX_TipiTrasporto_Codice",
                table: "TipiTrasporto",
                column: "Codice",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViaggiConsegna_DataConsegna",
                table: "ViaggiConsegna",
                column: "DataConsegna");

            migrationBuilder.CreateIndex(
                name: "IX_ViaggiConsegna_DataConsegna_Mezzo",
                table: "ViaggiConsegna",
                columns: new[] { "DataConsegna", "MezzoTrasportoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ViaggiConsegna_MezzoTrasportoId",
                table: "ViaggiConsegna",
                column: "MezzoTrasportoId");

            migrationBuilder.CreateIndex(
                name: "IX_ViaggiConsegna_Stato",
                table: "ViaggiConsegna",
                column: "Stato");

            migrationBuilder.CreateIndex(
                name: "IX_ViaggiConsegna_TipoTrasportoId",
                table: "ViaggiConsegna",
                column: "TipoTrasportoId");

            migrationBuilder.CreateIndex(
                name: "IX_ViaggioConsegnaRighe_OrdineRigaId",
                table: "ViaggioConsegnaRighe",
                column: "OrdineRigaId");

            migrationBuilder.CreateIndex(
                name: "IX_ViaggioConsegnaRighe_Viaggio_OrdineRiga",
                table: "ViaggioConsegnaRighe",
                columns: new[] { "ViaggioConsegnaId", "OrdineRigaId" },
                unique: true);

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'Consegne.DurataDefaultMinuti')
                BEGIN
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione])
                    VALUES ('Consegne.DurataDefaultMinuti', '90');
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViaggioConsegnaRighe");

            migrationBuilder.DropTable(
                name: "ViaggiConsegna");

            migrationBuilder.DropTable(
                name: "MezziTrasporto");

            migrationBuilder.DropTable(
                name: "TipiTrasporto");

            migrationBuilder.Sql(@"
                DELETE FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'Consegne.DurataDefaultMinuti';
            ");
        }
    }
}
