IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250306134254_InitialCreate', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [DocumentCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_DocumentCategories] PRIMARY KEY ([Id])
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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250527132702_CreateAllTables', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250909151511_SyncOrdiniProduzioneTabelle', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250909152713_AddCentriLavoro', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Lavorazioni] (
    [IdLavorazione] int NOT NULL IDENTITY,
    [CodiceLavorazione] varchar(1) NOT NULL,
    [DescrizioneLavorazione] nvarchar(100) NULL,
    [Attivo] bit NOT NULL,
    [DataCreazione] datetime2 NOT NULL,
    [DataUltimaModifica] datetime2 NULL,
    CONSTRAINT [PK_Lavorazioni] PRIMARY KEY ([IdLavorazione])
);
GO

CREATE INDEX [IX_Lavorazioni_Attivo] ON [Lavorazioni] ([Attivo]);
GO

CREATE UNIQUE INDEX [IX_Lavorazioni_CodiceLavorazione] ON [Lavorazioni] ([CodiceLavorazione]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250910070740_AddLavorazioniTable', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_Lavorazioni_CodiceLavorazione] ON [Lavorazioni];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Lavorazioni]') AND [c].[name] = N'DescrizioneLavorazione');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Lavorazioni] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [Lavorazioni] SET [DescrizioneLavorazione] = N'' WHERE [DescrizioneLavorazione] IS NULL;
ALTER TABLE [Lavorazioni] ALTER COLUMN [DescrizioneLavorazione] nvarchar(100) NOT NULL;
ALTER TABLE [Lavorazioni] ADD DEFAULT N'' FOR [DescrizioneLavorazione];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Lavorazioni]') AND [c].[name] = N'CodiceLavorazione');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Lavorazioni] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Lavorazioni] ALTER COLUMN [CodiceLavorazione] varchar(1) NULL;
GO

CREATE UNIQUE INDEX [IX_Lavorazioni_CodiceLavorazione] ON [Lavorazioni] ([CodiceLavorazione]) WHERE [CodiceLavorazione] IS NOT NULL;
GO

CREATE INDEX [IX_Lavorazioni_DescrizioneLavorazione] ON [Lavorazioni] ([DescrizioneLavorazione]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250910090304_ModifyLavorazioniFields', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ListaOP] ADD [IdLavorazione] int NULL;
GO

                UPDATE ListaOP 
                SET IdLavorazione = (
                    SELECT TOP 1 IdLavorazione 
                    FROM Lavorazioni 
                    WHERE Attivo = 1 
                    ORDER BY IdLavorazione
                )
                WHERE IdLavorazione IS NULL
GO

CREATE INDEX [IX_ListaOP_IdLavorazione] ON [ListaOP] ([IdLavorazione]);
GO

ALTER TABLE [ListaOP] ADD CONSTRAINT [FK_ListaOP_Lavorazioni_IdLavorazione] FOREIGN KEY ([IdLavorazione]) REFERENCES [Lavorazioni] ([IdLavorazione]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250910151604_AddIdLavorazioneToListaOP_v2', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_ListaOP_IdLavorazione] ON [ListaOP];
DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'IdLavorazione');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var2 + '];');
UPDATE [ListaOP] SET [IdLavorazione] = 0 WHERE [IdLavorazione] IS NULL;
ALTER TABLE [ListaOP] ALTER COLUMN [IdLavorazione] int NOT NULL;
ALTER TABLE [ListaOP] ADD DEFAULT 0 FOR [IdLavorazione];
CREATE INDEX [IX_ListaOP_IdLavorazione] ON [ListaOP] ([IdLavorazione]);
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-11T17:23:50.4531098+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-16T17:23:50.4531106+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-21T17:23:50.4531109+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-26T17:23:50.4531113+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-31T17:23:50.4531116+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-05T17:23:50.4531120+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-07-22T17:23:50.4531123+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250910152352_MakeIdLavorazioneRequired', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'Quantita');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [Quantita] decimal(10,3) NOT NULL;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'QuantitaProdotta');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [QuantitaProdotta] decimal(10,3) NOT NULL;
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-16T11:48:42.3950091+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-21T11:48:42.3950097+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-26T11:48:42.3950100+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-31T11:48:42.3950103+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-05T11:48:42.3950106+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-10T11:48:42.3950111+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-07-27T11:48:42.3950113+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250915094844_ChangeQuantitaDecimalPrecision', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'TempoSetup');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [TempoSetup] real NULL;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'TempoEffettivo');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [TempoEffettivo] real NULL;
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'TempoCiclo');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [TempoCiclo] real NOT NULL;
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'QuantitaProdotta');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [QuantitaProdotta] decimal(10,3) NOT NULL;
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'Quantita');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [Quantita] decimal(10,3) NOT NULL;
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-16T14:29:29.1791790+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-21T14:29:29.1791802+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-26T14:29:29.1791811+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-31T14:29:29.1791820+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-05T14:29:29.1791823+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-10T14:29:29.1791829+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-07-27T14:29:29.1791835+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250915122930_ChangeTempoCicloTempoSetupToFloat', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [OrdiniRighe] ADD [PercentualeInclusione] int NOT NULL DEFAULT 0;
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-18T08:36:00.2300047+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-23T08:36:00.2300056+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-28T08:36:00.2300060+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-03T08:36:00.2300064+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-08T08:36:00.2300068+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-13T08:36:00.2300073+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-29T08:36:00.2300076+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018063601_AddPercentualeInclusioneToOrdiniRighe', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ProgressiviArticoli] ADD [OrdinatoFornitoriDataOdierna] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-18T08:57:07.4190106+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-23T08:57:07.4190112+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-28T08:57:07.4190115+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-03T08:57:07.4190118+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-08T08:57:07.4190121+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-13T08:57:07.4190124+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-29T08:57:07.4190127+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018065708_UpdateProgressiviArticoliRemoveFieldsAndRenameOrdinato', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

                -- Elimina constraint e colonna ImpegnatoTotale
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND c.name = 'ImpegnatoTotale';
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @constraintName + ']');
                END
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND name = 'ImpegnatoTotale')
                BEGIN
                    ALTER TABLE [ProgressiviArticoli] DROP COLUMN [ImpegnatoTotale];
                END
