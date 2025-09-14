@echo off
echo ====================================
echo Client Notifier - Initial Setup
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

echo Setting up the application...
echo.

REM Create data directory if it doesn't exist
if not exist "ClientNotifierData" (
    mkdir ClientNotifierData
    echo Created data directory.
)

REM Run migrations
echo Initializing database...
dotnet ClientNotifier.API.dll --setup-only >nul 2>&1

echo.
echo ====================================
echo Setup completed successfully!
echo ====================================
echo.
echo You can now run START_APP.bat to start the application.
echo.
pause
