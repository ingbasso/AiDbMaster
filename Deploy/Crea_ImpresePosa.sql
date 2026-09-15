/* ================================================================
   Anagrafica Imprese di posa + collegamento ai cantieri.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF OBJECT_ID(N'dbo.ImpresePosa', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ImpresePosa] (
        [ID]       int            IDENTITY(1,1) NOT NULL,
        [Nome]     nvarchar(150)  NOT NULL,
        [Telefono] nvarchar(18)   NULL,
        [Note]     nvarchar(max)  NULL,
        [Attivo]   bit            NOT NULL CONSTRAINT [DF_ImpresePosa_Attivo] DEFAULT 1,
        CONSTRAINT [PK_ImpresePosa] PRIMARY KEY ([ID])
    );

    CREATE UNIQUE INDEX [IX_ImpresePosa_Nome] ON [dbo].[ImpresePosa] ([Nome]);
END
GO

MERGE [dbo].[ImpresePosa] AS t
USING (VALUES
    (N'posaedil di Trovato Maurizio'),
    (N'Loresa Pavimentazioni'),
    (N'Nechiforov Valeriu'),
    (N'Tecnoblock Srls'),
    (N'Miftari Ajet'),
    (N'Haxhiu Adrian'),
    (N'Pavimart Srls'),
    (N'Fratelli Coltro e Pistolato'),
    (N'Elettro PT sas di Tometto Alessandro')
) AS s(Nome)
ON t.Nome = s.Nome
WHEN NOT MATCHED THEN
    INSERT ([Nome], [Attivo]) VALUES (s.Nome, 1);
GO

IF COL_LENGTH(N'dbo.Cantieri', N'ImpresaPosaId') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [ImpresaPosaId] int NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Cantieri_ImpresePosa'
)
AND COL_LENGTH(N'dbo.Cantieri', N'ImpresaPosaId') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Cantieri]
        ADD CONSTRAINT [FK_Cantieri_ImpresePosa]
        FOREIGN KEY ([ImpresaPosaId]) REFERENCES [dbo].[ImpresePosa] ([ID]);
END
GO

IF COL_LENGTH(N'dbo.Cantieri', N'ImpresaPosa') IS NOT NULL
BEGIN
    UPDATE c
    SET c.ImpresaPosaId = i.ID
    FROM [dbo].[Cantieri] c
    INNER JOIN [dbo].[ImpresePosa] i ON i.Nome = c.ImpresaPosa
    WHERE c.ImpresaPosaId IS NULL AND c.ImpresaPosa IS NOT NULL AND c.ImpresaPosa <> N'';
END
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260915144500_CreateImpresePosa')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915144500_CreateImpresePosa', N'8.0.0');
GO
