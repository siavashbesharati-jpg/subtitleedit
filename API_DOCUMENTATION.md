# 🎬 CaptionFlow RESTful API - Complete Documentation

## Overview

CaptionFlow now includes a powerful **RESTful API** layer that allows you to programmatically:
- ✅ Upload video files 
- ✅ Extract embedded subtitles automatically
- ✅ Export subtitles in SRT format
- ✅ Track job status and progress
- ✅ Download subtitle files via HTTP

## 🚀 Quick Start

### 1. Start the API Server

```powershell
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### 2. Test the API

**PowerShell:**
```powershell
.\TestApi.ps1
```

**Bash/Linux:**
```bash
chmod +x TestApi.sh
./TestApi.sh
```

## 📡 API Endpoints

### Health Check
```http
GET /api/subtitle/health
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-10-18T12:34:56.789Z",
  "service": "CaptionFlow API"
}
```

### Extract Subtitles
```http
POST /api/subtitle/extract
Content-Type: multipart/form-data
```

**Parameters:**
- `videoFile` (file, required): Video file to process
- `sourceLanguage` (string, optional): Language code (e.g., "en", "es")
- `useOcr` (boolean, optional): Enable OCR for image subtitles

**Example (PowerShell):**
```powershell
$form = @{
    videoFile = Get-Item "C:\Videos\movie.mkv"
    sourceLanguage = "en"
}
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract" `
    -Method Post -Form $form -SkipCertificateCheck
```

**Response:**
```json
{
  "jobId": "abc123-def456",
  "status": "Processing",
  "message": "Video uploaded successfully. Processing subtitles...",
  "fileName": "movie.mkv",
  "subtitleCount": 0,
  "downloadUrl": null
}
```

### Get Job Status
```http
GET /api/subtitle/status/{jobId}
```

**Example:**
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/status/abc123" `
    -SkipCertificateCheck
```

**Response (Completed):**
```json
{
  "jobId": "abc123-def456",
  "status": "Completed",
  "message": "Subtitles extracted successfully",
  "fileName": "movie.mkv",
  "subtitleCount": 543,
  "downloadUrl": "/api/subtitle/download/abc123-def456/srt"
}
```

### Download SRT File
```http
GET /api/subtitle/download/{jobId}/srt
```

**Example:**
```powershell
Invoke-WebRequest -Uri "https://localhost:5001/api/subtitle/download/abc123/srt" `
    -OutFile "subtitles.srt" -SkipCertificateCheck
```

### List All Jobs
```http
GET /api/subtitle/jobs
```

**Response:**
```json
[
  {
    "jobId": "abc123",
    "fileName": "movie.mkv",
    "status": "Completed",
    "subtitleCount": 543,
    "createdAt": "2025-10-18T12:00:00Z",
    "completedAt": "2025-10-18T12:05:30Z"
  }
]
```

## 🎯 Complete Workflow Example

```powershell
# 1. Upload video
$upload = Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract" `
    -Method Post `
    -Form @{ videoFile = Get-Item "movie.mkv" } `
    -SkipCertificateCheck

$jobId = $upload.jobId

# 2. Poll for completion
do {
    Start-Sleep -Seconds 2
    $status = Invoke-RestMethod `
        -Uri "https://localhost:5001/api/subtitle/status/$jobId" `
        -SkipCertificateCheck
    Write-Host "Status: $($status.status)"
} while ($status.status -eq "Processing")

