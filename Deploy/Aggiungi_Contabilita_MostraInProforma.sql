/* ================================================================
   Contabilità: flag MostraInProforma.
   Sulla riga padre e sull'articolo singolo dice se la riga
   compare nella proforma. Di base sì.
   I figli e la posa non vanno in stampa in ogni caso.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NULL
BEGIN
    RAISERROR(N'Manca la tabella CantiereContabilitaRighe.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'MostraInProforma') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [MostraInProforma] bit NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_MostraInProforma] DEFAULT 1;
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260929180000_ContabilitaMostraInProforma')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929180000_ContabilitaMostraInProforma', N'8.0.0');
GO
