-- Script sicuro per creare la tabella CentriLavoro
-- Prima crea la tabella e i dati, poi aggiunge la foreign key

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
    
    -- Inserimento dati di esempio
    INSERT INTO [dbo].[CentriLavoro] ([DescrizioneCentro], [Attivo], [CodiceCentro], [CapacitaOraria], [CostoOrarioStandard], [Note]) VALUES
    ('Centro CNC 1', 1, 'CNC01', 10, 45.50, 'Centro di lavoro principale per lavorazioni CNC'),
    ('Centro Saldatura', 1, 'SALD01', 8, 38.00, 'Postazione saldatura TIG/MIG'),
    ('Centro Assemblaggio', 1, 'ASS01', 15, 32.00, 'Linea di assemblaggio componenti'),
    ('Centro Controllo Qualità', 1, 'CQ01', 20, 42.00, 'Controllo qualità e collaudi'),
    ('Centro Finitura', 1, 'FIN01', 12, 35.00, 'Operazioni di finitura e verniciatura');
    
    PRINT 'Dati di esempio inseriti in CentriLavoro';
END
ELSE
BEGIN
    PRINT 'Tabella CentriLavoro già esistente';
END

PRINT 'Script completato con successo!';
PRINT 'Centri di lavoro creati:';
PRINT '1. Centro CNC 1 (CNC01)';
PRINT '2. Centro Saldatura (SALD01)';
PRINT '3. Centro Assemblaggio (ASS01)';
PRINT '4. Centro Controllo Qualità (CQ01)';
PRINT '5. Centro Finitura (FIN01)';
PRINT '';
PRINT 'NOTA: La Foreign Key sarà aggiunta in un secondo momento dopo aver verificato i dati.';
