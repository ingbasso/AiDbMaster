# Test completo permessi AiDbMaster
param(
    [string]$AppPath = "C:\inetpub\wwwroot\AiDbMaster",
    [string]$AppPoolName = "AiDbMaster"
)

Write-Host "=============================================" -ForegroundColor Green
Write-Host "    TEST PERMESSI AIDBMASTER PRODUZIONE" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Test 1: Application Pool
Write-Host "`n1. VERIFICA APPLICATION POOL:" -ForegroundColor Cyan
try {
    Import-Module WebAdministration -ErrorAction Stop
    $poolState = Get-WebAppPoolState -Name $AppPoolName -ErrorAction Stop
    Write-Host "✓ Application Pool '$AppPoolName': $($poolState.Value)" -ForegroundColor Green
    
    # Verifica configurazione
    $poolConfig = Get-WebConfiguration -PSPath "IIS:\AppPools\$AppPoolName"
    $identity = Get-WebConfigurationProperty -PSPath "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType"
    Write-Host "  Identity Type: $($identity.Value)" -ForegroundColor Gray
    
} catch {
    Write-Host "✗ Errore Application Pool: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Soluzione: Eseguire 'New-WebAppPool -Name $AppPoolName'" -ForegroundColor Yellow
}

# Test 2: Directory Applicazione
Write-Host "`n2. VERIFICA DIRECTORY APPLICAZIONE:" -ForegroundColor Cyan
if (Test-Path $AppPath) {
    Write-Host "✓ Directory applicazione esistente: $AppPath" -ForegroundColor Green
    
    # Verifica permessi
    $permissions = icacls $AppPath 2>$null | Select-String "IIS APPPOOL\\$AppPoolName"
    if ($permissions) {
        Write-Host "✓ Permessi IIS APPPOOL trovati" -ForegroundColor Green
        $permissions | ForEach-Object { Write-Host "  $($_.Line)" -ForegroundColor Gray }
    } else {
        Write-Host "✗ Permessi IIS APPPOOL mancanti!" -ForegroundColor Red
        Write-Host "  Soluzione: icacls `"$AppPath`" /grant `"IIS APPPOOL\$AppPoolName`:(OI)(CI)RX`" /T" -ForegroundColor Yellow
    }
} else {
    Write-Host "✗ Directory applicazione non trovata: $AppPath" -ForegroundColor Red
    Write-Host "  Soluzione: Creare la directory e copiare i file dell'applicazione" -ForegroundColor Yellow
}

# Test 3: File Applicazione
Write-Host "`n3. VERIFICA FILE APPLICAZIONE:" -ForegroundColor Cyan
$requiredFiles = @(
    @{Name="AiDbMaster.dll"; Critical=$true},
    @{Name="web.config"; Critical=$true},
    @{Name="appsettings.Production.json"; Critical=$true},
    @{Name="appsettings.json"; Critical=$false}
)

foreach ($file in $requiredFiles) {
    $filePath = Join-Path $AppPath $file.Name
    if (Test-Path $filePath) {
        Write-Host "✓ File trovato: $($file.Name)" -ForegroundColor Green
        
        # Verifica dimensione file
        $fileInfo = Get-Item $filePath
        if ($fileInfo.Length -gt 0) {
            Write-Host "  Dimensione: $([math]::Round($fileInfo.Length/1KB, 2)) KB" -ForegroundColor Gray
        } else {
            Write-Host "  ⚠ File vuoto!" -ForegroundColor Yellow
        }
    } else {
        if ($file.Critical) {
            Write-Host "✗ File CRITICO mancante: $($file.Name)" -ForegroundColor Red
        } else {
            Write-Host "⚠ File opzionale mancante: $($file.Name)" -ForegroundColor Yellow
        }
    }
}

