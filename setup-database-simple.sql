-- =====================================================
-- SCRIPT SEMPLIFICATO - VERIFICA DATABASE AIDBMASTER
-- Server: SVRGEST
-- Database: AIDBMASTER
-- Account: sa (esistente)
-- =====================================================

USE [AIDBMASTER]
GO

PRINT '============================================='
PRINT 'VERIFICA CONFIGURAZIONE DATABASE AIDBMASTER'
PRINT 'Server: SVRGEST'
PRINT 'Account: sa'
PRINT '============================================='

-- =====================================================
-- 1. VERIFICA ESISTENZA DATABASE
-- =====================================================

IF DB_ID('AIDBMASTER') IS NOT NULL
BEGIN
    PRINT '✓ Database AIDBMASTER esistente'
END
ELSE
BEGIN
    PRINT '✗ ERRORE: Database AIDBMASTER non trovato!'
    PRINT 'Creare il database prima di continuare.'
END
GO

-- =====================================================
-- 2. VERIFICA TABELLE PRINCIPALI
-- =====================================================

PRINT ''
PRINT 'Verifica tabelle principali:'

-- Tabelle Identity ASP.NET Core
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
DECLARE @TablesFound INT = 0
DECLARE @TablesExpected INT = 7

DECLARE identity_cursor CURSOR FOR SELECT TableName FROM @IdentityTables
OPEN identity_cursor
FETCH NEXT FROM identity_cursor INTO @TableName

WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = @TableName)
    BEGIN
        PRINT '✓ Tabella Identity: ' + @TableName
        SET @TablesFound = @TablesFound + 1
    END
    ELSE
    BEGIN
        PRINT '✗ Tabella mancante: ' + @TableName
    END
    
    FETCH NEXT FROM identity_cursor INTO @TableName
END

CLOSE identity_cursor
DEALLOCATE identity_cursor

-- Tabelle applicazione
DECLARE @AppTables TABLE (TableName NVARCHAR(128))
INSERT INTO @AppTables VALUES 
    ('Documents'),
    ('DocumentCategories'),
    ('DocumentPermissions'),
    ('AnagraficaArticoli'),
    ('AnagraficaClienti'),
    ('AnagraficaFornitori'),
    ('ArticoliSostitutivi'),
    ('ProgressiviArticoli'),
    ('TabellaAgenti'),
    ('TabellaMagazzini'),
    ('OrdiniTestate'),
    ('OrdiniRighe')

DECLARE app_cursor CURSOR FOR SELECT TableName FROM @AppTables
OPEN app_cursor
FETCH NEXT FROM app_cursor INTO @TableName

WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = @TableName)
    BEGIN
        PRINT '✓ Tabella App: ' + @TableName
    END
    ELSE
    BEGIN
        PRINT '⚠ Tabella non trovata (verrà creata dalle migrazioni): ' + @TableName
    END
    
    FETCH NEXT FROM app_cursor INTO @TableName
END

CLOSE app_cursor
DEALLOCATE app_cursor
GO

-- =====================================================
-- 3. VERIFICA PERMESSI ACCOUNT SA
-- =====================================================

PRINT ''
PRINT 'Verifica permessi account SA:'

-- L'account SA ha sempre tutti i permessi, ma verifichiamo che sia abilitato
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = 'sa' AND is_disabled = 0)
BEGIN
    PRINT '✓ Account SA abilitato e funzionante'
    PRINT '✓ Permessi: COMPLETI (sysadmin)'
END
ELSE
BEGIN
    PRINT '✗ ATTENZIONE: Account SA potrebbe essere disabilitato'
    PRINT 'Verificare la configurazione del server SQL'
END
GO

-- =====================================================
-- 4. TEST OPERAZIONI PRINCIPALI
-- =====================================================

PRINT ''
PRINT 'Test operazioni database:'

