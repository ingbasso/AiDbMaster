-- =====================================================
-- SCRIPT CONFIGURAZIONE PERMESSI DATABASE AIDBMASTER
-- Server: SVRGEST
-- Database: AIDBMASTER
-- Applicazione: AiDbMaster ASP.NET Core 8.0
-- =====================================================

USE [AIDBMASTER]
GO

-- =====================================================
-- 1. CREAZIONE UTENTE DEDICATO PER L'APPLICAZIONE
-- =====================================================

-- Verifica se l'utente esiste già
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'AiDbMasterApp')
BEGIN
    -- Crea un utente dedicato per l'applicazione (più sicuro dell'account sa)
    CREATE USER [AiDbMasterApp] FOR LOGIN [AiDbMasterApp]
    PRINT 'Utente AiDbMasterApp creato con successo'
END
ELSE
BEGIN
    PRINT 'Utente AiDbMasterApp già esistente'
END
GO

-- =====================================================
-- 2. PERMESSI SULLE TABELLE IDENTITY (ASP.NET Core)
-- =====================================================

-- Tabelle di ASP.NET Core Identity
DECLARE @IdentityTables TABLE (TableName NVARCHAR(128))
INSERT INTO @IdentityTables VALUES 
    ('AspNetUsers'),
    ('AspNetRoles'),
    ('AspNetUserRoles'),
    ('AspNetUserClaims'),
    ('AspNetUserLogins'),
    ('AspNetUserTokens'),
    ('AspNetRoleClaims')

DECLARE @TableName NVARCHAR(128)
DECLARE identity_cursor CURSOR FOR SELECT TableName FROM @IdentityTables

OPEN identity_cursor
FETCH NEXT FROM identity_cursor INTO @TableName

WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = @TableName)
    BEGIN
        -- Permessi completi sulle tabelle Identity
        EXEC('GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[' + @TableName + '] TO [AiDbMasterApp]')
        PRINT 'Permessi concessi su tabella: ' + @TableName
    END
    ELSE
    BEGIN
        PRINT 'ATTENZIONE: Tabella non trovata: ' + @TableName
    END
    
    FETCH NEXT FROM identity_cursor INTO @TableName
END

CLOSE identity_cursor
DEALLOCATE identity_cursor
GO

-- =====================================================
-- 3. PERMESSI SULLE TABELLE APPLICAZIONE
-- =====================================================

-- Tabelle dell'applicazione con permessi completi (lettura/scrittura)
DECLARE @AppTables TABLE (TableName NVARCHAR(128), PermissionType NVARCHAR(50))
INSERT INTO @AppTables VALUES 
    ('Documents', 'FULL'),                    -- Gestione documenti
    ('DocumentCategories', 'FULL'),           -- Categorie documenti
    ('DocumentPermissions', 'FULL'),          -- Permessi documenti
    ('__EFMigrationsHistory', 'FULL')         -- Entity Framework migrations

-- Tabelle di sola lettura (dati aziendali)
INSERT INTO @AppTables VALUES 
    ('AnagraficaArticoli', 'READ'),           -- Anagrafica articoli
    ('AnagraficaClienti', 'READ'),            -- Anagrafica clienti
    ('AnagraficaFornitori', 'READ'),          -- Anagrafica fornitori
    ('ArticoliSostitutivi', 'READ'),          -- Articoli sostitutivi
    ('ProgressiviArticoli', 'READ'),          -- Progressivi articoli
    ('TabellaAgenti', 'READ'),                -- Tabella agenti
    ('TabellaMagazzini', 'READ'),             -- Tabella magazzini
    ('OrdiniTestate', 'READ'),                -- Testate ordini
    ('OrdiniRighe', 'READ')                   -- Righe ordini

DECLARE @AppTableName NVARCHAR(128), @PermType NVARCHAR(50)
DECLARE app_cursor CURSOR FOR SELECT TableName, PermissionType FROM @AppTables

OPEN app_cursor
FETCH NEXT FROM app_cursor INTO @AppTableName, @PermType

WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = @AppTableName)
    BEGIN
        IF @PermType = 'FULL'
        BEGIN
            -- Permessi completi
            EXEC('GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[' + @AppTableName + '] TO [AiDbMasterApp]')
            PRINT 'Permessi COMPLETI concessi su: ' + @AppTableName
        END
        ELSE
        BEGIN
            -- Solo lettura
            EXEC('GRANT SELECT ON [dbo].[' + @AppTableName + '] TO [AiDbMasterApp]')
            PRINT 'Permessi LETTURA concessi su: ' + @AppTableName
        END
    END
    ELSE
    BEGIN
        PRINT 'ATTENZIONE: Tabella non trovata: ' + @AppTableName
    END
    
    FETCH NEXT FROM app_cursor INTO @AppTableName, @PermType
END

CLOSE app_cursor
DEALLOCATE app_cursor
GO

-- =====================================================
-- 4. PERMESSI SULLE STORED PROCEDURE E FUNZIONI
-- =====================================================

-- Concedi permessi di esecuzione su tutte le stored procedure
DECLARE @ProcName NVARCHAR(128)
DECLARE proc_cursor CURSOR FOR 
    SELECT ROUTINE_NAME 
    FROM INFORMATION_SCHEMA.ROUTINES 
    WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_SCHEMA = 'dbo'

OPEN proc_cursor
FETCH NEXT FROM proc_cursor INTO @ProcName

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('GRANT EXECUTE ON [dbo].[' + @ProcName + '] TO [AiDbMasterApp]')
    PRINT 'Permesso EXECUTE concesso su stored procedure: ' + @ProcName
    
    FETCH NEXT FROM proc_cursor INTO @ProcName
END

CLOSE proc_cursor
DEALLOCATE proc_cursor
GO

-- Concedi permessi di esecuzione su tutte le funzioni
DECLARE @FuncName NVARCHAR(128)
DECLARE func_cursor CURSOR FOR 
    SELECT ROUTINE_NAME 
    FROM INFORMATION_SCHEMA.ROUTINES 
    WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_SCHEMA = 'dbo'

OPEN func_cursor
FETCH NEXT FROM func_cursor INTO @FuncName

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('GRANT EXECUTE ON [dbo].[' + @FuncName + '] TO [AiDbMasterApp]')
    PRINT 'Permesso EXECUTE concesso su funzione: ' + @FuncName
    
    FETCH NEXT FROM func_cursor INTO @FuncName
END

CLOSE func_cursor
DEALLOCATE func_cursor
GO

-- =====================================================
-- 5. PERMESSI SPECIALI PER ENTITY FRAMEWORK
-- =====================================================

-- Permessi per la creazione di tabelle (necessario per le migrazioni)
GRANT CREATE TABLE TO [AiDbMasterApp]
PRINT 'Permesso CREATE TABLE concesso'

-- Permessi per modificare la struttura delle tabelle
GRANT ALTER ON SCHEMA::dbo TO [AiDbMasterApp]
PRINT 'Permesso ALTER SCHEMA concesso'

-- Permessi per visualizzare i metadati del database
GRANT VIEW DEFINITION TO [AiDbMasterApp]
PRINT 'Permesso VIEW DEFINITION concesso'

-- Permessi per accedere alle viste di sistema
GRANT SELECT ON sys.tables TO [AiDbMasterApp]
GRANT SELECT ON sys.columns TO [AiDbMasterApp]
GRANT SELECT ON sys.indexes TO [AiDbMasterApp]
GRANT SELECT ON sys.foreign_keys TO [AiDbMasterApp]
GRANT SELECT ON INFORMATION_SCHEMA.TABLES TO [AiDbMasterApp]
GRANT SELECT ON INFORMATION_SCHEMA.COLUMNS TO [AiDbMasterApp]
PRINT 'Permessi su viste di sistema concessi'
GO

