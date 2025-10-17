@echo off
echo Converting CaptionFlow logo to ICO format...
echo.
echo Please make sure you have saved your CaptionFlow logo as "captionflow_logo.png" in this directory.
echo.
pause

if not exist "captionflow_logo.png" (
    echo Error: captionflow_logo.png not found in current directory!
    echo Please save your CaptionFlow logo as "captionflow_logo.png" first.
    pause
    exit /b 1
)

echo Converting PNG to ICO...
powershell -ExecutionPolicy Bypass -File "ConvertToIco.ps1" -InputPath "captionflow_logo.png" -OutputPath "src\ui\SE.ico"

if %errorlevel% equ 0 (
    echo.
    echo Success! Logo has been replaced.
    echo.
    echo Also copying to other icon locations...
    copy "src\ui\SE.ico" "src\ui\Resources\SE.ico"
    copy "src\ui\SE.ico" "src\ui\Icons\SE.ico"
    
    echo.
    echo Logo replacement complete! You can now rebuild and run the application.
) else (
    echo.
    echo Error occurred during conversion. Please check the image file.
)

pause