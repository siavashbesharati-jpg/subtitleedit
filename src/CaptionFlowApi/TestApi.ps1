# CaptionFlow API Test Script
# This script demonstrates how to use the CaptionFlow RESTful API

# Configuration
$apiBaseUrl = "https://localhost:5001"
$videoFile = "C:\Path\To\Your\Video.mkv"  # Change this to your video file path

Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     CaptionFlow API - Integration Test          ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Test 1: Health Check
Write-Host "1️⃣  Testing Health Check..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$apiBaseUrl/api/subtitle/health" -Method Get -SkipCertificateCheck
    Write-Host "✅ API is healthy!" -ForegroundColor Green
    Write-Host "   Status: $($health.status)" -ForegroundColor Gray
    Write-Host "   Service: $($health.service)" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "❌ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Upload Video (if video file exists)
if (Test-Path $videoFile) {
    Write-Host "2️⃣  Uploading video file..." -ForegroundColor Yellow
    Write-Host "   File: $videoFile" -ForegroundColor Gray
    
    try {
        $form = @{
            videoFile = Get-Item -Path $videoFile
            sourceLanguage = "en"
            useOcr = $false
        }
        
        $uploadResponse = Invoke-RestMethod -Uri "$apiBaseUrl/api/subtitle/extract" `
            -Method Post -Form $form -SkipCertificateCheck
        
        Write-Host "✅ Video uploaded successfully!" -ForegroundColor Green
        Write-Host "   Job ID: $($uploadResponse.jobId)" -ForegroundColor Gray
        Write-Host "   Status: $($uploadResponse.status)" -ForegroundColor Gray
        Write-Host "   Message: $($uploadResponse.message)" -ForegroundColor Gray
        Write-Host ""
        
        $jobId = $uploadResponse.jobId
        
        # Test 3: Poll for completion
        Write-Host "3️⃣  Waiting for subtitle extraction..." -ForegroundColor Yellow
        $maxAttempts = 30
        $attempt = 0
        $completed = $false
        
        do {
            Start-Sleep -Seconds 2
            $attempt++
            
            $statusResponse = Invoke-RestMethod -Uri "$apiBaseUrl/api/subtitle/status/$jobId" `
                -Method Get -SkipCertificateCheck
            
            Write-Host "   Attempt $attempt/$maxAttempts - Status: $($statusResponse.status)" -ForegroundColor Gray
            
            if ($statusResponse.status -eq "Completed") {
                $completed = $true
                Write-Host "✅ Subtitle extraction completed!" -ForegroundColor Green
                Write-Host "   Subtitle Count: $($statusResponse.subtitleCount)" -ForegroundColor Gray
                Write-Host "   Download URL: $($statusResponse.downloadUrl)" -ForegroundColor Gray
                Write-Host ""
                
                # Test 4: Download SRT file
                Write-Host "4️⃣  Downloading SRT file..." -ForegroundColor Yellow
                $outputFile = Join-Path $PSScriptRoot "extracted_subtitles.srt"
                
                Invoke-WebRequest -Uri "$apiBaseUrl$($statusResponse.downloadUrl)" `
                    -OutFile $outputFile -SkipCertificateCheck
                
                Write-Host "✅ SRT file downloaded!" -ForegroundColor Green
                Write-Host "   Output: $outputFile" -ForegroundColor Gray
                Write-Host ""
                
                # Show first few lines of the SRT file
                Write-Host "📄 Preview of SRT content:" -ForegroundColor Cyan
                Get-Content $outputFile -Head 20 | ForEach-Object {
                    Write-Host "   $_" -ForegroundColor Gray
                }
                
            } elseif ($statusResponse.status -eq "Failed") {
                Write-Host "❌ Subtitle extraction failed!" -ForegroundColor Red
                Write-Host "   Error: $($statusResponse.message)" -ForegroundColor Red
                break
            }
            
        } while (-not $completed -and $attempt -lt $maxAttempts)
        
        if (-not $completed -and $attempt -ge $maxAttempts) {
            Write-Host "⏱️  Timeout waiting for subtitle extraction" -ForegroundColor Yellow
        }
        
    } catch {
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "2️⃣  Skipping video upload test (video file not found)" -ForegroundColor Yellow
    Write-Host "   Please set `$videoFile variable to a valid video file path" -ForegroundColor Gray
    Write-Host ""
}

# Test 5: List all jobs
Write-Host "5️⃣  Listing all jobs..." -ForegroundColor Yellow
try {
    $jobs = Invoke-RestMethod -Uri "$apiBaseUrl/api/subtitle/jobs" -Method Get -SkipCertificateCheck
    Write-Host "✅ Retrieved $($jobs.Count) job(s)" -ForegroundColor Green
    
    foreach ($job in $jobs | Select-Object -First 5) {
        Write-Host "   Job: $($job.jobId)" -ForegroundColor Gray
        Write-Host "      Status: $($job.status)" -ForegroundColor Gray
        Write-Host "      File: $($job.fileName)" -ForegroundColor Gray
        Write-Host "      Created: $($job.createdAt)" -ForegroundColor Gray
        Write-Host ""
    }
} catch {
    Write-Host "❌ Error listing jobs: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║            Test Complete! 🎉                     ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "📖 API Documentation: $apiBaseUrl/swagger" -ForegroundColor Cyan
Write-Host "🌐 Health Endpoint: $apiBaseUrl/api/subtitle/health" -ForegroundColor Cyan
