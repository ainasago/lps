@echo off
echo Starting TTS Application Services...
echo.

echo Starting TtsWebApi...
cd /d "%~dp0TtsWebApi"
start "TtsWebApi" cmd /k "dotnet run"

echo.
echo Starting TtsWebApp...
cd /d "%~dp0TtsWebApp"
start "TtsWebApp" cmd /k "dotnet run" 

echo.
echo All services are starting...
echo TtsWebApi will be available at: http://localhost:5275
echo TtsWebApp will be available at: http://localhost:5261

echo.
echo Press any key to exit...
pause > nul