-- ============================================================================
-- VERIFICA ORDINE IdListaOP = 75
-- ============================================================================
USE AIDBMASTER;
GO

PRINT '============================================================================';
PRINT 'VERIFICA DATI ORDINE IdListaOP = 75';
PRINT '============================================================================';
PRINT '';

-- Dati completi dell'ordine
SELECT 
    IdListaOP,
    AnnoOrdine,
    NumeroOrdine,
    DataInizioOP,
    DataFinePrevista,
    Quantita,
    QuantitaProdotta,
    TempoCiclo,
    CodiceCentro,
    IdStato,
    Modificato,
    YEAR(DataInizioOP) AS AnnoDataInizio,
    YEAR(DataFinePrevista) AS AnnoDataFine
FROM ListaOP
WHERE IdListaOP = 75;

PRINT '';
PRINT 'Centro Lavoro:';
SELECT 
    CodiceCentro,
    DescrizioneCentro
FROM CentriLavoro
WHERE CodiceCentro = (SELECT CodiceCentro FROM ListaOP WHERE IdListaOP = 75);

PRINT '';
PRINT 'Stato Ordine:';
SELECT 
    IdStato,
    DescrizioneStato
FROM StatiOP
WHERE IdStato = (SELECT IdStato FROM ListaOP WHERE IdListaOP = 75);

PRINT '';
PRINT '============================================================================';
GO

