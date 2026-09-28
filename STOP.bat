@echo off
chcp 65001 >nul
title GiveAID v2.0 - Stop
color 0C

echo.
echo ============================================================
echo        GiveAID v2.0 - Stopping all dev servers
echo ============================================================
echo.

REM Kill dotnet (backend)
taskkill /F /IM dotnet.exe /T 2>nul
if errorlevel 1 (
    echo [INFO] Khong co dotnet dang chay.
) else (
    echo [OK] Da tat backend.
)

REM Kill node (frontend + react-scripts)
taskkill /F /IM node.exe /T 2>nul
if errorlevel 1 (
    echo [INFO] Khong co node dang chay.
) else (
    echo [OK] Da tat frontend.
)

REM Kill any powershell wrappers
taskkill /F /IM powershell.exe /FI "WINDOWTITLE eq GiveAID*" 2>nul

echo.
echo Da tat xong. Nhan phim bat ky de dong.
pause >nul
