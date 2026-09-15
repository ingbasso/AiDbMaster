/* ================================================================
   Crea le tabelle del modulo Gestione Cantieri.
   Script IDEMPOTENTE: si puo' eseguire piu' volte.

   Tabelle:
     - Cantieri
     - CantiereOrdini

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

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
        [TipoTrasporto]        nvarchar(50)   NULL,
        [PuliziaCantiere]      bit            NOT NULL CONSTRAINT [DF_Cantieri_PuliziaCantiere] DEFAULT 0,
        [CheckAffidabilitaCliente] bit        NOT NULL CONSTRAINT [DF_Cantieri_CheckAffidabilita] DEFAULT 0,
        [Pagamento]            nvarchar(100)  NULL,
        [ConfermaFirmata]      bit            NOT NULL CONSTRAINT [DF_Cantieri_ConfermaFirmata] DEFAULT 0,
        [TrasformatoInFavaro1] bit            NOT NULL CONSTRAINT [DF_Cantieri_TrasformatoFavaro1] DEFAULT 0,
        [MagazzinoN]           nvarchar(20)   NULL,
        [AccreditoAcconto]     bit            NOT NULL CONSTRAINT [DF_Cantieri_AccreditoAcconto] DEFAULT 0,
        [AnagraficaSdiPec]     bit            NOT NULL CONSTRAINT [DF_Cantieri_AnagraficaSdiPec] DEFAULT 0,
        [Banca]                nvarchar(100)  NULL,
        [AliquotaIva]          decimal(5,2)   NULL,
        [DocAgevolazioneIva]   bit            NOT NULL CONSTRAINT [DF_Cantieri_DocAgevolazioneIva] DEFAULT 0,
        [ContrattoPosatoreFirmato] bit        NOT NULL CONSTRAINT [DF_Cantieri_ContrattoPosatore] DEFAULT 0,
        [Psc]                  bit            NOT NULL CONSTRAINT [DF_Cantieri_Psc] DEFAULT 0,
        [PosFavaro1]           bit            NOT NULL CONSTRAINT [DF_Cantieri_PosFavaro1] DEFAULT 0,
        [FinePosaFirmato]      bit            NOT NULL CONSTRAINT [DF_Cantieri_FinePosaFirmato] DEFAULT 0,
        [FinePosaData]         datetime2      NULL,
        [ContabilitaCantiere]  bit            NOT NULL CONSTRAINT [DF_Cantieri_Contabilita] DEFAULT 0,
        [FatturaSaldoAttivo]   bit            NOT NULL CONSTRAINT [DF_Cantieri_FatturaAttivo] DEFAULT 0,
        [FatturaSaldoPassivo]  bit            NOT NULL CONSTRAINT [DF_Cantieri_FatturaPassivo] DEFAULT 0,
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
GO

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
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260915122000_CreateTabelleCantieri')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915122000_CreateTabelleCantieri', N'8.0.0');
GO
