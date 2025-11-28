-- ============================================================================
-- CONTA COLONNE PER OGNI TABELLA
-- ============================================================================
-- Script per contare rapidamente quante colonne ha ogni tabella
-- Utile per confrontare sviluppo vs produzione
-- ============================================================================

USE AIDBMASTER;
GO

SET NOCOUNT ON;

PRINT '============================================================================';
PRINT 'CONTEGGIO COLONNE PER TABELLA - DATABASE: ' + DB_NAME();
PRINT 'Server: ' + @@SERVERNAME;
PRINT 'Data: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================================';
PRINT '';

-- Conta colonne per ogni tabella
SELECT 
    SCHEMA_NAME(t.schema_id) AS [Schema],
    t.name AS Tabella,
    COUNT(c.column_id) AS NumeroColonne,
    STRING_AGG(c.name, ', ') AS ElencoColonne
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
GROUP BY t.schema_id, t.name
ORDER BY [Schema], Tabella;

PRINT '';

-- Riepilogo totali
DECLARE @TotTabelle INT = (SELECT COUNT(*) FROM sys.tables);
DECLARE @TotColonne INT = (SELECT COUNT(*) FROM sys.columns WHERE object_id IN (SELECT object_id FROM sys.tables));

PRINT '';
PRINT '============================================================================';
PRINT 'RIEPILOGO TOTALI';
PRINT '============================================================================';
PRINT 'Totale Tabelle: ' + CAST(@TotTabelle AS VARCHAR);
PRINT 'Totale Colonne: ' + CAST(@TotColonne AS VARCHAR);
PRINT 'Media Colonne per Tabella: ' + CAST(@TotColonne / @TotTabelle AS VARCHAR);
PRINT '';
PRINT '============================================================================';

GO



