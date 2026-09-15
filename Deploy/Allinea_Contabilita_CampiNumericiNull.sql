/* ================================================================
   Contabilità cantiere: quantità, costo e prezzo vendita
   possono restare vuoti (NULL).
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF OBJECT_ID(N'dbo.CantiereContabilitaRighe', N'U') IS NULL
BEGIN
    RAISERROR(N'Manca la tabella CantiereContabilitaRighe.', 16, 1);
    RETURN;
END
GO

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
GO

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'QuantitaPosata') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ALTER COLUMN [QuantitaPosata] decimal(18,4) NULL;
GO

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoMaterialeServizio') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ALTER COLUMN [CostoMaterialeServizio] decimal(18,4) NULL;
GO

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoVenditaCliente') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ALTER COLUMN [PrezzoVenditaCliente] decimal(18,4) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260915170000_ContabilitaCampiNumericiNull')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915170000_ContabilitaCampiNumericiNull', N'8.0.0');
GO
