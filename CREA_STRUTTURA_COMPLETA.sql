-- ============================================================================
-- SCRIPT CREAZIONE STRUTTURA COMPLETA DATABASE AIDBMASTER
-- ============================================================================
-- Script manuale per creare TUTTE le tabelle necessarie
-- IDEMPOTENTE: Può essere eseguito più volte senza errori
-- ============================================================================

USE AIDBMASTER;
GO

SET NOCOUNT ON;
PRINT '============================================================================';
PRINT 'CREAZIONE STRUTTURA COMPLETA DATABASE AIDBMASTER';
PRINT 'Inizio: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================================';
PRINT '';

-- ============================================================================
-- IDENTITY FRAMEWORK TABLES
-- ============================================================================

PRINT '1️⃣ CREAZIONE TABELLE IDENTITY FRAMEWORK...';
PRINT '';

-- AspNetRoles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(256) NULL,
        [NormalizedName] NVARCHAR(256) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL
    );
    CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
    PRINT '   ✅ AspNetRoles creata';
END
ELSE PRINT '   ⏭️  AspNetRoles già esiste';

-- AspNetUsers
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [UserName] NVARCHAR(256) NULL,
        [NormalizedUserName] NVARCHAR(256) NULL,
        [Email] NVARCHAR(256) NULL,
        [NormalizedEmail] NVARCHAR(256) NULL,
        [EmailConfirmed] BIT NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NULL,
        [SecurityStamp] NVARCHAR(MAX) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        [PhoneNumber] NVARCHAR(MAX) NULL,
        [PhoneNumberConfirmed] BIT NOT NULL,
        [TwoFactorEnabled] BIT NOT NULL,
        [LockoutEnd] DATETIMEOFFSET(7) NULL,
        [LockoutEnabled] BIT NOT NULL,
        [AccessFailedCount] INT NOT NULL,
        [FirstName] NVARCHAR(100) NULL,
        [LastName] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        [CodiceAgente] SMALLINT NULL
    );
    CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
    PRINT '   ✅ AspNetUsers creata';
END
ELSE
BEGIN
    -- Aggiungi colonne se mancano
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'CodiceAgente')
    BEGIN
        ALTER TABLE [AspNetUsers] ADD [CodiceAgente] SMALLINT NULL;
        PRINT '   ✅ Colonna CodiceAgente aggiunta a AspNetUsers';
    END
    PRINT '   ⏭️  AspNetUsers già esiste';
END

-- AspNetUserRoles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        PRIMARY KEY ([UserId], [RoleId]),
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
    PRINT '   ✅ AspNetUserRoles creata';
END
ELSE PRINT '   ⏭️  AspNetUserRoles già esiste';

-- AspNetUserClaims
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
    PRINT '   ✅ AspNetUserClaims creata';
END
ELSE PRINT '   ⏭️  AspNetUserClaims già esiste';

-- AspNetUserLogins
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider] NVARCHAR(450) NOT NULL,
        [ProviderKey] NVARCHAR(450) NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX) NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        PRIMARY KEY ([LoginProvider], [ProviderKey]),
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
    PRINT '   ✅ AspNetUserLogins creata';
END
ELSE PRINT '   ⏭️  AspNetUserLogins già esiste';

-- AspNetUserTokens
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId] NVARCHAR(450) NOT NULL,
        [LoginProvider] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(450) NOT NULL,
        [Value] NVARCHAR(MAX) NULL,
        PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT '   ✅ AspNetUserTokens creata';
END
ELSE PRINT '   ⏭️  AspNetUserTokens già esiste';

-- AspNetRoleClaims
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RoleId] NVARCHAR(450) NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
    PRINT '   ✅ AspNetRoleClaims creata';
END
ELSE PRINT '   ⏭️  AspNetRoleClaims già esiste';

PRINT '';
PRINT '✅ Tabelle Identity completate';
PRINT '';

