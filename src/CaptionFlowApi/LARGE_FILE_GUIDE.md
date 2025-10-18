# 🚀 CaptionFlow API - Large File Support & New Endpoints

## 📦 What's New

### ✨ New Features Added

1. **Large File Support (up to 2GB)**
   - Direct file uploads up to 2GB
   - Streaming upload for memory efficiency
   - Optimized for 1GB+ video files

2. **Audio File Upload Endpoint**
   - Upload audio files directly
   - Supports MP3, WAV, M4A, AAC, FLAC, OGG, WMA
   - Process audio files with embedded subtitle tracks

3. **URL-Based Video Extraction**
   - Download videos from URLs
   - Background download with progress tracking
   - Automatic processing after download

4. **Development Mode Responses**
   - Detailed JSON responses with debugging info
   - Error messages with stack traces
   - Processing notes and tips

---

## 🎯 New API Endpoints

### 1. Upload Audio File

**Endpoint:** `POST /api/subtitle/extract-audio`

**Description:** Upload an audio file and extract subtitles (for audio files with embedded subtitle streams).

**Form Data:**
- `audioFile` (file, required): Audio file to process
- `sourceLanguage` (string, optional): Language code
- `useOcr` (boolean, optional): Enable OCR

**Supported Formats:**
- MP3 (.mp3)
- WAV (.wav)
- M4A (.m4a)
- AAC (.aac)
- FLAC (.flac)
- OGG (.ogg)
- WMA (.wma)
- OPUS (.opus)

**Example (PowerShell):**
```powershell
$form = @{
    audioFile = Get-Item "C:\Audio\podcast.mp3"
    sourceLanguage = "en"
}

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/subtitle/extract-audio" `
    -Method Post `
    -Form $form `
    -SkipCertificateCheck

Write-Host "Job ID: $($response.jobId)"
```

**Example (cURL):**
```bash
curl -k -X POST "https://localhost:5001/api/subtitle/extract-audio" \
  -F "audioFile=@podcast.mp3" \
  -F "sourceLanguage=en"
