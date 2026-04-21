-- ============================================================================
-- Script per applicare le migrazioni mancanti al database di PRODUZIONE
-- Migrazioni: 20260421083133_AddStatoArticoli_CodiceAgente2
--             20260421091143_AddRimorchioDisponibile_ConRimorchio
--
-- SICURO E IDEMPOTENTE: ogni operazione verifica se esiste gia' prima di agire.
-- Puo' essere eseguito piu' volte senza rischi.
-- Le FK vengono tentate ma se falliscono (es. mancanza PK sulla tabella
-- referenziata) lo script prosegue con un avviso.
-- ============================================================================

PRINT '=== Inizio applicazione migrazioni mancanti ===';

-- ============================================================================
-- MIGRAZIONE 1: 20260421083133_AddStatoArticoli_CodiceAgente2
-- ============================================================================
PRINT '';
PRINT '--- Migrazione 1: AddStatoArticoli_CodiceAgente2 ---';

-- 1a) Colonna CodiceAgente2 su OrdiniTestate
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniTestate') AND name = 'CodiceAgente2')
BEGIN
    ALTER TABLE [OrdiniTestate] ADD [CodiceAgente2] smallint NULL;
    PRINT '  + Colonna CodiceAgente2 aggiunta a OrdiniTestate';
END
ELSE
    PRINT '  = Colonna CodiceAgente2 gia'' presente in OrdiniTestate (saltata)';

-- 1b) Colonna CodiceAgente2 su DestinazioniDiverse
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DestinazioniDiverse') AND name = 'CodiceAgente2')
BEGIN
    ALTER TABLE [DestinazioniDiverse] ADD [CodiceAgente2] smallint NULL;
    PRINT '  + Colonna CodiceAgente2 aggiunta a DestinazioniDiverse';
END
ELSE
    PRINT '  = Colonna CodiceAgente2 gia'' presente in DestinazioniDiverse (saltata)';

-- 1c) Colonna CodiceAgente2 su AnagraficaClienti
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaClienti') AND name = 'CodiceAgente2')
BEGIN
    ALTER TABLE [AnagraficaClienti] ADD [CodiceAgente2] smallint NULL;
    PRINT '  + Colonna CodiceAgente2 aggiunta a AnagraficaClienti';
END
ELSE
    PRINT '  = Colonna CodiceAgente2 gia'' presente in AnagraficaClienti (saltata)';

-- 1d) Colonna StatoArticoli su AnagraficaArticoli
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AnagraficaArticoli') AND name = 'StatoArticoli')
BEGIN
    ALTER TABLE [AnagraficaArticoli] ADD [StatoArticoli] varchar(1) NULL;
    PRINT '  + Colonna StatoArticoli aggiunta a AnagraficaArticoli';
END
ELSE
    PRINT '  = Colonna StatoArticoli gia'' presente in AnagraficaArticoli (saltata)';

-- 1e) Indice IX_OrdiniTestate_CodiceAgente2
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('OrdiniTestate') AND name = 'IX_OrdiniTestate_CodiceAgente2')
BEGIN
    CREATE INDEX [IX_OrdiniTestate_CodiceAgente2] ON [OrdiniTestate] ([CodiceAgente2]);
    PRINT '  + Indice IX_OrdiniTestate_CodiceAgente2 creato';
END
ELSE
    PRINT '  = Indice IX_OrdiniTestate_CodiceAgente2 gia'' presente (saltato)';

-- 1f) Indice IX_DestinazioniDiverse_CodiceAgente2
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('DestinazioniDiverse') AND name = 'IX_DestinazioniDiverse_CodiceAgente2')
BEGIN
    CREATE INDEX [IX_DestinazioniDiverse_CodiceAgente2] ON [DestinazioniDiverse] ([CodiceAgente2]);
    PRINT '  + Indice IX_DestinazioniDiverse_CodiceAgente2 creato';
END
ELSE
    PRINT '  = Indice IX_DestinazioniDiverse_CodiceAgente2 gia'' presente (saltato)';

-- 1g) Indice IX_AnagraficaClienti_CodiceAgente2
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('AnagraficaClienti') AND name = 'IX_AnagraficaClienti_CodiceAgente2')
BEGIN
    CREATE INDEX [IX_AnagraficaClienti_CodiceAgente2] ON [AnagraficaClienti] ([CodiceAgente2]);
    PRINT '  + Indice IX_AnagraficaClienti_CodiceAgente2 creato';
END
ELSE
    PRINT '  = Indice IX_AnagraficaClienti_CodiceAgente2 gia'' presente (saltato)';

-- 1h) FK AnagraficaClienti -> TabellaAgenti (CodiceAgente2)
--     Tentativo con TRY/CATCH: se TabellaAgenti non ha PK/UNIQUE su CodiceAgente, viene saltata
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AnagraficaClienti_TabellaAgenti_CodiceAgente2')
BEGIN
    BEGIN TRY
        ALTER TABLE [AnagraficaClienti]
            ADD CONSTRAINT [FK_AnagraficaClienti_TabellaAgenti_CodiceAgente2]
            FOREIGN KEY ([CodiceAgente2]) REFERENCES [TabellaAgenti] ([CodiceAgente])
            ON DELETE NO ACTION;
        PRINT '  + FK AnagraficaClienti -> TabellaAgenti (CodiceAgente2) creata';
    END TRY
    BEGIN CATCH
        PRINT '  ! FK AnagraficaClienti -> TabellaAgenti (CodiceAgente2) NON creata (TabellaAgenti non ha PK/UNIQUE su CodiceAgente) - SALTATA';
    END CATCH
