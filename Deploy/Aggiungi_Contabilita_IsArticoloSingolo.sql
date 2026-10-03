/* ================================================================
   Contabilità: flag IsArticoloSingolo.
   È una riga sola (codice, descrizione, quantità, costo, ricarico
   e prezzo di vendita), senza riga materiale e senza posa.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NULL
BEGIN
    RAISERROR(N'Manca la tabella CantiereContabilitaRighe.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'IsArticoloSingolo') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [IsArticoloSingolo] bit NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_IsArticoloSingolo] DEFAULT 0;
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260929100000_ContabilitaIsArticoloSingolo')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929100000_ContabilitaIsArticoloSingolo', N'8.0.0');
GO
