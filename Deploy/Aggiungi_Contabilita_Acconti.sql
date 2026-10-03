/* ================================================================
   Contabilità: più acconti imponibili per la stessa scheda.
   L'importo già presente in ImponibileAcconto diventa la prima riga.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF OBJECT_ID(N'dbo.CantiereContabilita', N'U') IS NULL
BEGIN
    RAISERROR(N'Manca la tabella CantiereContabilita.', 16, 1);
    RETURN;
END
GO

IF OBJECT_ID(N'dbo.CantiereContabilitaAcconti', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CantiereContabilitaAcconti] (
        [ID] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CantiereContabilitaAcconti] PRIMARY KEY,
        [ContabilitaId] int NOT NULL,
        [Ordine] int NOT NULL,
        [Descrizione] nvarchar(100) NULL,
        [Imponibile] decimal(18,2) NOT NULL,
        CONSTRAINT [FK_CantiereContabilitaAcconti_Contabilita]
            FOREIGN KEY ([ContabilitaId]) REFERENCES [dbo].[CantiereContabilita]([ID]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_CantiereContabilitaAcconti_Contabilita]
        ON [dbo].[CantiereContabilitaAcconti]([ContabilitaId], [Ordine]);
END
GO

INSERT INTO [dbo].[CantiereContabilitaAcconti] ([ContabilitaId], [Ordine], [Descrizione], [Imponibile])
SELECT [c].[ID], 1, N'Acconto', [c].[ImponibileAcconto]
FROM [dbo].[CantiereContabilita] AS [c]
WHERE [c].[ImponibileAcconto] <> 0
  AND NOT EXISTS (
        SELECT 1 FROM [dbo].[CantiereContabilitaAcconti] AS [a]
        WHERE [a].[ContabilitaId] = [c].[ID]
      );
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260922160000_ContabilitaAcconti')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922160000_ContabilitaAcconti', N'8.0.0');
GO
