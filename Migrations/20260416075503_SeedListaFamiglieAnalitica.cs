using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class SeedListaFamiglieAnalitica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Disabilita temporaneamente il vincolo FK su IdCodiceConto
            migrationBuilder.Sql(
                "ALTER TABLE [Pstree_ListaFamiglie] NOCHECK CONSTRAINT [FK_Pstree_ListaFamiglie_Pstree_StrutturaContoEconomico_IdCodiceConto];");

            // Inserisce il record radice "Analitica" (Id=0, IdCodiceConto=0, IdFamigliaPadre=NULL)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Pstree_ListaFamiglie] WHERE [Id] = 0)
                BEGIN
                    INSERT INTO [Pstree_ListaFamiglie] ([Id], [CodiceFamiglia], [NomeFamiglia], [DescrizioneFamiglia], [IdCodiceConto], [IdFamigliaPadre])
                    VALUES (0, N'0', N'Analitica', N'Analitica', 0, NULL);
                END");

            // Riabilita il vincolo FK senza validare i dati esistenti (WITH NOCHECK)
            migrationBuilder.Sql(
                "ALTER TABLE [Pstree_ListaFamiglie] WITH NOCHECK CHECK CONSTRAINT [FK_Pstree_ListaFamiglie_Pstree_StrutturaContoEconomico_IdCodiceConto];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM [Pstree_ListaFamiglie] WHERE [Id] = 0;");
        }
    }
}
