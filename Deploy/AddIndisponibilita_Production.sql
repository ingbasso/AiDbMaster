/* ============================================================
   Script idempotente per creare la tabella "Indisponibilita"
   (assenze autisti / fermi mezzi) sul database di PRODUZIONE.

   Database:  AIDBMASTER  (server SVRGEST)
   Migrazione EF: 20260617133417_AddIndisponibilita

   NOTE:
   - Lo script si puo' eseguire piu' volte senza errori
     (controlla sempre se gli oggetti esistono gia').
   - Eseguire con SSMS dopo aver selezionato il database AIDBMASTER.
   ============================================================ */

SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* 1) Tabella Indisponibilita */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'Indisponibilita')
BEGIN
    CREATE TABLE [Indisponibilita] (
        [ID]             int IDENTITY(1,1) NOT NULL,
        [Tipo]           nvarchar(20)  NOT NULL,
        [AutistaId]      int           NULL,
        [MezzoTrasportoId] int         NULL,
        [DataInizio]     datetime2     NOT NULL,
        [DataFine]       datetime2     NOT NULL,
        [GiornoIntero]   bit           NOT NULL,
        [OraInizio]      time          NULL,
        [OraFine]        time          NULL,
        [Causale]        nvarchar(30)  NOT NULL,
        [Note]           nvarchar(max) NULL,
        [DataCreazione]  datetime2     NOT NULL,
        [CreatoDa]       nvarchar(450) NULL,
        CONSTRAINT [PK_Indisponibilita] PRIMARY KEY ([ID])
    );
END;
GO

/* 2) Foreign key verso Autisti */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Indisponibilita_Autisti_AutistaId')
   AND EXISTS (SELECT 1 FROM sys.tables WHERE name = N'Autisti')
BEGIN
    ALTER TABLE [Indisponibilita]
    ADD CONSTRAINT [FK_Indisponibilita_Autisti_AutistaId]
        FOREIGN KEY ([AutistaId]) REFERENCES [Autisti] ([ID]) ON DELETE CASCADE;
END;
GO

/* 3) Foreign key verso MezziTrasportoInterni */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Indisponibilita_MezziTrasportoInterni_MezzoTrasportoId')
   AND EXISTS (SELECT 1 FROM sys.tables WHERE name = N'MezziTrasportoInterni')
BEGIN
    ALTER TABLE [Indisponibilita]
    ADD CONSTRAINT [FK_Indisponibilita_MezziTrasportoInterni_MezzoTrasportoId]
        FOREIGN KEY ([MezzoTrasportoId]) REFERENCES [MezziTrasportoInterni] ([ID]) ON DELETE CASCADE;
END;
GO

/* 4) Indici */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Indisponibilita_AutistaId' AND object_id = OBJECT_ID(N'Indisponibilita'))
    CREATE INDEX [IX_Indisponibilita_AutistaId] ON [Indisponibilita] ([AutistaId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Indisponibilita_Date' AND object_id = OBJECT_ID(N'Indisponibilita'))
    CREATE INDEX [IX_Indisponibilita_Date] ON [Indisponibilita] ([DataInizio], [DataFine]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Indisponibilita_MezzoTrasportoId' AND object_id = OBJECT_ID(N'Indisponibilita'))
    CREATE INDEX [IX_Indisponibilita_MezzoTrasportoId] ON [Indisponibilita] ([MezzoTrasportoId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Indisponibilita_Tipo' AND object_id = OBJECT_ID(N'Indisponibilita'))
    CREATE INDEX [IX_Indisponibilita_Tipo] ON [Indisponibilita] ([Tipo]);
GO

/* 5) Registra la migrazione nello storico EF (se la tabella di storico esiste) */
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = N'__EFMigrationsHistory')
   AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260617133417_AddIndisponibilita')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260617133417_AddIndisponibilita', N'8.0.2');
END;
GO

COMMIT TRANSACTION;
GO

PRINT 'Script completato: tabella Indisponibilita pronta in produzione.';
