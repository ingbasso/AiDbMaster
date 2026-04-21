using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddTabelleContoEconomicoPstree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pstree_ListaPianoDeiConti",
                columns: table => new
                {
                    CodicePdC = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DescrizionePdC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoPdC = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    NonAssociare = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_ListaPianoDeiConti", x => x.CodicePdC);
                });

            migrationBuilder.CreateTable(
                name: "Pstree_ListaSedi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Sede = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescrizioneSede = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_ListaSedi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pstree_StrutturaContoEconomico",
                columns: table => new
                {
                    IdCodiceConto = table.Column<int>(type: "int", nullable: false),
                    DescrizioneConto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TipoConto = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    Ordine = table.Column<int>(type: "int", nullable: false),
                    Livello = table.Column<int>(type: "int", nullable: false),
                    VoceRettifica = table.Column<bool>(type: "bit", nullable: false),
                    VoceRimanenza = table.Column<bool>(type: "bit", nullable: false),
                    GruppoPercentuale = table.Column<int>(type: "int", nullable: false),
                    CostiFD = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    CashFlowEconomico = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_StrutturaContoEconomico", x => x.IdCodiceConto);
                });

            migrationBuilder.CreateTable(
                name: "Pstree_AssociazioniCE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodicePdC = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdCodiceConto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_AssociazioniCE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pstree_AssociazioniCE_Pstree_ListaPianoDeiConti_CodicePdC",
                        column: x => x.CodicePdC,
                        principalTable: "Pstree_ListaPianoDeiConti",
                        principalColumn: "CodicePdC",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pstree_AssociazioniCE_Pstree_StrutturaContoEconomico_IdCodiceConto",
                        column: x => x.IdCodiceConto,
                        principalTable: "Pstree_StrutturaContoEconomico",
                        principalColumn: "IdCodiceConto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pstree_ListaFamiglie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CodiceFamiglia = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NomeFamiglia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescrizioneFamiglia = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IdCodiceConto = table.Column<int>(type: "int", nullable: false),
                    IdFamigliaPadre = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_ListaFamiglie", x => x.Id);
                    table.UniqueConstraint("AK_Pstree_ListaFamiglie_CodiceFamiglia", x => x.CodiceFamiglia);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaFamiglie_Pstree_ListaFamiglie_IdFamigliaPadre",
                        column: x => x.IdFamigliaPadre,
                        principalTable: "Pstree_ListaFamiglie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaFamiglie_Pstree_StrutturaContoEconomico_IdCodiceConto",
                        column: x => x.IdCodiceConto,
                        principalTable: "Pstree_StrutturaContoEconomico",
                        principalColumn: "IdCodiceConto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pstree_ListaRettifiche",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCodiceConto = table.Column<int>(type: "int", nullable: false),
                    Dare = table.Column<decimal>(type: "money", nullable: false),
                    Avere = table.Column<decimal>(type: "money", nullable: false),
                    Mese = table.Column<int>(type: "int", nullable: false),
                    Anno = table.Column<int>(type: "int", nullable: false),
                    IdFamiglia = table.Column<int>(type: "int", nullable: false),
                    IdSede = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_ListaRettifiche", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaRettifiche_Pstree_ListaFamiglie_IdFamiglia",
                        column: x => x.IdFamiglia,
                        principalTable: "Pstree_ListaFamiglie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaRettifiche_Pstree_ListaSedi_IdSede",
                        column: x => x.IdSede,
                        principalTable: "Pstree_ListaSedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaRettifiche_Pstree_StrutturaContoEconomico_IdCodiceConto",
                        column: x => x.IdCodiceConto,
                        principalTable: "Pstree_StrutturaContoEconomico",
                        principalColumn: "IdCodiceConto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pstree_ListaRimanenze",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Valore = table.Column<double>(type: "float", nullable: false),
                    Mese = table.Column<int>(type: "int", nullable: false),
                    Anno = table.Column<int>(type: "int", nullable: false),
                    IdFamiglia = table.Column<int>(type: "int", nullable: false),
                    IdSede = table.Column<int>(type: "int", nullable: false),
                    RettificaValore = table.Column<double>(type: "float", nullable: false),
                    NoteRettifica = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_ListaRimanenze", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaRimanenze_Pstree_ListaFamiglie_IdFamiglia",
                        column: x => x.IdFamiglia,
                        principalTable: "Pstree_ListaFamiglie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaRimanenze_Pstree_ListaSedi_IdSede",
                        column: x => x.IdSede,
                        principalTable: "Pstree_ListaSedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pstree_ListaSaldi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodicePdC = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Dare = table.Column<decimal>(type: "money", nullable: true),
                    Avere = table.Column<decimal>(type: "money", nullable: true),
                    Mese = table.Column<int>(type: "int", nullable: true),
                    Anno = table.Column<int>(type: "int", nullable: true),
                    IdFamiglia = table.Column<int>(type: "int", nullable: true),
                    IdSede = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_ListaSaldi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaSaldi_Pstree_ListaFamiglie_IdFamiglia",
                        column: x => x.IdFamiglia,
                        principalTable: "Pstree_ListaFamiglie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaSaldi_Pstree_ListaPianoDeiConti_CodicePdC",
                        column: x => x.CodicePdC,
                        principalTable: "Pstree_ListaPianoDeiConti",
                        principalColumn: "CodicePdC",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pstree_ListaSaldi_Pstree_ListaSedi_IdSede",
                        column: x => x.IdSede,
                        principalTable: "Pstree_ListaSedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pstree_PercentualiFamiglie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Percentuale = table.Column<double>(type: "float", nullable: false),
                    Mese = table.Column<int>(type: "int", nullable: false),
                    Anno = table.Column<int>(type: "int", nullable: false),
                    IdFamiglia = table.Column<int>(type: "int", nullable: false),
                    IdSede = table.Column<int>(type: "int", nullable: false),
                    IdCodiceConto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_PercentualiFamiglie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pstree_PercentualiFamiglie_Pstree_ListaFamiglie_IdFamiglia",
                        column: x => x.IdFamiglia,
                        principalTable: "Pstree_ListaFamiglie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pstree_PercentualiFamiglie_Pstree_ListaSedi_IdSede",
                        column: x => x.IdSede,
                        principalTable: "Pstree_ListaSedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pstree_PercentualiFamiglie_Pstree_StrutturaContoEconomico_IdCodiceConto",
                        column: x => x.IdCodiceConto,
                        principalTable: "Pstree_StrutturaContoEconomico",
                        principalColumn: "IdCodiceConto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pstree_SottoGruppi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodiceFamiglia = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CodiceGruppo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NomeSottoGruppo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescrizioneSottoGruppo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pstree_SottoGruppi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pstree_SottoGruppi_Pstree_ListaFamiglie_CodiceFamiglia",
                        column: x => x.CodiceFamiglia,
                        principalTable: "Pstree_ListaFamiglie",
                        principalColumn: "CodiceFamiglia",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_AssociazioniCE_CodicePdC",
                table: "Pstree_AssociazioniCE",
                column: "CodicePdC");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_AssociazioniCE_IdCodiceConto",
                table: "Pstree_AssociazioniCE",
                column: "IdCodiceConto");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaFamiglie_CodiceFamiglia",
                table: "Pstree_ListaFamiglie",
                column: "CodiceFamiglia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaFamiglie_IdCodiceConto",
                table: "Pstree_ListaFamiglie",
                column: "IdCodiceConto");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaFamiglie_IdFamigliaPadre",
                table: "Pstree_ListaFamiglie",
                column: "IdFamigliaPadre");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaRettifiche_IdCodiceConto",
                table: "Pstree_ListaRettifiche",
                column: "IdCodiceConto");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaRettifiche_IdFamiglia",
                table: "Pstree_ListaRettifiche",
                column: "IdFamiglia");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaRettifiche_IdSede",
                table: "Pstree_ListaRettifiche",
                column: "IdSede");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaRimanenze_IdFamiglia",
                table: "Pstree_ListaRimanenze",
                column: "IdFamiglia");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaRimanenze_IdSede",
                table: "Pstree_ListaRimanenze",
                column: "IdSede");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaSaldi_CodicePdC",
                table: "Pstree_ListaSaldi",
                column: "CodicePdC");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaSaldi_IdFamiglia",
                table: "Pstree_ListaSaldi",
                column: "IdFamiglia");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_ListaSaldi_IdSede",
                table: "Pstree_ListaSaldi",
                column: "IdSede");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_PercentualiFamiglie_IdCodiceConto",
                table: "Pstree_PercentualiFamiglie",
                column: "IdCodiceConto");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_PercentualiFamiglie_IdFamiglia",
                table: "Pstree_PercentualiFamiglie",
                column: "IdFamiglia");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_PercentualiFamiglie_IdSede",
                table: "Pstree_PercentualiFamiglie",
                column: "IdSede");

            migrationBuilder.CreateIndex(
                name: "IX_Pstree_SottoGruppi_CodiceFamiglia",
                table: "Pstree_SottoGruppi",
                column: "CodiceFamiglia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pstree_AssociazioniCE");

            migrationBuilder.DropTable(
                name: "Pstree_ListaRettifiche");

            migrationBuilder.DropTable(
                name: "Pstree_ListaRimanenze");

            migrationBuilder.DropTable(
                name: "Pstree_ListaSaldi");

            migrationBuilder.DropTable(
                name: "Pstree_PercentualiFamiglie");

            migrationBuilder.DropTable(
                name: "Pstree_SottoGruppi");

            migrationBuilder.DropTable(
                name: "Pstree_ListaPianoDeiConti");

            migrationBuilder.DropTable(
                name: "Pstree_ListaSedi");

            migrationBuilder.DropTable(
                name: "Pstree_ListaFamiglie");

            migrationBuilder.DropTable(
                name: "Pstree_StrutturaContoEconomico");
        }
    }
}