GO

                -- Elimina constraint e colonna Prenotato
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND c.name = 'Prenotato';
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @constraintName + ']');
                END
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND name = 'Prenotato')
                BEGIN
                    ALTER TABLE [ProgressiviArticoli] DROP COLUMN [Prenotato];
                END
GO

                -- Elimina constraint e colonna Impegnato
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND c.name = 'Impegnato';
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @constraintName + ']');
                END
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND name = 'Impegnato')
                BEGIN
                    ALTER TABLE [ProgressiviArticoli] DROP COLUMN [Impegnato];
                END
GO

                -- Elimina constraint e colonna Ordinato
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND c.name = 'Ordinato';
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @constraintName + ']');
                END
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressiviArticoli]') AND name = 'Ordinato')
                BEGIN
                    ALTER TABLE [ProgressiviArticoli] DROP COLUMN [Ordinato];
                END
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-18T09:07:23.6527560+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-23T09:07:23.6527572+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-28T09:07:23.6527575+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-03T09:07:23.6527578+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-08T09:07:23.6527581+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-13T09:07:23.6527583+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-29T09:07:23.6527586+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018070724_RemoveImpegnatoTotaleAndPrenotatoFromProgressiviArticoli', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProgressiviArticoli]') AND [c].[name] = N'OrdinatoFornitoriDataOdierna');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [ProgressiviArticoli] ALTER COLUMN [OrdinatoFornitoriDataOdierna] decimal(27,9) NOT NULL;
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProgressiviArticoli]') AND [c].[name] = N'Esistenza');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [ProgressiviArticoli] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [ProgressiviArticoli] ALTER COLUMN [Esistenza] decimal(27,9) NOT NULL;
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-18T09:15:37.0612894+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-23T09:15:37.0612900+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-28T09:15:37.0612903+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-03T09:15:37.0612906+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-08T09:15:37.0612909+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-13T09:15:37.0612912+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-29T09:15:37.0612914+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018071537_ChangeDecimalPrecisionProgressiviArticoli', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AnagraficaArticoli] ADD [MakeOrBuy] varchar(1) NULL;
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-18T09:36:07.1217888+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-23T09:36:07.1217894+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-28T09:36:07.1217898+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-03T09:36:07.1217901+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-08T09:36:07.1217904+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-13T09:36:07.1217907+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-29T09:36:07.1217911+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018073608_AddMakeOrBuyToAnagraficaArticoli', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_codiva');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_codiva];
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_colpre');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_colpre];
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_flevapre');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_flevapre];
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_prelist');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_prelist];
GO

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_preziva');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_preziva];
GO

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_prezvalc');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_prezvalc];
GO

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_provv');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_provv];
GO

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_quapre');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_quapre];
GO

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_scont1');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_scont1];
GO

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_scont2');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_scont2];
GO

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrdiniRighe]') AND [c].[name] = N'mo_scont3');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [OrdiniRighe] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [OrdiniRighe] DROP COLUMN [mo_scont3];
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-18T09:49:15.6372748+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-23T09:49:15.6372756+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-28T09:49:15.6372768+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-03T09:49:15.6372781+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-08T09:49:15.6372784+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-13T09:49:15.6372792+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-29T09:49:15.6372801+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018074916_RemoveFieldsFromOrdiniRighe', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[OrdiniRighe].[mo_quaeva]', N'QuantitaEvasa', N'COLUMN';
GO

