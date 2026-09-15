/* ================================================================
   Anagrafica Referenti di cantiere + collegamento ai cantieri.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF OBJECT_ID(N'dbo.ReferentiCantiere', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReferentiCantiere] (
        [ID]       int            IDENTITY(1,1) NOT NULL,
        [Nome]     nvarchar(100)  NOT NULL,
        [Telefono] nvarchar(18)   NULL,
        [Email]    nvarchar(100)  NULL,
        [Note]     nvarchar(max)  NULL,
        [Attivo]   bit            NOT NULL CONSTRAINT [DF_ReferentiCantiere_Attivo] DEFAULT 1,
        CONSTRAINT [PK_ReferentiCantiere] PRIMARY KEY ([ID])
    );

    CREATE INDEX [IX_ReferentiCantiere_Nome] ON [dbo].[ReferentiCantiere] ([Nome]);
END
GO

IF OBJECT_ID(N'dbo.CantiereReferenti', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CantiereReferenti] (
        [ID]          int           IDENTITY(1,1) NOT NULL,
        [CantiereId]  int           NOT NULL,
        [ReferenteId] int           NOT NULL,
        [Ruolo]       nvarchar(80)  NULL,
        CONSTRAINT [PK_CantiereReferenti] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_CantiereReferenti_Cantieri] FOREIGN KEY ([CantiereId])
            REFERENCES [dbo].[Cantieri] ([ID]) ON DELETE CASCADE,
        CONSTRAINT [FK_CantiereReferenti_Referenti] FOREIGN KEY ([ReferenteId])
            REFERENCES [dbo].[ReferentiCantiere] ([ID]) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX [IX_CantiereReferenti_Cantiere_Referente]
        ON [dbo].[CantiereReferenti] ([CantiereId], [ReferenteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260915150000_CreateReferentiCantiere')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915150000_CreateReferentiCantiere', N'8.0.0');
GO
