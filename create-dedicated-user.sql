-- =====================================================
-- CREAZIONE UTENTE DEDICATO PER AIDBMASTER
-- Server: SVRGEST
-- Database: AIDBMASTER
-- =====================================================

-- IMPORTANTE: Eseguire questo script come amministratore (sa)
-- per creare un utente dedicato più sicuro dell'account sa

USE [master]
GO

-- =====================================================
-- 1. CONFIGURAZIONE PARAMETRI
-- =====================================================

DECLARE @LoginName NVARCHAR(128) = 'AiDbMasterApp'
DECLARE @Password NVARCHAR(128) = 'AiDb2024!Secure#'  -- CAMBIA QUESTA PASSWORD!
DECLARE @DatabaseName NVARCHAR(128) = 'AIDBMASTER'

PRINT '============================================='
PRINT 'CREAZIONE UTENTE DEDICATO AIDBMASTER'
PRINT 'Login: ' + @LoginName
PRINT 'Database: ' + @DatabaseName
PRINT '============================================='

-- =====================================================
-- 2. CREAZIONE LOGIN A LIVELLO SERVER
-- =====================================================

-- Verifica se il login esiste già
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = @LoginName)
BEGIN
    -- Crea il login
    DECLARE @CreateLoginSQL NVARCHAR(500)
    SET @CreateLoginSQL = 'CREATE LOGIN [' + @LoginName + '] WITH PASSWORD = ''' + @Password + ''', 
                          DEFAULT_DATABASE = [' + @DatabaseName + '], 
                          CHECK_POLICY = ON, 
                          CHECK_EXPIRATION = OFF'
    
    EXEC sp_executesql @CreateLoginSQL
    PRINT '✓ Login ' + @LoginName + ' creato con successo'
END
ELSE
BEGIN
    PRINT '⚠ Login ' + @LoginName + ' già esistente'
    
    -- Aggiorna la password se necessario
    DECLARE @UpdatePasswordSQL NVARCHAR(500)
    SET @UpdatePasswordSQL = 'ALTER LOGIN [' + @LoginName + '] WITH PASSWORD = ''' + @Password + ''''
    EXEC sp_executesql @UpdatePasswordSQL
    PRINT '✓ Password aggiornata per ' + @LoginName
END
GO

-- =====================================================
-- 3. CONFIGURAZIONE UTENTE NEL DATABASE
-- =====================================================

USE [AIDBMASTER]
GO

DECLARE @LoginName NVARCHAR(128) = 'AiDbMasterApp'

-- Verifica se l'utente esiste nel database
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = @LoginName)
BEGIN
    -- Crea l'utente nel database
    DECLARE @CreateUserSQL NVARCHAR(500)
    SET @CreateUserSQL = 'CREATE USER [' + @LoginName + '] FOR LOGIN [' + @LoginName + ']'
    EXEC sp_executesql @CreateUserSQL
    PRINT '✓ Utente ' + @LoginName + ' creato nel database AIDBMASTER'
END
ELSE
BEGIN
    PRINT '⚠ Utente ' + @LoginName + ' già esistente nel database'
END
GO

-- =====================================================
-- 4. ASSEGNAZIONE RUOLI DATABASE
-- =====================================================

DECLARE @LoginName NVARCHAR(128) = 'AiDbMasterApp'

-- Aggiungi ai ruoli necessari
IF IS_ROLEMEMBER('db_datareader', @LoginName) = 0
BEGIN
    EXEC sp_addrolemember 'db_datareader', @LoginName
    PRINT '✓ Ruolo db_datareader assegnato'
END

IF IS_ROLEMEMBER('db_datawriter', @LoginName) = 0
BEGIN
    EXEC sp_addrolemember 'db_datawriter', @LoginName
    PRINT '✓ Ruolo db_datawriter assegnato'
END

IF IS_ROLEMEMBER('db_ddladmin', @LoginName) = 0
BEGIN
    EXEC sp_addrolemember 'db_ddladmin', @LoginName
    PRINT '✓ Ruolo db_ddladmin assegnato (per Entity Framework migrations)'
END
GO

-- =====================================================
-- 5. PERMESSI AGGIUNTIVI PER ENTITY FRAMEWORK
-- =====================================================

DECLARE @LoginName NVARCHAR(128) = 'AiDbMasterApp'

-- Permessi per visualizzare metadati
GRANT VIEW DEFINITION TO [AiDbMasterApp]
PRINT '✓ Permesso VIEW DEFINITION concesso'

-- Permessi sulle viste di sistema necessarie per EF
GRANT SELECT ON sys.tables TO [AiDbMasterApp]
GRANT SELECT ON sys.columns TO [AiDbMasterApp]
GRANT SELECT ON sys.indexes TO [AiDbMasterApp]
GRANT SELECT ON sys.foreign_keys TO [AiDbMasterApp]
GRANT SELECT ON INFORMATION_SCHEMA.TABLES TO [AiDbMasterApp]
GRANT SELECT ON INFORMATION_SCHEMA.COLUMNS TO [AiDbMasterApp]
PRINT '✓ Permessi su viste di sistema concessi'
GO

-- =====================================================
-- 6. TEST CONNESSIONE
-- =====================================================

PRINT ''
PRINT '============================================='
PRINT 'TEST CONNESSIONE UTENTE DEDICATO'
PRINT '============================================='

-- Simula una connessione con il nuovo utente
EXECUTE AS USER = 'AiDbMasterApp'

BEGIN TRY
    -- Test lettura
    SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
    PRINT '✓ Test lettura: OK'
    
    -- Test scrittura (tabella temporanea)
    CREATE TABLE #TestWrite (Id INT, TestData NVARCHAR(50))
    INSERT INTO #TestWrite VALUES (1, 'Test')
    SELECT * FROM #TestWrite
    DROP TABLE #TestWrite
    PRINT '✓ Test scrittura: OK'
    
END TRY
BEGIN CATCH
    PRINT '✗ Errore durante i test: ' + ERROR_MESSAGE()
END CATCH

-- Torna al contesto originale
REVERT
GO

-- =====================================================
-- 7. NUOVA CONNECTION STRING
-- =====================================================

PRINT ''
PRINT '============================================='
PRINT 'NUOVA CONNECTION STRING'
PRINT '============================================='
PRINT ''
PRINT 'Aggiorna il file appsettings.Production.json con:'
PRINT ''
PRINT '"ConnectionStrings": {'
PRINT '  "DefaultConnection": "Data Source=SVRGEST;Initial Catalog=AIDBMASTER;User ID=AiDbMasterApp;Password=AiDb2024!Secure#;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False"'
PRINT '}'
PRINT ''
PRINT 'IMPORTANTE:'
PRINT '- Cambia la password "AiDb2024!Secure#" con una più sicura'
PRINT '- Testa la connessione prima del deploy in produzione'
PRINT '- Mantieni un backup della connection string con account SA'
GO

-- =====================================================
-- 8. SCRIPT DI ROLLBACK (SE NECESSARIO)
-- =====================================================

PRINT ''
PRINT '============================================='
PRINT 'SCRIPT DI ROLLBACK (se necessario)'
PRINT '============================================='
PRINT ''
PRINT 'Per rimuovere l''utente creato:'
PRINT ''
PRINT 'USE [AIDBMASTER]'
PRINT 'DROP USER [AiDbMasterApp]'
PRINT 'GO'
PRINT ''
PRINT 'USE [master]'
PRINT 'DROP LOGIN [AiDbMasterApp]'
PRINT 'GO'
GO

-- =====================================================
-- 9. VERIFICA FINALE
-- =====================================================

PRINT ''
PRINT '============================================='
PRINT 'VERIFICA FINALE'
PRINT '============================================='

-- Mostra informazioni sull'utente creato
SELECT 
    dp.name AS PrincipalName,
    dp.type_desc AS PrincipalType,
    r.name AS RoleName
FROM sys.database_principals dp
LEFT JOIN sys.database_role_members rm ON dp.principal_id = rm.member_principal_id
LEFT JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
WHERE dp.name = 'AiDbMasterApp'
ORDER BY dp.name, r.name

PRINT ''
PRINT 'Utente AiDbMasterApp configurato con successo!'
PRINT ''
PRINT 'PROSSIMI PASSI:'
PRINT '1. Aggiorna la connection string nell''applicazione'
PRINT '2. Testa la connessione in ambiente di sviluppo'
PRINT '3. Esegui il deploy dell''applicazione'
PRINT '4. Monitora i log per eventuali errori di connessione'
GO