-- Test lettura
BEGIN TRY
    SELECT TOP 1 * FROM INFORMATION_SCHEMA.TABLES
    PRINT '✓ Lettura metadati: OK'
END TRY
BEGIN CATCH
    PRINT '✗ Errore lettura metadati: ' + ERROR_MESSAGE()
END CATCH

-- Test creazione tabella temporanea (simula Entity Framework)
BEGIN TRY
    CREATE TABLE #TestTable (Id INT, Nome NVARCHAR(50))
    DROP TABLE #TestTable
    PRINT '✓ Creazione tabelle: OK'
END TRY
BEGIN CATCH
    PRINT '✗ Errore creazione tabelle: ' + ERROR_MESSAGE()
END CATCH

-- Test se esistono dati nelle tabelle anagrafiche
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AnagraficaClienti')
BEGIN
    DECLARE @ClientiCount INT
    SELECT @ClientiCount = COUNT(*) FROM AnagraficaClienti
    PRINT '✓ Tabella AnagraficaClienti: ' + CAST(@ClientiCount AS NVARCHAR(10)) + ' record'
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AnagraficaArticoli')
BEGIN
    DECLARE @ArticoliCount INT
    SELECT @ArticoliCount = COUNT(*) FROM AnagraficaArticoli
    PRINT '✓ Tabella AnagraficaArticoli: ' + CAST(@ArticoliCount AS NVARCHAR(10)) + ' record'
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OrdiniTestate')
BEGIN
    DECLARE @OrdiniCount INT
    SELECT @OrdiniCount = COUNT(*) FROM OrdiniTestate
    PRINT '✓ Tabella OrdiniTestate: ' + CAST(@OrdiniCount AS NVARCHAR(10)) + ' record'
END
GO

-- =====================================================
-- 5. CONFIGURAZIONE CONSIGLIATA
-- =====================================================

PRINT ''
PRINT '============================================='
PRINT 'CONFIGURAZIONE CONSIGLIATA:'
PRINT '============================================='
PRINT ''
PRINT '1. CONNECTION STRING ATTUALE (funzionante):'
PRINT '"Data Source=SVRGEST;Initial Catalog=AIDBMASTER;User ID=sa;Password=Novoincipient3!;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False"'
PRINT ''
PRINT '2. BACKUP RACCOMANDATO:'
PRINT '   - Eseguire backup completo del database prima del deploy'
PRINT '   - Testare il ripristino su ambiente di test'
PRINT ''
PRINT '3. MONITORAGGIO:'
PRINT '   - Verificare i log dell''applicazione dopo il deploy'
PRINT '   - Monitorare le performance delle query'
PRINT '   - Controllare l''utilizzo delle connessioni'
PRINT ''
PRINT '4. SICUREZZA:'
PRINT '   - Considerare la creazione di un utente dedicato (vedi script completo)'
PRINT '   - Limitare l''accesso alla rete solo dai server necessari'
PRINT '   - Aggiornare regolarmente le password'
GO

-- =====================================================
-- 6. CHECKLIST FINALE
-- =====================================================

PRINT ''
PRINT '============================================='
PRINT 'CHECKLIST DEPLOY:'
PRINT '============================================='
PRINT ''
PRINT '□ Database AIDBMASTER esistente su SVRGEST'
PRINT '□ Account SA abilitato con password corretta'
PRINT '□ Firewall configurato (porta 1433)'
PRINT '□ Connection string aggiornata in appsettings.Production.json'
PRINT '□ Backup database eseguito'
PRINT '□ Script migrazioni Entity Framework pronti'
PRINT '□ Server IIS configurato con ASP.NET Core 8.0 Runtime'
PRINT '□ Certificato SSL installato e configurato'
PRINT ''
PRINT 'Una volta completata la checklist, l''applicazione'
PRINT 'dovrebbe connettersi correttamente al database.'
GO

PRINT ''
PRINT '============================================='
PRINT 'VERIFICA COMPLETATA'
PRINT '============================================='

