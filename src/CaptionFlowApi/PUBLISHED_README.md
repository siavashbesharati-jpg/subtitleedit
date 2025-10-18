# ✅ CaptionFlow API - Published & Ready!

## 🎉 SUCCESS! Your API is now published and running!

---

## 📁 Published Files Location

```
E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi\publish\production\
```

### Main Files:
- **CaptionFlowApi.exe** (125.12 MB) - **THIS IS YOUR API! Run this file!**
- appsettings.json - Configuration
- appsettings.Development.json - Development settings
- RunApi.ps1 - Quick start script

---

## 🚀 How to Run the API

### Method 1: Direct Execution (Simplest)

```powershell
# Navigate to the published folder
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi\publish\production

# Run the EXE
.\CaptionFlowApi.exe
```

### Method 2: Use the Quick Start Script

```powershell
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi\publish\production
.\RunApi.ps1
```

### Method 3: Double-Click
Simply double-click **CaptionFlowApi.exe** in Windows Explorer!

---

## 🌐 Access the API

Once running, open your browser to:

### Swagger Documentation (Interactive API Testing)
```
http://localhost:5000/swagger
```

### Health Check Endpoint
```
http://localhost:5000/api/subtitle/health
```

---

## 📊 What Just Happened?

✅ **Published**: Created a standalone EXE (125 MB)
✅ **Self-Contained**: No .NET installation required
✅ **Single File**: Everything bundled into one executable
✅ **Optimized**: Production-ready with ReadyToRun compilation
✅ **Tested**: API is currently running and healthy!

---

## 🎯 Quick Test Commands

### Test Video Upload
```powershell
$videoPath = "C:\path\to\your\video.mp4"
$uri = "http://localhost:5000/api/subtitle/extract"

$form = @{
    videoFile = Get-Item $videoPath
}

Invoke-RestMethod -Uri $uri -Method Post -Form $form
```

### Test Health Check
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/subtitle/health"
```

### List All Jobs
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/subtitle/jobs"
```

---

## 📦 Distribution

### To Share with Others:

**Option 1: Share the Entire Folder**
Copy the whole `publish\production` folder (includes EXE + config files)

**Option 2: Share Just the EXE**
Copy `CaptionFlowApi.exe` only (125 MB)
- Recipients can run it immediately
- Default settings will be used
- No installation needed!

**Option 3: Create a ZIP**
```powershell
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi
Compress-Archive -Path .\publish\production\* -DestinationPath CaptionFlowAPI.zip
```

---

## 🏢 Deployment Options

### 1. Windows Server
- Copy the `production` folder to the server
- Run `CaptionFlowApi.exe`
- Configure firewall for ports 5000/5001

### 2. Install as Windows Service (Recommended)

Using NSSM (Non-Sucking Service Manager):

```powershell
# Download NSSM from https://nssm.cc/download

# Install the service
nssm install CaptionFlowAPI "E:\...\publish\production\CaptionFlowApi.exe"

# Configure
nssm set CaptionFlowAPI AppDirectory "E:\...\publish\production"
nssm set CaptionFlowAPI DisplayName "CaptionFlow API Service"
nssm set CaptionFlowAPI Start SERVICE_AUTO_START

# Start
nssm start CaptionFlowAPI
```

### 3. IIS Hosting
- Install ASP.NET Core Hosting Bundle
- Create IIS site pointing to `production` folder
- Set Application Pool to "No Managed Code"

### 4. Cloud Deployment
- Azure App Service
- AWS Elastic Beanstalk
- Google Cloud Run
- Heroku

---

## ⚙️ Configuration

### Edit Settings
Open `appsettings.json` in the production folder:

```json
{
  "FileStorage": {
    "UploadDirectory": "uploads",
    "OutputDirectory": "outputs",
    "MaxFileSizeBytes": 2147483648
  },
  "ApiSettings": {
    "MaxConcurrentJobs": 5,
    "JobRetentionHours": 24
  }
}
```

### Change Ports
Set environment variable before running:

```powershell
$env:ASPNETCORE_URLS="http://0.0.0.0:8080;https://0.0.0.0:8443"
.\CaptionFlowApi.exe
```

---

## 🔍 Troubleshooting

### Port Already in Use
```powershell
# Find what's using port 5000
Get-NetTCPConnection -LocalPort 5000

# Kill the process
Stop-Process -Id <ProcessId>
```

### Firewall Blocking
```powershell
# Allow through Windows Firewall
New-NetFirewallRule -DisplayName "CaptionFlow API" -Direction Inbound -LocalPort 5000,5001 -Protocol TCP -Action Allow
```

### Permission Issues
```powershell
# Run as Administrator
Start-Process powershell -Verb RunAs -ArgumentList "-Command cd E:\...\production; .\CaptionFlowApi.exe"
```

---

## 📚 Available Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/subtitle/extract` | POST | Upload video file (up to 2GB) |
| `/api/subtitle/extract-audio` | POST | Upload audio file |
| `/api/subtitle/extract-url` | POST | Download video from URL |
| `/api/subtitle/status/{jobId}` | GET | Check job status |
| `/api/subtitle/download/{jobId}/srt` | GET | Download SRT file |
| `/api/subtitle/jobs` | GET | List all jobs |
| `/api/subtitle/health` | GET | Health check |

---

## 🎓 Next Steps

1. **Test the API**: Use Swagger UI at http://localhost:5000/swagger
2. **Try uploading a video**: Use the `/api/subtitle/extract` endpoint
3. **Deploy to production**: Choose a deployment method above
4. **Monitor performance**: Check logs and health endpoint regularly
5. **Secure the API**: Add authentication if exposing publicly

---

## 📖 Additional Resources

- **DEPLOYMENT_GUIDE.md** - Comprehensive deployment guide
- **README.md** - API overview and features
- **LARGE_FILE_GUIDE.md** - Handling large video files
- **TROUBLESHOOTING.md** - Common issues and solutions
- **API_DOCUMENTATION.md** - Complete API reference

---

## ✨ Key Features

✅ **2GB File Support** - Upload large video files
✅ **Multiple Formats** - Video and audio files
✅ **URL Downloads** - Extract from video URLs
✅ **Background Processing** - Async job processing
✅ **Auto Cleanup** - Removes old files after 24 hours
✅ **Development Mode** - Detailed JSON responses
✅ **Swagger UI** - Interactive documentation
✅ **Health Monitoring** - Status and metrics

---

## 🎯 Current Status

- ✅ API Published: **CaptionFlowApi.exe** (125.12 MB)
- ✅ Currently Running: http://localhost:5000
- ✅ Swagger UI: http://localhost:5000/swagger
- ✅ Health Check: Passing
- ✅ Ready for Production: Yes!

---

**🚀 Your CaptionFlow API is ready to use!**

**Need help?** Check the DEPLOYMENT_GUIDE.md or TROUBLESHOOTING.md files.