EXEC sp_rename N'[OrdiniRighe].[mo_coleva]', N'ColliEvasi', N'COLUMN';
GO

EXEC sp_rename N'[OrdiniRighe].[mo_note]', N'NoteRiga', N'COLUMN';
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-18T09:57:02.4400225+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-23T09:57:02.4400232+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-28T09:57:02.4400235+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-03T09:57:02.4400238+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-08T09:57:02.4400242+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-13T09:57:02.4400245+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-29T09:57:02.4400248+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018075703_RenameColumnsInOrdiniRighe', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ListaOP] ADD [CodiceCentro] nvarchar(10) NULL;
GO

                UPDATE ListaOP 
                SET CodiceCentro = c.CodiceCentro
                FROM ListaOP l
                INNER JOIN CentriLavoro c ON l.IdCentroLavoro = c.IdCentroLavoro
                WHERE l.CodiceCentro IS NULL
GO

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'CodiceCentro');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var23 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [CodiceCentro] nvarchar(10) NOT NULL;
GO

                DECLARE @fkName NVARCHAR(200);
                SELECT @fkName = fk.name
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                WHERE fk.parent_object_id = OBJECT_ID(N'ListaOP')
                  AND c.name = 'IdCentroLavoro';
                IF @fkName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE ListaOP DROP CONSTRAINT [' + @fkName + ']');
                END
GO

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ListaOP_IdCentroLavoro' AND object_id = OBJECT_ID(N'ListaOP'))
                BEGIN
                    DROP INDEX [IX_ListaOP_IdCentroLavoro] ON [ListaOP];
                END
GO

ALTER TABLE [CentriLavoro] DROP CONSTRAINT [PK_CentriLavoro];
GO

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CentriLavoro_CodiceCentro' AND object_id = OBJECT_ID(N'CentriLavoro'))
                BEGIN
                    DROP INDEX [IX_CentriLavoro_CodiceCentro] ON [CentriLavoro];
                END
GO

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'IdCentroLavoro');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var24 + '];');
ALTER TABLE [ListaOP] DROP COLUMN [IdCentroLavoro];
GO

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CentriLavoro]') AND [c].[name] = N'IdCentroLavoro');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [CentriLavoro] DROP CONSTRAINT [' + @var25 + '];');
ALTER TABLE [CentriLavoro] DROP COLUMN [IdCentroLavoro];
GO

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CentriLavoro]') AND [c].[name] = N'CodiceCentro');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [CentriLavoro] DROP CONSTRAINT [' + @var26 + '];');
UPDATE [CentriLavoro] SET [CodiceCentro] = N'' WHERE [CodiceCentro] IS NULL;
ALTER TABLE [CentriLavoro] ALTER COLUMN [CodiceCentro] nvarchar(10) NOT NULL;
ALTER TABLE [CentriLavoro] ADD DEFAULT N'' FOR [CodiceCentro];
GO

