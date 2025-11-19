-- ============================================================================
-- SEED SOLO PERMISSIONS - AIDBMASTER
-- ============================================================================
-- Script per popolare SOLO la tabella Permissions
-- Da eseguire quando le Resources sono già state create
-- ============================================================================

USE AIDBMASTER;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

PRINT '============================================================================';
PRINT 'INIZIO SEED PERMISSIONS (solo permessi)';
PRINT '============================================================================';
PRINT '';

-- Verifica che le tabelle Permissions sia vuota
DECLARE @PermissionCount INT = (SELECT COUNT(*) FROM Permissions);
IF @PermissionCount > 0
BEGIN
    PRINT '⚠️  ATTENZIONE: La tabella Permissions contiene già ' + CAST(@PermissionCount AS VARCHAR) + ' record!';
    PRINT '   Se vuoi fare un re-seed dei permessi, svuota prima la tabella con:';
    PRINT '   DELETE FROM Permissions;';
    PRINT '   E poi riesegui questo script.';
    PRINT '';
    ROLLBACK TRANSACTION;
    RETURN;
END

-- Verifica che ci siano risorse
DECLARE @ResourceCount INT = (SELECT COUNT(*) FROM Resources);
IF @ResourceCount = 0
BEGIN
    PRINT '❌ ERRORE: Nessuna risorsa trovata nella tabella Resources!';
    PRINT '   Esegui prima lo script SEED_RESOURCES_PERMISSIONS.sql completo.';
    ROLLBACK TRANSACTION;
    RETURN;
END

PRINT 'Risorse trovate: ' + CAST(@ResourceCount AS VARCHAR);
PRINT '';

-- ============================================================================
-- FASE 1: PERMESSI ADMIN (tutti i permessi su tutte le risorse NON gruppo)
-- ============================================================================

PRINT '==> FASE 1: Creazione permessi per ruolo ADMIN';
PRINT '';

DECLARE @AdminRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin');

IF @AdminRoleId IS NULL
BEGIN
    PRINT '❌ ERRORE: Ruolo Admin non trovato!';
    PRINT '   Verifica che il ruolo "Admin" esista nella tabella AspNetRoles.';
    ROLLBACK TRANSACTION;
    RETURN;
END

PRINT '   → Ruolo Admin ID: ' + @AdminRoleId;

-- Crea permessi completi per Admin su tutte le risorse NON gruppo
INSERT INTO Permissions (RoleId, ResourceId, CanView, CanCreate, CanEdit, CanDelete, CreatedDate)
SELECT 
    @AdminRoleId,
    r.Id,
    1, -- CanView
    1, -- CanCreate
    1, -- CanEdit
    1, -- CanDelete
    GETDATE() -- CreatedDate
FROM Resources r
WHERE r.IsMenuGroup = 0; -- Solo pagine, non gruppi menu

DECLARE @AdminPermCount INT = @@ROWCOUNT;
PRINT '   ✅ ' + CAST(@AdminPermCount AS VARCHAR) + ' permessi Admin creati (accesso completo a tutte le pagine)';
PRINT '';

-- ============================================================================
-- FASE 2: PERMESSI AGENTI (accesso limitato)
-- ============================================================================

PRINT '==> FASE 2: Creazione permessi per ruolo AGENTI';
PRINT '';

DECLARE @AgentiRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Agenti');

IF @AgentiRoleId IS NULL
BEGIN
    PRINT '⚠️  ATTENZIONE: Ruolo Agenti non trovato!';
    PRINT '   I permessi per Agenti NON verranno creati.';
    PRINT '   Se necessario, crea il ruolo "Agenti" e riesegui questa sezione dello script.';
    PRINT '';
