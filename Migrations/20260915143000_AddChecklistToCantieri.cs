using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260915143000_AddChecklistToCantieri")]
    public class AddChecklistToCantieri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Cantieri', N'TipoTrasporto') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [TipoTrasporto] nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.Cantieri', N'PuliziaCantiere') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [PuliziaCantiere] bit NOT NULL CONSTRAINT [DF_Cantieri_PuliziaCantiere] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'CheckAffidabilitaCliente') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [CheckAffidabilitaCliente] bit NOT NULL CONSTRAINT [DF_Cantieri_CheckAffidabilita] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'Pagamento') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [Pagamento] nvarchar(100) NULL;
IF COL_LENGTH(N'dbo.Cantieri', N'ConfermaFirmata') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [ConfermaFirmata] bit NOT NULL CONSTRAINT [DF_Cantieri_ConfermaFirmata] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'TrasformatoInFavaro1') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [TrasformatoInFavaro1] bit NOT NULL CONSTRAINT [DF_Cantieri_TrasformatoFavaro1] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'MagazzinoN') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [MagazzinoN] nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.Cantieri', N'AccreditoAcconto') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [AccreditoAcconto] bit NOT NULL CONSTRAINT [DF_Cantieri_AccreditoAcconto] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'AnagraficaSdiPec') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [AnagraficaSdiPec] bit NOT NULL CONSTRAINT [DF_Cantieri_AnagraficaSdiPec] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'Banca') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [Banca] nvarchar(100) NULL;
IF COL_LENGTH(N'dbo.Cantieri', N'AliquotaIva') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [AliquotaIva] decimal(5,2) NULL;
IF COL_LENGTH(N'dbo.Cantieri', N'DocAgevolazioneIva') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [DocAgevolazioneIva] bit NOT NULL CONSTRAINT [DF_Cantieri_DocAgevolazioneIva] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'ContrattoPosatoreFirmato') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [ContrattoPosatoreFirmato] bit NOT NULL CONSTRAINT [DF_Cantieri_ContrattoPosatore] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'Psc') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [Psc] bit NOT NULL CONSTRAINT [DF_Cantieri_Psc] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'PosFavaro1') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [PosFavaro1] bit NOT NULL CONSTRAINT [DF_Cantieri_PosFavaro1] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'FinePosaFirmato') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [FinePosaFirmato] bit NOT NULL CONSTRAINT [DF_Cantieri_FinePosaFirmato] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'FinePosaData') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [FinePosaData] datetime2 NULL;
IF COL_LENGTH(N'dbo.Cantieri', N'ContabilitaCantiere') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [ContabilitaCantiere] bit NOT NULL CONSTRAINT [DF_Cantieri_Contabilita] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'FatturaSaldoAttivo') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [FatturaSaldoAttivo] bit NOT NULL CONSTRAINT [DF_Cantieri_FatturaAttivo] DEFAULT 0;
IF COL_LENGTH(N'dbo.Cantieri', N'FatturaSaldoPassivo') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [FatturaSaldoPassivo] bit NOT NULL CONSTRAINT [DF_Cantieri_FatturaPassivo] DEFAULT 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [TipoTrasporto];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [PuliziaCantiere];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [CheckAffidabilitaCliente];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [Pagamento];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [ConfermaFirmata];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [TrasformatoInFavaro1];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [MagazzinoN];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [AccreditoAcconto];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [AnagraficaSdiPec];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [Banca];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [AliquotaIva];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [DocAgevolazioneIva];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [ContrattoPosatoreFirmato];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [Psc];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [PosFavaro1];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [FinePosaFirmato];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [FinePosaData];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [ContabilitaCantiere];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [FatturaSaldoAttivo];
ALTER TABLE [dbo].[Cantieri] DROP COLUMN IF EXISTS [FatturaSaldoPassivo];
");
        }
    }
}
