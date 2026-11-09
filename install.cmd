@echo off
setlocal
set "GAME=%~1"
if not defined GAME set "GAME=D:\Steam\steamapps\common\Restory"
if not exist "%GAME%\Restory.exe" (
  echo Cartella di Restory non trovata: "%GAME%"
  echo Trascina la cartella del gioco su questo file oppure avvia:
  echo RestoryItalianPatch.exe install "percorso\Restory"
  pause
  exit /b 1
)
"%~dp0RestoryItalianPatch.exe" install "%GAME%"
set "RESULT=%ERRORLEVEL%"
if "%RESULT%"=="0" echo Patch italiana installata. Nel gioco seleziona Italiano nelle impostazioni.
pause
exit /b %RESULT%