ALTER TABLE [CentriLavoro] ADD CONSTRAINT [PK_CentriLavoro] PRIMARY KEY ([CodiceCentro]);
GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-18T10:18:42.7312634+02:00'
WHERE [IdLavorazione] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-23T10:18:42.7312644+02:00'
WHERE [IdLavorazione] = 2;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-09-28T10:18:42.7312649+02:00'
WHERE [IdLavorazione] = 3;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-03T10:18:42.7312653+02:00'
WHERE [IdLavorazione] = 4;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-08T10:18:42.7312658+02:00'
WHERE [IdLavorazione] = 5;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-10-13T10:18:42.7312663+02:00'
WHERE [IdLavorazione] = 6;
SELECT @@ROWCOUNT;

GO

UPDATE [Lavorazioni] SET [DataCreazione] = '2025-08-29T10:18:42.7312667+02:00'
WHERE [IdLavorazione] = 7;
SELECT @@ROWCOUNT;

GO

CREATE INDEX [IX_ListaOP_CodiceCentro] ON [ListaOP] ([CodiceCentro]);
GO

ALTER TABLE [ListaOP] ADD CONSTRAINT [FK_ListaOP_CentriLavoro_CodiceCentro] FOREIGN KEY ([CodiceCentro]) REFERENCES [CentriLavoro] ([CodiceCentro]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018081844_ChangeCentriLavoroPrimaryKeyToCodiceCentro', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

                -- Crea una tabella temporanea con codici univoci
                DECLARE @Chars VARCHAR(36) = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
                WITH RankedLavorazioni AS (
                    SELECT IdLavorazione, 
                           CodiceLavorazione,
                           ROW_NUMBER() OVER (ORDER BY IdLavorazione) - 1 AS RowNum
                    FROM Lavorazioni
                ),
                NewCodes AS (
                    SELECT IdLavorazione,
                           CodiceLavorazione,
                           CASE 
                               WHEN CodiceLavorazione IS NOT NULL AND CodiceLavorazione != '' 
                                    AND LEN(CodiceLavorazione) = 1
                               THEN CodiceLavorazione
                               ELSE SUBSTRING(@Chars, (RowNum % 36) + 1, 1)
                           END AS NewCode
                    FROM RankedLavorazioni
                )
                UPDATE L
                SET L.CodiceLavorazione = NC.NewCode
                FROM Lavorazioni L
                INNER JOIN NewCodes NC ON L.IdLavorazione = NC.IdLavorazione
                WHERE L.CodiceLavorazione IS NULL 
                   OR L.CodiceLavorazione = '' 
                   OR LEN(L.CodiceLavorazione) != 1
GO

                DECLARE @Chars VARCHAR(36) = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
                WITH Duplicates AS (
                    SELECT IdLavorazione,
                           CodiceLavorazione, 
                           ROW_NUMBER() OVER (PARTITION BY CodiceLavorazione ORDER BY IdLavorazione) AS RowNum,
                           COUNT(*) OVER (PARTITION BY CodiceLavorazione) AS DupCount
                    FROM Lavorazioni
                )
                UPDATE L
                SET L.CodiceLavorazione = SUBSTRING(@Chars, ((L.IdLavorazione - 1) % 36) + 1, 1)
                FROM Lavorazioni L
                INNER JOIN Duplicates D ON L.IdLavorazione = D.IdLavorazione
                WHERE D.DupCount > 1 AND D.RowNum > 1
GO

ALTER TABLE [ListaOP] ADD [CodiceLavorazione] varchar(1) NULL;
GO

                UPDATE ListaOP 
                SET CodiceLavorazione = LEFT(ISNULL(l.CodiceLavorazione, CAST(l.IdLavorazione AS VARCHAR(1))), 1)
                FROM ListaOP lo
                INNER JOIN Lavorazioni l ON lo.IdLavorazione = l.IdLavorazione
                WHERE lo.CodiceLavorazione IS NULL
GO

DECLARE @var27 sysname;
SELECT @var27 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'CodiceLavorazione');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var27 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [CodiceLavorazione] varchar(1) NOT NULL;
GO

                DECLARE @fkName NVARCHAR(200);
                SELECT @fkName = fk.name
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                WHERE fk.parent_object_id = OBJECT_ID(N'ListaOP')
                  AND c.name = 'IdLavorazione';
                IF @fkName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE ListaOP DROP CONSTRAINT [' + @fkName + ']');
                END
GO

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ListaOP_IdLavorazione' AND object_id = OBJECT_ID(N'ListaOP'))
                BEGIN
                    DROP INDEX [IX_ListaOP_IdLavorazione] ON [ListaOP];
                END
GO

