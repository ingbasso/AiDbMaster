/* ================================================================
   Contabilità cantiere: crea le tabelle se mancano,
   altrimenti le allinea alla struttura padre/figlio.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   Se manca la tabella Cantieri, eseguire prima Crea_Tabelle_Cantieri.sql.
   ================================================================ */

IF OBJECT_ID(N'dbo.Cantieri', N'U') IS NULL
BEGIN
    RAISERROR(N'Manca la tabella Cantieri. Esegui prima Deploy\Crea_Tabelle_Cantieri.sql.', 16, 1);
    RETURN;
END
GO

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
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CantiereContabilitaRighe] (
        [ID]                      int             IDENTITY(1,1) NOT NULL,
        [ContabilitaId]           int             NOT NULL,
        [PadreId]                 int             NULL,
        [Ordine]                  int             NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_Ordine] DEFAULT 0,
        [CodiceArticolo]          nvarchar(50)    NULL,
        [Descrizione]             nvarchar(255)   NOT NULL,
        [UnitaMisura]             nvarchar(10)    NULL,
        [QuantitaPosata]          decimal(18,4)   NULL,
        [CostoMaterialeServizio]  decimal(18,4)   NULL,
        [PrezzoVenditaCliente]    decimal(18,4)   NULL,
        [Note]                    nvarchar(200)   NULL,
        CONSTRAINT [PK_CantiereContabilitaRighe] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_CantiereContabilitaRighe_Testata] FOREIGN KEY ([ContabilitaId])
            REFERENCES [dbo].[CantiereContabilita] ([ID]) ON DELETE CASCADE,
        CONSTRAINT [FK_CantiereContabilitaRighe_Padre] FOREIGN KEY ([PadreId])
            REFERENCES [dbo].[CantiereContabilitaRighe] ([ID])
    );

    CREATE INDEX [IX_CantiereContabilitaRighe_Contabilita_Ordine]
        ON [dbo].[CantiereContabilitaRighe] ([ContabilitaId], [Ordine]);
    CREATE INDEX [IX_CantiereContabilitaRighe_PadreId]
        ON [dbo].[CantiereContabilitaRighe] ([PadreId]);
END
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PadreId') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ADD [PadreId] int NULL;
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'QuantitaPosata') IS NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'Quantita') IS NOT NULL
    EXEC sp_rename N'dbo.CantiereContabilitaRighe.Quantita', N'QuantitaPosata', N'COLUMN';
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoMaterialeServizio') IS NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoMateriale') IS NOT NULL
    EXEC sp_rename N'dbo.CantiereContabilitaRighe.CostoMateriale', N'CostoMaterialeServizio', N'COLUMN';
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoVenditaCliente') IS NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoCliente') IS NOT NULL
    EXEC sp_rename N'dbo.CantiereContabilitaRighe.PrezzoCliente', N'PrezzoVenditaCliente', N'COLUMN';
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'QuantitaPosata') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ADD [QuantitaPosata] decimal(18,4) NULL;
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoMaterialeServizio') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ADD [CostoMaterialeServizio] decimal(18,4) NULL;
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoVenditaCliente') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ADD [PrezzoVenditaCliente] decimal(18,4) NULL;
GO

-- I campi numerici devono accettare vuoto (NULL): la tabella vecchia li aveva NOT NULL.
IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
BEGIN
    DECLARE @sqlDropDf nvarchar(max);

    SELECT @sqlDropDf = STRING_AGG(
        N'ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP CONSTRAINT [' + dc.name + N'];',
        CHAR(10))
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.CantiereContabilitaRighe')
      AND c.name IN (N'QuantitaPosata', N'CostoMaterialeServizio', N'PrezzoVenditaCliente');

    IF @sqlDropDf IS NOT NULL
        EXEC sp_executesql @sqlDropDf;

    IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'QuantitaPosata') IS NOT NULL
        ALTER TABLE [dbo].[CantiereContabilitaRighe] ALTER COLUMN [QuantitaPosata] decimal(18,4) NULL;

    IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoMaterialeServizio') IS NOT NULL
        ALTER TABLE [dbo].[CantiereContabilitaRighe] ALTER COLUMN [CostoMaterialeServizio] decimal(18,4) NULL;

    IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoVenditaCliente') IS NOT NULL
        ALTER TABLE [dbo].[CantiereContabilitaRighe] ALTER COLUMN [PrezzoVenditaCliente] decimal(18,4) NULL;
END
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoPietrisco') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [CostoPietrisco];
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoSabbia') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [CostoSabbia];
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoGeotessuto') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [CostoGeotessuto];
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoPosaVendita') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [PrezzoPosaVendita];
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoPosaAcquisto') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [PrezzoPosaAcquisto];
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_CantiereContabilitaRighe_Padre'
          AND parent_object_id = OBJECT_ID(N'dbo.CantiereContabilitaRighe')
   )
    ALTER TABLE [dbo].[CantiereContabilitaRighe] WITH CHECK
    ADD CONSTRAINT [FK_CantiereContabilitaRighe_Padre]
        FOREIGN KEY ([PadreId]) REFERENCES [dbo].[CantiereContabilitaRighe] ([ID]);
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_CantiereContabilitaRighe_PadreId'
          AND object_id = OBJECT_ID(N'dbo.CantiereContabilitaRighe')
   )
    CREATE INDEX [IX_CantiereContabilitaRighe_PadreId]
        ON [dbo].[CantiereContabilitaRighe] ([PadreId]);
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260915153000_CreateCantiereContabilita')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915153000_CreateCantiereContabilita', N'8.0.0');
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260915161000_ContabilitaPadreFiglio')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915161000_ContabilitaPadreFiglio', N'8.0.0');
GO
