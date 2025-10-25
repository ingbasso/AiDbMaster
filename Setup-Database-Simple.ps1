# Script per configurare i permessi del database SQL Server per AiDbMaster

param(
    [string]$ServerName = "SVRGEST",
    [string]$DatabaseName = "AIDBMASTER",
    [string]$AppPoolName = "AiDbMaster"
)

Write-Host "=== CONFIGURAZIONE PERMESSI DATABASE ===" -ForegroundColor Green
Write-Host "Server: $ServerName" -ForegroundColor Yellow
Write-Host "Database: $DatabaseName" -ForegroundColor Yellow
Write-Host "Application Pool: $AppPoolName" -ForegroundColor Yellow

# Identità dell'Application Pool
$ComputerName = $env:COMPUTERNAME
$AppPoolIdentity = "$ComputerName\$AppPoolName"

Write-Host "`nGenerazione script T-SQL..." -ForegroundColor Cyan

# Script T-SQL per i permessi
$sqlScript = @"
-- Script per configurare i permessi database per AiDbMaster
-- Eseguire su SQL Server Management Studio connesso a $ServerName

USE master;
GO

-- 1. Crea il login per Application Pool Identity
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'$AppPoolIdentity')
BEGIN
    CREATE LOGIN [$AppPoolIdentity] FROM WINDOWS WITH DEFAULT_DATABASE=[$DatabaseName];
    PRINT 'Login creato per: $AppPoolIdentity';
END
ELSE
BEGIN
    PRINT 'Login già esistente per: $AppPoolIdentity';
END
GO

-- 2. Passa al database AiDbMaster
USE [$DatabaseName];
GO

-- 3. Crea utente nel database
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$AppPoolIdentity')
BEGIN
    CREATE USER [$AppPoolIdentity] FOR LOGIN [$AppPoolIdentity];
    PRINT 'Utente creato nel database per: $AppPoolIdentity';
END
ELSE
BEGIN
    PRINT 'Utente già esistente nel database per: $AppPoolIdentity';
END
GO

-- 4. Assegna ruoli necessari per Entity Framework Core
ALTER ROLE [db_datareader] ADD MEMBER [$AppPoolIdentity];
ALTER ROLE [db_datawriter] ADD MEMBER [$AppPoolIdentity];
ALTER ROLE [db_ddladmin] ADD MEMBER [$AppPoolIdentity];
GO

-- 5. Permessi specifici per Entity Framework Migrations
GRANT CREATE TABLE TO [$AppPoolIdentity];
GRANT ALTER ON SCHEMA::dbo TO [$AppPoolIdentity];
GRANT CREATE PROCEDURE TO [$AppPoolIdentity];
GRANT CREATE FUNCTION TO [$AppPoolIdentity];
GRANT CREATE VIEW TO [$AppPoolIdentity];
GO

-- 6. Permessi per tabella EF Migrations
IF EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[__EFMigrationsHistory] TO [$AppPoolIdentity];
    PRINT 'Permessi assegnati per __EFMigrationsHistory';
END
GO

-- 7. Verifica permessi assegnati
SELECT 
    dp.name AS principal_name,
    dp.type_desc AS principal_type,
    r.name AS role_name
FROM sys.database_role_members rm
JOIN sys.database_principals dp ON rm.member_principal_id = dp.principal_id
JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
WHERE dp.name = '$AppPoolIdentity'
ORDER BY r.name;

PRINT '=== CONFIGURAZIONE DATABASE COMPLETATA ===';
PRINT 'Application Pool Identity ha ora i permessi necessari per:';
PRINT '- Leggere e scrivere dati (db_datareader, db_datawriter)';
PRINT '- Modificare strutture database (db_ddladmin)';
PRINT '- Eseguire migrations Entity Framework';
PRINT '- Gestire tabelle, procedure, funzioni e viste';
"@

# Salva lo script T-SQL
$sqlScriptPath = "Database-Permissions.sql"
$sqlScript | Out-File -FilePath $sqlScriptPath -Encoding UTF8

Write-Host "`nScript T-SQL generato: $sqlScriptPath" -ForegroundColor Green

Write-Host "`nPROSSIMI PASSI:" -ForegroundColor Yellow
Write-Host "1. Apri SQL Server Management Studio" -ForegroundColor White
Write-Host "2. Connettiti al server: $ServerName" -ForegroundColor White
Write-Host "3. Apri il file: $sqlScriptPath" -ForegroundColor White
Write-Host "4. Esegui lo script per configurare i permessi" -ForegroundColor White

Write-Host "`nIDENTITA' CONFIGURATA:" -ForegroundColor Cyan
Write-Host "Computer: $ComputerName" -ForegroundColor White
Write-Host "Application Pool: $AppPoolName" -ForegroundColor White
Write-Host "Identità completa: $AppPoolIdentity" -ForegroundColor White

# Verifica se sqlcmd è disponibile
$sqlcmdAvailable = $false
try {
    $null = Get-Command sqlcmd -ErrorAction Stop
    $sqlcmdAvailable = $true
    Write-Host "`nsqlcmd disponibile per esecuzione automatica" -ForegroundColor Green
} catch {
    Write-Host "`nsqlcmd non disponibile - usa SQL Server Management Studio" -ForegroundColor Yellow
}

if ($sqlcmdAvailable) {
    Write-Host "`nVuoi eseguire lo script automaticamente? (S/N): " -ForegroundColor Green -NoNewline
    $response = Read-Host
    if ($response -eq "S" -or $response -eq "s") {
        Write-Host "Esecuzione script database..." -ForegroundColor Cyan
        $result = & sqlcmd -S $ServerName -E -i $sqlScriptPath 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Script database eseguito con successo!" -ForegroundColor Green
            Write-Host $result
        } else {
            Write-Host "Errore esecuzione script database" -ForegroundColor Red
            Write-Host $result
        }
    }
}
