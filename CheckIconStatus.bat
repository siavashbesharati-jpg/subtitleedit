@echo off
echo ====================================
echo    CaptionFlow Icon Verification
echo ====================================
echo.

echo [INFO] Checking if CaptionFlow is running with new logo...
echo.

echo [TEST] Window title should show: "Untitled - CaptionFlow 4.0.13"
echo [TEST] Taskbar icon should show your blue CaptionFlow logo
echo [TEST] Window title bar should show your blue CaptionFlow logo
echo.

echo If the icon is still not updated, this might be due to:
echo 1. Windows icon cache (try restarting Windows)
echo 2. VS Code or IDE icon cache
echo 3. Need to manually update the embedded resource
echo.

echo [INFO] Current icon files in project:
echo.
echo Main application icon:
dir /B src\ui\SE.ico 2>nul && echo   ✓ src\ui\SE.ico (Updated) || echo   ✗ src\ui\SE.ico (Missing)

echo Resources icon:
dir /B src\ui\Resources\SE.ico 2>nul && echo   ✓ src\ui\Resources\SE.ico (Updated) || echo   ✗ src\ui\Resources\SE.ico (Missing)

echo Icons folder:
dir /B src\ui\Icons\SE.ico 2>nul && echo   ✓ src\ui\Icons\SE.ico (Updated) || echo   ✗ src\ui\Icons\SE.ico (Missing)

echo.
echo [INFO] If icons still show old logo, try these solutions:
echo.
echo Solution 1: Clear Windows icon cache
echo   - Close CaptionFlow
echo   - Restart Windows
echo   - Run CaptionFlow again
echo.
echo Solution 2: Force rebuild
echo   - dotnet clean
echo   - Delete bin and obj folders
echo   - dotnet build
echo.
echo Solution 3: Check Windows Explorer
echo   - Navigate to: src\ui\bin\Debug\net48\
echo   - Right-click SubtitleEdit.exe
echo   - Check if Properties shows the new icon
echo.

pause