@echo off
echo Stopping TTS Application Services...
echo.

echo Stopping all running processes...
taskkill /f /im dotnet.exe > nul 2>&1
taskkill /f /im node.exe > nul 2>&1

echo.
echo All services have been stopped.
echo Press any key to exit...
pause > nul