/* ================================================================
   Contabilità: flag IsSconto sulla riga che scala il totale vendita.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NULL
BEGIN
    RAISERROR(N'Manca la tabella CantiereContabilitaRighe.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'IsSconto') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [IsSconto] bit NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_IsSconto] DEFAULT 0;
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260922150000_ContabilitaIsSconto')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922150000_ContabilitaIsSconto', N'8.0.0');
GO