# Test 4: Cartelle Speciali
Write-Host "`n4. VERIFICA CARTELLE SPECIALI:" -ForegroundColor Cyan
$specialFolders = @("Logs", "DocumentsStorage", "Uploads")
foreach ($folder in $specialFolders) {
    $folderPath = Join-Path $AppPath $folder
    if (Test-Path $folderPath) {
        Write-Host "✓ Cartella esistente: $folder" -ForegroundColor Green
        
        # Test scrittura
        $testFile = Join-Path $folderPath "test-write-$(Get-Date -Format 'yyyyMMdd-HHmmss').tmp"
        try {
            "Test scrittura $(Get-Date)" | Out-File $testFile -ErrorAction Stop
            if (Test-Path $testFile) {
                Remove-Item $testFile -ErrorAction SilentlyContinue
                Write-Host "  ✓ Scrittura OK" -ForegroundColor Green
            }
        } catch {
            Write-Host "  ✗ Errore scrittura: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "    Soluzione: icacls `"$folderPath`" /grant `"IIS APPPOOL\$AppPoolName`:(OI)(CI)F`" /T" -ForegroundColor Yellow
        }
        
        # Verifica permessi specifici
        $folderPerms = icacls $folderPath 2>$null | Select-String "IIS APPPOOL\\$AppPoolName"
        if ($folderPerms) {
            Write-Host "  ✓ Permessi IIS APPPOOL presenti" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Permessi IIS APPPOOL mancanti" -ForegroundColor Red
        }
        
    } else {
        Write-Host "✗ Cartella mancante: $folder" -ForegroundColor Red
        Write-Host "  Soluzione: New-Item -Path `"$folderPath`" -ItemType Directory -Force" -ForegroundColor Yellow
    }
}

# Test 5: Connessione Database
Write-Host "`n5. TEST CONNESSIONE DATABASE:" -ForegroundColor Cyan
try {
    # Leggi connection string da appsettings.Production.json
    $settingsPath = Join-Path $AppPath "appsettings.Production.json"
    if (Test-Path $settingsPath) {
        $settings = Get-Content $settingsPath | ConvertFrom-Json
        if ($settings.ConnectionStrings -and $settings.ConnectionStrings.DefaultConnection) {
            $connectionString = $settings.ConnectionStrings.DefaultConnection
            Write-Host "✓ Connection string trovata" -ForegroundColor Green
            
            # Analizza connection string (senza mostrare password)
            if ($connectionString -match "Data Source=([^;]+)") {
                Write-Host "  Server: $($matches[1])" -ForegroundColor Gray
            }
            if ($connectionString -match "Initial Catalog=([^;]+)") {
                Write-Host "  Database: $($matches[1])" -ForegroundColor Gray
            }
            if ($connectionString -match "User ID=([^;]+)") {
                Write-Host "  User ID: $($matches[1])" -ForegroundColor Gray
            } elseif ($connectionString -match "Integrated Security=true") {
                Write-Host "  Autenticazione: Windows (Integrated Security)" -ForegroundColor Gray
            }
            
            # Test connessione base (se disponibile SqlServer module)
            try {
                if (Get-Module -ListAvailable -Name SqlServer) {
                    Import-Module SqlServer -ErrorAction Stop
                    $result = Invoke-Sqlcmd -ConnectionString $connectionString -Query "SELECT 1 as Test" -QueryTimeout 10 -ErrorAction Stop
                    Write-Host "✓ Connessione database OK" -ForegroundColor Green
                } else {
                    Write-Host "⚠ Modulo SqlServer non disponibile - test connessione saltato" -ForegroundColor Yellow
                    Write-Host "  Installa con: Install-Module -Name SqlServer" -ForegroundColor Gray
                }
            } catch {
                Write-Host "✗ Errore connessione database: $($_.Exception.Message)" -ForegroundColor Red
                Write-Host "  Verifica: Server SQL attivo, database esistente, credenziali corrette" -ForegroundColor Yellow
            }
            
        } else {
            Write-Host "✗ Connection string non trovata in appsettings.Production.json" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ File appsettings.Production.json non trovato" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Errore lettura configurazione: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Verifica Sito Web IIS
Write-Host "`n6. VERIFICA CONFIGURAZIONE SITO IIS:" -ForegroundColor Cyan
try {
    # Trova siti che usano questo Application Pool
    $sites = Get-WebApplication | Where-Object {$_.ApplicationPool -eq $AppPoolName}
    if ($sites) {
        Write-Host "✓ Siti configurati con Application Pool '$AppPoolName':" -ForegroundColor Green
        $sites | ForEach-Object {
            Write-Host "  - Sito: $($_.Site) | Path: $($_.Path) | Fisica: $($_.PhysicalPath)" -ForegroundColor Gray
        }
    } else {
        Write-Host "⚠ Nessun sito configurato con Application Pool '$AppPoolName'" -ForegroundColor Yellow
        Write-Host "  Soluzione: Assegnare l'Application Pool al sito web" -ForegroundColor Yellow
    }
    
    # Verifica binding del sito
    $bindings = Get-WebBinding | Where-Object {$_.ItemXPath -like "*AiDbMaster*"}
    if ($bindings) {
        Write-Host "✓ Binding trovati:" -ForegroundColor Green
        $bindings | ForEach-Object {
            Write-Host "  - $($_.protocol)://$($_.bindingInformation)" -ForegroundColor Gray
        }
    }
    
} catch {
    Write-Host "✗ Errore verifica sito IIS: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Verifica .NET Runtime
Write-Host "`n7. VERIFICA .NET RUNTIME:" -ForegroundColor Cyan
try {
    $dotnetInfo = dotnet --info 2>$null
    if ($dotnetInfo) {
        $runtimeLines = $dotnetInfo | Select-String "Microsoft.AspNetCore.App" | Select-Object -First 3
        if ($runtimeLines) {
            Write-Host "✓ .NET Runtime installato:" -ForegroundColor Green
            $runtimeLines | ForEach-Object {
                Write-Host "  $($_.Line.Trim())" -ForegroundColor Gray
            }
            
            # Verifica versione 8.0
            $net8Runtime = $dotnetInfo | Select-String "Microsoft.AspNetCore.App 8\."
            if ($net8Runtime) {
                Write-Host "✓ .NET 8.0 Runtime disponibile" -ForegroundColor Green
            } else {
                Write-Host "⚠ .NET 8.0 Runtime non trovato" -ForegroundColor Yellow
                Write-Host "  L'applicazione richiede .NET 8.0 Runtime" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "✗ .NET non trovato o non funzionante" -ForegroundColor Red
        Write-Host "  Installare .NET 8.0 Runtime" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Errore verifica .NET: $($_.Exception.Message)" -ForegroundColor Red
}

# Riepilogo finale
Write-Host "`n=============================================" -ForegroundColor Green
Write-Host "    RIEPILOGO TEST" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

$issues = @()
if (-not (Get-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue)) {
    $issues += "Application Pool mancante"
}
if (-not (Test-Path $AppPath)) {
    $issues += "Directory applicazione mancante"
}
if (-not (Test-Path (Join-Path $AppPath "AiDbMaster.dll"))) {
    $issues += "File applicazione mancanti"
}

if ($issues.Count -eq 0) {
    Write-Host "✅ TUTTI I TEST SUPERATI!" -ForegroundColor Green
    Write-Host "L'applicazione dovrebbe funzionare correttamente." -ForegroundColor Green
} else {
    Write-Host "⚠️ PROBLEMI TROVATI:" -ForegroundColor Yellow
    $issues | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host "`nConsulta la GUIDA-CONFIGURAZIONE-UTENTI-PRODUZIONE.md per le soluzioni." -ForegroundColor Cyan
}

Write-Host "`n🔧 COMANDI RAPIDI PER RISOLVERE I PROBLEMI:" -ForegroundColor Cyan
Write-Host "1. Crea Application Pool:" -ForegroundColor White
Write-Host "   New-WebAppPool -Name '$AppPoolName' -Force" -ForegroundColor Gray
Write-Host "2. Configura permessi:" -ForegroundColor White
Write-Host "   icacls `"$AppPath`" /grant `"IIS APPPOOL\$AppPoolName`:(OI)(CI)RX`" /T" -ForegroundColor Gray
Write-Host "3. Riavvia servizi:" -ForegroundColor White
Write-Host "   Restart-WebAppPool -Name '$AppPoolName'; iisreset" -ForegroundColor Gray

Write-Host "`n=============================================" -ForegroundColor Green
Write-Host "    TEST COMPLETATO" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

