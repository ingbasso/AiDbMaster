using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddCampiArticoli_Marca_Famiglia_ClasseProvvigione_Outlet_FuoriProduzione : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Le colonne sono già state aggiunte manualmente nel database.
            // Usiamo IF NOT EXISTS per sicurezza, così funziona sia su DB nuovi che esistenti.

            // === AGGIUNTA COLONNE (se non esistono già) ===
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaArticoli') AND name = 'Marca')
                    ALTER TABLE [dbo].[AnagraficaArticoli] ADD [Marca] [smallint] NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaArticoli') AND name = 'Famiglia')
                    ALTER TABLE [dbo].[AnagraficaArticoli] ADD [Famiglia] [varchar](4) NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaArticoli') AND name = 'ClasseProvvigione')
                    ALTER TABLE [dbo].[AnagraficaArticoli] ADD [ClasseProvvigione] [smallint] NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaArticoli') AND name = 'Outlet')
                    ALTER TABLE [dbo].[AnagraficaArticoli] ADD [Outlet] [varchar](1) NOT NULL CONSTRAINT [DF_AnagraficaArticoli_Outlet] DEFAULT ('N');
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaArticoli') AND name = 'FuoriProduzione')
                    ALTER TABLE [dbo].[AnagraficaArticoli] ADD [FuoriProduzione] [varchar](1) NOT NULL CONSTRAINT [DF_AnagraficaArticoli_FuoriProduzione] DEFAULT ('N');
            ");

            // === VINCOLI UNIVOCI (alternate keys per le FK) ===
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'AK_TabellaMarche_CodiceMarca')
                    ALTER TABLE [dbo].[TabellaMarche] ADD CONSTRAINT [AK_TabellaMarche_CodiceMarca] UNIQUE ([CodiceMarca]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'AK_TabellaFamiglie_CodiceFamiglia')
                    ALTER TABLE [dbo].[TabellaFamiglie] ADD CONSTRAINT [AK_TabellaFamiglie_CodiceFamiglia] UNIQUE ([CodiceFamiglia]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'AK_TabellaClassiProvvigioni_CodiceClasse')
                    ALTER TABLE [dbo].[TabellaClassiProvvigioni] ADD CONSTRAINT [AK_TabellaClassiProvvigioni_CodiceClasse] UNIQUE ([CodiceClasse]);
            ");

            // === INDICI sulle colonne FK ===
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AnagraficaArticoli_Marca' AND object_id = OBJECT_ID('AnagraficaArticoli'))
                    CREATE INDEX [IX_AnagraficaArticoli_Marca] ON [dbo].[AnagraficaArticoli] ([Marca]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AnagraficaArticoli_Famiglia' AND object_id = OBJECT_ID('AnagraficaArticoli'))
                    CREATE INDEX [IX_AnagraficaArticoli_Famiglia] ON [dbo].[AnagraficaArticoli] ([Famiglia]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AnagraficaArticoli_ClasseProvvigione' AND object_id = OBJECT_ID('AnagraficaArticoli'))
                    CREATE INDEX [IX_AnagraficaArticoli_ClasseProvvigione] ON [dbo].[AnagraficaArticoli] ([ClasseProvvigione]);
            ");

            // === ASSICURARSI CHE LE COLONNE FK SIANO NULLABLE ===
            // Nel database fisico potrebbero essere state create come NOT NULL.
            // Le rendiamo nullable come richiesto.
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaArticoli') AND name = 'Marca' AND is_nullable = 0)
                    ALTER TABLE [dbo].[AnagraficaArticoli] ALTER COLUMN [Marca] [smallint] NULL;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaArticoli') AND name = 'Famiglia' AND is_nullable = 0)
                    ALTER TABLE [dbo].[AnagraficaArticoli] ALTER COLUMN [Famiglia] [varchar](4) NULL;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaArticoli') AND name = 'ClasseProvvigione' AND is_nullable = 0)
                    ALTER TABLE [dbo].[AnagraficaArticoli] ALTER COLUMN [ClasseProvvigione] [smallint] NULL;
            ");

            // === PULIZIA DATI ORFANI ===
            // Imposta a NULL i valori 0 (default NOT NULL) e quelli che non esistono nelle tabelle FK
            migrationBuilder.Sql(@"
                UPDATE [dbo].[AnagraficaArticoli]
                SET [Marca] = NULL
                WHERE [Marca] = 0
                   OR ([Marca] IS NOT NULL AND [Marca] NOT IN (SELECT [CodiceMarca] FROM [dbo].[TabellaMarche]));
            ");

            migrationBuilder.Sql(@"
                UPDATE [dbo].[AnagraficaArticoli]
                SET [Famiglia] = NULL
                WHERE [Famiglia] = ''
                   OR ([Famiglia] IS NOT NULL AND [Famiglia] NOT IN (SELECT [CodiceFamiglia] FROM [dbo].[TabellaFamiglie]));
            ");

            migrationBuilder.Sql(@"
                UPDATE [dbo].[AnagraficaArticoli]
                SET [ClasseProvvigione] = NULL
                WHERE [ClasseProvvigione] = 0
                   OR ([ClasseProvvigione] IS NOT NULL AND [ClasseProvvigione] NOT IN (SELECT [CodiceClasse] FROM [dbo].[TabellaClassiProvvigioni]));
            ");

            // === FOREIGN KEYS ===
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AnagraficaArticoli_TabellaMarche_Marca')
                    ALTER TABLE [dbo].[AnagraficaArticoli] ADD CONSTRAINT [FK_AnagraficaArticoli_TabellaMarche_Marca]
                    FOREIGN KEY ([Marca]) REFERENCES [dbo].[TabellaMarche] ([CodiceMarca]) ON DELETE NO ACTION;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AnagraficaArticoli_TabellaFamiglie_Famiglia')
                    ALTER TABLE [dbo].[AnagraficaArticoli] ADD CONSTRAINT [FK_AnagraficaArticoli_TabellaFamiglie_Famiglia]
                    FOREIGN KEY ([Famiglia]) REFERENCES [dbo].[TabellaFamiglie] ([CodiceFamiglia]) ON DELETE NO ACTION;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AnagraficaArticoli_TabellaClassiProvvigioni_ClasseProvvigione')
                    ALTER TABLE [dbo].[AnagraficaArticoli] ADD CONSTRAINT [FK_AnagraficaArticoli_TabellaClassiProvvigioni_ClasseProvvigione]
                    FOREIGN KEY ([ClasseProvvigione]) REFERENCES [dbo].[TabellaClassiProvvigioni] ([CodiceClasse]) ON DELETE NO ACTION;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnagraficaArticoli_TabellaClassiProvvigioni_ClasseProvvigione",
                table: "AnagraficaArticoli");

            migrationBuilder.DropForeignKey(
                name: "FK_AnagraficaArticoli_TabellaFamiglie_Famiglia",
                table: "AnagraficaArticoli");

            migrationBuilder.DropForeignKey(
                name: "FK_AnagraficaArticoli_TabellaMarche_Marca",
                table: "AnagraficaArticoli");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_TabellaMarche_CodiceMarca",
                table: "TabellaMarche");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_TabellaFamiglie_CodiceFamiglia",
                table: "TabellaFamiglie");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_TabellaClassiProvvigioni_CodiceClasse",
                table: "TabellaClassiProvvigioni");

            migrationBuilder.DropIndex(
                name: "IX_AnagraficaArticoli_ClasseProvvigione",
                table: "AnagraficaArticoli");

            migrationBuilder.DropIndex(
                name: "IX_AnagraficaArticoli_Famiglia",
                table: "AnagraficaArticoli");

            migrationBuilder.DropIndex(
                name: "IX_AnagraficaArticoli_Marca",
                table: "AnagraficaArticoli");

            migrationBuilder.DropColumn(
                name: "ClasseProvvigione",
                table: "AnagraficaArticoli");

            migrationBuilder.DropColumn(
                name: "Famiglia",
                table: "AnagraficaArticoli");

            migrationBuilder.DropColumn(
                name: "FuoriProduzione",
                table: "AnagraficaArticoli");

            migrationBuilder.DropColumn(
                name: "Marca",
                table: "AnagraficaArticoli");

            migrationBuilder.DropColumn(
                name: "Outlet",
                table: "AnagraficaArticoli");
        }
    }
}
