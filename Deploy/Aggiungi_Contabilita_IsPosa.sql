/* ================================================================
   Contabilità: flag IsPosa sulla riga figlio che quadra i prezzi.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NULL
BEGIN
    RAISERROR(N'Manca la tabella CantiereContabilitaRighe.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'IsPosa') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [IsPosa] bit NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_IsPosa] DEFAULT 0;
GO

UPDATE [dbo].[CantiereContabilitaRighe]
SET [IsPosa] = 1
WHERE [PadreId] IS NOT NULL
  AND [IsPosa] = 0
  AND (
        [Descrizione] = N'Posa'
        OR [Descrizione] LIKE N'Posa %'
      );
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260922113000_ContabilitaIsPosa')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922113000_ContabilitaIsPosa', N'8.0.0');
GO