-- =====================================================
-- 6. CONFIGURAZIONE SICUREZZA AGGIUNTIVA
-- =====================================================

-- Nega permessi pericolosi
DENY DROP ANY TABLE TO [AiDbMasterApp]
DENY DROP ANY DATABASE TO [AiDbMasterApp]
DENY SHUTDOWN TO [AiDbMasterApp]
DENY CONTROL SERVER TO [AiDbMasterApp]
PRINT 'Permessi pericolosi negati per sicurezza'
GO

-- =====================================================
-- 7. VERIFICA PERMESSI CONCESSI
-- =====================================================

PRINT '============================================='
PRINT 'RIEPILOGO PERMESSI UTENTE AiDbMasterApp:'
PRINT '============================================='

-- Mostra tutti i permessi concessi all'utente
SELECT 
    p.state_desc AS PermissionState,
    p.permission_name AS PermissionName,
    s.name AS SecurableName,
    pr.name AS PrincipalName,
    s.type_desc AS SecurableType
FROM sys.database_permissions p
    LEFT JOIN sys.objects s ON p.major_id = s.object_id
    LEFT JOIN sys.database_principals pr ON p.grantee_principal_id = pr.principal_id
WHERE pr.name = 'AiDbMasterApp'
ORDER BY p.permission_name, s.name
GO

-- =====================================================
-- 8. SCRIPT ALTERNATIVO CON ACCOUNT SA (SE NECESSARIO)
-- =====================================================

PRINT '============================================='
PRINT 'CONFIGURAZIONE ALTERNATIVA CON ACCOUNT SA:'
PRINT '============================================='
PRINT 'Se preferisci continuare ad usare l''account SA,'
PRINT 'assicurati che:'
PRINT '1. L''account SA sia abilitato'
PRINT '2. La password sia sicura e aggiornata'
PRINT '3. L''accesso SA sia limitato solo all''applicazione'
PRINT ''
PRINT 'RACCOMANDAZIONE: Usa l''utente AiDbMasterApp creato'
PRINT 'sopra per maggiore sicurezza.'
GO

-- =====================================================
-- 9. ISTRUZIONI PER L'AGGIORNAMENTO CONNECTION STRING
-- =====================================================

PRINT '============================================='
PRINT 'AGGIORNAMENTO CONNECTION STRING:'
PRINT '============================================='
PRINT 'Per usare il nuovo utente, aggiorna la connection string in appsettings.Production.json:'
PRINT ''
PRINT 'OPZIONE 1 - Utente dedicato (RACCOMANDATO):'
PRINT '"DefaultConnection": "Data Source=SVRGEST;Initial Catalog=AIDBMASTER;User ID=AiDbMasterApp;Password=[PASSWORD_SICURA];TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False"'
PRINT ''
PRINT 'OPZIONE 2 - Account SA (attuale):'
PRINT '"DefaultConnection": "Data Source=SVRGEST;Initial Catalog=AIDBMASTER;User ID=sa;Password=Novoincipient3!;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False"'
PRINT ''
PRINT 'NOTA: Ricorda di creare il LOGIN per l''utente AiDbMasterApp se non esiste:'
PRINT 'CREATE LOGIN [AiDbMasterApp] WITH PASSWORD = ''[PASSWORD_SICURA]'', CHECK_POLICY = ON'
GO

-- =====================================================
-- 10. TEST CONNESSIONE
-- =====================================================

PRINT '============================================='
PRINT 'TEST PERMESSI COMPLETATO'
PRINT '============================================='
PRINT 'Verifica che l''applicazione possa:'
PRINT '✓ Connettersi al database'
PRINT '✓ Leggere le tabelle anagrafiche'
PRINT '✓ Gestire utenti e ruoli (Identity)'
PRINT '✓ Caricare e gestire documenti'
PRINT '✓ Eseguire le migrazioni Entity Framework'
PRINT ''
PRINT 'In caso di problemi, controlla i log dell''applicazione'
PRINT 'e verifica che tutte le tabelle esistano nel database.'
GO

