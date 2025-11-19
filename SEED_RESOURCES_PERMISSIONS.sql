-- ============================================================================
-- SEED RESOURCES E PERMISSIONS - AIDBMASTER
-- ============================================================================
-- Script per popolare manualmente le tabelle Resources e Permissions
-- Da eseguire SOLO se le tabelle esistono ma sono vuote
-- ============================================================================

USE AIDBMASTER;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

PRINT '============================================================================';
PRINT 'INIZIO SEED RESOURCES E PERMISSIONS';
PRINT '============================================================================';
PRINT '';

-- Verifica che le tabelle siano vuote
DECLARE @ResourceCount INT = (SELECT COUNT(*) FROM Resources);
IF @ResourceCount > 0
BEGIN
    PRINT '⚠️  ATTENZIONE: La tabella Resources contiene già ' + CAST(@ResourceCount AS VARCHAR) + ' record!';
    PRINT '   Se vuoi fare un re-seed completo, svuota prima le tabelle con:';
    PRINT '   DELETE FROM Permissions;';
    PRINT '   DELETE FROM Resources;';
    PRINT '   E poi riesegui questo script.';
    PRINT '';
    ROLLBACK TRANSACTION;
    RETURN;
END

PRINT '==> FASE 1: Creazione risorse ROOT (5 gruppi principali)';
PRINT '';

-- Variabili per gli ID generati
DECLARE @HomeId INT, @TabelleId INT, @ProduzioneId INT, @InterrogazioniDBId INT, @AmministrazioneId INT;

-- 1. Home
INSERT INTO Resources (Name, DisplayName, Description, MenuIcon, MenuOrder, ParentResourceId, IsMenuGroup, IsConfigured, IsActive, CreatedDate)
VALUES ('Home', 'Home', 'Pagina principale del sistema', 'bi-house-door', 1, NULL, 0, 1, 1, GETDATE());
SET @HomeId = SCOPE_IDENTITY();
PRINT '   ✅ Home (ID: ' + CAST(@HomeId AS VARCHAR) + ')';

-- 2. Tabelle (Gruppo)
INSERT INTO Resources (Name, DisplayName, Description, MenuIcon, MenuOrder, ParentResourceId, IsMenuGroup, IsConfigured, IsActive, CreatedDate)
VALUES ('Tabelle', 'Tabelle', 'Gruppo tabelle anagrafiche', 'bi-table', 2, NULL, 1, 1, 1, GETDATE());
SET @TabelleId = SCOPE_IDENTITY();
PRINT '   ✅ Tabelle (ID: ' + CAST(@TabelleId AS VARCHAR) + ')';

-- 3. Produzione (Gruppo)
INSERT INTO Resources (Name, DisplayName, Description, MenuIcon, MenuOrder, ParentResourceId, IsMenuGroup, IsConfigured, IsActive, CreatedDate)
VALUES ('Produzione', 'Produzione', 'Gruppo pianificazione produzione', 'bi-gear-wide-connected', 3, NULL, 1, 1, 1, GETDATE());
SET @ProduzioneId = SCOPE_IDENTITY();
PRINT '   ✅ Produzione (ID: ' + CAST(@ProduzioneId AS VARCHAR) + ')';

-- 4. Interrogazioni DB (Gruppo)
INSERT INTO Resources (Name, DisplayName, Description, MenuIcon, MenuOrder, ParentResourceId, IsMenuGroup, IsConfigured, IsActive, CreatedDate)
VALUES ('InterrogazioniDB', 'Interrogazioni DB', 'Interrogazioni e analisi database', 'bi-search', 4, NULL, 1, 1, 1, GETDATE());
SET @InterrogazioniDBId = SCOPE_IDENTITY();
PRINT '   ✅ InterrogazioniDB (ID: ' + CAST(@InterrogazioniDBId AS VARCHAR) + ')';

-- 5. Amministrazione (Gruppo)
INSERT INTO Resources (Name, DisplayName, Description, MenuIcon, MenuOrder, ParentResourceId, IsMenuGroup, IsConfigured, IsActive, CreatedDate)
VALUES ('Amministrazione', 'Amministrazione', 'Gruppo amministrazione sistema', 'bi-gear', 5, NULL, 1, 1, 1, GETDATE());
SET @AmministrazioneId = SCOPE_IDENTITY();
PRINT '   ✅ Amministrazione (ID: ' + CAST(@AmministrazioneId AS VARCHAR) + ')';

