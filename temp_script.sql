CREATE TABLE [AnagraficaArticoli] (
    [ID] int NOT NULL IDENTITY,
    [CodiceArticolo] nvarchar(50) NOT NULL,
    [CodiceAlternativo] nvarchar(50) NULL,
    [Descrizione] nvarchar(255) NOT NULL,
    [DescrizioneUlteriore] nvarchar(50) NULL,
    [TipoArticolo] nvarchar(1) NULL,
    [UnitàMisura] nvarchar(3) NOT NULL,
    [SecondaUnitàMisura] nvarchar(3) NULL,
    [Conversione] decimal(18,6) NOT NULL,
    [UnitàMisuraConfezione] nvarchar(3) NULL,
    [ConversioneConfezione] decimal(18,6) NOT NULL,
    [MakeOrBuy] varchar(1) NULL,
    CONSTRAINT [PK_AnagraficaArticoli] PRIMARY KEY ([ID]),
    CONSTRAINT [AK_AnagraficaArticoli_CodiceArticolo] UNIQUE ([CodiceArticolo])
);
GO


CREATE TABLE [AnagraficaFornitori] (
    [ID] int NOT NULL IDENTITY,
    [CodiceFornitore] int NOT NULL,
    [Tipo] nvarchar(1) NOT NULL,
    [RagioneSociale] nvarchar(50) NOT NULL,
    [DescrizioneUlteriore] nvarchar(50) NULL,
    [Indirizzo] nvarchar(70) NULL,
    [CAP] nvarchar(10) NULL,
    [Citta] nvarchar(50) NULL,
    [Provincia] nvarchar(2) NULL,
    [CodiceFiscale] nvarchar(16) NULL,
    [PartitaIva] nvarchar(11) NULL,
    [Telefono] nvarchar(18) NULL,
    CONSTRAINT [PK_AnagraficaFornitori] PRIMARY KEY ([ID])
);
GO


CREATE TABLE [ArticoliSostitutivi] (
    [CodiceArticolo] nvarchar(50) NOT NULL,
    [CodiceArticoloSostitutivo] nvarchar(50) NOT NULL,
    [Note] nvarchar(max) NULL,
    CONSTRAINT [PK_ArticoliSostitutivi] PRIMARY KEY ([CodiceArticolo], [CodiceArticoloSostitutivo])
);
GO


CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [CentriLavoro] (
    [CodiceCentro] nvarchar(10) NOT NULL,
    [DescrizioneCentro] nvarchar(100) NOT NULL,
    [Attivo] bit NOT NULL,
    [CapacitaOraria] int NULL,
    [CostoOrarioStandard] decimal(10,2) NULL,
    [Note] nvarchar(500) NULL,
    [DataCreazione] datetime2 NOT NULL,
    [DataUltimaModifica] datetime2 NULL,
    CONSTRAINT [PK_CentriLavoro] PRIMARY KEY ([CodiceCentro])
);
GO


CREATE TABLE [DocumentCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_DocumentCategories] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Lavorazioni] (
    [CodiceLavorazione] smallint NOT NULL,
    [DescrizioneLavorazione] nvarchar(100) NOT NULL,
    [Attivo] bit NOT NULL,
    [DataCreazione] datetime2 NOT NULL,
    [DataUltimaModifica] datetime2 NULL,
    CONSTRAINT [PK_Lavorazioni] PRIMARY KEY ([CodiceLavorazione])
);
GO


CREATE TABLE [Operatori] (
    [IdOperatore] int NOT NULL IDENTITY,
    [CodiceOperatore] nvarchar(10) NOT NULL,
    [Nome] nvarchar(50) NOT NULL,
    [Cognome] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Telefono] nvarchar(20) NULL,
    [Attivo] bit NOT NULL,
    [DataAssunzione] datetime2 NULL,
    [LivelloCompetenza] int NULL,
    [Note] nvarchar(500) NULL,
    CONSTRAINT [PK_Operatori] PRIMARY KEY ([IdOperatore])
);
GO


