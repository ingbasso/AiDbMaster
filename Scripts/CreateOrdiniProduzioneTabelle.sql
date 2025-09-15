-- Script per creare le tabelle degli Ordini di Produzione
-- Eseguire questo script direttamente sul database

-- ===== TABELLA STATIOP =====
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='StatiOP' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[StatiOP] (
        [IdStato] int IDENTITY(1,1) NOT NULL,
        [CodiceStato] nvarchar(2) NOT NULL,
        [DescrizioneStato] nvarchar(20) NOT NULL,
        [Attivo] bit NOT NULL DEFAULT 1,
        [Ordine] int NOT NULL,
        CONSTRAINT [PK_StatiOP] PRIMARY KEY CLUSTERED ([IdStato] ASC)
    );

    -- Indici per StatiOP
    CREATE UNIQUE NONCLUSTERED INDEX [IX_StatiOP_CodiceStato] ON [dbo].[StatiOP] ([CodiceStato] ASC);
    CREATE NONCLUSTERED INDEX [IX_StatiOP_Ordine] ON [dbo].[StatiOP] ([Ordine] ASC);

    -- Dati iniziali per StatiOP
    INSERT INTO [dbo].[StatiOP] ([CodiceStato], [DescrizioneStato], [Attivo], [Ordine]) VALUES
    ('EM', 'Emesso', 1, 1),
    ('PR', 'Produzione', 1, 2),
    ('SO', 'Sospeso', 1, 3),
    ('CH', 'Chiuso', 1, 4);

    PRINT 'Tabella StatiOP creata con successo';
END
ELSE
BEGIN
    PRINT 'Tabella StatiOP già esistente';
END

-- ===== TABELLA OPERATORI =====
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Operatori' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Operatori] (
        [IdOperatore] int IDENTITY(1,1) NOT NULL,
        [CodiceOperatore] nvarchar(10) NOT NULL,
        [Nome] nvarchar(50) NOT NULL,
        [Cognome] nvarchar(50) NOT NULL,
        [Email] nvarchar(100) NULL,
        [Telefono] nvarchar(20) NULL,
        [Attivo] bit NOT NULL DEFAULT 1,
        [DataAssunzione] datetime2 NULL,
        [LivelloCompetenza] int NULL,
        [Note] nvarchar(500) NULL,
        CONSTRAINT [PK_Operatori] PRIMARY KEY CLUSTERED ([IdOperatore] ASC)
    );

    -- Indici per Operatori
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Operatori_CodiceOperatore] ON [dbo].[Operatori] ([CodiceOperatore] ASC);
    CREATE NONCLUSTERED INDEX [IX_Operatori_NomeCognome] ON [dbo].[Operatori] ([Nome] ASC, [Cognome] ASC);
    CREATE NONCLUSTERED INDEX [IX_Operatori_Email] ON [dbo].[Operatori] ([Email] ASC);

    PRINT 'Tabella Operatori creata con successo';
END
ELSE
BEGIN
    PRINT 'Tabella Operatori già esistente';
END

-- ===== TABELLA LISTAOP =====
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ListaOP' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[ListaOP] (
        [IdListaOP] int IDENTITY(1,1) NOT NULL,
        [TipoOrdine] nvarchar(1) NOT NULL,
        [AnnoOrdine] smallint NOT NULL,
        [SerieOrdine] nvarchar(3) NOT NULL,
        [NumeroOrdine] int NOT NULL,
        [RigaOrdine] int NOT NULL,
        [DescrOrdine] nvarchar(100) NULL,
        [CodiceArticolo] nvarchar(50) NOT NULL,
        [DescrizioneArticolo] nvarchar(50) NOT NULL,
        [UnitaMisura] nvarchar(3) NOT NULL,
        [Quantita] decimal(10,3) NOT NULL,
        [QuantitaProdotta] decimal(10,3) NOT NULL,
        [DataInizioOP] datetime2 NOT NULL,
        [TempoCiclo] int NOT NULL,
        [DataInizioSetup] datetime2 NULL,
        [TempoSetup] int NULL,
        [IdStato] int NOT NULL,
        [IdCentroLavoro] int NOT NULL,
        [Note] nvarchar(400) NULL,
        -- Campi aggiuntivi
        [DataFineOP] datetime2 NULL,
        [DataFinePrevista] datetime2 NULL,
        [Priorita] int NULL,
        [IdOperatore] int NULL,
        [CostoOrario] decimal(10,2) NULL,
        [TempoEffettivo] int NULL,
        CONSTRAINT [PK_ListaOP] PRIMARY KEY CLUSTERED ([IdListaOP] ASC)
    );

    -- Indici per ListaOP
    CREATE NONCLUSTERED INDEX [IX_ListaOP_ChiaveComposita] ON [dbo].[ListaOP] ([TipoOrdine] ASC, [AnnoOrdine] ASC, [SerieOrdine] ASC, [NumeroOrdine] ASC);
    CREATE NONCLUSTERED INDEX [IX_ListaOP_IdStato] ON [dbo].[ListaOP] ([IdStato] ASC);
    CREATE NONCLUSTERED INDEX [IX_ListaOP_DataInizioOP] ON [dbo].[ListaOP] ([DataInizioOP] ASC);
    CREATE NONCLUSTERED INDEX [IX_ListaOP_IdCentroLavoro] ON [dbo].[ListaOP] ([IdCentroLavoro] ASC);
    CREATE NONCLUSTERED INDEX [IX_ListaOP_IdOperatore] ON [dbo].[ListaOP] ([IdOperatore] ASC);
    CREATE NONCLUSTERED INDEX [IX_ListaOP_Priorita] ON [dbo].[ListaOP] ([Priorita] ASC);
    CREATE NONCLUSTERED INDEX [IX_ListaOP_CodiceArticolo] ON [dbo].[ListaOP] ([CodiceArticolo] ASC);

    -- Foreign Key verso StatiOP
    ALTER TABLE [dbo].[ListaOP] ADD CONSTRAINT [FK_ListaOP_StatiOP_IdStato] 
    FOREIGN KEY ([IdStato]) REFERENCES [dbo].[StatiOP] ([IdStato]) ON DELETE NO ACTION;

    -- Foreign Key verso Operatori
    ALTER TABLE [dbo].[ListaOP] ADD CONSTRAINT [FK_ListaOP_Operatori_IdOperatore] 
    FOREIGN KEY ([IdOperatore]) REFERENCES [dbo].[Operatori] ([IdOperatore]) ON DELETE SET NULL;

    PRINT 'Tabella ListaOP creata con successo';
END
ELSE
BEGIN
    PRINT 'Tabella ListaOP già esistente';
END

PRINT 'Script completato con successo!';
PRINT 'Tabelle create:';
PRINT '- StatiOP (con dati iniziali: Emesso, Produzione, Sospeso, Chiuso)';
PRINT '- Operatori';
PRINT '- ListaOP (con tutti gli indici e foreign key)';
PRINT '';
PRINT 'Note:';
PRINT '- TempoCiclo è in secondi';
PRINT '- TempoSetup è in minuti';
PRINT '- TempoEffettivo è in secondi';
PRINT '- Priorita: 1=Bassa, 2=Normale, 3=Media, 4=Alta, 5=Urgente';
PRINT '- IdCentroLavoro sarà collegato alla tabella CentriLavoro (da creare)';