ALTER TABLE [Lavorazioni] DROP CONSTRAINT [PK_Lavorazioni];
GO

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Lavorazioni_CodiceLavorazione' AND object_id = OBJECT_ID(N'Lavorazioni'))
                BEGIN
                    DROP INDEX [IX_Lavorazioni_CodiceLavorazione] ON [Lavorazioni];
                END
GO

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'IdLavorazione');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var28 + '];');
ALTER TABLE [ListaOP] DROP COLUMN [IdLavorazione];
GO

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Lavorazioni]') AND [c].[name] = N'IdLavorazione');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [Lavorazioni] DROP CONSTRAINT [' + @var29 + '];');
ALTER TABLE [Lavorazioni] DROP COLUMN [IdLavorazione];
GO

                UPDATE Lavorazioni
                SET CodiceLavorazione = LEFT(CodiceLavorazione, 1)
                WHERE LEN(CodiceLavorazione) > 1 OR CodiceLavorazione IS NULL
GO

DECLARE @var30 sysname;
SELECT @var30 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Lavorazioni]') AND [c].[name] = N'CodiceLavorazione');
IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [Lavorazioni] DROP CONSTRAINT [' + @var30 + '];');
UPDATE [Lavorazioni] SET [CodiceLavorazione] = '' WHERE [CodiceLavorazione] IS NULL;
ALTER TABLE [Lavorazioni] ALTER COLUMN [CodiceLavorazione] varchar(1) NOT NULL;
ALTER TABLE [Lavorazioni] ADD DEFAULT '' FOR [CodiceLavorazione];
GO

ALTER TABLE [Lavorazioni] ADD CONSTRAINT [PK_Lavorazioni] PRIMARY KEY ([CodiceLavorazione]);
GO

CREATE INDEX [IX_ListaOP_CodiceLavorazione] ON [ListaOP] ([CodiceLavorazione]);
GO

ALTER TABLE [ListaOP] ADD CONSTRAINT [FK_ListaOP_Lavorazioni_CodiceLavorazione] FOREIGN KEY ([CodiceLavorazione]) REFERENCES [Lavorazioni] ([CodiceLavorazione]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018084704_ChangeLavorazioniPrimaryKeyToCodiceLavorazione', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

                DECLARE @fkName NVARCHAR(200);
                SELECT @fkName = fk.name
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                WHERE fk.parent_object_id = OBJECT_ID(N'OrdiniTestate')
                  AND c.name = 'td_magaz';
                IF @fkName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE OrdiniTestate DROP CONSTRAINT [' + @fkName + ']');
                END
GO

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrdiniTestate_td_magaz' AND object_id = OBJECT_ID(N'OrdiniTestate'))
                BEGIN
                    DROP INDEX [IX_OrdiniTestate_td_magaz] ON [OrdiniTestate];
                END
GO

                DECLARE @constraintName NVARCHAR(200);
                -- Elimina constraint e colonna TotaleColli
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[OrdiniTestate]') AND c.name = 'TotaleColli';
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [OrdiniTestate] DROP CONSTRAINT [' + @constraintName + ']');
                END
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrdiniTestate]') AND name = 'TotaleColli')
                BEGIN
                    ALTER TABLE [OrdiniTestate] DROP COLUMN [TotaleColli];
                END
                -- Elimina constraint e colonna td_magaz
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[OrdiniTestate]') AND c.name = 'td_magaz';
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [OrdiniTestate] DROP CONSTRAINT [' + @constraintName + ']');
                END
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrdiniTestate]') AND name = 'td_magaz')
                BEGIN
                    ALTER TABLE [OrdiniTestate] DROP COLUMN [td_magaz];
                END
                -- Elimina constraint e colonna td_tipobf
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE c.object_id = OBJECT_ID(N'[OrdiniTestate]') AND c.name = 'td_tipobf';
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [OrdiniTestate] DROP CONSTRAINT [' + @constraintName + ']');
                END
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrdiniTestate]') AND name = 'td_tipobf')
                BEGIN
                    ALTER TABLE [OrdiniTestate] DROP COLUMN [td_tipobf];
                END
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018115410_RemoveFieldsFromOrdiniTestate', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[OrdiniTestate].[td_riferim]', N'RiferimentoOrdine', N'COLUMN';
GO

