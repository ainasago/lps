@echo off
chcp 65001 >nul
title TTS 快速启动

cls
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                      TTS 快速启动菜单                          ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.
echo  [1] 启动开发环境（API + Web）
echo  [2] 只启动 API
echo  [3] 只启动 Web
echo  [4] 编译并启动
echo  [5] 打开完整管理工具
echo.
echo  [0] 退出
echo.
echo ════════════════════════════════════════════════════════════════
set /p choice=请选择 [0-5]: 

if "%choice%"=="1" goto START_ALL
if "%choice%"=="2" goto START_API
if "%choice%"=="3" goto START_WEB
if "%choice%"=="4" goto BUILD_START
if "%choice%"=="5" goto MANAGE
if "%choice%"=="0" exit

echo 无效选择！
timeout /t 2 >nul
exit

:START_ALL
cls
echo ════════════════════════════════════════════════════════════════
echo  正在启动 TTS 开发环境...
echo ════════════════════════════════════════════════════════════════
echo.
echo [1/2] 启动 TtsWebApi...
cd /d "%~dp0TtsWebApi"
start "TtsWebApi" cmd /k "dotnet run"
echo ✅ API 正在启动...
echo.
echo [2/2] 启动 TtsWebApp...
cd /d "%~dp0TtsWebApp"
start "TtsWebApp" cmd /k "dotnet run"
echo ✅ Web 正在启动...
echo.
echo ════════════════════════════════════════════════════════════════
echo ✅ 所有服务正在启动...
echo ════════════════════════════════════════════════════════════════
echo.
echo 📍 TtsWebApi: http://localhost:5275
echo 📍 TtsWebApp: http://localhost:5261
echo.
echo 💡 提示: 关闭命令行窗口即可停止服务
echo.
pause
exit

:START_API
cls
echo 启动 TtsWebApi...
cd /d "%~dp0TtsWebApi"
start "TtsWebApi" cmd /k "dotnet run"
echo.
echo ✅ API 正在启动...
echo 📍 地址: http://localhost:5275
echo.
pause
exit

:START_WEB
cls
echo 启动 TtsWebApp...
cd /d "%~dp0TtsWebApp"
start "TtsWebApp" cmd /k "dotnet run"
echo.
echo ✅ Web 正在启动...
echo 📍 地址: http://localhost:5261
echo.
pause
exit

:BUILD_START
cls
echo ════════════════════════════════════════════════════════════════
echo  编译并启动...
echo ════════════════════════════════════════════════════════════════
echo.
echo [1/3] 编译 API...
cd /d "%~dp0TtsWebApi"
dotnet build
if %errorlevel% neq 0 (
    echo ❌ API 编译失败！
    pause
    exit
)
echo ✅ API 编译成功
echo.
echo [2/3] 编译 Web...
cd /d "%~dp0TtsWebApp"
dotnet build
if %errorlevel% neq 0 (
    echo ❌ Web 编译失败！
    pause
    exit
)
echo ✅ Web 编译成功
echo.
echo [3/3] 启动服务...
cd /d "%~dp0TtsWebApi"
start "TtsWebApi" cmd /k "dotnet run"
cd /d "%~dp0TtsWebApp"
start "TtsWebApp" cmd /k "dotnet run"
echo.
echo ✅ 所有服务正在启动...
echo 📍 TtsWebApi: http://localhost:5275
echo 📍 TtsWebApp: http://localhost:5261
echo.
pause
exit

:MANAGE
start "" "%~dp0manage.bat"
exit
