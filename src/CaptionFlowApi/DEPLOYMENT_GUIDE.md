# 🚀 CaptionFlow API - Deployment & Publishing Guide

## 📋 Table of Contents
1. [Development vs Production](#development-vs-production)
2. [Publishing the API](#publishing-the-api)
3. [Running in Production](#running-in-production)
4. [Deployment Options](#deployment-options)
5. [Configuration](#configuration)

---

## 🔍 Development vs Production

### Development Mode (Current)
**What you're doing now:**
```powershell
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi
dotnet run
```

**Characteristics:**
- ✅ Quick to start
- ✅ Shows detailed errors
- ✅ Hot reload enabled
- ❌ Slower performance
- ❌ Requires .NET SDK installed
- ❌ Runs from source code

### Production Mode (Recommended)
**What you should do for production:**
```powershell
# Publish the API
dotnet publish -c Release -o ./publish

# Run the published EXE
./publish/CaptionFlowApi.exe
```

**Characteristics:**
- ✅ Optimized for performance
- ✅ Standalone executable
- ✅ No .NET SDK required (only runtime)
- ✅ Smaller memory footprint
- ✅ Ready for deployment

---

## 📦 Publishing the API

### Option 1: Framework-Dependent Deployment (Smaller Size)

**Requirements:** .NET 8 Runtime must be installed on target machine

```powershell
# Navigate to project directory
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi

# Publish
dotnet publish -c Release -o ./publish

# Result: Creates executable in ./publish folder
# Size: ~50-100 MB
# Requires: .NET 8 Runtime on target machine
```

### Option 2: Self-Contained Deployment (Recommended)

**Requirements:** Nothing! Includes .NET runtime

```powershell
# For Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish-win-x64

# For Windows x86 (32-bit)
dotnet publish -c Release -r win-x86 --self-contained true -o ./publish-win-x86

# For Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish-linux-x64

# Result: Creates standalone executable
# Size: ~70-150 MB
# Requires: Nothing! Runtime included
```

### Option 3: Single File Deployment (Most Portable)

**Best for distribution** - Everything in one EXE!

```powershell
dotnet publish -c Release -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o ./publish-single-file

# Result: Single CaptionFlowApi.exe file
# Size: ~70-150 MB
# Requires: Nothing!
# Portable: Yes!
```

---

## 🎯 Step-by-Step Publishing Guide

### Step 1: Choose Publishing Method

**For Production Server:**
```powershell
# Self-contained, optimized, single file
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi

dotnet publish -c Release -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:PublishTrimmed=false `
  -p:PublishReadyToRun=true `
  -o .\publish\production
```

### Step 2: Find the EXE

After publishing, the EXE will be located at:
```
E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi\publish\production\CaptionFlowApi.exe
```

### Step 3: Test the Published Version

```powershell
# Navigate to publish folder
cd .\publish\production

# Run the EXE
.\CaptionFlowApi.exe

# You should see:
# ╔══════════════════════════════════════════════════╗
# ║          CaptionFlow RESTful API v1.0           ║
# ╚══════════════════════════════════════════════════╝
```

### Step 4: Verify It's Working

```powershell
# In another PowerShell window, test the API
Invoke-RestMethod -Uri "http://localhost:5000/api/subtitle/health"

# You should get:
# status: healthy
# service: CaptionFlow API
```

---

## 📁 What Files Are Needed?

### Minimum Required Files

After publishing, you need these files:

```
publish\production\
├── CaptionFlowApi.exe          ← Main executable (RUN THIS!)
├── appsettings.json            ← Configuration
├── appsettings.Production.json ← Production settings (optional)
└── web.config                  ← IIS configuration (if using IIS)
```

If NOT using single-file publish, you'll also have:
```
├── CaptionFlowApi.dll
├── CaptionFlowApi.pdb
├── libse.dll                   ← Subtitle processing library
├── *.dll                       ← Various dependencies
└── wwwroot\                    ← Static files (if any)
```

---

## 🚀 Running in Production

### Method 1: Direct Execution (Simple)

```powershell
# Just run the EXE!
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi\publish\production
.\CaptionFlowApi.exe
```

**Pros:**
- ✅ Simple
- ✅ Good for testing

**Cons:**
- ❌ Stops when you close terminal
- ❌ No auto-restart
- ❌ No monitoring

### Method 2: Windows Service (Recommended for Production)

**Install as Windows Service:**

```powershell
# Install NSSM (Non-Sucking Service Manager)
# Download from: https://nssm.cc/download

# Install the service
nssm install CaptionFlowAPI "E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi\publish\production\CaptionFlowApi.exe"

# Configure the service
nssm set CaptionFlowAPI AppDirectory "E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi\publish\production"
nssm set CaptionFlowAPI DisplayName "CaptionFlow API Service"
nssm set CaptionFlowAPI Description "RESTful API for video subtitle extraction"
nssm set CaptionFlowAPI Start SERVICE_AUTO_START

# Start the service
nssm start CaptionFlowAPI

# Check status
nssm status CaptionFlowAPI
```

**Pros:**
- ✅ Runs as background service
- ✅ Auto-starts on boot
- ✅ Auto-restarts on crash
- ✅ Logs to Windows Event Log

### Method 3: IIS Hosting (Enterprise)

**Requirements:**
- Windows Server or Windows 10/11 Pro
- IIS installed
- ASP.NET Core Hosting Bundle

**Setup Steps:**

1. **Install ASP.NET Core Hosting Bundle**
```powershell
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
# Install: dotnet-hosting-8.0.x-win.exe
```

2. **Publish for IIS**
```powershell
dotnet publish -c Release -o ./publish-iis
```

3. **Create IIS Site**
- Open IIS Manager
- Add Website
- Point to publish folder
- Set Application Pool to "No Managed Code"

4. **Configure web.config** (auto-generated)
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" />
    </handlers>
    <aspNetCore processPath=".\CaptionFlowApi.exe" 
                stdoutLogEnabled="true" 
                stdoutLogFile=".\logs\stdout" />
  </system.webServer>
</configuration>
```

### Method 4: Docker Container (Modern)

**Create Dockerfile:**

```dockerfile
# E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi\Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CaptionFlowApi.csproj", "./"]
COPY ["../libse/LibSE.csproj", "../libse/"]
RUN dotnet restore "CaptionFlowApi.csproj"
COPY . .
RUN dotnet build "CaptionFlowApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CaptionFlowApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CaptionFlowApi.dll"]
```

**Build and Run:**
```powershell
# Build Docker image
docker build -t captionflow-api .

# Run container
docker run -d -p 5000:80 -p 5001:443 --name captionflow captionflow-api
```

---

## ⚙️ Configuration for Production

### Update appsettings.Production.json

Create this file in the publish folder:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "FileStorage": {
    "UploadDirectory": "C:\\CaptionFlow\\Uploads",
    "OutputDirectory": "C:\\CaptionFlow\\Outputs",
    "MaxFileSizeBytes": 2147483648
  },
  "Urls": "http://0.0.0.0:5000;https://0.0.0.0:5001",
  "ApiSettings": {
    "DevelopmentMode": false,
    "MaxConcurrentJobs": 10,
    "JobRetentionHours": 24
  }
}
```

### Create Production Folders

```powershell
# Create required folders
New-Item -ItemType Directory -Path "C:\CaptionFlow\Uploads" -Force
New-Item -ItemType Directory -Path "C:\CaptionFlow\Outputs" -Force
New-Item -ItemType Directory -Path "C:\CaptionFlow\Logs" -Force

# Set permissions (if needed)
icacls "C:\CaptionFlow" /grant "IIS AppPool\CaptionFlowAPI:(OI)(CI)F" /T
```

---

## 🎯 Complete Publishing Script

Save this as `PublishProduction.ps1`:

```powershell
# CaptionFlow API - Production Publishing Script

param(
    [string]$OutputPath = ".\publish\production",
    [switch]$SingleFile = $true,
    [switch]$SelfContained = $true
)

Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   CaptionFlow API - Production Publisher        ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Navigate to project directory
$projectPath = "E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi"
Set-Location $projectPath

Write-Host "📦 Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean -c Release

Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build -c Release

Write-Host "🚀 Publishing for production..." -ForegroundColor Yellow

$publishArgs = @(
    "publish",
    "-c", "Release",
    "-r", "win-x64",
    "-o", $OutputPath
)

if ($SelfContained) {
    $publishArgs += "--self-contained", "true"
}

if ($SingleFile) {
    $publishArgs += "-p:PublishSingleFile=true"
    $publishArgs += "-p:IncludeNativeLibrariesForSelfExtract=true"
}

$publishArgs += "-p:PublishReadyToRun=true"

& dotnet @publishArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Publishing completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📁 Published files location:" -ForegroundColor Cyan
    Write-Host "   $OutputPath" -ForegroundColor White
    Write-Host ""
    Write-Host "🎯 To run the API:" -ForegroundColor Cyan
    Write-Host "   cd $OutputPath" -ForegroundColor White
    Write-Host "   .\CaptionFlowApi.exe" -ForegroundColor White
    Write-Host ""
    
    # Show file size
    $exePath = Join-Path $OutputPath "CaptionFlowApi.exe"
    if (Test-Path $exePath) {
        $size = (Get-Item $exePath).Length / 1MB
        Write-Host "📊 Executable size: $([math]::Round($size, 2)) MB" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "🌐 After starting, access the API at:" -ForegroundColor Cyan
    Write-Host "   http://localhost:5000/swagger" -ForegroundColor White
    Write-Host "   https://localhost:5001/swagger" -ForegroundColor White
    
} else {
    Write-Host ""
    Write-Host "❌ Publishing failed!" -ForegroundColor Red
    exit 1
}
```

**Run it:**
```powershell
.\PublishProduction.ps1
```

---

## 📋 Deployment Checklist

### Pre-Deployment
- [ ] Test API locally with `dotnet run`
- [ ] Update `appsettings.Production.json`
- [ ] Configure file storage paths
- [ ] Test with sample video files
- [ ] Check all endpoints in Swagger

### Publishing
- [ ] Run publish command
- [ ] Verify EXE is created
- [ ] Check file size is reasonable
- [ ] Test published EXE locally

### Deployment
- [ ] Copy published files to server
- [ ] Create required folders (Uploads, Outputs)
- [ ] Set folder permissions
- [ ] Configure firewall for ports 5000/5001
- [ ] Install as Windows Service (optional)

### Post-Deployment
- [ ] Start the service/EXE
- [ ] Test health endpoint
- [ ] Upload test video file
- [ ] Verify SRT file generation
- [ ] Check logs for errors
- [ ] Set up monitoring

---

## 🔒 Security Considerations

### For Production Deployment:

1. **Use HTTPS Only**
```csharp
// In Program.cs
app.UseHttpsRedirection();
app.UseHsts(); // Add this
```

2. **Add Authentication** (optional)
```powershell
# Install JWT package
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

3. **Configure CORS Properly**
```csharp
// Don't use AllowAll in production!
options.AddPolicy("Production", policy =>
{
    policy.WithOrigins("https://yourdomain.com")
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

4. **Set Rate Limiting**
```powershell
dotnet add package AspNetCoreRateLimit
```

5. **Use Proper Logging**
```powershell
dotnet add package Serilog.AspNetCore
```

---

## 📊 Monitoring & Maintenance

### Check API Status
```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5000/api/subtitle/health"

# List active jobs
Invoke-RestMethod -Uri "http://localhost:5000/api/subtitle/jobs"
```

### View Logs
```powershell
# If using Windows Service with NSSM
Get-EventLog -LogName Application -Source "CaptionFlowAPI" -Newest 50

# If logging to file
Get-Content C:\CaptionFlow\Logs\api.log -Tail 50 -Wait
```

### Clean Up Old Files
```powershell
# Add to scheduled task
$uploadDir = "C:\CaptionFlow\Uploads"
$outputDir = "C:\CaptionFlow\Outputs"
$daysOld = 7

Get-ChildItem $uploadDir -Recurse | Where-Object {$_.CreationTime -lt (Get-Date).AddDays(-$daysOld)} | Remove-Item -Force
Get-ChildItem $outputDir -Recurse | Where-Object {$_.CreationTime -lt (Get-Date).AddDays(-$daysOld)} | Remove-Item -Force
```

---

## 🎉 Quick Summary

### To Run API in Production:

**1. Publish:**
```powershell
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/production
```

**2. Run:**
```powershell
cd ./publish/production
.\CaptionFlowApi.exe
```

**3. Access:**
```
https://localhost:5001/swagger
```

**That's it!** 🚀

---

**Made with ❤️ by the CaptionFlow Team**
