-- ============================================
-- Script per verificare e inserire agenti di test
-- ============================================

-- 1. Verifica agenti esistenti
PRINT '=== AGENTI ESISTENTI ==='
SELECT COUNT(*) AS NumeroAgenti FROM TabellaAgenti;
SELECT * FROM TabellaAgenti;

-- 2. Se non ci sono agenti, inserisci alcuni di test
IF NOT EXISTS (SELECT 1 FROM TabellaAgenti)
BEGIN
    PRINT '=== INSERIMENTO AGENTI DI TEST ==='
    
    INSERT INTO TabellaAgenti (CodiceAgente, DescrizioneAgente, IndirizzoAgente, CAPAgente, CittaAgente, ProvinciaAgente, Attivo)
    VALUES 
        (1, 'Mario Rossi', 'Via Roma 123', '20100', 'Milano', 'MI', 1),
        (2, 'Luca Bianchi', 'Corso Italia 45', '10121', 'Torino', 'TO', 1),
        (3, 'Giuseppe Verdi', 'Piazza Duomo 7', '50122', 'Firenze', 'FI', 1),
        (4, 'Anna Neri', 'Via Veneto 89', '00187', 'Roma', 'RM', 1),
        (5, 'Paolo Ferrari', 'Viale Europa 12', '35131', 'Padova', 'PD', 0);
    
    PRINT 'Inseriti 5 agenti di test';
    
    -- Verifica inserimento
    SELECT * FROM TabellaAgenti;
END
ELSE
BEGIN
    PRINT 'Ci sono già agenti nel database';
END

-- 3. Verifica utenti con CodiceAgente
PRINT '=== AGENTI GIÀ CONVERTITI IN UTENTI ==='
SELECT 
    u.Id,
    u.Email,
    u.FirstName,
    u.LastName,
    u.CodiceAgente,
    ta.DescrizioneAgente
FROM AspNetUsers u
LEFT JOIN TabellaAgenti ta ON u.CodiceAgente = ta.CodiceAgente
WHERE u.CodiceAgente IS NOT NULL;

