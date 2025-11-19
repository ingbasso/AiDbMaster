-- ============================================================================
-- CONFRONTO DIRETTO SVILUPPO vs PRODUZIONE
-- ============================================================================
-- Eseguire questo script sul SERVER DI PRODUZIONE (SVRGEST)
-- Confronta la struttura tra sviluppo (locale) e produzione (SVRGEST)
-- ============================================================================

-- NOTA: Modifica il nome del server di sviluppo se diverso
DECLARE @ServerSviluppo NVARCHAR(128) = 'LOCALHOST'; -- O il nome del tuo server dev
DECLARE @ServerProduzione NVARCHAR(128) = 'SVRGEST';

PRINT '============================================================================';
PRINT 'CONFRONTO DEV vs PROD - AIDBMASTER';
PRINT '============================================================================';
PRINT 'Server Sviluppo: ' + @ServerSviluppo;
PRINT 'Server Produzione: ' + @ServerProduzione;
PRINT '';

-- ============================================================================
-- 1. CONFRONTO TABELLE
-- ============================================================================

PRINT '1️⃣ CONFRONTO TABELLE:';
PRINT '----------------------------------------';

-- Tabelle presenti su PRODUZIONE (questo server)
IF OBJECT_ID('tempdb..#TabelleProd') IS NOT NULL DROP TABLE #TabelleProd;
SELECT 
    SCHEMA_NAME(t.schema_id) + '.' + t.name AS NomeCompleto,
    t.name AS NomeTabella
INTO #TabelleProd
FROM sys.tables t;

PRINT '';
PRINT '📊 Tabelle su PRODUZIONE: ' + CAST((SELECT COUNT(*) FROM #TabelleProd) AS VARCHAR);
PRINT '';

-- Mostra tabelle presenti
SELECT 
    '✅ Presente in PRODUZIONE' AS Stato,
    NomeCompleto AS Tabella
FROM #TabelleProd
ORDER BY NomeCompleto;

PRINT '';

-- ============================================================================
-- 2. CONFRONTO COLONNE (per tabelle comuni)
-- ============================================================================

PRINT '2️⃣ VERIFICA COLONNE TABELLE:';
PRINT '----------------------------------------';

-- Per ogni tabella, mostra struttura colonne
SELECT 
    SCHEMA_NAME(t.schema_id) + '.' + t.name AS Tabella,
    c.name AS Colonna,
    TYPE_NAME(c.user_type_id) AS TipoDato,
    CASE WHEN c.is_nullable = 1 THEN 'NULL' ELSE 'NOT NULL' END AS Nullable,
    CASE WHEN c.is_identity = 1 THEN 'IDENTITY' ELSE '' END AS IsIdentity
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
ORDER BY Tabella, c.column_id;

PRINT '';

-- ============================================================================
-- 3. CONTEGGIO COLONNE PER TABELLA
-- ============================================================================

PRINT '3️⃣ NUMERO COLONNE PER TABELLA:';
PRINT '----------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) + '.' + t.name AS Tabella,
    COUNT(c.column_id) AS NumeroColonne
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
GROUP BY t.schema_id, t.name
ORDER BY Tabella;

PRINT '';

-- ============================================================================
-- 4. VERIFICA TABELLE IDENTITY (ApplicationUser, Resources, Permissions, ecc.)
-- ============================================================================

PRINT '4️⃣ VERIFICA TABELLE CRITICHE IDENTITY/PERMISSIONS:';
PRINT '----------------------------------------';

DECLARE @TabelleCritiche TABLE (NomeTabella VARCHAR(100));
INSERT INTO @TabelleCritiche VALUES 
    ('AspNetUsers'),
    ('AspNetRoles'),
    ('AspNetUserRoles'),
    ('Resources'),
    ('Permissions'),
    ('UserDataFilters'),
    ('AnagraficaClienti'),
    ('AnagraficaArticoli'),
    ('OrdiniTestate'),
    ('OrdiniRighe'),
    ('ListaOP');

SELECT 
    tc.NomeTabella AS TabellaCritica,
    CASE 
        WHEN t.name IS NOT NULL THEN '✅ PRESENTE' 
        ELSE '❌ MANCANTE' 
    END AS Stato,
    ISNULL(CAST(SUM(p.rows) AS VARCHAR), 'N/A') AS NumeroRighe
FROM @TabelleCritiche tc
LEFT JOIN sys.tables t ON tc.NomeTabella = t.name
LEFT JOIN sys.partitions p ON t.object_id = p.object_id AND p.index_id IN (0,1)
GROUP BY tc.NomeTabella, t.name
ORDER BY tc.NomeTabella;

PRINT '';

-- ============================================================================
-- 5. VERIFICA MIGRATIONS
-- ============================================================================

PRINT '5️⃣ MIGRATIONS ENTITY FRAMEWORK:';
PRINT '----------------------------------------';

IF EXISTS(SELECT 1 FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    SELECT TOP 10
        MigrationId,
        ProductVersion
    FROM [__EFMigrationsHistory]
    ORDER BY MigrationId DESC;
    
    DECLARE @LastMigration VARCHAR(200) = (
        SELECT TOP 1 MigrationId 
        FROM [__EFMigrationsHistory] 
        ORDER BY MigrationId DESC
    );
    
    PRINT '';
    PRINT '📌 Ultima migration applicata: ' + @LastMigration;
END
ELSE
BEGIN
    PRINT '❌ Tabella __EFMigrationsHistory NON TROVATA!';
END

PRINT '';

-- ============================================================================
-- 6. RIEPILOGO CONFRONTO
-- ============================================================================

PRINT '============================================================================';
PRINT 'RIEPILOGO';
PRINT '============================================================================';

DECLARE @TotTabelle INT = (SELECT COUNT(*) FROM sys.tables);
DECLARE @TotColonne INT = (SELECT COUNT(*) FROM sys.columns WHERE object_id IN (SELECT object_id FROM sys.tables));
DECLARE @TotPK INT = (SELECT COUNT(*) FROM sys.key_constraints WHERE type = 'PK');
DECLARE @TotFK INT = (SELECT COUNT(*) FROM sys.foreign_keys);

SELECT 
    'Tabelle' AS Elemento,
    @TotTabelle AS ConteggioAttuale
UNION ALL
SELECT 'Colonne', @TotColonne
UNION ALL
SELECT 'Primary Keys', @TotPK
UNION ALL
SELECT 'Foreign Keys', @TotFK;

PRINT '';
PRINT '✅ REPORT COMPLETATO';
PRINT '';
PRINT 'AZIONI SUGGERITE:';
PRINT '1. Se vedi tabelle/colonne mancanti, esegui gli script di migration';
PRINT '2. Se le strutture sono diverse, rigenera gli script EF e riesegui il deploy';
PRINT '3. Confronta le ultime migration applicate tra dev e prod';
PRINT '';
PRINT '============================================================================';

-- Cleanup
DROP TABLE #TabelleProd;

GO