END
ELSE
    PRINT '  = FK AnagraficaClienti -> TabellaAgenti (CodiceAgente2) gia'' presente (saltata)';

-- 1i) FK DestinazioniDiverse -> TabellaAgenti (CodiceAgente2)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DestinazioniDiverse_TabellaAgenti_CodiceAgente2')
BEGIN
    BEGIN TRY
        ALTER TABLE [DestinazioniDiverse]
            ADD CONSTRAINT [FK_DestinazioniDiverse_TabellaAgenti_CodiceAgente2]
            FOREIGN KEY ([CodiceAgente2]) REFERENCES [TabellaAgenti] ([CodiceAgente])
            ON DELETE NO ACTION;
        PRINT '  + FK DestinazioniDiverse -> TabellaAgenti (CodiceAgente2) creata';
    END TRY
    BEGIN CATCH
        PRINT '  ! FK DestinazioniDiverse -> TabellaAgenti (CodiceAgente2) NON creata (TabellaAgenti non ha PK/UNIQUE su CodiceAgente) - SALTATA';
    END CATCH
END
ELSE
    PRINT '  = FK DestinazioniDiverse -> TabellaAgenti (CodiceAgente2) gia'' presente (saltata)';

-- 1j) FK OrdiniTestate -> TabellaAgenti (CodiceAgente2)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_OrdiniTestate_TabellaAgenti_CodiceAgente2')
BEGIN
    BEGIN TRY
        ALTER TABLE [OrdiniTestate]
            ADD CONSTRAINT [FK_OrdiniTestate_TabellaAgenti_CodiceAgente2]
            FOREIGN KEY ([CodiceAgente2]) REFERENCES [TabellaAgenti] ([CodiceAgente])
            ON DELETE NO ACTION;
        PRINT '  + FK OrdiniTestate -> TabellaAgenti (CodiceAgente2) creata';
    END TRY
    BEGIN CATCH
        PRINT '  ! FK OrdiniTestate -> TabellaAgenti (CodiceAgente2) NON creata (TabellaAgenti non ha PK/UNIQUE su CodiceAgente) - SALTATA';
    END CATCH
END
ELSE
    PRINT '  = FK OrdiniTestate -> TabellaAgenti (CodiceAgente2) gia'' presente (saltata)';

-- 1k) Record in __EFMigrationsHistory
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260421083133_AddStatoArticoli_CodiceAgente2')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260421083133_AddStatoArticoli_CodiceAgente2', '8.0.11');
    PRINT '  + Record migrazione 20260421083133 inserito in __EFMigrationsHistory';
END
ELSE
    PRINT '  = Record migrazione 20260421083133 gia'' presente in __EFMigrationsHistory (saltato)';

PRINT '--- Migrazione 1 completata ---';

-- ============================================================================
-- MIGRAZIONE 2: 20260421091143_AddRimorchioDisponibile_ConRimorchio
-- ============================================================================
PRINT '';
PRINT '--- Migrazione 2: AddRimorchioDisponibile_ConRimorchio ---';

-- 2a) Colonna ConRimorchio su ViaggiConsegna
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ViaggiConsegna') AND name = 'ConRimorchio')
BEGIN
    ALTER TABLE [ViaggiConsegna] ADD [ConRimorchio] bit NOT NULL CONSTRAINT [DF_ViaggiConsegna_ConRimorchio] DEFAULT (0);
    PRINT '  + Colonna ConRimorchio aggiunta a ViaggiConsegna';
END
ELSE
    PRINT '  = Colonna ConRimorchio gia'' presente in ViaggiConsegna (saltata)';

-- 2b) Colonna PortataMaxConRimorchioKg su MezziTrasportoInterni
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MezziTrasportoInterni') AND name = 'PortataMaxConRimorchioKg')
BEGIN
    ALTER TABLE [MezziTrasportoInterni] ADD [PortataMaxConRimorchioKg] decimal(18,3) NULL;
    PRINT '  + Colonna PortataMaxConRimorchioKg aggiunta a MezziTrasportoInterni';
END
ELSE
    PRINT '  = Colonna PortataMaxConRimorchioKg gia'' presente in MezziTrasportoInterni (saltata)';

-- 2c) Colonna RimorchioDisponibile su MezziTrasportoInterni
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MezziTrasportoInterni') AND name = 'RimorchioDisponibile')
BEGIN
    ALTER TABLE [MezziTrasportoInterni] ADD [RimorchioDisponibile] bit NOT NULL CONSTRAINT [DF_MezziTrasportoInterni_RimorchioDisponibile] DEFAULT (0);
    PRINT '  + Colonna RimorchioDisponibile aggiunta a MezziTrasportoInterni';
END
ELSE
    PRINT '  = Colonna RimorchioDisponibile gia'' presente in MezziTrasportoInterni (saltata)';

-- 2d) Record in __EFMigrationsHistory
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260421091143_AddRimorchioDisponibile_ConRimorchio')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260421091143_AddRimorchioDisponibile_ConRimorchio', '8.0.2');
    PRINT '  + Record migrazione 20260421091143 inserito in __EFMigrationsHistory';
END
ELSE
    PRINT '  = Record migrazione 20260421091143 gia'' presente in __EFMigrationsHistory (saltato)';

PRINT '--- Migrazione 2 completata ---';

-- ============================================================================
PRINT '';
PRINT '=== Tutte le migrazioni applicate con successo ===';
