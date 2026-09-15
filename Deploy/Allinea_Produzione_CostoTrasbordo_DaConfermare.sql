/* ================================================================
   Allineamento DB PRODUZIONE dopo il deploy del 15/09/2026
   Database: AIDBMASTER su SVRGEST

   Aggiunge (solo se mancano) le colonne usate dalla nuova versione:
     - ViaggioConsegnaDestinazioni.CostoTrasbordo
     - ViaggiConsegna.DaConfermare
   e le registra in __EFMigrationsHistory cosi' l'app non tenta
   di ricrearle all'avvio.

   IDEMPOTENTE: si puo' eseguire piu' volte.

   ISTRUZIONI: in SSMS selezionare AIDBMASTER, poi Esegui.
   ================================================================ */

SET QUOTED_IDENTIFIER ON;
GO

/* ---------- 1) CostoTrasbordo sulle destinazioni ---------- */
IF COL_LENGTH(N'dbo.ViaggioConsegnaDestinazioni', N'CostoTrasbordo') IS NULL
BEGIN
    ALTER TABLE [ViaggioConsegnaDestinazioni]
    ADD [CostoTrasbordo] decimal(18,2) NOT NULL
        CONSTRAINT [DF_ViaggioConsegnaDestinazioni_CostoTrasbordo] DEFAULT 0;
END
GO

UPDATE [ViaggioConsegnaDestinazioni]
SET [CostoTrasbordo] = [PrezzoVendita]
WHERE [CostoTrasbordo] = 0 AND [PrezzoVendita] <> 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915091500_AddCostoTrasbordoToDestinazioni'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915091500_AddCostoTrasbordoToDestinazioni', N'8.0.2');
END
GO

/* ---------- 2) DaConfermare sui viaggi ---------- */
IF COL_LENGTH(N'dbo.ViaggiConsegna', N'DaConfermare') IS NULL
BEGIN
    ALTER TABLE [ViaggiConsegna]
    ADD [DaConfermare] bit NOT NULL
        CONSTRAINT [DF_ViaggiConsegna_DaConfermare] DEFAULT 0;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915114500_AddDaConfermareToViaggiConsegna'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915114500_AddDaConfermareToViaggiConsegna', N'8.0.2');
END
GO

/* ---------- Controllo finale (deve restituire 1 e 1) ---------- */
SELECT
    CASE WHEN COL_LENGTH(N'dbo.ViaggioConsegnaDestinazioni', N'CostoTrasbordo') IS NULL THEN 0 ELSE 1 END AS CostoTrasbordoPresente,
    CASE WHEN COL_LENGTH(N'dbo.ViaggiConsegna', N'DaConfermare') IS NULL THEN 0 ELSE 1 END AS DaConfermarePresente;

SELECT [MigrationId]
FROM [__EFMigrationsHistory]
WHERE [MigrationId] IN (
    N'20260915091500_AddCostoTrasbordoToDestinazioni',
    N'20260915114500_AddDaConfermareToViaggiConsegna'
);
GO
