@echo off
chcp 65001 >nul
title GiveAID v2.0 - Dev Server
color 0B

echo.
echo ============================================================
echo        GiveAID v2.0 - Backend + Frontend starter
echo ============================================================
echo.

REM ── Pre-flight: check tools ────────────────────────────────
where dotnet >nul 2>nul
if errorlevel 1 (
    echo [ERROR] Chua cai .NET SDK 10. Cai tai:
    echo         https://dotnet.microsoft.com/download/dotnet/10.0
    pause
    exit /b 1
)

where node >nul 2>nul
if errorlevel 1 (
    echo [ERROR] Chua cai Node.js 18+. Cai tai:
    echo         https://nodejs.org/
    pause
    exit /b 1
)

where npm >nul 2>nul
if errorlevel 1 (
    echo [ERROR] Chua cai npm. Cai tai:
    echo         https://nodejs.org/
    pause
    exit /b 1
)

echo [OK] Da co .NET SDK, Node.js, npm.
echo.

REM ── Set dev-only secrets (DO NOT use in production!) ────────
REM Required by SeedData: ADMIN_PASSWORD (min 8 chars), DEMO_PASSWORD
REM Required by JwtSettings: Jwt__Secret (min 32 chars)
set "ADMIN_PASSWORD=DevAdmin@123"
set "DEMO_PASSWORD=Demo@123"
set "Jwt__Secret=dev-only-jwt-secret-do-not-use-in-production-please-32+"
set "ASPNETCORE_ENVIRONMENT=Development"

REM ── Step 1: install frontend deps (first time only) ────────
if not exist "GiveAID.Client\node_modules" (
    echo [STEP 1/2] Dang cai dat React dependencies (lan dau, mat 2-5 phut)...
    cd GiveAID.Client
    call npm install
    if errorlevel 1 (
        echo [ERROR] npm install that bai. Kiem tra internet va thu lai.
        pause
        exit /b 1
    )
    cd ..
) else (
    echo [STEP 1/2] React dependencies da san, bo qua.
)
echo.

REM ── Step 2: open browser after 25s ──────────────────────────
echo [STEP 2/2] Dang khoi dong backend + frontend...
echo.
echo    Backend  -> http://localhost:5231
echo    Frontend -> http://localhost:3000
echo.
echo    Sau ~20 giay, trinh duyet se tu mo trang web.
echo    Nhan Ctrl+C bat ky luc nao de dung server.
echo.
echo ============================================================
echo.

REM Launch browser in background after delay
start /min "" cmd /c "timeout /t 25 /nobreak >nul && start http://localhost:3000"

REM Start the dev stack (this blocks; Ctrl+C stops everything)
cd GiveAID.Client
call npm start
