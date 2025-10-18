# 🚀 CaptionFlow API - Quick Reference

## 📡 Base URL
```
https://localhost:5001
```

## 🎯 New Endpoints

### 1. Upload Audio File
```http
POST /api/subtitle/extract-audio
```
```powershell
$form = @{ audioFile = Get-Item "audio.mp3" }
Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract-audio" -Method Post -Form $form
```

### 2. Extract from URL
```http
POST /api/subtitle/extract-url
```
```powershell
$body = @{ videoUrl = "https://example.com/video.mp4" } | ConvertTo-Json
Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract-url" -Method Post -Body $body -ContentType "application/json"
```

## 📊 All Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/subtitle/extract` | POST | Upload video (2GB max) |
| `/api/subtitle/extract-audio` | POST | Upload audio (2GB max) |
| `/api/subtitle/extract-url` | POST | Download from URL |
| `/api/subtitle/status/{id}` | GET | Check job status |
| `/api/subtitle/download/{id}/srt` | GET | Download SRT file |
| `/api/subtitle/jobs` | GET | List all jobs |
| `/api/subtitle/health` | GET | Health check |

## 💻 Quick Examples

### Upload Large Video (1GB+)
```powershell
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract" `
    -Method Post -Form @{ videoFile = Get-Item "large_video.mkv" }
```

### Download & Extract from URL
```powershell
$body = @{ videoUrl = "https://cdn.example.com/movie.mp4" } | ConvertTo-Json
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract-url" `
    -Method Post -Body $body -ContentType "application/json"

# Poll for completion
do {
    Start-Sleep 3
    $status = Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/status/$($response.jobId)"
    Write-Host $status.status
} while ($status.status -in @("Downloading","Processing"))
```

### Upload Audio File
```powershell
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract-audio" `
    -Method Post -Form @{ audioFile = Get-Item "podcast.mp3" }
```

## 📝 Response Format (Development Mode)

```json
{
  "jobId": "abc123-def456",
  "status": "Processing",
  "message": "Processing...",
  "fileName": "video.mp4",
  "subtitleCount": 0,
  "downloadUrl": null,
  "development": true,
  "uploadedFileSize": 1073741824,
  "processingNote": "[DEV] Helpful tip..."
}
```

## 🎯 Status Flow

```
Upload → Pending → Processing → Completed
                              → Failed

URL    → Downloading → Processing → Completed
                                  → Failed
```

## 💾 File Size Limits

- **Max Upload:** 2GB (2,147,483,648 bytes)
- **URL Download:** Unlimited (limited by disk space)
- **Request Timeout:** 30 minutes

## 📁 Supported Formats

**Video:** .mp4, .mkv, .avi, .mov, .wmv, .flv, .webm, .ts  
**Audio:** .mp3, .wav, .m4a, .aac, .flac, .ogg, .wma, .opus

## 🧪 Testing

```powershell
# Run test script
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi
.\TestNewEndpoints.ps1

# View Swagger docs
Start-Process "https://localhost:5001/swagger"
```

## 🔍 Check API Health

```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/health"
```

## 📖 Full Documentation

- `README.md` - Main documentation
- `LARGE_FILE_GUIDE.md` - Large file handling
- `API_DOCUMENTATION.md` - Complete reference
- Swagger UI: https://localhost:5001/swagger

---

**CaptionFlow API v1.0**  
*Made with ❤️ by the CaptionFlow Team*
