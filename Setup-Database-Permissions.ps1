# Script per configurare i permessi del database SQL Server per AiDbMaster
# Eseguire su SQL Server Management Studio o come script T-SQL

param(
    [string]$ServerName = "SVRGEST",
    [string]$DatabaseName = "AIDBMASTER",
    [string]$AppPoolName = "AiDbMaster"
)

Write-Host "=== CONFIGURAZIONE PERMESSI DATABASE PER AIDBMASTER ===" -ForegroundColor Green
Write-Host "Server: $ServerName" -ForegroundColor Yellow
Write-Host "Database: $DatabaseName" -ForegroundColor Yellow
Write-Host "Application Pool: $AppPoolName" -ForegroundColor Yellow

# Identità dell'Application Pool
$AppPoolIdentity = "IIS AppPool\$AppPoolName"
$ComputerName = $env:COMPUTERNAME

Write-Host "`nGenerazione script T-SQL per la configurazione dei permessi..." -ForegroundColor Cyan

# Genera lo script T-SQL
$sqlScript = @"
-- Script per configurare i permessi database per AiDbMaster
-- Eseguire su SQL Server Management Studio connesso a $ServerName

USE master;
GO

-- 1. Crea il login per l'Application Pool Identity se non esiste
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'$ComputerName\$AppPoolName')
BEGIN
    CREATE LOGIN [$ComputerName\$AppPoolName] FROM WINDOWS WITH DEFAULT_DATABASE=[$DatabaseName];
    PRINT 'Login creato per: $ComputerName\$AppPoolName';
END
ELSE
BEGIN
    PRINT 'Login già esistente per: $ComputerName\$AppPoolName';
END
GO

-- 2. Passa al database AiDbMaster
USE [$DatabaseName];
GO

-- 3. Crea l'utente nel database se non esiste
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$ComputerName\$AppPoolName')
BEGIN
    CREATE USER [$ComputerName\$AppPoolName] FOR LOGIN [$ComputerName\$AppPoolName];
    PRINT 'Utente creato nel database per: $ComputerName\$AppPoolName';
END
ELSE
BEGIN
    PRINT 'Utente già esistente nel database per: $ComputerName\$AppPoolName';
END
GO

-- 4. Assegna i ruoli necessari per Entity Framework Core
ALTER ROLE [db_datareader] ADD MEMBER [$ComputerName\$AppPoolName];
ALTER ROLE [db_datawriter] ADD MEMBER [$ComputerName\$AppPoolName];
ALTER ROLE [db_ddladmin] ADD MEMBER [$ComputerName\$AppPoolName];
GO

-- 5. Permessi specifici per Entity Framework Migrations
GRANT CREATE TABLE TO [$ComputerName\$AppPoolName];
GRANT ALTER ON SCHEMA::dbo TO [$ComputerName\$AppPoolName];
GRANT CREATE PROCEDURE TO [$ComputerName\$AppPoolName];
GRANT CREATE FUNCTION TO [$ComputerName\$AppPoolName];
GRANT CREATE VIEW TO [$ComputerName\$AppPoolName];
GO

-- 6. Permessi per la tabella __EFMigrationsHistory (Entity Framework)
IF EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[__EFMigrationsHistory] TO [$ComputerName\$AppPoolName];
    PRINT 'Permessi assegnati per __EFMigrationsHistory';
END
GO

-- 7. Verifica dei permessi assegnati
SELECT 
    dp.name AS principal_name,
    dp.type_desc AS principal_type,
    r.name AS role_name
FROM sys.database_role_members rm
JOIN sys.database_principals dp ON rm.member_principal_id = dp.principal_id
JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
WHERE dp.name = '$ComputerName\$AppPoolName'
ORDER BY r.name;

PRINT '=== CONFIGURAZIONE DATABASE COMPLETATA ===';
PRINT 'L''Application Pool Identity ha ora i permessi necessari per:';
PRINT '- Leggere e scrivere dati (db_datareader, db_datawriter)';
PRINT '- Modificare strutture database (db_ddladmin)';
PRINT '- Eseguire migrations Entity Framework';
PRINT '- Gestire tabelle, procedure, funzioni e viste';
"@

# Salva lo script T-SQL in un file
$sqlScriptPath = "Setup-Database-Permissions.sql"
$sqlScript | Out-File -FilePath $sqlScriptPath -Encoding UTF8

Write-Host "`n✓ Script T-SQL generato: $sqlScriptPath" -ForegroundColor Green
Write-Host "`nPROSSIMI PASSI:" -ForegroundColor Yellow
Write-Host "1. Apri SQL Server Management Studio" -ForegroundColor White
Write-Host "2. Connettiti al server: $ServerName" -ForegroundColor White
Write-Host "3. Apri il file: $sqlScriptPath" -ForegroundColor White
Write-Host "4. Esegui lo script per configurare i permessi" -ForegroundColor White

Write-Host "`nALTERNATIVA - Esecuzione automatica (se hai sqlcmd installato):" -ForegroundColor Yellow
Write-Host "sqlcmd -S $ServerName -E -i `"$sqlScriptPath`"" -ForegroundColor Cyan

# Verifica se sqlcmd è disponibile
try {
    $sqlcmdVersion = & sqlcmd -? 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ sqlcmd disponibile. Vuoi eseguire lo script automaticamente? (S/N)" -ForegroundColor Green
        $response = Read-Host
        if ($response -eq "S" -or $response -eq "s") {
            Write-Host "Esecuzione script database..." -ForegroundColor Cyan
            & sqlcmd -S $ServerName -E -i $sqlScriptPath
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Script database eseguito con successo!" -ForegroundColor Green
            } else {
                Write-Host "✗ Errore nell'esecuzione dello script database" -ForegroundColor Red
            }
        }
    }
} catch {
    Write-Host "sqlcmd non disponibile. Usa SQL Server Management Studio." -ForegroundColor Yellow
}