END
ELSE
BEGIN
    PRINT '   → Ruolo Agenti ID: ' + @AgentiRoleId;
    
    -- Home (solo view)
    INSERT INTO Permissions (RoleId, ResourceId, CanView, CanCreate, CanEdit, CanDelete, CreatedDate)
    SELECT @AgentiRoleId, Id, 1, 0, 0, 0, GETDATE() FROM Resources WHERE Name = 'Home';
    
    -- AnagraficaClienti (view + edit)
    INSERT INTO Permissions (RoleId, ResourceId, CanView, CanCreate, CanEdit, CanDelete, CreatedDate)
    SELECT @AgentiRoleId, Id, 1, 0, 1, 0, GETDATE() FROM Resources WHERE Name = 'AnagraficaClienti';
    
    -- OrdiniTestate (view + edit)
    INSERT INTO Permissions (RoleId, ResourceId, CanView, CanCreate, CanEdit, CanDelete, CreatedDate)
    SELECT @AgentiRoleId, Id, 1, 0, 1, 0, GETDATE() FROM Resources WHERE Name = 'OrdiniTestate';
    
    -- ConsegneProgrammate (solo view)
    INSERT INTO Permissions (RoleId, ResourceId, CanView, CanCreate, CanEdit, CanDelete, CreatedDate)
    SELECT @AgentiRoleId, Id, 1, 0, 0, 0, GETDATE() FROM Resources WHERE Name = 'ConsegneProgrammate';
    
    -- DashboardConsegne (solo view)
    INSERT INTO Permissions (RoleId, ResourceId, CanView, CanCreate, CanEdit, CanDelete, CreatedDate)
    SELECT @AgentiRoleId, Id, 1, 0, 0, 0, GETDATE() FROM Resources WHERE Name = 'DashboardConsegne';
    
    DECLARE @AgentiPermCount INT = 5;
    PRINT '   ✅ ' + CAST(@AgentiPermCount AS VARCHAR) + ' permessi Agenti creati (Home, Clienti, Ordini, Consegne, Dashboard)';
    PRINT '';
END

-- ============================================================================
-- RIEPILOGO E COMMIT
-- ============================================================================

DECLARE @FinalResourceCount INT = (SELECT COUNT(*) FROM Resources);
DECLARE @FinalPermissionCount INT = (SELECT COUNT(*) FROM Permissions);

PRINT '';
PRINT '============================================================================';
PRINT 'SEED PERMESSI COMPLETATO CON SUCCESSO';
PRINT '============================================================================';
PRINT 'Risorse esistenti: ' + CAST(@FinalResourceCount AS VARCHAR) + ' (atteso: 33)';
PRINT 'Permessi creati: ' + CAST(@FinalPermissionCount AS VARCHAR) + ' (atteso: ~33 per Admin + 5 per Agenti)';
PRINT '';
PRINT '🎯 VERIFICA FINALE:';
SELECT 
    CASE 
        WHEN @FinalResourceCount = 33 THEN '✅ Risorse: OK (' + CAST(@FinalResourceCount AS VARCHAR) + '/33)'
        ELSE '⚠️  Risorse: ATTENZIONE (' + CAST(@FinalResourceCount AS VARCHAR) + '/33 - verifica manualmente)'
    END AS StatoRisorse;

SELECT 
    CASE 
        WHEN @FinalPermissionCount >= 30 THEN '✅ Permessi: OK (' + CAST(@FinalPermissionCount AS VARCHAR) + ' creati)'
        ELSE '⚠️  Permessi: ATTENZIONE (' + CAST(@FinalPermissionCount AS VARCHAR) + ' creati - verifica ruoli)'
    END AS StatoPermessi;

PRINT '';
PRINT 'Commit transazione...';
COMMIT TRANSACTION;
PRINT '✅ Transazione committata con successo!';
PRINT '';
PRINT 'PROSSIMI PASSI:';
PRINT '1. Riavvia l''applicazione web (IIS Application Pool: Restart-WebAppPool -Name "AiDbMaster")';
PRINT '2. Prova ad accedere con un utente Admin';
PRINT '3. Vai su "Amministrazione > Gestione Permessi" per verificare';
PRINT '';
PRINT '============================================================================';

GO

