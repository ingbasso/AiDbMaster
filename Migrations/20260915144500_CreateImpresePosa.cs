using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260915144500_CreateImpresePosa")]
    public class CreateImpresePosa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.ImpresePosa', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ImpresePosa] (
        [ID]       int            IDENTITY(1,1) NOT NULL,
        [Nome]     nvarchar(150)  NOT NULL,
        [Telefono] nvarchar(18)   NULL,
        [Note]     nvarchar(max)  NULL,
        [Attivo]   bit            NOT NULL CONSTRAINT [DF_ImpresePosa_Attivo] DEFAULT 1,
        CONSTRAINT [PK_ImpresePosa] PRIMARY KEY ([ID])
    );
    CREATE UNIQUE INDEX [IX_ImpresePosa_Nome] ON [dbo].[ImpresePosa] ([Nome]);
END

MERGE [dbo].[ImpresePosa] AS t
USING (VALUES
    (N'posaedil di Trovato Maurizio'),
    (N'Loresa Pavimentazioni'),
    (N'Nechiforov Valeriu'),
    (N'Tecnoblock Srls'),
    (N'Miftari Ajet'),
    (N'Haxhiu Adrian'),
    (N'Pavimart Srls'),
    (N'Fratelli Coltro e Pistolato'),
    (N'Elettro PT sas di Tometto Alessandro')
) AS s(Nome)
ON t.Nome = s.Nome
WHEN NOT MATCHED THEN
    INSERT ([Nome], [Attivo]) VALUES (s.Nome, 1);

IF COL_LENGTH(N'dbo.Cantieri', N'ImpresaPosaId') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [ImpresaPosaId] int NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Cantieri_ImpresePosa')
AND COL_LENGTH(N'dbo.Cantieri', N'ImpresaPosaId') IS NOT NULL
    ALTER TABLE [dbo].[Cantieri]
        ADD CONSTRAINT [FK_Cantieri_ImpresePosa]
        FOREIGN KEY ([ImpresaPosaId]) REFERENCES [dbo].[ImpresePosa] ([ID]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Cantieri_ImpresePosa')
    ALTER TABLE [dbo].[Cantieri] DROP CONSTRAINT [FK_Cantieri_ImpresePosa];
IF COL_LENGTH(N'dbo.Cantieri', N'ImpresaPosaId') IS NOT NULL
    ALTER TABLE [dbo].[Cantieri] DROP COLUMN [ImpresaPosaId];
IF OBJECT_ID(N'dbo.ImpresePosa', N'U') IS NOT NULL
    DROP TABLE [dbo].[ImpresePosa];
");
        }
    }
}
