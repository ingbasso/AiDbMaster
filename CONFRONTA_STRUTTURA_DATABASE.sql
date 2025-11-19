-- ============================================================================
-- SCRIPT DI CONFRONTO STRUTTURA DATABASE
-- ============================================================================
-- Questo script genera un report completo della struttura del database
-- Eseguilo su SVILUPPO e su PRODUZIONE, poi confronta i risultati
-- ============================================================================

USE AIDBMASTER;
GO

SET NOCOUNT ON;

PRINT '============================================================================';
PRINT 'CONFRONTO STRUTTURA DATABASE - AIDBMASTER';
PRINT 'Server: ' + @@SERVERNAME;
PRINT 'Database: ' + DB_NAME();
PRINT 'Data: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================================';
PRINT '';

-- ============================================================================
-- 1. ELENCO TABELLE
-- ============================================================================

PRINT '1️⃣ TABELLE NEL DATABASE:';
PRINT '----------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) AS [Schema],
    t.name AS NomeTabella,
    SUM(p.rows) AS NumeroRighe
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0,1)
GROUP BY t.schema_id, t.name
ORDER BY [Schema], NomeTabella;

DECLARE @TabelleCount INT = (SELECT COUNT(*) FROM sys.tables);
PRINT '';
PRINT '📊 TOTALE TABELLE: ' + CAST(@TabelleCount AS VARCHAR);
PRINT '';

-- ============================================================================
-- 2. COLONNE PER OGNI TABELLA (dettagliato)
-- ============================================================================

PRINT '2️⃣ STRUTTURA COLONNE PER TABELLA:';
PRINT '----------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) AS [Schema],
    t.name AS Tabella,
    c.column_id AS Ordine,
    c.name AS Colonna,
    TYPE_NAME(c.user_type_id) AS TipoDato,
    c.max_length AS LunghezzaMax,
    c.precision AS Precisione,
    c.scale AS Scala,
    CASE WHEN c.is_nullable = 1 THEN 'NULL' ELSE 'NOT NULL' END AS Nullable,
    CASE WHEN c.is_identity = 1 THEN 'IDENTITY' ELSE '' END AS IsIdentity,
    ISNULL(dc.definition, '') AS DefaultValue
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
ORDER BY [Schema], Tabella, Ordine;

DECLARE @ColonneCount INT = (SELECT COUNT(*) FROM sys.columns WHERE object_id IN (SELECT object_id FROM sys.tables));
PRINT '';
PRINT '📊 TOTALE COLONNE: ' + CAST(@ColonneCount AS VARCHAR);
PRINT '';

-- ============================================================================
-- 3. PRIMARY KEYS
-- ============================================================================

PRINT '3️⃣ PRIMARY KEYS:';
PRINT '----------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) AS [Schema],
    t.name AS Tabella,
    kc.name AS NomeConstraint,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS Colonne
FROM sys.tables t
INNER JOIN sys.key_constraints kc ON t.object_id = kc.parent_object_id
INNER JOIN sys.index_columns ic ON kc.parent_object_id = ic.object_id AND kc.unique_index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE kc.type = 'PK'
GROUP BY t.schema_id, t.name, kc.name
ORDER BY [Schema], Tabella;

PRINT '';

-- ============================================================================
-- 4. FOREIGN KEYS
-- ============================================================================

PRINT '4️⃣ FOREIGN KEYS:';
PRINT '----------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) AS [Schema],
    t.name AS TabellaFiglio,
    fk.name AS NomeConstraint,
    STRING_AGG(c.name, ', ') AS ColonneFiglio,
    SCHEMA_NAME(rt.schema_id) AS SchemaPadre,
    rt.name AS TabellaPadre,
    STRING_AGG(rc.name, ', ') AS ColonnePadre
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
INNER JOIN sys.tables rt ON fk.referenced_object_id = rt.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
INNER JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
GROUP BY t.schema_id, t.name, fk.name, rt.schema_id, rt.name
ORDER BY [Schema], TabellaFiglio;

DECLARE @FKCount INT = (SELECT COUNT(*) FROM sys.foreign_keys);
PRINT '';
PRINT '📊 TOTALE FOREIGN KEYS: ' + CAST(@FKCount AS VARCHAR);
PRINT '';

-- ============================================================================
-- 5. INDEXES (non clustered)
-- ============================================================================

PRINT '5️⃣ INDEXES (non-clustered):';
PRINT '----------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) AS [Schema],
    t.name AS Tabella,
    i.name AS NomeIndex,
    i.type_desc AS TipoIndex,
    CASE WHEN i.is_unique = 1 THEN 'UNIQUE' ELSE '' END AS IsUnique,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS Colonne
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.type > 1 -- Esclude heap e clustered (già mostrati come PK)
    AND i.is_primary_key = 0
    AND i.is_unique_constraint = 0
GROUP BY t.schema_id, t.name, i.name, i.type_desc, i.is_unique
ORDER BY [Schema], Tabella, NomeIndex;

PRINT '';

-- ============================================================================
-- 6. MIGRATIONS HISTORY (se presente)
-- ============================================================================

PRINT '6️⃣ ENTITY FRAMEWORK MIGRATIONS:';
PRINT '----------------------------------------';

IF EXISTS(SELECT 1 FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    SELECT 
        MigrationId,
        ProductVersion,
        'Applied' AS Stato
    FROM [__EFMigrationsHistory]
    ORDER BY MigrationId;
    
    DECLARE @MigrationCount INT = (SELECT COUNT(*) FROM [__EFMigrationsHistory]);
    PRINT '';
    PRINT '📊 TOTALE MIGRATIONS APPLICATE: ' + CAST(@MigrationCount AS VARCHAR);
END
ELSE
BEGIN
    PRINT '⚠️  Tabella __EFMigrationsHistory NON TROVATA';
END

PRINT '';

-- ============================================================================
-- 7. RIEPILOGO FINALE
-- ============================================================================

PRINT '';
PRINT '============================================================================';
PRINT 'RIEPILOGO FINALE';
PRINT '============================================================================';

DECLARE @Statistiche TABLE (
    Elemento VARCHAR(50),
    Conteggio INT
);

INSERT INTO @Statistiche VALUES ('Tabelle', (SELECT COUNT(*) FROM sys.tables));
INSERT INTO @Statistiche VALUES ('Colonne', (SELECT COUNT(*) FROM sys.columns WHERE object_id IN (SELECT object_id FROM sys.tables)));
INSERT INTO @Statistiche VALUES ('Primary Keys', (SELECT COUNT(*) FROM sys.key_constraints WHERE type = 'PK'));
INSERT INTO @Statistiche VALUES ('Foreign Keys', (SELECT COUNT(*) FROM sys.foreign_keys));
INSERT INTO @Statistiche VALUES ('Indexes', (SELECT COUNT(*) FROM sys.indexes WHERE object_id IN (SELECT object_id FROM sys.tables) AND type > 1 AND is_primary_key = 0));

SELECT Elemento, Conteggio FROM @Statistiche ORDER BY Elemento;

PRINT '';
PRINT '✅ REPORT COMPLETATO';
PRINT '============================================================================';

GO