-- ============================================================================
-- SISTEMA PERMESSI E RISORSE
-- ============================================================================

PRINT '2️⃣ CREAZIONE TABELLE SISTEMA PERMESSI...';
PRINT '';

-- Resources
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resources')
BEGIN
    CREATE TABLE [dbo].[Resources] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [DisplayName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [MenuIcon] NVARCHAR(50) NULL,
        [MenuOrder] INT NOT NULL DEFAULT 0,
        [ParentResourceId] INT NULL,
        [IsMenuGroup] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsConfigured] BIT NOT NULL DEFAULT 0,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(450) NULL,
        FOREIGN KEY ([ParentResourceId]) REFERENCES [Resources] ([Id])
    );
    CREATE INDEX [IX_Resources_ParentResourceId] ON [Resources] ([ParentResourceId]);
    CREATE INDEX [IX_Resources_Name] ON [Resources] ([Name]);
    PRINT '   ✅ Resources creata';
END
ELSE
BEGIN
    -- Verifica colonne
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Resources') AND name = 'IsActive')
    BEGIN
        ALTER TABLE [Resources] ADD [IsActive] BIT NOT NULL DEFAULT 1;
        PRINT '   ✅ Colonna IsActive aggiunta a Resources';
    END
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Resources') AND name = 'CreatedDate')
    BEGIN
        ALTER TABLE [Resources] ADD [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE();
        PRINT '   ✅ Colonna CreatedDate aggiunta a Resources';
    END
    PRINT '   ⏭️  Resources già esiste';
END

-- Permissions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
BEGIN
    CREATE TABLE [dbo].[Permissions] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RoleId] NVARCHAR(450) NOT NULL,
        [ResourceId] INT NOT NULL,
        [CanView] BIT NOT NULL DEFAULT 0,
        [CanCreate] BIT NOT NULL DEFAULT 0,
        [CanEdit] BIT NOT NULL DEFAULT 0,
        [CanDelete] BIT NOT NULL DEFAULT 0,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        [ModifiedDate] DATETIME2(7) NULL,
        [ModifiedBy] NVARCHAR(450) NULL,
        FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([ResourceId]) REFERENCES [Resources] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Permissions_RoleId] ON [Permissions] ([RoleId]);
    CREATE INDEX [IX_Permissions_ResourceId] ON [Permissions] ([ResourceId]);
    PRINT '   ✅ Permissions creata';
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'CreatedDate')
    BEGIN
        ALTER TABLE [Permissions] ADD [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE();
        PRINT '   ✅ Colonna CreatedDate aggiunta a Permissions';
    END
    PRINT '   ⏭️  Permissions già esiste';
END

-- UserDataFilters
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserDataFilters')
BEGIN
    CREATE TABLE [dbo].[UserDataFilters] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [EntityName] NVARCHAR(100) NOT NULL,
        [FilterType] NVARCHAR(50) NOT NULL,
        [FilterValue] NVARCHAR(500) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_UserDataFilters_UserId] ON [UserDataFilters] ([UserId]);
    PRINT '   ✅ UserDataFilters creata';
END
ELSE PRINT '   ⏭️  UserDataFilters già esiste';

PRINT '';
PRINT '✅ Tabelle Sistema Permessi completate';
PRINT '';

-- ============================================================================
-- CONTINUA NEL PROSSIMO BLOCCO (ANAGRAFICHE E BUSINESS)
-- ============================================================================

PRINT '3️⃣ CREAZIONE TABELLE ANAGRAFICHE...';
PRINT '';

-- Qui continuo con tutte le altre tabelle...
-- AnagraficaClienti, AnagraficaArticoli, OrdiniTestate, ListaOP, ecc.

PRINT '';
PRINT '============================================================================';
PRINT 'SCRIPT COMPLETATO PARZIALMENTE (parte 1/3)';
PRINT 'Fine: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================================';

GO