EXEC sp_rename N'[OrdiniTestate].[td_note]', N'NoteTestata', N'COLUMN';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018115804_RenameColumnsInOrdiniTestate', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ListaOP] ADD [Modificato] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018120113_AddModificatoToListaOP', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var31 sysname;
SELECT @var31 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AnagraficaClienti]') AND [c].[name] = N'an_faxtlx');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [AnagraficaClienti] DROP CONSTRAINT [' + @var31 + '];');
ALTER TABLE [AnagraficaClienti] DROP COLUMN [an_faxtlx];
GO

EXEC sp_rename N'[AnagraficaClienti].[an_pariva]', N'PartitaIva', N'COLUMN';
GO

EXEC sp_rename N'[AnagraficaClienti].[an_codfis]', N'CodiceFiscale', N'COLUMN';
GO

EXEC sp_rename N'[AnagraficaClienti].[an_tipo]', N'Tipo', N'COLUMN';
GO

EXEC sp_rename N'[AnagraficaClienti].[an_descr2]', N'DescrizioneUlteriore', N'COLUMN';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018120858_UpdateAnagraficaClientiFields', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var32 sysname;
SELECT @var32 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AnagraficaFornitori]') AND [c].[name] = N'an_faxtlx');
IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [AnagraficaFornitori] DROP CONSTRAINT [' + @var32 + '];');
ALTER TABLE [AnagraficaFornitori] DROP COLUMN [an_faxtlx];
GO

EXEC sp_rename N'[AnagraficaFornitori].[an_pariva]', N'PartitaIva', N'COLUMN';
GO

EXEC sp_rename N'[AnagraficaFornitori].[an_codfis]', N'CodiceFiscale', N'COLUMN';
GO

EXEC sp_rename N'[AnagraficaFornitori].[an_tipo]', N'Tipo', N'COLUMN';
GO

EXEC sp_rename N'[AnagraficaFornitori].[an_descr2]', N'DescrizioneUlteriore', N'COLUMN';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018121354_UpdateAnagraficaFornitoriFields', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[ArticoliSostitutivi].[apa_note]', N'Note', N'COLUMN';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018121630_RenameNoteColumnInArticoliSostitutivi', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
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

CREATE INDEX [IX_CalendarioFermiCentriLavoro_CodiceCentro] ON [CalendarioFermiCentriLavoro] ([CodiceCentro]);
GO

CREATE INDEX [IX_CalendarioFermiCentriLavoro_DataFineFermo] ON [CalendarioFermiCentriLavoro] ([DataFineFermo]);
GO

CREATE INDEX [IX_CalendarioFermiCentriLavoro_DataInizioFermo] ON [CalendarioFermiCentriLavoro] ([DataInizioFermo]);
GO

CREATE INDEX [IX_CalendarioFermiCentriLavoro_TipoFermo] ON [CalendarioFermiCentriLavoro] ([TipoFermo]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251018122342_CreateCalendarioFermiCentriLavoro', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ListaOP] DROP CONSTRAINT [FK_ListaOP_Lavorazioni_CodiceLavorazione];
GO

ALTER TABLE [Lavorazioni] DROP CONSTRAINT [PK_Lavorazioni];
GO

DROP INDEX [IX_ListaOP_CodiceLavorazione] ON [ListaOP];
DECLARE @var33 sysname;
SELECT @var33 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'CodiceLavorazione');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var33 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [CodiceLavorazione] smallint NOT NULL;
CREATE INDEX [IX_ListaOP_CodiceLavorazione] ON [ListaOP] ([CodiceLavorazione]);
GO

DECLARE @var34 sysname;
SELECT @var34 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Lavorazioni]') AND [c].[name] = N'CodiceLavorazione');
IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [Lavorazioni] DROP CONSTRAINT [' + @var34 + '];');
ALTER TABLE [Lavorazioni] ALTER COLUMN [CodiceLavorazione] smallint NOT NULL;
GO

ALTER TABLE [Lavorazioni] ADD CONSTRAINT [PK_Lavorazioni] PRIMARY KEY ([CodiceLavorazione]);
GO

