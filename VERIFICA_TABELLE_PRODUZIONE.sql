-- ============================================================================
-- VERIFICA TABELLE DATABASE AIDBMASTER PRODUZIONE
-- ============================================================================
-- Eseguire questo script sul server SVRGEST per diagnosticare il problema

-- 1. Verifica tutte le tabelle presenti nel database
SELECT 
    SCHEMA_NAME(t.schema_id) AS [Schema],
    t.name AS [NomeTabella],
    SUM(p.rows) AS [NumeroRighe]
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0,1)
GROUP BY t.schema_id, t.name
ORDER BY [Schema], [NomeTabella];

-- 2. Verifica specificamente le tabelle critiche (case-insensitive)
SELECT 
    'Resources' AS TabellaRicercata,
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'Resources') 
        THEN 'TROVATA' ELSE 'NON TROVATA' END AS Stato,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'Resources'), 'N/A') AS NomeCompleto
UNION ALL
SELECT 'Permissions', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'Permissions') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'Permissions'), 'N/A')
UNION ALL
SELECT 'UserDataFilters', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'UserDataFilters') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'UserDataFilters'), 'N/A')
UNION ALL
SELECT 'AspNetUsers', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'AspNetUsers') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'AspNetUsers'), 'N/A')
UNION ALL
SELECT 'AspNetRoles', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'AspNetRoles') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'AspNetRoles'), 'N/A')
UNION ALL
SELECT 'OrdiniTestate', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'OrdiniTestate') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'OrdiniTestate'), 'N/A')
UNION ALL
SELECT 'OrdiniRighe', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'OrdiniRighe') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'OrdiniRighe'), 'N/A')
UNION ALL
SELECT 'ListaOP', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'ListaOP') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'ListaOP'), 'N/A')
UNION ALL
SELECT 'AnagraficaClienti', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'AnagraficaClienti') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'AnagraficaClienti'), 'N/A')
UNION ALL
SELECT 'AnagraficaArticoli', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'AnagraficaArticoli') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'AnagraficaArticoli'), 'N/A')
UNION ALL
SELECT 'TabellaAgenti', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'TabellaAgenti') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'TabellaAgenti'), 'N/A')
UNION ALL
SELECT 'Lavorazioni', 
    CASE WHEN EXISTS(SELECT 1 FROM sys.tables WHERE name LIKE 'Lavorazioni') THEN 'TROVATA' ELSE 'NON TROVATA' END,
    ISNULL((SELECT TOP 1 SCHEMA_NAME(schema_id) + '.' + name FROM sys.tables WHERE name LIKE 'Lavorazioni'), 'N/A');

-- 3. Verifica le migration applicate
IF EXISTS(SELECT 1 FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    SELECT TOP 10
        MigrationId,
        ProductVersion
    FROM [__EFMigrationsHistory]
    ORDER BY MigrationId DESC;
END
ELSE
BEGIN
    SELECT 'TABELLA __EFMigrationsHistory NON TROVATA!' AS Errore;
END

-- 4. Verifica constraint e foreign key su Lavorazioni (se esiste)
IF EXISTS(SELECT 1 FROM sys.tables WHERE name = 'Lavorazioni')
BEGIN
    SELECT 
        'PRIMARY KEY' AS TipoConstraint,
        kc.name AS NomeConstraint
    FROM sys.key_constraints kc
    INNER JOIN sys.tables t ON kc.parent_object_id = t.object_id
    WHERE t.name = 'Lavorazioni' AND kc.type = 'PK'
    
    UNION ALL
    
    SELECT 
        'FOREIGN KEY' AS TipoConstraint,
        fk.name AS NomeConstraint
    FROM sys.foreign_keys fk
    INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
    WHERE t.name = 'Lavorazioni' OR fk.name LIKE '%Lavorazioni%';
END
ELSE
BEGIN
    SELECT 'Tabella Lavorazioni non trovata' AS Info;
END

