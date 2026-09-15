using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260915122000_CreateTabelleCantieri")]
    public class CreateTabelleCantieri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Cantieri', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Cantieri] (
        [ID]                   int            IDENTITY(1,1) NOT NULL,
        [Codice]               nvarchar(20)   NOT NULL,
        [Nome]                 nvarchar(150)  NOT NULL,
        [CodiceCliente]        int            NOT NULL,
        [CodiceDestinazione]   int            NULL,
        [Indirizzo]            nvarchar(70)   NULL,
        [Cap]                  nvarchar(10)   NULL,
        [Localita]             nvarchar(50)   NULL,
        [Provincia]            nvarchar(2)    NULL,
        [Telefono]             nvarchar(18)   NULL,
        [Referente]            nvarchar(100)  NULL,
        [ImpresaPosa]          nvarchar(150)  NULL,
        [DataInizioPrevista]   datetime2      NULL,
        [DataFinePrevista]     datetime2      NULL,
        [Stato]                int            NOT NULL,
        [Note]                 nvarchar(max)  NULL,
        [PercorsoDocumenti]    nvarchar(500)  NULL,
        [DataCreazione]        datetime2      NOT NULL,
        [DataUltimaModifica]   datetime2      NULL,
        [UtenteCreazione]      nvarchar(100)  NULL,
        [UtenteModifica]       nvarchar(100)  NULL,
        CONSTRAINT [PK_Cantieri] PRIMARY KEY ([ID])
    );

    CREATE UNIQUE INDEX [IX_Cantieri_Codice] ON [dbo].[Cantieri] ([Codice]);
    CREATE INDEX [IX_Cantieri_CodiceCliente] ON [dbo].[Cantieri] ([CodiceCliente]);
    CREATE INDEX [IX_Cantieri_Stato] ON [dbo].[Cantieri] ([Stato]);
END

IF OBJECT_ID(N'dbo.CantiereOrdini', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CantiereOrdini] (
        [ID]               int      IDENTITY(1,1) NOT NULL,
        [CantiereId]       int      NOT NULL,
        [OrdineTestataId]  int      NOT NULL,
        [DataCollegamento] datetime2 NOT NULL,
        CONSTRAINT [PK_CantiereOrdini] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_CantiereOrdini_Cantieri] FOREIGN KEY ([CantiereId])
            REFERENCES [dbo].[Cantieri] ([ID]) ON DELETE CASCADE,
        CONSTRAINT [FK_CantiereOrdini_OrdiniTestate] FOREIGN KEY ([OrdineTestataId])
            REFERENCES [dbo].[OrdiniTestate] ([ID]) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX [IX_CantiereOrdini_Cantiere_Ordine]
        ON [dbo].[CantiereOrdini] ([CantiereId], [OrdineTestataId]);
    CREATE UNIQUE INDEX [IX_CantiereOrdini_OrdineTestataId]
        ON [dbo].[CantiereOrdini] ([OrdineTestataId]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CantiereOrdini', N'U') IS NOT NULL
    DROP TABLE [dbo].[CantiereOrdini];

IF OBJECT_ID(N'dbo.Cantieri', N'U') IS NOT NULL
    DROP TABLE [dbo].[Cantieri];
");
        }
    }
}