# 3. Download SRT if completed
if ($status.status -eq "Completed") {
    Invoke-WebRequest `
        -Uri "https://localhost:5001/api/subtitle/download/$jobId/srt" `
        -OutFile "output.srt" `
        -SkipCertificateCheck
    Write-Host "Downloaded $($status.subtitleCount) subtitles!"
}
```

## 📦 Supported Video Formats

- **MP4** (.mp4) - MPEG-4 Part 14
- **MKV** (.mkv) - Matroska
- **AVI** (.avi) - Audio Video Interleave
- **MOV** (.mov) - QuickTime
- **WMV** (.wmv) - Windows Media Video
- **FLV** (.flv) - Flash Video
- **WebM** (.webm) - WebM
- **TS** (.ts) - Transport Stream

## 🔧 Configuration

Edit `appsettings.json` to customize:

```json
{
  "FileStorage": {
    "UploadDirectory": "C:\\Temp\\CaptionFlow\\Uploads",
    "OutputDirectory": "C:\\Temp\\CaptionFlow\\Outputs"
  },
  "Urls": "http://localhost:5000;https://localhost:5001"
}
```

## 🏗️ Project Structure

```
CaptionFlowApi/
├── Controllers/
│   └── SubtitleController.cs    # API endpoints
├── Models/
│   ├── SubtitleExtractionRequest.cs
│   ├── SubtitleExtractionResponse.cs
│   └── SubtitleJob.cs
├── Services/
│   └── SubtitleProcessingService.cs  # Core processing logic
├── Program.cs                    # API configuration
├── appsettings.json             # Configuration
├── README.md                     # API documentation
├── TestApi.ps1                  # PowerShell test script
└── TestApi.sh                   # Bash test script
```

## 🔐 Security Notes

### Development
- Uses self-signed HTTPS certificate
- CORS enabled for all origins
- No authentication required

### Production Recommendations
1. **Enable Authentication**: Add JWT/API Key authentication
2. **Restrict CORS**: Limit allowed origins
3. **Use Valid SSL Certificate**: Replace self-signed cert
4. **Add Rate Limiting**: Prevent abuse
5. **Implement File Size Limits**: Already set to 500MB
6. **Enable Logging**: Configure proper logging
7. **Add Monitoring**: Track API usage and performance

## 📊 Status Codes

- **200 OK**: Request successful
- **400 Bad Request**: Invalid request (unsupported format, missing file)
- **404 Not Found**: Job or resource not found
- **500 Internal Server Error**: Server error during processing

## 🛠️ Development

### Build
```powershell
dotnet build
```

### Run in Development
```powershell
dotnet run --environment Development
```

### Run in Production
```powershell
dotnet run --environment Production
```

## 🐛 Troubleshooting

### API Won't Start
```powershell
# Check if ports are in use
netstat -ano | findstr ":5000"
netstat -ano | findstr ":5001"

# Kill conflicting process
taskkill /PID <process_id> /F
```

### SSL Certificate Errors
```powershell
# Trust development certificate
dotnet dev-certs https --trust
```

### Subtitle Extraction Fails
- Ensure video file contains embedded subtitles
- Check video file is not corrupted
- Verify supported format (.mkv, .mp4, etc.)
- Review logs for detailed error messages

## 📈 Performance

- **Concurrent Processing**: Multiple jobs processed simultaneously
- **Background Jobs**: Non-blocking async processing
- **Automatic Cleanup**: Old files cleaned up after 24 hours
- **Memory Efficient**: Streams large files instead of loading into memory

## 🌐 Integration Examples

### Python
```python
import requests

# Upload video
with open('video.mkv', 'rb') as f:
    response = requests.post(
        'https://localhost:5001/api/subtitle/extract',
        files={'videoFile': f},
        verify=False
    )
job_id = response.json()['jobId']

# Check status
status = requests.get(
    f'https://localhost:5001/api/subtitle/status/{job_id}',
    verify=False
).json()

# Download SRT
if status['status'] == 'Completed':
    srt = requests.get(
        f'https://localhost:5001{status["downloadUrl"]}',
        verify=False
    )
    with open('output.srt', 'wb') as f:
        f.write(srt.content)
```

### C#
```csharp
using System.Net.Http;
using System.Net.Http.Json;

var client = new HttpClient();
var content = new MultipartFormDataContent();
content.Add(new StreamContent(File.OpenRead("video.mkv")), "videoFile", "video.mkv");

var response = await client.PostAsync(
    "https://localhost:5001/api/subtitle/extract", 
    content
);
var job = await response.Content.ReadFromJsonAsync<SubtitleExtractionResponse>();
```

### JavaScript/Node.js
```javascript
const FormData = require('form-data');
const axios = require('axios');
const fs = require('fs');

const form = new FormData();
form.append('videoFile', fs.createReadStream('video.mkv'));

axios.post('https://localhost:5001/api/subtitle/extract', form, {
    headers: form.getHeaders()
}).then(response => {
    console.log('Job ID:', response.data.jobId);
});
```

## 📝 License

This API is part of CaptionFlow. See main project LICENSE for details.

## 🤝 Contributing

Found a bug or want to add a feature? Visit:
https://github.com/siavashbesharati-jpg/subtitleedit

## 📞 Support

- API Documentation: `/swagger`
- GitHub Issues: https://github.com/siavashbesharati-jpg/subtitleedit/issues

---

**Made with ❤️ by the CaptionFlow Team**
