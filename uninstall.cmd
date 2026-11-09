@echo off
setlocal
set "GAME=%~1"
if not defined GAME set "GAME=D:\Steam\steamapps\common\Restory"
"%~dp0RestoryItalianPatch.exe" uninstall "%GAME%"
set "RESULT=%ERRORLEVEL%"
pause
exit /b %RESULT%
