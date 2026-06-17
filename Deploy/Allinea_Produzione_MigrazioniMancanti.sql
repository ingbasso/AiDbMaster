/* ================================================================
   Allineamento DB di PRODUZIONE (server SVRGEST, database AIDBMASTER)
   Applica le 3 migrazioni risultate mancanti:

     1) 20260603151342_AddGiorniAsciugaturaExtraToAnagraficaArticoli
     2) 20260615102724_AddViaggioConsegnaDestinazioni
     3) 20260615161122_AddPrezzoVenditaToDestinazioni

   Lo script e' IDEMPOTENTE: ogni blocco viene eseguito solo se la
   migrazione NON risulta gia' registrata in __EFMigrationsHistory.
   Si puo' quindi eseguire piu' volte senza rischi.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

/* ---------- Migrazione 1: AddGiorniAsciugaturaExtraToAnagraficaArticoli ---------- */
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260603151342_AddGiorniAsciugaturaExtraToAnagraficaArticoli')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE [Name] = N'GiorniAsciugaturaExtra' AND [Object_ID] = OBJECT_ID(N'AnagraficaArticoli'))
        ALTER TABLE [AnagraficaArticoli] ADD [GiorniAsciugaturaExtra] int NOT NULL CONSTRAINT [DF_AnagraficaArticoli_GiorniAsciugaturaExtra] DEFAULT 0;

    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [name] = N'DurataDelleScorte')
        CREATE TABLE [DurataDelleScorte] (
            [ID]                     int IDENTITY(1,1) NOT NULL,
            [CodMarca]               smallint        NOT NULL,
            [DescrizioneMarca]       varchar(50)     NULL,
            [CodFamiglia]            varchar(4)      NOT NULL,
            [DescrFamiglia]          varchar(50)     NULL,
            [CodiceArticolo]         varchar(50)     NOT NULL,
            [Descrizione]            varchar(255)    NOT NULL,
            [UnitàMisura]            varchar(3)      NOT NULL,
            [Magazzino]              smallint        NULL,
            [DataUltimoScarico]      datetime2       NULL,
            [Esistenza]              decimal(27,9)   NULL,
            [Disponibilità]          decimal(27,9)   NULL,
            [ConsumoUltimomese]      decimal(27,9)   NULL,
            [ConsumoDueMesifa]       decimal(27,9)   NULL,
            [ConsumoTreMesifa]       decimal(27,9)   NULL,
            [ConsumoMedioPonderato]  decimal(27,9)   NULL,
            [DurataDelleScorte]      decimal(27,9)   NULL,
            CONSTRAINT [PK_DurataDelleScorte] PRIMARY KEY ([ID])
        );

    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [name] = N'LogEmailAutomatico')
        CREATE TABLE [LogEmailAutomatico] (
            [ID]                int IDENTITY(1,1) NOT NULL,
            [DataOra]           datetime2     NOT NULL,
            [Tipo]              varchar(20)   NOT NULL,
            [AnnoOrdine]        smallint      NULL,
            [SerieOrdine]       varchar(3)    NULL,
            [NumeroOrdine]      int           NULL,
            [RigaOrdine]        int           NULL,
            [CodiceCliente]     int           NULL,
            [RagioneSociale]    varchar(200)  NULL,
            [EmailDestinatario] varchar(200)  NULL,
            [Esito]             varchar(20)   NOT NULL,
            [Motivo]            varchar(500)  NULL,
            [Dettagli]          varchar(max)  NULL,
            CONSTRAINT [PK_LogEmailAutomatico] PRIMARY KEY ([ID])
        );

    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [name] = N'StoricoMaterialeLiberato')
        CREATE TABLE [StoricoMaterialeLiberato] (
            [ID]                  int IDENTITY(1,1) NOT NULL,
            [DataLiberazione]     datetime2     NOT NULL,
            [TipoOrdine]          varchar(1)    NOT NULL,
            [AnnoOrdine]          smallint      NOT NULL,
            [SerieOrdine]         varchar(3)    NOT NULL,
            [NumeroOrdine]        int           NOT NULL,
            [RigaOrdine]          int           NOT NULL,
            [CodiceCliente]       int           NOT NULL,
            [RagioneSociale]      varchar(50)   NOT NULL,
            [CodiceArticolo]      varchar(50)   NOT NULL,
            [DescrizioneArticolo] nvarchar(255) NULL,
            [DataConsegna]        datetime2     NOT NULL,
            [UnitaMisura]         varchar(3)    NULL,
            [Quantita]            decimal(27,9) NOT NULL CONSTRAINT [DF_StoricoMaterialeLiberato_Quantita] DEFAULT 0.0,
            [UnitaMisuraColli]    varchar(3)    NULL,
            [NumeroColli]         decimal(27,9) NOT NULL CONSTRAINT [DF_StoricoMaterialeLiberato_NumeroColli] DEFAULT 0.0,
            [UltimoAggiornamento] datetime2     NOT NULL CONSTRAINT [DF_StoricoMaterialeLiberato_UltimoAggiornamento] DEFAULT (GETDATE()),
            CONSTRAINT [PK_StoricoMaterialeLiberato] PRIMARY KEY ([ID])
        );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_DurataDelleScorte_CodFamiglia' AND [object_id] = OBJECT_ID(N'DurataDelleScorte'))
        CREATE INDEX [IX_DurataDelleScorte_CodFamiglia] ON [DurataDelleScorte] ([CodFamiglia]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_DurataDelleScorte_CodiceArticolo' AND [object_id] = OBJECT_ID(N'DurataDelleScorte'))
        CREATE INDEX [IX_DurataDelleScorte_CodiceArticolo] ON [DurataDelleScorte] ([CodiceArticolo]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_DurataDelleScorte_CodMarca' AND [object_id] = OBJECT_ID(N'DurataDelleScorte'))
        CREATE INDEX [IX_DurataDelleScorte_CodMarca] ON [DurataDelleScorte] ([CodMarca]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_StoricoMaterialeLiberato_CodiceArticolo' AND [object_id] = OBJECT_ID(N'StoricoMaterialeLiberato'))
        CREATE INDEX [IX_StoricoMaterialeLiberato_CodiceArticolo] ON [StoricoMaterialeLiberato] ([CodiceArticolo]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_StoricoMaterialeLiberato_DataLiberazione' AND [object_id] = OBJECT_ID(N'StoricoMaterialeLiberato'))
        CREATE INDEX [IX_StoricoMaterialeLiberato_DataLiberazione] ON [StoricoMaterialeLiberato] ([DataLiberazione]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260603151342_AddGiorniAsciugaturaExtraToAnagraficaArticoli', N'8.0.2');
END;
GO

/* ---------- Migrazione 2: AddViaggioConsegnaDestinazioni ---------- */
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260615102724_AddViaggioConsegnaDestinazioni')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [name] = N'ViaggioConsegnaDestinazioni')
        CREATE TABLE [ViaggioConsegnaDestinazioni] (
            [ID]                 int IDENTITY(1,1) NOT NULL,
            [ViaggioConsegnaId]  int           NOT NULL,
            [CodiceCliente]      int           NOT NULL,
            [CodiceDestinazione] int           NULL,
            [Gru]                bit           NOT NULL,
            [Trasbordo]          bit           NOT NULL,
            [OrdineConsegna]     int           NOT NULL,
            [Note]               nvarchar(max) NULL,
            CONSTRAINT [PK_ViaggioConsegnaDestinazioni] PRIMARY KEY ([ID]),
            CONSTRAINT [FK_ViaggioConsegnaDestinazioni_ViaggiConsegna_ViaggioConsegnaId]
                FOREIGN KEY ([ViaggioConsegnaId]) REFERENCES [ViaggiConsegna] ([ID]) ON DELETE CASCADE
        );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_ViaggioConsegnaDestinazioni_Viaggio_Cliente_Dest' AND [object_id] = OBJECT_ID(N'ViaggioConsegnaDestinazioni'))
        CREATE UNIQUE INDEX [IX_ViaggioConsegnaDestinazioni_Viaggio_Cliente_Dest]
            ON [ViaggioConsegnaDestinazioni] ([ViaggioConsegnaId], [CodiceCliente], [CodiceDestinazione])
            WHERE [CodiceDestinazione] IS NOT NULL;

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260615102724_AddViaggioConsegnaDestinazioni', N'8.0.2');
END;
GO

/* ---------- Migrazione 3: AddPrezzoVenditaToDestinazioni ---------- */
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260615161122_AddPrezzoVenditaToDestinazioni')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE [Name] = N'PrezzoVendita' AND [Object_ID] = OBJECT_ID(N'ViaggioConsegnaDestinazioni'))
        ALTER TABLE [ViaggioConsegnaDestinazioni] ADD [PrezzoVendita] decimal(18,2) NOT NULL CONSTRAINT [DF_ViaggioConsegnaDestinazioni_PrezzoVendita] DEFAULT 0.0;

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260615161122_AddPrezzoVenditaToDestinazioni', N'8.0.2');
END;
GO

PRINT 'Allineamento completato: 3 migrazioni applicate (o gia presenti).';
