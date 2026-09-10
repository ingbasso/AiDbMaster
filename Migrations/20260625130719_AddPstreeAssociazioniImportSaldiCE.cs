using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddPstreeAssociazioniImportSaldiCE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NB: la tabella puo' gia' esistere (creata manualmente in sviluppo).
            // Lo Up e' reso idempotente: crea tabella/indici/FK solo se mancanti,
            // cosi' funziona sia in sviluppo sia in produzione senza errori.

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Pstree_AssociazioniImportSaldiCE]', N'U') IS NULL
BEGIN
    CREATE TABLE [Pstree_AssociazioniImportSaldiCE] (
        [Id]            int IDENTITY(1,1) NOT NULL,
        [CodicePdC]     nvarchar(20) NOT NULL,
        [IdCodiceConto] int          NOT NULL,
        [IdSede]        int          NOT NULL,
        [Anno]          int          NOT NULL,
        [Mese]          int          NOT NULL,
        [Percentuale]   float        NOT NULL,
        CONSTRAINT [PK_Pstree_AssociazioniImportSaldiCE] PRIMARY KEY ([Id])
    );
END;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pstree_AssociazioniImportSaldiCE_IdCodiceConto' AND object_id = OBJECT_ID(N'Pstree_AssociazioniImportSaldiCE'))
    CREATE INDEX [IX_Pstree_AssociazioniImportSaldiCE_IdCodiceConto] ON [Pstree_AssociazioniImportSaldiCE] ([IdCodiceConto]);");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pstree_AssociazioniImportSaldiCE_IdSede' AND object_id = OBJECT_ID(N'Pstree_AssociazioniImportSaldiCE'))
    CREATE INDEX [IX_Pstree_AssociazioniImportSaldiCE_IdSede] ON [Pstree_AssociazioniImportSaldiCE] ([IdSede]);");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pstree_AssociazioniImportSaldiCE_Ripartizione' AND object_id = OBJECT_ID(N'Pstree_AssociazioniImportSaldiCE'))
    CREATE UNIQUE INDEX [IX_Pstree_AssociazioniImportSaldiCE_Ripartizione] ON [Pstree_AssociazioniImportSaldiCE] ([CodicePdC], [IdCodiceConto], [IdSede], [Anno], [Mese]);");

            // Alcune righe storiche hanno CodicePdC con CR/LF (es. "63010001\r\n").
            // SQL Server non le considera uguali al codice pulito del Piano dei Conti,
            // quindi la FK fallirebbe. Si puliscono o, se sono duplicati, si eliminano.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Pstree_AssociazioniImportSaldiCE]', N'U') IS NOT NULL
BEGIN
    ;WITH Puliti AS (
        SELECT
            Id,
            LTRIM(RTRIM(REPLACE(REPLACE(CodicePdC, CHAR(13), ''), CHAR(10), ''))) AS CodicePulito,
            IdCodiceConto,
            IdSede,
            Anno,
            Mese
        FROM [Pstree_AssociazioniImportSaldiCE]
    )
    DELETE a
    FROM [Pstree_AssociazioniImportSaldiCE] a
    INNER JOIN Puliti p ON a.Id = p.Id
    WHERE a.CodicePdC <> p.CodicePulito
      AND EXISTS (
          SELECT 1
          FROM [Pstree_AssociazioniImportSaldiCE] b
          WHERE b.CodicePdC = p.CodicePulito
            AND b.IdCodiceConto = p.IdCodiceConto
            AND b.IdSede = p.IdSede
            AND b.Anno = p.Anno
            AND b.Mese = p.Mese
            AND b.Id <> p.Id
      );

    UPDATE [Pstree_AssociazioniImportSaldiCE]
    SET CodicePdC = LTRIM(RTRIM(REPLACE(REPLACE(CodicePdC, CHAR(13), ''), CHAR(10), '')))
    WHERE CodicePdC <> LTRIM(RTRIM(REPLACE(REPLACE(CodicePdC, CHAR(13), ''), CHAR(10), '')));

    DELETE a
    FROM [Pstree_AssociazioniImportSaldiCE] a
    WHERE NOT EXISTS (
        SELECT 1
        FROM [Pstree_ListaPianoDeiConti] p
        WHERE p.CodicePdC = a.CodicePdC
    );
END;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Pstree_AssociazioniImportSaldiCE_Pstree_ListaPianoDeiConti_CodicePdC')
    ALTER TABLE [Pstree_AssociazioniImportSaldiCE] ADD CONSTRAINT [FK_Pstree_AssociazioniImportSaldiCE_Pstree_ListaPianoDeiConti_CodicePdC]
        FOREIGN KEY ([CodicePdC]) REFERENCES [Pstree_ListaPianoDeiConti] ([CodicePdC]);");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Pstree_AssociazioniImportSaldiCE_Pstree_ListaSedi_IdSede')
    ALTER TABLE [Pstree_AssociazioniImportSaldiCE] ADD CONSTRAINT [FK_Pstree_AssociazioniImportSaldiCE_Pstree_ListaSedi_IdSede]
        FOREIGN KEY ([IdSede]) REFERENCES [Pstree_ListaSedi] ([Id]);");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Pstree_AssociazioniImportSaldiCE_Pstree_StrutturaContoEconomico_IdCodiceConto')
    ALTER TABLE [Pstree_AssociazioniImportSaldiCE] ADD CONSTRAINT [FK_Pstree_AssociazioniImportSaldiCE_Pstree_StrutturaContoEconomico_IdCodiceConto]
        FOREIGN KEY ([IdCodiceConto]) REFERENCES [Pstree_StrutturaContoEconomico] ([IdCodiceConto]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pstree_AssociazioniImportSaldiCE");
        }
    }
}
