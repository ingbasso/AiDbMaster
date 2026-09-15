/* ================================================================
   Contabilità di cantiere (iniziale / finale) + righe.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

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

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260915153000_CreateCantiereContabilita')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915153000_CreateCantiereContabilita', N'8.0.0');
GO
