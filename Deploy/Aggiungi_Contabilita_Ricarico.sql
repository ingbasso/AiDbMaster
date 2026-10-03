/* ================================================================
   Contabilità: percentuale di ricarico sui figli (non sulla posa).
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NULL
BEGIN
    RAISERROR(N'Manca la tabella CantiereContabilitaRighe.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'RicaricoPercentuale') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [RicaricoPercentuale] decimal(9,4) NULL;
GO

UPDATE [dbo].[CantiereContabilitaRighe]
SET [RicaricoPercentuale] = ROUND(([PrezzoVenditaCliente] / [CostoMaterialeServizio] - 1) * 100, 4)
WHERE [PadreId] IS NOT NULL
  AND [IsPosa] = 0
  AND [RicaricoPercentuale] IS NULL
  AND [CostoMaterialeServizio] > 0
  AND [PrezzoVenditaCliente] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260922114500_ContabilitaRicarico')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922114500_ContabilitaRicarico', N'8.0.0');
GO
