@echo off
echo ========================================
echo    CaptionFlow Icon Size Fix
echo ========================================
echo.

echo The small icon issue in Windows is typically caused by:
echo.
echo 1. ICO file contains only small sizes (16x16, 32x32)
echo 2. Missing larger sizes needed for taskbar and title bar
echo 3. Windows icon cache showing old version
echo.

echo SOLUTIONS:
echo.
echo [Option 1] Use an online ICO converter (RECOMMENDED):
echo   1. Go to: https://convertio.co/png-ico/ or https://favicon.io/favicon-converter/
echo   2. Upload your original CaptionFlow PNG image
echo   3. Select "Create ICO with multiple sizes" option
echo   4. Download the result as "captionflow_logo_new.ico"
echo   5. Replace the current icon files
echo.

echo [Option 2] Use IcoFX or similar icon editor:
echo   - Download IcoFX (free icon editor)
echo   - Open your PNG logo
echo   - Export as ICO with sizes: 16, 24, 32, 48, 64, 128, 256
echo.

echo [Option 3] Windows Icon Cache Clear:
echo   - The icon might actually be correct but cached
echo   - Try restarting Windows completely
echo.

echo Current icon file info:
dir captionflow_logo.ico 2>nul && (
    echo   File exists: captionflow_logo.ico
    echo   Size: 
    for %%i in (captionflow_logo.ico) do echo     %%~zi bytes
) || echo   File not found: captionflow_logo.ico

echo.
echo After getting a new multi-size ICO file:
echo   1. Save it as "captionflow_logo_new.ico"
echo   2. Run: ReplaceIconFiles.bat
echo.

pause