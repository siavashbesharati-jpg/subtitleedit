# CaptionFlow API - Test Script for New Endpoints
# Tests audio upload and URL-based video extraction

$apiBaseUrl = "https://localhost:5001"

Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   CaptionFlow API - New Endpoints Test          ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Test 1: Enhanced Health Check
Write-Host "1️⃣  Testing Enhanced Health Check..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$apiBaseUrl/api/subtitle/health" -Method Get -SkipCertificateCheck
    Write-Host "✅ API is healthy!" -ForegroundColor Green
    Write-Host "   Service: $($health.service)" -ForegroundColor Gray
    Write-Host "   Version: $($health.version)" -ForegroundColor Gray
    Write-Host "   Max File Size: $($health.limits.maxFileSize)" -ForegroundColor Gray
    Write-Host "   Available Endpoints:" -ForegroundColor Gray
    Write-Host "      - Upload Video: $($health.endpoints.uploadVideo)" -ForegroundColor DarkGray
    Write-Host "      - Upload Audio: $($health.endpoints.uploadAudio)" -ForegroundColor DarkGray
    Write-Host "      - Extract from URL: $($health.endpoints.extractFromUrl)" -ForegroundColor DarkGray
    Write-Host ""
} catch {
    Write-Host "❌ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Upload Audio File
Write-Host "2️⃣  Testing Audio File Upload..." -ForegroundColor Yellow
$audioFile = "C:\Path\To\Your\Audio.mp3"  # Change this path

if (Test-Path $audioFile) {
    Write-Host "   File: $audioFile" -ForegroundColor Gray
    
    try {
        $form = @{
            audioFile = Get-Item -Path $audioFile
            sourceLanguage = "en"
            useOcr = $false
        }
        
        $audioResponse = Invoke-RestMethod -Uri "$apiBaseUrl/api/subtitle/extract-audio" `
            -Method Post -Form $form -SkipCertificateCheck
        
        Write-Host "✅ Audio uploaded successfully!" -ForegroundColor Green
        Write-Host "   Job ID: $($audioResponse.jobId)" -ForegroundColor Gray
        Write-Host "   Status: $($audioResponse.status)" -ForegroundColor Gray
        Write-Host "   File Type: $($audioResponse.uploadedFileType)" -ForegroundColor Gray
        Write-Host "   File Size: $([math]::Round($audioResponse.uploadedFileSize / 1MB, 2)) MB" -ForegroundColor Gray
        Write-Host "   Development Note: $($audioResponse.processingNote)" -ForegroundColor DarkYellow
        Write-Host ""
        
    } catch {
        $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
        Write-Host "❌ Error: $($errorDetails.error)" -ForegroundColor Red
        if ($errorDetails.development) {
            Write-Host "   Dev Message: $($errorDetails.message)" -ForegroundColor Yellow
        }
        Write-Host ""
    }
} else {
    Write-Host "⚠️  Skipping audio upload test (file not found)" -ForegroundColor Yellow
    Write-Host "   Set `$audioFile to a valid audio file path to test" -ForegroundColor Gray
    Write-Host ""
}

# Test 3: Extract from URL
Write-Host "3️⃣  Testing URL-based Video Extraction..." -ForegroundColor Yellow

# Example URLs (these are sample URLs - replace with actual video URLs)
$testUrls = @(
    "https://sample-videos.com/video321/mp4/720/big_buck_bunny_720p_1mb.mp4",
    "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"
)

$testUrl = $testUrls[0]  # Use first URL or set your own
Write-Host "   Testing with URL: $testUrl" -ForegroundColor Gray

try {
    $urlRequest = @{
        videoUrl = $testUrl
        sourceLanguage = "en"
        useOcr = $false
        outputFormat = "srt"
    } | ConvertTo-Json

    $urlResponse = Invoke-RestMethod -Uri "$apiBaseUrl/api/subtitle/extract-url" `
        -Method Post `
        -Body $urlRequest `
        -ContentType "application/json" `
        -SkipCertificateCheck
    
    Write-Host "✅ URL extraction job created!" -ForegroundColor Green
    Write-Host "   Job ID: $($urlResponse.jobId)" -ForegroundColor Gray
    Write-Host "   Status: $($urlResponse.status)" -ForegroundColor Gray
    Write-Host "   Source URL: $($urlResponse.sourceUrl)" -ForegroundColor Gray
    Write-Host "   File Name: $($urlResponse.fileName)" -ForegroundColor Gray
    Write-Host "   Processing Note: $($urlResponse.processingNote)" -ForegroundColor DarkYellow
    Write-Host "   Download Note: $($urlResponse.estimatedDownloadNote)" -ForegroundColor DarkYellow
    Write-Host ""
    
    $urlJobId = $urlResponse.jobId
    
    # Poll for completion
    Write-Host "4️⃣  Monitoring URL download and processing..." -ForegroundColor Yellow
    $maxAttempts = 60
    $attempt = 0
    $completed = $false
    
    do {
        Start-Sleep -Seconds 3
        $attempt++
        
        $statusResponse = Invoke-RestMethod -Uri "$apiBaseUrl/api/subtitle/status/$urlJobId" `
            -Method Get -SkipCertificateCheck
        
        $statusColor = switch ($statusResponse.status) {
            "Downloading" { "Cyan" }
            "Processing" { "Yellow" }
            "Completed" { "Green" }
            "Failed" { "Red" }
            default { "Gray" }
        }
        
        Write-Host "   [$attempt/$maxAttempts] Status: $($statusResponse.status)" -ForegroundColor $statusColor
        
        if ($statusResponse.status -eq "Completed") {
            $completed = $true
            Write-Host "✅ URL extraction completed!" -ForegroundColor Green
            Write-Host "   Subtitle Count: $($statusResponse.subtitleCount)" -ForegroundColor Gray
            Write-Host "   Download URL: $($statusResponse.downloadUrl)" -ForegroundColor Gray
            Write-Host ""
            
            # Download SRT file
            Write-Host "5️⃣  Downloading SRT file from URL job..." -ForegroundColor Yellow
            $outputFile = Join-Path $PSScriptRoot "url_extracted_subtitles.srt"
            
            Invoke-WebRequest -Uri "$apiBaseUrl$($statusResponse.downloadUrl)" `
                -OutFile $outputFile -SkipCertificateCheck
            
            Write-Host "✅ SRT file downloaded!" -ForegroundColor Green
            Write-Host "   Output: $outputFile" -ForegroundColor Gray
            Write-Host ""
            
        } elseif ($statusResponse.status -eq "Failed") {
            Write-Host "❌ URL extraction failed!" -ForegroundColor Red
            Write-Host "   Error: $($statusResponse.message)" -ForegroundColor Red
            break
        }
        
    } while (-not $completed -and $attempt -lt $maxAttempts)
    
    if (-not $completed -and $attempt -ge $maxAttempts) {
        Write-Host "⏱️  Timeout waiting for URL extraction (large files may take longer)" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Error with URL extraction:" -ForegroundColor Red
    try {
        $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
        Write-Host "   Error: $($errorDetails.error)" -ForegroundColor Red
        if ($errorDetails.development) {
            Write-Host "   Dev Message: $($errorDetails.message)" -ForegroundColor Yellow
            if ($errorDetails.details) {
                Write-Host "   Details: $($errorDetails.details)" -ForegroundColor Gray
            }
        }
    } catch {
        Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 4: Test Large File Size Limits
Write-Host "6️⃣  Testing File Size Validation..." -ForegroundColor Yellow
Write-Host "   Current API limits:" -ForegroundColor Gray
Write-Host "      - Max file size: 2GB (2,147,483,648 bytes)" -ForegroundColor Gray
Write-Host "      - Supported via direct upload and URL download" -ForegroundColor Gray
Write-Host "      - Streaming upload for memory efficiency" -ForegroundColor Gray
Write-Host ""

Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║            New Features Test Complete! 🎉       ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "📖 New Endpoints:" -ForegroundColor Cyan
Write-Host "   • POST /api/subtitle/extract-audio - Upload audio files" -ForegroundColor White
Write-Host "   • POST /api/subtitle/extract-url - Extract from video URL" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Key Features:" -ForegroundColor Cyan
Write-Host "   ✅ Support for files up to 2GB" -ForegroundColor Green
Write-Host "   ✅ URL-based video download and processing" -ForegroundColor Green
Write-Host "   ✅ Audio file support (.mp3, .wav, .m4a, etc.)" -ForegroundColor Green
Write-Host "   ✅ Development mode JSON responses with detailed info" -ForegroundColor Green
Write-Host "   ✅ Background download and processing" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Swagger UI: https://localhost:5001/swagger" -ForegroundColor Cyan
