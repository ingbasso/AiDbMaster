-- ============================================================================
-- GENERA SCRIPT CREATE COMPLETO DAL DATABASE DI SVILUPPO
-- ============================================================================
-- Esegui questo script sul DATABASE DI SVILUPPO (localhost)
-- Genera gli script CREATE TABLE per TUTTE le tabelle
-- Poi esegui l'output sul database di PRODUZIONE
-- ============================================================================

USE AIDBMASTER;
GO

SET NOCOUNT ON;

DECLARE @SQL NVARCHAR(MAX) = '';
DECLARE @CRLF CHAR(2) = CHAR(13) + CHAR(10);

PRINT '-- ============================================================================';
PRINT '-- SCRIPT GENERATO AUTOMATICAMENTE DA DATABASE DI SVILUPPO';
PRINT '-- Data: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '-- ============================================================================';
PRINT '';
PRINT 'USE AIDBMASTER;';
PRINT 'GO';
PRINT '';

-- ============================================================================
-- GENERA CREATE TABLE PER OGNI TABELLA
-- ============================================================================

DECLARE @TableName NVARCHAR(128);
DECLARE @SchemaName NVARCHAR(128);

DECLARE table_cursor CURSOR FOR
SELECT 
    s.name AS SchemaName,
    t.name AS TableName
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY 
    CASE t.name
        -- Identity tables prima
        WHEN 'AspNetRoles' THEN 1
        WHEN 'AspNetUsers' THEN 2
        WHEN 'AspNetUserRoles' THEN 3
        WHEN 'AspNetUserClaims' THEN 4
        WHEN 'AspNetUserLogins' THEN 5
        WHEN 'AspNetUserTokens' THEN 6
        WHEN 'AspNetRoleClaims' THEN 7
        -- Resources e Permissions
        WHEN 'Resources' THEN 10
        WHEN 'Permissions' THEN 11
        WHEN 'UserDataFilters' THEN 12
        -- Altre tabelle
        ELSE 100
    END,
    t.name;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @SchemaName, @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT '-- ============================================================================';
    PRINT '-- Tabella: [' + @SchemaName + '].[' + @TableName + ']';
    PRINT '-- ============================================================================';
    PRINT '';
    PRINT 'IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = ''' + @TableName + ''' AND schema_id = SCHEMA_ID(''' + @SchemaName + '''))';
    PRINT 'BEGIN';
    PRINT '    CREATE TABLE [' + @SchemaName + '].[' + @TableName + '] (';
    
    -- Genera definizione colonne
    DECLARE @ColumnDef NVARCHAR(MAX) = '';
    
    SELECT @ColumnDef = @ColumnDef + 
        '        [' + c.name + '] ' + 
        TYPE_NAME(c.user_type_id) + 
        CASE 
            WHEN TYPE_NAME(c.user_type_id) IN ('varchar', 'nvarchar', 'char', 'nchar') THEN 
                CASE WHEN c.max_length = -1 THEN '(MAX)' 
                     WHEN TYPE_NAME(c.user_type_id) LIKE 'n%' THEN '(' + CAST(c.max_length/2 AS VARCHAR) + ')'
                     ELSE '(' + CAST(c.max_length AS VARCHAR) + ')' 
                END
            WHEN TYPE_NAME(c.user_type_id) IN ('decimal', 'numeric') THEN 
                '(' + CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR) + ')'
            ELSE ''
        END +
        CASE WHEN c.is_identity = 1 THEN ' IDENTITY(1,1)' ELSE '' END +
        CASE WHEN c.is_nullable = 0 THEN ' NOT NULL' ELSE ' NULL' END +
        CASE 
            WHEN dc.definition IS NOT NULL THEN ' DEFAULT ' + dc.definition
            ELSE ''
        END +
        ',' + @CRLF
    FROM sys.columns c
    LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
    WHERE c.object_id = OBJECT_ID('[' + @SchemaName + '].[' + @TableName + ']')
    ORDER BY c.column_id;
    
    -- Rimuovi ultima virgola
    IF LEN(@ColumnDef) > 0
        SET @ColumnDef = LEFT(@ColumnDef, LEN(@ColumnDef) - 2);
    
    PRINT @ColumnDef;
    PRINT '    );';
    
    -- Genera Primary Key
    IF EXISTS (
        SELECT 1 FROM sys.key_constraints 
        WHERE parent_object_id = OBJECT_ID('[' + @SchemaName + '].[' + @TableName + ']') 
        AND type = 'PK'
    )
    BEGIN
        DECLARE @PKName NVARCHAR(128);
        DECLARE @PKColumns NVARCHAR(MAX);
        
        SELECT @PKName = kc.name,
               @PKColumns = STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal)
        FROM sys.key_constraints kc
        INNER JOIN sys.index_columns ic ON kc.parent_object_id = ic.object_id AND kc.unique_index_id = ic.index_id
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE kc.parent_object_id = OBJECT_ID('[' + @SchemaName + '].[' + @TableName + ']')
          AND kc.type = 'PK'
        GROUP BY kc.name;
        
        PRINT '    ALTER TABLE [' + @SchemaName + '].[' + @TableName + '] ADD CONSTRAINT [' + @PKName + '] PRIMARY KEY (' + @PKColumns + ');';
    END
    
    PRINT '    PRINT ''   ✅ ' + @TableName + ' creata'';';
    PRINT 'END';
    PRINT 'ELSE PRINT ''   ⏭️  ' + @TableName + ' già esiste'';';
    PRINT '';
    PRINT 'GO';
    PRINT '';
    
    FETCH NEXT FROM table_cursor INTO @SchemaName, @TableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;

-- ============================================================================
-- GENERA FOREIGN KEYS (dopo che tutte le tabelle esistono)
-- ============================================================================

PRINT '';
PRINT '-- ============================================================================';
PRINT '-- FOREIGN KEYS';
PRINT '-- ============================================================================';
PRINT '';

DECLARE @FKScript NVARCHAR(MAX);

DECLARE fk_cursor CURSOR FOR
SELECT 
    'IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = ''' + fk.name + ''')' + @CRLF +
    'BEGIN' + @CRLF +
    '    ALTER TABLE [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + ']' + @CRLF +
    '    ADD CONSTRAINT [' + fk.name + '] FOREIGN KEY (' + 
    STRING_AGG(c.name, ', ') + ') REFERENCES [' + 
    SCHEMA_NAME(rt.schema_id) + '].[' + rt.name + '] (' + 
    STRING_AGG(rc.name, ', ') + ')' + 
    CASE WHEN fk.delete_referential_action = 1 THEN ' ON DELETE CASCADE' ELSE '' END + ';' + @CRLF +
    '    PRINT ''   ✅ FK: ' + fk.name + ''';' + @CRLF +
    'END' + @CRLF +
    'GO' + @CRLF AS FKScript
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
INNER JOIN sys.tables rt ON fk.referenced_object_id = rt.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
INNER JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
GROUP BY fk.name, t.schema_id, t.name, rt.schema_id, rt.name, fk.delete_referential_action;

OPEN fk_cursor;
FETCH NEXT FROM fk_cursor INTO @FKScript;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT @FKScript;
    FETCH NEXT FROM fk_cursor INTO @FKScript;
END

CLOSE fk_cursor;
DEALLOCATE fk_cursor;

PRINT '';
PRINT '-- ============================================================================';
PRINT '-- SCRIPT COMPLETATO';
PRINT '-- ============================================================================';

GO