CREATE TABLE [ProgressiviArticoli] (
    [ID] int NOT NULL IDENTITY,
    [CodiceArticolo] nvarchar(50) NOT NULL,
    [CodiceMagazzino] smallint NOT NULL,
    [Esistenza] decimal(27,9) NOT NULL,
    [OrdinatoFornitoriDataOdierna] decimal(27,9) NOT NULL,
    CONSTRAINT [PK_ProgressiviArticoli] PRIMARY KEY ([ID])
);
GO


CREATE TABLE [StatiOP] (
    [IdStato] int NOT NULL IDENTITY,
    [CodiceStato] nvarchar(2) NOT NULL,
    [DescrizioneStato] nvarchar(20) NOT NULL,
    [Attivo] bit NOT NULL,
    [Ordine] int NOT NULL,
    CONSTRAINT [PK_StatiOP] PRIMARY KEY ([IdStato])
);
GO


CREATE TABLE [TabellaAgenti] (
    [CodiceAgente] smallint NOT NULL IDENTITY,
    [DescrizioneAgente] nvarchar(50) NULL,
    [IndirizzoAgente] nvarchar(70) NULL,
    [CAPAgente] nvarchar(10) NULL,
    [CittaAgente] nvarchar(50) NULL,
    [ProvinciaAgente] nvarchar(2) NULL,
    [Attivo] bit NOT NULL,
    CONSTRAINT [PK_TabellaAgenti] PRIMARY KEY ([CodiceAgente])
);
GO


