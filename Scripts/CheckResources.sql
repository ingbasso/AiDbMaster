-- Script per verificare lo stato attuale delle risorse nel database

-- 1. Conta risorse totali
SELECT 'Totale Risorse' AS Info, COUNT(*) AS Valore
FROM Resources;

-- 2. Risorse Root (gruppi principali)
SELECT 
    Id,
    Name,
    DisplayName,
    MenuOrder,
    IsMenuGroup,
    IsConfigured
FROM Resources
WHERE ParentResourceId IS NULL
ORDER BY MenuOrder;

-- 3. Gruppi con loro figlie (count)
SELECT 
    parent.Id AS ParentId,
    parent.Name AS ParentName,
    COUNT(child.Id) AS NumeroFiglie
FROM Resources parent
LEFT JOIN Resources child ON child.ParentResourceId = parent.Id
WHERE parent.IsMenuGroup = 1
GROUP BY parent.Id, parent.Name
ORDER BY parent.MenuOrder;

-- 4. Risorse con ParentResourceId non valido (problema!)
SELECT 
    r.Id,
    r.Name,
    r.DisplayName,
    r.ParentResourceId AS ParentId_NonEsistente
FROM Resources r
LEFT JOIN Resources parent ON parent.Id = r.ParentResourceId
WHERE r.ParentResourceId IS NOT NULL 
  AND parent.Id IS NULL;

-- 5. Verifica esistenza gruppo "Ordini" (da eliminare)
SELECT 
    Id,
    Name,
    DisplayName,
    IsMenuGroup
FROM Resources
WHERE Name = 'Ordini';

-- 6. Verifica esistenza gruppo "InterrogazioniDB" (deve essere creato)
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM Resources WHERE Name = 'InterrogazioniDB')
        THEN 'ESISTE ✅'
        ELSE 'NON ESISTE ❌ - DA CREARE'
    END AS StatoInterrogazioniDB;

-- 7. Lista completa per confronto
SELECT 
    Id,
    Name,
    DisplayName,
    ParentResourceId,
    MenuOrder,
    IsMenuGroup,
    IsConfigured,
    IsActive
FROM Resources
ORDER BY 
    COALESCE(ParentResourceId, 0),
    MenuOrder;