```

**Response (Development Mode):**
```json
{
  "jobId": "abc123-def456",
  "status": "Processing",
  "message": "Audio uploaded successfully. Processing subtitles...",
  "fileName": "podcast.mp3",
  "subtitleCount": 0,
  "downloadUrl": null,
  "development": true,
  "uploadedFileSize": 52428800,
  "uploadedFileType": ".mp3",
  "processingNote": "[DEV] Audio files are treated as video containers and processed for embedded subtitle tracks"
}
```

---

### 2. Extract from URL

**Endpoint:** `POST /api/subtitle/extract-url`

**Description:** Download a video from a URL and extract subtitles automatically.

**Request Body (JSON):**
```json
{
  "videoUrl": "https://example.com/video.mp4",
  "sourceLanguage": "en",
  "useOcr": false,
  "outputFormat": "srt"
}
```

**Example (PowerShell):**
```powershell
$request = @{
    videoUrl = "https://example.com/video.mp4"
    sourceLanguage = "en"
    useOcr = $false
    outputFormat = "srt"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/subtitle/extract-url" `
    -Method Post `
    -Body $request `
    -ContentType "application/json" `
    -SkipCertificateCheck

Write-Host "Job ID: $($response.jobId)"
Write-Host "Status: $($response.status)"
```

**Example (cURL):**
```bash
curl -k -X POST "https://localhost:5001/api/subtitle/extract-url" \
  -H "Content-Type: application/json" \
  -d '{
    "videoUrl": "https://example.com/video.mp4",
    "sourceLanguage": "en",
    "useOcr": false
  }'
```

**Response (Development Mode):**
```json
{
  "jobId": "xyz789-abc123",
  "status": "Downloading",
  "message": "Video URL received. Downloading and processing...",
  "fileName": "video.mp4",
  "subtitleCount": 0,
  "downloadUrl": null,
  "development": true,
  "sourceUrl": "https://example.com/video.mp4",
  "processingNote": "[DEV] Video is being downloaded in background. Poll /api/subtitle/status/{jobId} to check progress",
  "estimatedDownloadNote": "[DEV] Download time depends on file size and network speed. Status will change from 'Downloading' -> 'Processing' -> 'Completed'"
}
```

---

## 📊 Status Flow for URL-Based Extraction

1. **Downloading** → Video is being downloaded from URL
2. **Processing** → Video downloaded, extracting subtitles
3. **Completed** → Subtitles extracted, ready for download
4. **Failed** → Error occurred (check error message)

**Polling Example:**
```powershell
$jobId = "xyz789-abc123"

do {
    Start-Sleep -Seconds 3
    $status = Invoke-RestMethod `
        -Uri "https://localhost:5001/api/subtitle/status/$jobId" `
        -SkipCertificateCheck
    
    Write-Host "Status: $($status.status)"
    
} while ($status.status -in @("Downloading", "Processing"))

if ($status.status -eq "Completed") {
    Write-Host "✅ Done! Subtitle count: $($status.subtitleCount)"
}
```

---

## 💾 Large File Upload Guide

### For 1GB+ Files

**1. Direct Upload (Up to 2GB)**
```powershell
# Works for files up to 2GB
$largeFile = Get-Item "C:\Videos\large_movie.mkv"
Write-Host "File size: $([math]::Round($largeFile.Length / 1GB, 2)) GB"

$form = @{
    videoFile = $largeFile
    sourceLanguage = "en"
}

# This will stream the file to avoid memory issues
$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/subtitle/extract" `
    -Method Post `
    -Form $form `
    -SkipCertificateCheck
```

**2. URL-Based Download (Recommended for Large Files)**
```powershell
# Better for very large files
$request = @{
    videoUrl = "https://cdn.example.com/2gb-movie.mp4"
    sourceLanguage = "en"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/subtitle/extract-url" `
    -Method Post `
    -Body $request `
    -ContentType "application/json" `
    -SkipCertificateCheck
```

### Configuration Limits

**Current Settings:**
- Max file size: 2GB (2,147,483,648 bytes)
- Request timeout: 30 minutes
- Memory buffer: Streaming mode (efficient for large files)

**To Increase Limits (if needed):**

Edit `appsettings.json`:
```json
{
  "FileStorage": {
    "MaxFileSizeBytes": 5368709120  // 5GB
  }
}
```

Edit `Program.cs`:
```csharp
options.Limits.MaxRequestBodySize = 5_368_709_120; // 5GB
```

---

## 🔧 Performance Tips

### For Large Files:

1. **Use URL-based extraction** for files > 1GB
   - Avoids client-side upload time
   - Better for slow connections
   - Background processing

2. **Monitor download progress**
   - Poll `/api/subtitle/status/{jobId}` every 3-5 seconds
   - Status shows "Downloading" → "Processing" → "Completed"

3. **Network considerations**
   - Ensure stable internet connection
   - Use CDN URLs when possible
   - Check server disk space

### For Audio Files:

- Audio files are processed like video containers
- Extraction works if audio file has subtitle streams
- Most music files won't have subtitles
- Podcast/audiobook files may have chapter markers

---

## 🧪 Testing the New Features

**Run the test script:**
```powershell
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi
.\TestNewEndpoints.ps1
```

**Manual Test - Audio Upload:**
```powershell
# Test with a sample audio file
$form = @{
    audioFile = Get-Item "sample.mp3"
}

Invoke-RestMethod `
    -Uri "https://localhost:5001/api/subtitle/extract-audio" `
    -Method Post -Form $form -SkipCertificateCheck
```

**Manual Test - URL Extraction:**
```powershell
# Test with a sample video URL
$body = @{
    videoUrl = "https://sample-videos.com/video.mp4"
} | ConvertTo-Json

Invoke-RestMethod `
    -Uri "https://localhost:5001/api/subtitle/extract-url" `
    -Method Post -Body $body `
    -ContentType "application/json" -SkipCertificateCheck
```

---

## 📋 Complete Endpoint Summary

| Endpoint | Method | Purpose | Max Size |
|----------|--------|---------|----------|
| `/api/subtitle/extract` | POST | Upload video file | 2GB |
| `/api/subtitle/extract-audio` | POST | Upload audio file | 2GB |
| `/api/subtitle/extract-url` | POST | Download from URL | Unlimited* |
| `/api/subtitle/status/{jobId}` | GET | Check job status | - |
| `/api/subtitle/download/{jobId}/srt` | GET | Download SRT file | - |
| `/api/subtitle/jobs` | GET | List all jobs | - |
| `/api/subtitle/health` | GET | Health check | - |

*URL downloads limited by available disk space and timeout settings

---

## 🚨 Error Handling

### Development Mode Errors

All errors include detailed information:

```json
{
  "error": "Invalid URL. Must be a valid HTTP or HTTPS URL",
  "development": true,
  "providedUrl": "ftp://example.com/video.mp4",
  "details": "URI scheme 'ftp' is not supported",
  "stackTrace": "..."
}
```

### Common Issues:

1. **File too large**
   - Error: "Audio file is too large. Maximum size is 2GB"
   - Solution: Use URL-based extraction or split the file

2. **Invalid URL**
   - Error: "Invalid URL. Must be a valid HTTP or HTTPS URL"
   - Solution: Ensure URL starts with http:// or https://

3. **Download timeout**
   - Status: "Failed"
   - Message: "Failed to download or process video: The operation has timed out"
   - Solution: Check internet connection or use direct upload

4. **Unsupported format**
   - Error: "Unsupported audio format. Allowed formats: .mp3, .wav, ..."
   - Solution: Convert file or use supported format

---

## 🎉 What's Possible Now

✅ Upload 1GB+ video files directly  
✅ Process audio files with embedded subtitles  
✅ Download videos from any HTTP/HTTPS URL  
✅ Extract subtitles from remote servers  
✅ Handle large podcast/audiobook files  
✅ Process streaming URLs  
✅ Get detailed development debugging info  

---

## 📞 Support

- **API Documentation:** https://localhost:5001/swagger
- **Test Scripts:** `TestNewEndpoints.ps1`
- **GitHub:** https://github.com/siavashbesharati-jpg/subtitleedit

---

**Made with ❤️ by the CaptionFlow Team**
