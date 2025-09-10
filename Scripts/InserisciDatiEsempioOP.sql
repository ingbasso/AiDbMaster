-- Script per inserire dati di esempio nelle tabelle degli Ordini di Produzione

-- ===== INSERIMENTO OPERATORI DI ESEMPIO =====
INSERT INTO [dbo].[Operatori] ([CodiceOperatore], [Nome], [Cognome], [Email], [Telefono], [Attivo], [DataAssunzione], [LivelloCompetenza], [Note]) VALUES
('OP001', 'Mario', 'Rossi', 'mario.rossi@azienda.it', '333-1234567', 1, '2020-01-15', 4, 'Operatore esperto su macchine CNC'),
('OP002', 'Luigi', 'Bianchi', 'luigi.bianchi@azienda.it', '333-2345678', 1, '2021-03-10', 3, 'Specializzato in saldatura'),
('OP003', 'Anna', 'Verdi', 'anna.verdi@azienda.it', '333-3456789', 1, '2019-06-20', 5, 'Capo turno, esperta in controllo qualità'),
('OP004', 'Paolo', 'Neri', 'paolo.neri@azienda.it', '333-4567890', 1, '2022-09-05', 2, 'Operatore junior in formazione');

-- ===== INSERIMENTO ORDINI DI PRODUZIONE DI ESEMPIO =====
INSERT INTO [dbo].[ListaOP] (
    [TipoOrdine], [AnnoOrdine], [SerieOrdine], [NumeroOrdine], [RigaOrdine], 
    [DescrOrdine], [CodiceArticolo], [DescrizioneArticolo], [UnitaMisura], 
    [Quantita], [QuantitaProdotta], [DataInizioOP], [TempoCiclo], 
    [DataInizioSetup], [TempoSetup], [IdStato], [IdCentroLavoro], [Note],
    [DataFineOP], [DataFinePrevista], [Priorita], [IdOperatore], [CostoOrario], [TempoEffettivo]
) VALUES
-- Ordine 1: In produzione
('P', 2025, 'PRD', 1001, 1, 
'Produzione flangia acciaio', 'FL001', 'Flangia DN100 PN16', 'PZ', 
50.000000000, 35.000000000, '2025-01-10 08:00:00', 1800, 
'2025-01-10 07:30:00', 45, 2, 1, 'Lavorazione in corso, tutto regolare',
NULL, '2025-01-12 17:00:00', 3, 1, 45.50, 63000),

-- Ordine 2: Emesso
('P', 2025, 'PRD', 1002, 1, 
'Produzione cuscinetto bronzo', 'CU002', 'Cuscinetto Ø50x80', 'PZ', 
100.000000000, 0.000000000, '2025-01-15 08:00:00', 900, 
NULL, NULL, 1, 2, 'Da iniziare lunedì prossimo',
NULL, '2025-01-16 16:00:00', 2, 2, 38.00, NULL),

-- Ordine 3: Sospeso
('P', 2025, 'PRD', 1003, 1, 
'Produzione albero motore', 'AL003', 'Albero motore L=500mm', 'PZ', 
25.000000000, 8.000000000, '2025-01-08 08:00:00', 3600, 
'2025-01-08 07:00:00', 90, 4, 3, 'Sospeso per mancanza materiale',
NULL, '2025-01-14 17:00:00', 4, 3, 52.00, 28800),

-- Ordine 4: Chiuso
('P', 2025, 'PRD', 1004, 1, 
'Produzione piastra supporto', 'PI004', 'Piastra supporto 200x300', 'PZ', 
75.000000000, 75.000000000, '2025-01-05 08:00:00', 1200, 
'2025-01-05 07:45:00', 30, 3, 1, 'Completato con successo',
'2025-01-07 15:30:00', '2025-01-07 17:00:00', 2, 1, 45.50, 90000),

-- Ordine 5: Urgente in produzione
('P', 2025, 'PRD', 1005, 1, 
'Produzione emergenza guarnizione', 'GU005', 'Guarnizione speciale Ø120', 'PZ', 
10.000000000, 3.000000000, '2025-01-11 14:00:00', 600, 
'2025-01-11 13:45:00', 15, 2, 2, 'Ordine urgente per fermo macchina',
NULL, '2025-01-11 18:00:00', 5, 4, 38.00, 1800);

PRINT 'Dati di esempio inseriti con successo!';
PRINT '';
PRINT 'Operatori inseriti: 4';
PRINT 'Ordini di Produzione inseriti: 5';
PRINT '';
PRINT 'Stati degli ordini:';
PRINT '- 1 Emesso';
PRINT '- 2 In Produzione'; 
PRINT '- 1 Sospeso';
PRINT '- 1 Chiuso';
PRINT '';
PRINT 'Priorità:';
PRINT '- 2 Normale';
PRINT '- 1 Media';
PRINT '- 1 Alta';
PRINT '- 1 Urgente';
