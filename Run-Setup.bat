@echo off
echo ===================================
echo   SETUP AIDBMASTER SU IIS
echo ===================================
echo.

REM Verifica se eseguito come amministratore
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Esecuzione come Amministratore: OK
    echo.
) else (
    echo ERRORE: Questo script deve essere eseguito come Amministratore!
    echo Clicca destro su "Run-Setup.bat" e seleziona "Esegui come amministratore"
    pause
    exit /b 1
)

REM Naviga nella directory dello script
cd /d "%~dp0"

echo Avvio configurazione IIS e permessi...
echo.

REM Esegui lo script PowerShell
powershell.exe -ExecutionPolicy Bypass -File ".\Setup-IIS-Permissions.ps1"

if %errorLevel% == 0 (
    echo.
    echo ===================================
    echo   CONFIGURAZIONE COMPLETATA!
    echo ===================================
    echo.
    echo Prossimi passi:
    echo 1. Esegui Setup-Database-Permissions.ps1
    echo 2. Esegui lo script SQL generato su SQL Server
    echo 3. Testa l'applicazione
    echo.
) else (
    echo.
    echo ERRORE durante la configurazione!
    echo Controlla i messaggi sopra per dettagli.
    echo.
)

pause
