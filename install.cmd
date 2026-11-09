@echo off
setlocal
set "PATCHER=%~dp0RestoryItalianPatch.exe"
if not exist "%PATCHER%" (
  for /f "usebackq delims=" %%P in (`powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Find-Patcher.ps1" -BuildFolder "%~dp0dist"`) do set "PATCHER=%%P"
)
if not exist "%PATCHER%" (
  echo Non trovo RestoryItalianPatch.exe accanto a questo script o in dist.
  echo Scarica la release completa oppure compila prima il progetto con build.ps1.
  pause
  exit /b 1
)
set "GAME=%~1"
if not defined GAME (
  for /f "usebackq delims=" %%G in (`powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Find-Restory.ps1"`) do set "GAME=%%G"
)
if defined GAME echo Trovato Restory: "%GAME%"
if not defined GAME set /p "GAME=Percorso della cartella Restory: "
if not exist "%GAME%\Restory.exe" (
  echo Cartella di Restory non trovata: "%GAME%"
  echo Trascina la cartella del gioco su questo file oppure avvia:
  echo "%PATCHER%" install "percorso\Restory"
  pause
  exit /b 1
)
"%PATCHER%" install "%GAME%"
set "RESULT=%ERRORLEVEL%"
if "%RESULT%"=="0" echo Patch italiana installata. Nel gioco seleziona Italiano nelle impostazioni.
pause
exit /b %RESULT%
