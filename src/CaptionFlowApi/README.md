# CaptionFlow RESTful API

A powerful RESTful API for extracting subtitles from video files and exporting them in SRT format.

## Features

- 📹 **Video Upload**: Accept video files in multiple formats (MP4, MKV, AVI, MOV, WebM, etc.)
- 🎬 **Subtitle Extraction**: Automatically extract embedded subtitles from video containers
- 📝 **SRT Export**: Generate standard SubRip (.srt) subtitle files
- 🔄 **Async Processing**: Background job processing for large video files
- 📊 **Job Status Tracking**: Monitor the progress of subtitle extraction jobs
- 🚀 **RESTful Design**: Clean, intuitive API endpoints
- 📖 **Swagger Documentation**: Interactive API documentation at `/swagger`

## Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- Windows, Linux, or macOS

### Running the API

1. Navigate to the API directory:
```powershell
cd src\CaptionFlowApi
```

2. Run the application:
```powershell
dotnet run
```

3. Open your browser and navigate to:
```
https://localhost:5001/swagger
```

## API Endpoints

### 1. Extract Subtitles from Video

**POST** `/api/subtitle/extract`

Upload a video file and extract embedded subtitles.

**Parameters:**
- `videoFile` (form-data, required): The video file to process
- `sourceLanguage` (form-data, optional): Source language code (e.g., "en", "es")
- `useOcr` (form-data, optional): Enable OCR for image-based subtitles (default: false)

**Example using cURL:**
```bash
curl -X POST "https://localhost:5001/api/subtitle/extract" \
  -F "videoFile=@/path/to/video.mp4" \
  -F "sourceLanguage=en"
```

**Example using PowerShell:**
```powershell
$form = @{
    videoFile = Get-Item -Path "C:\Videos\movie.mp4"
    sourceLanguage = "en"
}
Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract" -Method Post -Form $form
```

**Response:**
```json
{
  "jobId": "abc123-def456-ghi789",
  "status": "Processing",
  "message": "Video uploaded successfully. Processing subtitles...",
  "fileName": "movie.mp4",
  "subtitleCount": 0,
  "downloadUrl": null
}
```

### 2. Get Job Status

**GET** `/api/subtitle/status/{jobId}`

Check the status of a subtitle extraction job.

**Example:**
```bash
curl "https://localhost:5001/api/subtitle/status/abc123-def456-ghi789"
```

**Response (Processing):**
```json
{
  "jobId": "abc123-def456-ghi789",
  "status": "Processing",
  "message": "Processing in progress...",
  "fileName": "movie.mp4",
  "subtitleCount": 0,
  "downloadUrl": null
}
```

**Response (Completed):**
```json
{
  "jobId": "abc123-def456-ghi789",
  "status": "Completed",
  "message": "Subtitles extracted successfully",
  "fileName": "movie.mp4",
  "subtitleCount": 543,
  "downloadUrl": "/api/subtitle/download/abc123-def456-ghi789/srt"
}
```

### 3. Download SRT File

**GET** `/api/subtitle/download/{jobId}/srt`

Download the extracted subtitle file in SRT format.

**Example:**
```bash
curl "https://localhost:5001/api/subtitle/download/abc123-def456-ghi789/srt" \
  -o subtitles.srt
```

**Example using PowerShell:**
```powershell
Invoke-WebRequest -Uri "https://localhost:5001/api/subtitle/download/abc123-def456-ghi789/srt" `
  -OutFile "subtitles.srt"
```

### 4. List All Jobs

**GET** `/api/subtitle/jobs`

Get a list of all subtitle extraction jobs (useful for monitoring/debugging).

**Example:**
```bash
curl "https://localhost:5001/api/subtitle/jobs"
```

### 5. Health Check

**GET** `/api/subtitle/health`

Check if the API is running and healthy.

**Example:**
```bash
curl "https://localhost:5001/api/subtitle/health"
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-10-18T12:34:56.789Z",
  "service": "CaptionFlow API"
}
```

## Supported Video Formats

- MP4 (.mp4)
- Matroska (.mkv)
- AVI (.avi)
- QuickTime (.mov)
- Windows Media Video (.wmv)
- Flash Video (.flv)
- WebM (.webm)
- Transport Stream (.ts)

## Configuration

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

## Error Handling

The API returns standard HTTP status codes:

- `200 OK`: Request successful
- `400 Bad Request`: Invalid request (e.g., unsupported file format)
- `404 Not Found`: Job or resource not found
- `500 Internal Server Error`: Server error during processing

## Workflow Example

Complete workflow for extracting subtitles:

```powershell
# 1. Upload video and start extraction
$uploadResponse = Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/extract" `
  -Method Post -Form @{ videoFile = Get-Item "movie.mp4" }

$jobId = $uploadResponse.jobId
Write-Host "Job ID: $jobId"

# 2. Poll for completion
do {
    Start-Sleep -Seconds 2
    $status = Invoke-RestMethod -Uri "https://localhost:5001/api/subtitle/status/$jobId"
    Write-Host "Status: $($status.status)"
} while ($status.status -eq "Processing")

# 3. Download SRT file if completed
if ($status.status -eq "Completed") {
    Invoke-WebRequest -Uri "https://localhost:5001/api/subtitle/download/$jobId/srt" `
      -OutFile "movie.srt"
    Write-Host "Downloaded $($status.subtitleCount) subtitles to movie.srt"
}
```

## Integration with CaptionFlow

This API uses the core subtitle processing engine from CaptionFlow (SubtitleEdit), providing:

- Accurate subtitle extraction from multiple container formats
- Support for various subtitle codecs
- High-quality SRT generation
- Robust error handling

## Development

### Build the API

```powershell
dotnet build
```

### Run in Development Mode

```powershell
dotnet run --environment Development
```

### Run Tests (when available)

```powershell
dotnet test
```

## License

This API is part of the CaptionFlow project. See the main project LICENSE for details.

## Support

For issues, feature requests, or contributions, visit:
https://github.com/siavashbesharati-jpg/subtitleedit

---

**Made with ❤️ by the CaptionFlow Team**