PRINT '';
PRINT '==> FASE 2: Creazione risorse FIGLIE (28 pagine)';
PRINT '';

-- ==== TABELLE (13 pagine) ====
PRINT '   → Tabelle (13 pagine):';
INSERT INTO Resources (Name, DisplayName, Description, MenuIcon, MenuOrder, ParentResourceId, IsMenuGroup, IsConfigured, IsActive, CreatedDate) VALUES
('AnagraficaArticoli', 'Anagrafica Articoli', 'Gestione articoli di magazzino', 'bi-table', 1, @TabelleId, 0, 1, 1, GETDATE()),
('AnagraficaClienti', 'Anagrafica Clienti', 'Gestione clienti', 'bi-people', 2, @TabelleId, 0, 1, 1, GETDATE()),
('AnagraficaFornitori', 'Anagrafica Fornitori', 'Gestione fornitori', 'bi-truck', 3, @TabelleId, 0, 1, 1, GETDATE()),
('ArticoliSostitutivi', 'Articoli Sostitutivi', 'Gestione sostituzioni articoli', 'bi-arrow-left-right', 4, @TabelleId, 0, 1, 1, GETDATE()),
('ProgressiviArticoli', 'Progressivi Articoli', 'Gestione giacenze e progressivi', 'bi-boxes', 5, @TabelleId, 0, 1, 1, GETDATE()),
('TabellaAgenti', 'Agenti', 'Gestione agenti di vendita', 'bi-person-workspace', 6, @TabelleId, 0, 1, 1, GETDATE()),
('TabellaMagazzini', 'Magazzini', 'Gestione magazzini', 'bi-building', 7, @TabelleId, 0, 1, 1, GETDATE()),
('Lavorazioni', 'Lavorazioni', 'Gestione lavorazioni di produzione', 'bi-gear-wide', 8, @TabelleId, 0, 1, 1, GETDATE()),
('CentriLavoro', 'Centri di Lavoro', 'Gestione centri di lavoro', 'bi-building-gear', 9, @TabelleId, 0, 1, 1, GETDATE()),
('Operatori', 'Operatori', 'Gestione operatori produzione', 'bi-people', 10, @TabelleId, 0, 1, 1, GETDATE()),
('StatiOP', 'Stati OP', 'Gestione stati ordini di produzione', 'bi-flag', 11, @TabelleId, 0, 1, 1, GETDATE()),
('OrdiniTestate', 'Gestione Ordini CF', 'Gestione ordini clienti', 'bi-clipboard-check', 12, @TabelleId, 0, 1, 1, GETDATE()),
('TempiAsciugatura', 'Tempi di Asciugatura', 'Gestione tempi di asciugatura', 'bi-calendar-day', 13, @TabelleId, 0, 1, 1, GETDATE());
PRINT '      ✅ 13 pagine Tabelle inserite';

-- ==== PRODUZIONE (4 pagine) ====
PRINT '   → Produzione (4 pagine):';
INSERT INTO Resources (Name, DisplayName, Description, MenuIcon, MenuOrder, ParentResourceId, IsMenuGroup, IsConfigured, IsActive, CreatedDate) VALUES
('ListaOPDashboard', 'Dashboard', 'Dashboard produzione', 'bi-graph-up', 1, @ProduzioneId, 0, 1, 1, GETDATE()),
('SchedulatoreOP', 'Schedulatore OP', 'Schedulazione ordini di produzione', 'bi-calendar2-check', 2, @ProduzioneId, 0, 1, 1, GETDATE()),
('ListaOP', 'Ordini di Produzione', 'Gestione ordini di produzione', 'bi-list-ul', 3, @ProduzioneId, 0, 1, 1, GETDATE()),
('FermiSchedulati', 'Fermi Schedulati', 'Gestione fermi centri lavoro', 'bi-calendar-check', 4, @ProduzioneId, 0, 1, 1, GETDATE());
PRINT '      ✅ 4 pagine Produzione inserite';

