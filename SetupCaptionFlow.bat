@echo off
echo ========================================
echo    CaptionFlow Logo Replacement
echo ========================================
echo.

REM Check if the logo file exists
if not exist "captionflow_logo.png" (
    echo [ERROR] captionflow_logo.png not found!
    echo.
    echo Please follow these steps:
    echo 1. Save your CaptionFlow logo image as "captionflow_logo.png" 
    echo 2. Place it in this directory: %CD%
    echo 3. Run this script again
    echo.
    echo [INFO] You can drag and drop your PNG file into this folder,
    echo        then rename it to "captionflow_logo.png"
    echo.
    pause
    exit /b 1
)

echo [INFO] Found captionflow_logo.png - Converting to ICO format...
echo.

REM Run the PowerShell conversion script
powershell -ExecutionPolicy Bypass -File "ConvertToIco.ps1" -InputPath "captionflow_logo.png" -OutputPath "src\ui\SE.ico"

if %errorlevel% equ 0 (
    echo.
    echo [SUCCESS] Logo converted successfully!
    echo.
    echo [INFO] Copying logo to all required locations...
    
    REM Copy to all icon locations
    copy "src\ui\SE.ico" "src\ui\Resources\SE.ico" >nul 2>&1
    copy "src\ui\SE.ico" "src\ui\Icons\SE.ico" >nul 2>&1
    
    echo [SUCCESS] Logo files updated!
    echo.
    echo [INFO] Building CaptionFlow application...
    
    REM Build the application
    dotnet build
    
    if %errorlevel% equ 0 (
        echo.
        echo [SUCCESS] Build completed successfully!
        echo.
        echo [INFO] Starting CaptionFlow...
        echo.
        
        REM Run the application
        start "" "src\ui\bin\Debug\net48\SubtitleEdit.exe"
        
        echo [SUCCESS] CaptionFlow is now running with your custom logo!
        echo.
        echo Your application window title should now show: "Untitled - CaptionFlow 4.0.13"
        echo with your custom logo in the window and taskbar.
        echo.
    ) else (
        echo [ERROR] Build failed. Please check the error messages above.
    )
) else (
    echo.
    echo [ERROR] Logo conversion failed. Please check if:
    echo 1. The PNG file is valid and not corrupted
    echo 2. You have necessary permissions
    echo 3. PowerShell execution policy allows script execution
    echo.
    echo To fix PowerShell execution policy, run as Administrator:
    echo Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
)

echo.
pause