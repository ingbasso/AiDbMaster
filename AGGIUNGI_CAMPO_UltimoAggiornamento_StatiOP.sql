-- ============================================================================
-- AGGIUNGI CAMPO UltimoAggiornamento A TABELLA StatiOP
-- ============================================================================
-- Script per allineare la struttura della tabella StatiOP
-- Aggiunge il campo UltimoAggiornamento (datetime)
-- ============================================================================

USE AIDBMASTER;
GO

PRINT '============================================================================';
PRINT 'AGGIORNAMENTO STRUTTURA TABELLA StatiOP';
PRINT 'Data: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================================';
PRINT '';

-- Verifica che la tabella esista
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StatiOP')
BEGIN
    PRINT '❌ ERRORE: Tabella StatiOP non trovata!';
    PRINT '';
    RETURN;
END

-- Aggiungi colonna UltimoAggiornamento se non esiste
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('StatiOP') 
    AND name = 'UltimoAggiornamento'
)
BEGIN
    PRINT 'Aggiunta colonna UltimoAggiornamento...';
    
    ALTER TABLE [dbo].[StatiOP] 
    ADD [UltimoAggiornamento] DATETIME NULL;
    
    PRINT '✅ Colonna UltimoAggiornamento aggiunta con successo!';
    PRINT '';
    
    -- Aggiorna i record esistenti con la data corrente (opzionale)
    UPDATE [dbo].[StatiOP]
    SET [UltimoAggiornamento] = GETDATE()
    WHERE [UltimoAggiornamento] IS NULL;
    
    PRINT '✅ Record esistenti aggiornati con data corrente';
END
ELSE
BEGIN
    PRINT '⏭️  Colonna UltimoAggiornamento già presente nella tabella StatiOP';
END

PRINT '';

-- Verifica finale
SELECT 
    'StatiOP' AS Tabella,
    COUNT(c.column_id) AS NumeroColonne,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM sys.columns 
            WHERE object_id = OBJECT_ID('StatiOP') 
            AND name = 'UltimoAggiornamento'
        ) THEN '✅ PRESENTE'
        ELSE '❌ MANCANTE'
    END AS CampoUltimoAggiornamento
FROM sys.columns c
WHERE c.object_id = OBJECT_ID('StatiOP')
GROUP BY c.object_id;

PRINT '';
PRINT '============================================================================';
PRINT 'OPERAZIONE COMPLETATA';
PRINT '============================================================================';

GO



