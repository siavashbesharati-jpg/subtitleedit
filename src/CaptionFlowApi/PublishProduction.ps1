# CaptionFlow API - Production Publishing Script
# Open PowerShell and Enter : .\PublishProduction.ps1

param(
    [string]$OutputPath = ".\publish\production"
)

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "   CaptionFlow API - Production Publisher" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean -c Release | Out-Null

Write-Host "Building project..." -ForegroundColor Yellow
dotnet build -c Release | Out-Null

Write-Host "Publishing for production..." -ForegroundColor Yellow
Write-Host "   Configuration: Release" -ForegroundColor Gray
Write-Host "   Runtime: win-x64" -ForegroundColor Gray
Write-Host "   Self-contained: Yes" -ForegroundColor Gray
Write-Host "   Single file: Yes" -ForegroundColor Gray
Write-Host ""

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -o $OutputPath

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS! Publishing completed" -ForegroundColor Green
    Write-Host ""
    
    $fullPath = Resolve-Path $OutputPath
    Write-Host "Published files location:" -ForegroundColor Cyan
    Write-Host "   $fullPath" -ForegroundColor White
    Write-Host ""
    
    $exePath = Join-Path $OutputPath "CaptionFlowApi.exe"
    if (Test-Path $exePath) {
        $size = (Get-Item $exePath).Length / 1MB
        $sizeRounded = [math]::Round($size, 2)
        Write-Host "Executable size: $sizeRounded MB" -ForegroundColor Cyan
    }
    
    Write-Host ""
    Write-Host "To run the API:" -ForegroundColor Yellow
    Write-Host "   cd $OutputPath" -ForegroundColor White
    Write-Host "   .\CaptionFlowApi.exe" -ForegroundColor White
    Write-Host ""
    Write-Host "After starting, open in browser:" -ForegroundColor Yellow
    Write-Host "   http://localhost:5000/swagger" -ForegroundColor White
    Write-Host ""
    
    # Create run script
    $runScript = "Write-Host 'Starting CaptionFlow API...' -ForegroundColor Cyan`n.\CaptionFlowApi.exe"
    $runScript | Out-File -FilePath (Join-Path $OutputPath "RunApi.ps1") -Encoding UTF8
    
} else {
    Write-Host ""
    Write-Host "Publishing failed!" -ForegroundColor Red
    exit 1
}

Write-Host "=====================================================" -ForegroundColor Cyan
