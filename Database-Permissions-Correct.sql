-- Script CORRETTO per configurare i permessi database per AiDbMaster
-- L'identità Application Pool è "IIS AppPool\AiDbMaster" NON "SVRGEST\AiDbMaster"

USE master;
GO

-- 1. Crea il login per Application Pool Identity CORRETTA
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'IIS AppPool\AiDbMaster')
BEGIN
    CREATE LOGIN [IIS AppPool\AiDbMaster] FROM WINDOWS WITH DEFAULT_DATABASE=[AIDBMASTER];
    PRINT 'Login creato per: IIS AppPool\AiDbMaster';
END
ELSE
BEGIN
    PRINT 'Login già esistente per: IIS AppPool\AiDbMaster';
END
GO

-- 2. Passa al database AiDbMaster
USE [AIDBMASTER];
GO

-- 3. Crea utente nel database
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'IIS AppPool\AiDbMaster')
BEGIN
    CREATE USER [IIS AppPool\AiDbMaster] FOR LOGIN [IIS AppPool\AiDbMaster];
    PRINT 'Utente creato nel database per: IIS AppPool\AiDbMaster';
END
ELSE
BEGIN
    PRINT 'Utente già esistente nel database per: IIS AppPool\AiDbMaster';
END
GO

-- 4. Assegna ruoli necessari per Entity Framework Core
ALTER ROLE [db_datareader] ADD MEMBER [IIS AppPool\AiDbMaster];
ALTER ROLE [db_datawriter] ADD MEMBER [IIS AppPool\AiDbMaster];
ALTER ROLE [db_ddladmin] ADD MEMBER [IIS AppPool\AiDbMaster];
GO

-- 5. Permessi specifici per Entity Framework Migrations
GRANT CREATE TABLE TO [IIS AppPool\AiDbMaster];
GRANT ALTER ON SCHEMA::dbo TO [IIS AppPool\AiDbMaster];
GRANT CREATE PROCEDURE TO [IIS AppPool\AiDbMaster];
GRANT CREATE FUNCTION TO [IIS AppPool\AiDbMaster];
GRANT CREATE VIEW TO [IIS AppPool\AiDbMaster];
GO

-- 6. Permessi per tabella EF Migrations (se esiste)
IF EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[__EFMigrationsHistory] TO [IIS AppPool\AiDbMaster];
    PRINT 'Permessi assegnati per __EFMigrationsHistory';
END
ELSE
BEGIN
    PRINT 'Tabella __EFMigrationsHistory non trovata (verrà creata al primo avvio)';
END
GO

-- 7. Verifica permessi assegnati
PRINT '=== VERIFICA PERMESSI ===';
SELECT 
    dp.name AS principal_name,
    dp.type_desc AS principal_type,
    r.name AS role_name
FROM sys.database_role_members rm
JOIN sys.database_principals dp ON rm.member_principal_id = dp.principal_id
JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
WHERE dp.name = 'IIS AppPool\AiDbMaster'
ORDER BY r.name;

-- 8. Verifica login a livello server
USE master;
SELECT 
    name,
    type_desc,
    is_disabled,
    default_database_name
FROM sys.server_principals 
WHERE name = 'IIS AppPool\AiDbMaster';

PRINT '=== CONFIGURAZIONE DATABASE COMPLETATA ===';
PRINT 'Application Pool Identity "IIS AppPool\AiDbMaster" ha ora i permessi necessari per:';
PRINT '- Leggere e scrivere dati (db_datareader, db_datawriter)';
PRINT '- Modificare strutture database (db_ddladmin)';
PRINT '- Eseguire migrations Entity Framework';
PRINT '- Gestire tabelle, procedure, funzioni e viste';

-- 9. Cleanup del login errato (se esiste)
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'SVRGEST\AiDbMaster')
BEGIN
    USE [AIDBMASTER];
    IF EXISTS (SELECT * FROM sys.database_principals WHERE name = N'SVRGEST\AiDbMaster')
    BEGIN
        DROP USER [SVRGEST\AiDbMaster];
        PRINT 'Rimosso utente errato: SVRGEST\AiDbMaster dal database';
    END
    
    USE master;
    DROP LOGIN [SVRGEST\AiDbMaster];
    PRINT 'Rimosso login errato: SVRGEST\AiDbMaster dal server';
END
