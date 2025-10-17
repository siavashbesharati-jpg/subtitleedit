@echo off
echo ========================================
echo    Replace Icon Files with New Version
echo ========================================
echo.

REM Check if the new high-quality icon exists
if exist "captionflow_logo_new.ico" (
    echo [INFO] Found new icon file: captionflow_logo_new.ico
    echo.
    
    echo [INFO] Backing up current icon...
    copy "src\ui\SE.ico" "src\ui\SE_backup.ico" >nul 2>&1
    
    echo [INFO] Replacing all icon files...
    
    REM Replace all icon files
    copy "captionflow_logo_new.ico" "src\ui\SE.ico" >nul 2>&1
    copy "captionflow_logo_new.ico" "src\ui\Resources\SE.ico" >nul 2>&1
    copy "captionflow_logo_new.ico" "src\ui\Icons\SE.ico" >nul 2>&1
    
    if %errorlevel% equ 0 (
        echo [SUCCESS] Icon files updated!
        echo.
        echo [INFO] Cleaning and rebuilding project...
        
        dotnet clean >nul 2>&1
        dotnet build
        
        if %errorlevel% equ 0 (
            echo.
            echo [SUCCESS] Build completed!
            echo.
            echo [INFO] Starting CaptionFlow with new high-quality icon...
            start "" "src\ui\bin\Debug\net48\SubtitleEdit.exe"
            
            echo.
            echo [SUCCESS] CaptionFlow launched!
            echo.
            echo The icon should now appear larger and clearer in:
            echo   - Window title bar
            echo   - Windows taskbar
            echo   - Alt+Tab switcher
            echo   - File properties
            echo.
            echo If still small, restart Windows to clear icon cache.
        ) else (
            echo [ERROR] Build failed. Check error messages above.
        )
    ) else (
        echo [ERROR] Failed to copy icon files.
    )
) else (
    echo [ERROR] captionflow_logo_new.ico not found!
    echo.
    echo Please follow these steps:
    echo.
    echo 1. Go to: https://convertio.co/png-ico/
    echo 2. Upload your original CaptionFlow PNG logo
    echo 3. Select options for multiple sizes (16,32,48,64,128,256)
    echo 4. Download the result
    echo 5. Save it as "captionflow_logo_new.ico" in this folder
    echo 6. Run this script again
    echo.
)

pause