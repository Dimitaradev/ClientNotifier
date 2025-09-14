@echo off
echo ====================================
echo Starting Client Notifier Application
echo ====================================
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET Runtime is not installed!
    echo Please install .NET 9 Runtime first.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/9.0
    echo.
    pause
    exit /b 1
)

REM Set environment to Production
set ASPNETCORE_ENVIRONMENT=Production

echo Starting application...
echo.
echo The application will start at: http://localhost:5000
echo.
echo Press Ctrl+C to stop the application
echo ====================================
echo.

REM Wait 3 seconds then open browser
timeout /t 3 /nobreak >nul
start http://localhost:5000

REM Start the application
dotnet ClientNotifier.API.dll --urls="http://localhost:5000"
