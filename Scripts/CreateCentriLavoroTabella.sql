-- Script per creare la tabella CentriLavoro
-- Eseguire questo script direttamente sul database

-- ===== TABELLA CENTRILAVORO =====
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CentriLavoro' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[CentriLavoro] (
        [IdCentroLavoro] int IDENTITY(1,1) NOT NULL,
        [DescrizioneCentro] varchar(100) NOT NULL,
        [Attivo] bit NOT NULL DEFAULT 1,
        [CodiceCentro] nvarchar(10) NULL,
        [CapacitaOraria] int NULL,
        [CostoOrarioStandard] decimal(10,2) NULL,
        [Note] nvarchar(500) NULL,
        [DataCreazione] datetime2 NOT NULL DEFAULT GETDATE(),
        [DataUltimaModifica] datetime2 NULL,
        CONSTRAINT [PK_CentriLavoro] PRIMARY KEY CLUSTERED ([IdCentroLavoro] ASC)
    );

    -- Indici per CentriLavoro
    CREATE UNIQUE NONCLUSTERED INDEX [IX_CentriLavoro_CodiceCentro] ON [dbo].[CentriLavoro] ([CodiceCentro] ASC) WHERE [CodiceCentro] IS NOT NULL;
    CREATE NONCLUSTERED INDEX [IX_CentriLavoro_DescrizioneCentro] ON [dbo].[CentriLavoro] ([DescrizioneCentro] ASC);
    CREATE NONCLUSTERED INDEX [IX_CentriLavoro_Attivo] ON [dbo].[CentriLavoro] ([Attivo] ASC);

    PRINT 'Tabella CentriLavoro creata con successo';
END
ELSE
BEGIN
    PRINT 'Tabella CentriLavoro già esistente';
END

-- ===== AGGIORNAMENTO FOREIGN KEY IN LISTAOP =====
-- Aggiungiamo la foreign key verso CentriLavoro se non esiste già
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ListaOP_CentriLavoro_IdCentroLavoro')
BEGIN
    ALTER TABLE [dbo].[ListaOP] ADD CONSTRAINT [FK_ListaOP_CentriLavoro_IdCentroLavoro] 
    FOREIGN KEY ([IdCentroLavoro]) REFERENCES [dbo].[CentriLavoro] ([IdCentroLavoro]) ON DELETE NO ACTION;
    
    PRINT 'Foreign Key FK_ListaOP_CentriLavoro_IdCentroLavoro aggiunta con successo';
END
ELSE
BEGIN
    PRINT 'Foreign Key FK_ListaOP_CentriLavoro_IdCentroLavoro già esistente';
END

PRINT 'Script completato con successo!';
PRINT 'Tabella CentriLavoro creata con:';
PRINT '- IdCentroLavoro (PK, Identity)';
PRINT '- DescrizioneCentro (varchar(100), NOT NULL)';
PRINT '- Campi aggiuntivi: Attivo, CodiceCentro, CapacitaOraria, CostoOrarioStandard, Note';
PRINT '- Campi di audit: DataCreazione, DataUltimaModifica';
PRINT '- Indici ottimizzati per performance';
PRINT '- Foreign Key verso ListaOP configurata';