CREATE TABLE [TabellaMagazzini] (
    [ID] int NOT NULL IDENTITY,
    [CodiceMagazzino] smallint NOT NULL,
    [DescrizioneMagazzino] nvarchar(50) NULL,
    CONSTRAINT [PK_TabellaMagazzini] PRIMARY KEY ([ID]),
    CONSTRAINT [AK_TabellaMagazzini_CodiceMagazzino] UNIQUE ([CodiceMagazzino])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [CalendarioFermiCentriLavoro] (
    [Id] int NOT NULL IDENTITY,
    [CodiceCentro] nvarchar(10) NOT NULL,
    [DataInizioFermo] datetime2 NOT NULL,
    [DataFineFermo] datetime2 NULL,
    [TipoFermo] int NOT NULL,
    [Motivo] nvarchar(200) NULL,
    [Note] nvarchar(max) NULL,
    [IsPianificato] bit NOT NULL,
    [DataCreazione] datetime2 NOT NULL,
    [DataUltimaModifica] datetime2 NULL,
    CONSTRAINT [PK_CalendarioFermiCentriLavoro] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CalendarioFermiCentriLavoro_CentriLavoro_CodiceCentro] FOREIGN KEY ([CodiceCentro]) REFERENCES [CentriLavoro] ([CodiceCentro]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Documents] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(255) NOT NULL,
    [Description] nvarchar(500) NULL,
    [FileType] int NOT NULL,
    [FilePath] nvarchar(max) NOT NULL,
    [FileSize] bigint NOT NULL,
    [UploadDate] datetime2 NOT NULL,
    [LastModifiedDate] datetime2 NULL,
    [UploadedById] nvarchar(450) NOT NULL,
    [CategoryId] int NULL,
    [Tags] nvarchar(max) NULL,
    [IsConfidential] bit NOT NULL,
    [DocumentCategoryId] int NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Documents_AspNetUsers_UploadedById] FOREIGN KEY ([UploadedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Documents_DocumentCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [DocumentCategories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Documents_DocumentCategories_DocumentCategoryId] FOREIGN KEY ([DocumentCategoryId]) REFERENCES [DocumentCategories] ([Id])
);
GO


CREATE TABLE [ListaOP] (
    [IdListaOP] int NOT NULL IDENTITY,
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
    [TempoCiclo] real NOT NULL,
    [DataInizioSetup] datetime2 NULL,
    [TempoSetup] real NULL,
    [IdStato] int NOT NULL,
    [CodiceCentro] nvarchar(10) NOT NULL,
    [CodiceLavorazione] smallint NOT NULL,
    [Note] nvarchar(400) NULL,
    [DataFineOP] datetime2 NULL,
    [DataFinePrevista] datetime2 NULL,
    [Priorita] int NULL,
    [IdOperatore] int NULL,
    [CostoOrario] decimal(10,2) NULL,
    [TempoEffettivo] real NULL,
    [Modificato] bit NOT NULL,
    CONSTRAINT [PK_ListaOP] PRIMARY KEY ([IdListaOP]),
    CONSTRAINT [FK_ListaOP_CentriLavoro_CodiceCentro] FOREIGN KEY ([CodiceCentro]) REFERENCES [CentriLavoro] ([CodiceCentro]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ListaOP_Lavorazioni_CodiceLavorazione] FOREIGN KEY ([CodiceLavorazione]) REFERENCES [Lavorazioni] ([CodiceLavorazione]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ListaOP_Operatori_IdOperatore] FOREIGN KEY ([IdOperatore]) REFERENCES [Operatori] ([IdOperatore]) ON DELETE SET NULL,
    CONSTRAINT [FK_ListaOP_StatiOP_IdStato] FOREIGN KEY ([IdStato]) REFERENCES [StatiOP] ([IdStato]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AnagraficaClienti] (
    [ID] int NOT NULL IDENTITY,
    [CodiceCliente] int NOT NULL,
    [Tipo] nvarchar(1) NOT NULL,
    [RagioneSociale] nvarchar(50) NOT NULL,
    [DescrizioneUlteriore] nvarchar(50) NULL,
    [Indirizzo] nvarchar(70) NULL,
    [CAP] nvarchar(10) NULL,
    [Citta] nvarchar(50) NULL,
    [Provincia] nvarchar(2) NULL,
    [CodiceFiscale] nvarchar(16) NULL,
    [PartitaIva] nvarchar(11) NULL,
    [Telefono] nvarchar(18) NULL,
    [CodiceAgente] smallint NOT NULL,
    CONSTRAINT [PK_AnagraficaClienti] PRIMARY KEY ([ID]),
    CONSTRAINT [AK_AnagraficaClienti_CodiceCliente] UNIQUE ([CodiceCliente]),
    CONSTRAINT [FK_AnagraficaClienti_TabellaAgenti_CodiceAgente] FOREIGN KEY ([CodiceAgente]) REFERENCES [TabellaAgenti] ([CodiceAgente]) ON DELETE CASCADE
);
GO


CREATE TABLE [DocumentPermissions] (
    [Id] int NOT NULL IDENTITY,
    [DocumentId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [PermissionType] int NOT NULL,
    [GrantedDate] datetime2 NOT NULL,
    [GrantedById] nvarchar(450) NULL,
    CONSTRAINT [PK_DocumentPermissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DocumentPermissions_AspNetUsers_GrantedById] FOREIGN KEY ([GrantedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DocumentPermissions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DocumentPermissions_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [OrdiniTestate] (
    [ID] int NOT NULL IDENTITY,
    [CodiceCliente] int NOT NULL,
    [TipoOrdine] nvarchar(1) NOT NULL,
    [AnnoOrdine] smallint NOT NULL,
    [SerieOrdine] nvarchar(3) NOT NULL,
    [NumeroOrdine] int NOT NULL,
    [DataOrdine] datetime2 NOT NULL,
    [RiferimentoOrdine] nvarchar(50) NULL,
    [DataConsegna] datetime2 NULL,
    [CodiceAgente] smallint NOT NULL,
    [NoteTestata] nvarchar(max) NULL,
    CONSTRAINT [PK_OrdiniTestate] PRIMARY KEY ([ID]),
    CONSTRAINT [AK_OrdiniTestate_TipoOrdine_AnnoOrdine_SerieOrdine_NumeroOrdine] UNIQUE ([TipoOrdine], [AnnoOrdine], [SerieOrdine], [NumeroOrdine]),
    CONSTRAINT [FK_OrdiniTestate_AnagraficaClienti_CodiceCliente] FOREIGN KEY ([CodiceCliente]) REFERENCES [AnagraficaClienti] ([CodiceCliente]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OrdiniTestate_TabellaAgenti_CodiceAgente] FOREIGN KEY ([CodiceAgente]) REFERENCES [TabellaAgenti] ([CodiceAgente]) ON DELETE NO ACTION
);
GO


CREATE TABLE [OrdiniRighe] (
    [ID] int NOT NULL IDENTITY,
    [TipoOrdine] nvarchar(1) NOT NULL,
    [AnnoOrdine] smallint NOT NULL,
    [SerieOrdine] nvarchar(3) NOT NULL,
    [NumeroOrdine] int NOT NULL,
    [RigaOrdine] int NOT NULL,
    [CodiceMagazzino] smallint NOT NULL,
    [CodiceArticolo] nvarchar(50) NOT NULL,
    [DescrizioneArticolo] nvarchar(50) NULL,
    [DataConsegna] datetime2 NOT NULL,
    [UnitaMisura] nvarchar(3) NULL,
    [Quantita] decimal(18,4) NOT NULL,
    [UnitaMisuraColli] nvarchar(3) NULL,
    [NumeroColli] decimal(18,4) NOT NULL,
    [ColliEvasi] decimal(18,4) NOT NULL,
    [QuantitaEvasa] decimal(18,4) NOT NULL,
    [Prezzo] decimal(18,4) NOT NULL,
    [PercentualeInclusione] int NOT NULL,
    [NoteRiga] nvarchar(max) NULL,
    [ValoreRiga] money NOT NULL,
    CONSTRAINT [PK_OrdiniRighe] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_OrdiniRighe_AnagraficaArticoli_CodiceArticolo] FOREIGN KEY ([CodiceArticolo]) REFERENCES [AnagraficaArticoli] ([CodiceArticolo]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OrdiniRighe_OrdiniTestate_TipoOrdine_AnnoOrdine_SerieOrdine_NumeroOrdine] FOREIGN KEY ([TipoOrdine], [AnnoOrdine], [SerieOrdine], [NumeroOrdine]) REFERENCES [OrdiniTestate] ([TipoOrdine], [AnnoOrdine], [SerieOrdine], [NumeroOrdine]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrdiniRighe_TabellaMagazzini_CodiceMagazzino] FOREIGN KEY ([CodiceMagazzino]) REFERENCES [TabellaMagazzini] ([CodiceMagazzino]) ON DELETE NO ACTION
);
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdStato', N'Attivo', N'CodiceStato', N'DescrizioneStato', N'Ordine') AND [object_id] = OBJECT_ID(N'[StatiOP]'))
    SET IDENTITY_INSERT [StatiOP] ON;
INSERT INTO [StatiOP] ([IdStato], [Attivo], [CodiceStato], [DescrizioneStato], [Ordine])
VALUES (1, CAST(1 AS bit), N'EM', N'Emesso', 1),
(2, CAST(1 AS bit), N'PR', N'Produzione', 2),
(3, CAST(1 AS bit), N'CH', N'Chiuso', 4),
(4, CAST(1 AS bit), N'SO', N'Sospeso', 3);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdStato', N'Attivo', N'CodiceStato', N'DescrizioneStato', N'Ordine') AND [object_id] = OBJECT_ID(N'[StatiOP]'))
    SET IDENTITY_INSERT [StatiOP] OFF;
GO


CREATE INDEX [IX_AnagraficaClienti_CodiceAgente] ON [AnagraficaClienti] ([CodiceAgente]);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_CalendarioFermiCentriLavoro_CodiceCentro] ON [CalendarioFermiCentriLavoro] ([CodiceCentro]);
GO


CREATE INDEX [IX_CalendarioFermiCentriLavoro_DataFineFermo] ON [CalendarioFermiCentriLavoro] ([DataFineFermo]);
GO


CREATE INDEX [IX_CalendarioFermiCentriLavoro_DataInizioFermo] ON [CalendarioFermiCentriLavoro] ([DataInizioFermo]);
GO


CREATE INDEX [IX_CalendarioFermiCentriLavoro_TipoFermo] ON [CalendarioFermiCentriLavoro] ([TipoFermo]);
GO


CREATE INDEX [IX_CentriLavoro_Attivo] ON [CentriLavoro] ([Attivo]);
GO


CREATE INDEX [IX_CentriLavoro_DescrizioneCentro] ON [CentriLavoro] ([DescrizioneCentro]);
GO


CREATE INDEX [IX_DocumentPermissions_DocumentId] ON [DocumentPermissions] ([DocumentId]);
GO


CREATE INDEX [IX_DocumentPermissions_GrantedById] ON [DocumentPermissions] ([GrantedById]);
GO


CREATE INDEX [IX_DocumentPermissions_UserId] ON [DocumentPermissions] ([UserId]);
GO


CREATE INDEX [IX_Documents_CategoryId] ON [Documents] ([CategoryId]);
GO


CREATE INDEX [IX_Documents_DocumentCategoryId] ON [Documents] ([DocumentCategoryId]);
GO


CREATE INDEX [IX_Documents_UploadedById] ON [Documents] ([UploadedById]);
GO


CREATE INDEX [IX_Lavorazioni_Attivo] ON [Lavorazioni] ([Attivo]);
GO


CREATE INDEX [IX_Lavorazioni_DescrizioneLavorazione] ON [Lavorazioni] ([DescrizioneLavorazione]);
GO


CREATE INDEX [IX_ListaOP_ChiaveComposita] ON [ListaOP] ([TipoOrdine], [AnnoOrdine], [SerieOrdine], [NumeroOrdine]);
GO


CREATE INDEX [IX_ListaOP_CodiceArticolo] ON [ListaOP] ([CodiceArticolo]);
GO


CREATE INDEX [IX_ListaOP_CodiceCentro] ON [ListaOP] ([CodiceCentro]);
GO


CREATE INDEX [IX_ListaOP_CodiceLavorazione] ON [ListaOP] ([CodiceLavorazione]);
GO


CREATE INDEX [IX_ListaOP_DataInizioOP] ON [ListaOP] ([DataInizioOP]);
GO


CREATE INDEX [IX_ListaOP_IdOperatore] ON [ListaOP] ([IdOperatore]);
GO


CREATE INDEX [IX_ListaOP_IdStato] ON [ListaOP] ([IdStato]);
GO


CREATE INDEX [IX_ListaOP_Priorita] ON [ListaOP] ([Priorita]);
GO


CREATE UNIQUE INDEX [IX_Operatori_CodiceOperatore] ON [Operatori] ([CodiceOperatore]);
GO


CREATE INDEX [IX_Operatori_Email] ON [Operatori] ([Email]);
GO


CREATE INDEX [IX_Operatori_NomeCognome] ON [Operatori] ([Nome], [Cognome]);
GO


CREATE INDEX [IX_OrdiniRighe_ChiaveComposita] ON [OrdiniRighe] ([TipoOrdine], [AnnoOrdine], [SerieOrdine], [NumeroOrdine]);
GO


CREATE INDEX [IX_OrdiniRighe_CodiceArticolo] ON [OrdiniRighe] ([CodiceArticolo]);
GO


CREATE INDEX [IX_OrdiniRighe_CodiceMagazzino] ON [OrdiniRighe] ([CodiceMagazzino]);
GO


CREATE INDEX [IX_OrdiniRighe_DataConsegna] ON [OrdiniRighe] ([DataConsegna]);
GO


CREATE UNIQUE INDEX [IX_OrdiniTestate_ChiaveComposita] ON [OrdiniTestate] ([TipoOrdine], [AnnoOrdine], [SerieOrdine], [NumeroOrdine]);
GO


CREATE INDEX [IX_OrdiniTestate_CodiceAgente] ON [OrdiniTestate] ([CodiceAgente]);
GO


CREATE INDEX [IX_OrdiniTestate_CodiceCliente] ON [OrdiniTestate] ([CodiceCliente]);
GO


CREATE INDEX [IX_OrdiniTestate_DataOrdine] ON [OrdiniTestate] ([DataOrdine]);
GO


CREATE UNIQUE INDEX [IX_StatiOP_CodiceStato] ON [StatiOP] ([CodiceStato]);
GO


CREATE INDEX [IX_StatiOP_Ordine] ON [StatiOP] ([Ordine]);
GO


