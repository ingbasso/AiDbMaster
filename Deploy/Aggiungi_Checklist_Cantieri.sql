/* ================================================================
   Aggiunge alla tabella Cantieri i campi della scheda cliente A4
   (checklist avvio, posa, chiusura).
   Script IDEMPOTENTE: si puo' eseguire piu' volte.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

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
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260915143000_AddChecklistToCantieri')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915143000_AddChecklistToCantieri', N'8.0.0');
GO