ALTER TABLE [ListaOP] ADD CONSTRAINT [FK_ListaOP_Lavorazioni_CodiceLavorazione] FOREIGN KEY ([CodiceLavorazione]) REFERENCES [Lavorazioni] ([CodiceLavorazione]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251020081123_ChangeCodiceLavorazioneToSmallInt', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename 'OrdiniRighe.mo_magaz', 'CodiceMagazzino', 'COLUMN'
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251020085555_RenameColumnMoMagazToCodiceMagazzino', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var35 sysname;
SELECT @var35 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListaOP]') AND [c].[name] = N'Modificato');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [ListaOP] DROP CONSTRAINT [' + @var35 + '];');
ALTER TABLE [ListaOP] ALTER COLUMN [Modificato] int NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251027083712_ChangeModificatoToInt', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [TempiAsciugatura] (
    [IdMese] int NOT NULL IDENTITY,
    [Mese] nvarchar(20) NOT NULL,
    [GiorniAsciugatura] int NOT NULL,
    CONSTRAINT [PK_TempiAsciugatura] PRIMARY KEY ([IdMese])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251101095836_AddTempiAsciugaturaTable', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdMese', N'Mese', N'GiorniAsciugatura') AND [object_id] = OBJECT_ID(N'[TempiAsciugatura]'))
    SET IDENTITY_INSERT [TempiAsciugatura] ON;
INSERT INTO [TempiAsciugatura] ([IdMese], [Mese], [GiorniAsciugatura])
VALUES (1, N'Gennaio', 0),
(2, N'Febbraio', 0),
(3, N'Marzo', 0),
(4, N'Aprile', 0),
(5, N'Maggio', 0),
(6, N'Giugno', 0),
(7, N'Luglio', 0),
(8, N'Agosto', 0),
(9, N'Settembre', 0),
(10, N'Ottobre', 0),
(11, N'Novembre', 0),
(12, N'Dicembre', 0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdMese', N'Mese', N'GiorniAsciugatura') AND [object_id] = OBJECT_ID(N'[TempiAsciugatura]'))
    SET IDENTITY_INSERT [TempiAsciugatura] OFF;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251101100348_SeedTempiAsciugatura', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 10 
                WHERE TipoFermo = 0; -- WeekEnd: 0 -> temp 10
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 11 
                WHERE TipoFermo = 1; -- Festivo: 1 -> temp 11
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 12 
                WHERE TipoFermo = 2; -- TurnoNotturno: 2 -> temp 12
GO

                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 2 
                WHERE TipoFermo = 10; -- WeekEnd: temp 10 -> 2
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 4 
                WHERE TipoFermo = 11; -- Festivo: temp 11 -> 4
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 1 
                WHERE TipoFermo = 12; -- TurnoNotturno: temp 12 -> 1
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251101165920_AggiornaTipoFermoEnum', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AspNetUsers] ADD [CodiceAgente] smallint NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107101628_AddCodiceAgenteToApplicationUser', N'8.0.2');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Resources] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [DisplayName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [MenuIcon] nvarchar(50) NULL,
    [MenuOrder] int NOT NULL,
    [ParentResourceId] int NULL,
    [IsMenuGroup] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [IsConfigured] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [CreatedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_Resources] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Resources_Resources_ParentResourceId] FOREIGN KEY ([ParentResourceId]) REFERENCES [Resources] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [UserDataFilters] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ResourceName] nvarchar(100) NOT NULL,
    [FilterType] nvarchar(50) NOT NULL,
    [FilterValue] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_UserDataFilters] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserDataFilters_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Permissions] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ResourceId] int NOT NULL,
    [CanView] bit NOT NULL,
    [CanCreate] bit NOT NULL,
    [CanEdit] bit NOT NULL,
    [CanDelete] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [ModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Permissions_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Permissions_Resources_ResourceId] FOREIGN KEY ([ResourceId]) REFERENCES [Resources] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Permissions_ResourceId] ON [Permissions] ([ResourceId]);
GO

CREATE UNIQUE INDEX [IX_Permissions_RoleId_ResourceId] ON [Permissions] ([RoleId], [ResourceId]);
GO

CREATE UNIQUE INDEX [IX_Resources_Name] ON [Resources] ([Name]);
GO

CREATE INDEX [IX_Resources_ParentResourceId] ON [Resources] ([ParentResourceId]);
GO

CREATE INDEX [IX_UserDataFilters_UserId_ResourceName] ON [UserDataFilters] ([UserId], [ResourceName]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107162544_AddPermissionSystem', N'8.0.2');
GO

COMMIT;
GO