-- ==== INTERROGAZIONI DB (5 pagine) ====
PRINT '   → Interrogazioni DB (5 pagine):';
INSERT INTO Resources (Name, DisplayName, Description, MenuIcon, MenuOrder, ParentResourceId, IsMenuGroup, IsConfigured, IsActive, CreatedDate) VALUES
('Disponibilita', 'Disponibilità', 'Verifica disponibilità articoli', 'bi-boxes', 1, @InterrogazioniDBId, 0, 1, 1, GETDATE()),
('ConsegneProgrammate', 'Consegne Programmate', 'Gestione consegne programmate', 'bi-calendar-event', 2, @InterrogazioniDBId, 0, 1, 1, GETDATE()),
('DashboardConsegne', 'Dashboard Consegne', 'Dashboard analisi consegne', 'bi-graph-up', 3, @InterrogazioniDBId, 0, 1, 1, GETDATE()),
('Grafici', 'Grafici', 'Grafici e statistiche avanzate', 'bi-graph-up', 4, @InterrogazioniDBId, 0, 1, 1, GETDATE()),
('InterrogazioniAI', 'Interrogazioni AI', 'Interrogazioni con intelligenza artificiale', 'bi-robot', 5, @InterrogazioniDBId, 0, 1, 1, GETDATE());
PRINT '      ✅ 5 pagine Interrogazioni DB inserite';

-- ==== AMMINISTRAZIONE (6 pagine) ====
PRINT '   → Amministrazione (6 pagine):';
INSERT INTO Resources (Name, DisplayName, Description, MenuIcon, MenuOrder, ParentResourceId, IsMenuGroup, IsConfigured, IsActive, CreatedDate) VALUES
('UserManagement', 'Gestione Utenti', 'Amministrazione utenti', 'bi-people', 1, @AmministrazioneId, 0, 1, 1, GETDATE()),
('RoleManagement', 'Gestione Ruoli', 'Amministrazione ruoli', 'bi-shield-lock', 2, @AmministrazioneId, 0, 1, 1, GETDATE()),
('PermissionManagement', 'Gestione Permessi', 'Configurazione permessi sistema', 'bi-shield-lock-fill', 3, @AmministrazioneId, 0, 1, 1, GETDATE()),
('AgentiToUser', 'Converti Agenti in Utenti', 'Conversione agenti in utenti sistema', 'bi-person-plus-fill', 4, @AmministrazioneId, 0, 1, 1, GETDATE()),
('AISettings', 'Impostazioni AI', 'Configurazione sistema AI', 'bi-robot', 5, @AmministrazioneId, 0, 1, 1, GETDATE()),
('SyncfusionTest', 'Test Syncfusion', 'Test componenti Syncfusion', 'bi-grid-3x3-gap', 6, @AmministrazioneId, 0, 1, 1, GETDATE());
PRINT '      ✅ 6 pagine Amministrazione inserite';

DECLARE @TotalResources INT = (SELECT COUNT(*) FROM Resources);
PRINT '';
PRINT '✅ FASE 2 COMPLETATA: ' + CAST(@TotalResources AS VARCHAR) + ' risorse create in totale (5 root + 28 figlie)';
PRINT '';

-- ============================================================================
-- FASE 3: PERMESSI ADMIN (tutti i permessi su tutte le risorse NON gruppo)
-- ============================================================================

PRINT '==> FASE 3: Creazione permessi per ruolo ADMIN';
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
-- FASE 4: PERMESSI AGENTI (accesso limitato)
-- ============================================================================

PRINT '==> FASE 4: Creazione permessi per ruolo AGENTI';
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
PRINT 'SEED COMPLETATO CON SUCCESSO';
PRINT '============================================================================';
PRINT 'Risorse create: ' + CAST(@FinalResourceCount AS VARCHAR) + ' (atteso: 33)';
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
PRINT '1. Riavvia l''applicazione web (IIS Application Pool)';
PRINT '2. Prova ad accedere con un utente Admin';
PRINT '3. Vai su "Amministrazione > Gestione Permessi" per verificare';
PRINT '';
PRINT '============================================================================';

GO

