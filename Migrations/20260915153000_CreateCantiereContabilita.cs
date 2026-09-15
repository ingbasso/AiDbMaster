using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260915153000_CreateCantiereContabilita")]
    public class CreateCantiereContabilita : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CantiereContabilita', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CantiereContabilita] (
        [ID]                 int             IDENTITY(1,1) NOT NULL,
        [CantiereId]         int             NOT NULL,
        [Tipo]               int             NOT NULL CONSTRAINT [DF_CantiereContabilita_Tipo] DEFAULT 0,
        [AliquotaIva]        decimal(5,2)    NOT NULL CONSTRAINT [DF_CantiereContabilita_Iva] DEFAULT 22,
        [ImponibileAcconto]  decimal(18,2)   NOT NULL CONSTRAINT [DF_CantiereContabilita_Acconto] DEFAULT 0,
        [RiferimentoOrdine]  nvarchar(150)   NULL,
        [Note]               nvarchar(max)   NULL,
        [DataUltimaModifica] datetime2       NULL,
        CONSTRAINT [PK_CantiereContabilita] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_CantiereContabilita_Cantieri] FOREIGN KEY ([CantiereId])
            REFERENCES [dbo].[Cantieri] ([ID]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_CantiereContabilita_Cantiere_Tipo]
        ON [dbo].[CantiereContabilita] ([CantiereId], [Tipo]);
END

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CantiereContabilitaRighe] (
        [ID]                 int             IDENTITY(1,1) NOT NULL,
        [ContabilitaId]      int             NOT NULL,
        [Ordine]             int             NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_Ordine] DEFAULT 0,
        [CodiceArticolo]     nvarchar(50)    NULL,
        [Descrizione]        nvarchar(255)   NOT NULL,
        [UnitaMisura]        nvarchar(10)    NULL,
        [Quantita]           decimal(18,4)   NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_Qty] DEFAULT 0,
        [PrezzoCliente]      decimal(18,4)   NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_Prz] DEFAULT 0,
        [CostoMateriale]     decimal(18,4)   NULL,
        [CostoPietrisco]     decimal(18,4)   NULL,
        [CostoSabbia]        decimal(18,4)   NULL,
        [CostoGeotessuto]    decimal(18,4)   NULL,
        [PrezzoPosaVendita]  decimal(18,4)   NULL,
        [PrezzoPosaAcquisto] decimal(18,4)   NULL,
        [Note]               nvarchar(200)   NULL,
        CONSTRAINT [PK_CantiereContabilitaRighe] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_CantiereContabilitaRighe_Testata] FOREIGN KEY ([ContabilitaId])
            REFERENCES [dbo].[CantiereContabilita] ([ID]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_CantiereContabilitaRighe_Contabilita_Ordine]
        ON [dbo].[CantiereContabilitaRighe] ([ContabilitaId], [Ordine]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
    DROP TABLE [dbo].[CantiereContabilitaRighe];
IF OBJECT_ID(N'dbo.CantiereContabilita', N'U') IS NOT NULL
    DROP TABLE [dbo].[CantiereContabilita];
");
        }
    }
}
