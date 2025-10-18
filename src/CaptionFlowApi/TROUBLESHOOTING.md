# 🔧 CaptionFlow API - Troubleshooting Guide

## ✅ FIXED: HTTP ERROR 404 on /swagger

### Problem
Accessing `https://localhost:5001/swagger` returned **HTTP ERROR 404**

### Root Cause
The Swagger UI was configured with:
1. `RoutePrefix = string.Empty` (trying to serve at root `/`)
2. Only enabled in Development mode
3. Conflicting with the root redirect endpoint

### Solution Applied
Updated `Program.cs` to:
1. ✅ Change `RoutePrefix` to `"swagger"` 
2. ✅ Enable Swagger in all environments
3. ✅ Add a proper HTML landing page at root
4. ✅ Fix routing conflicts

### How to Verify It's Fixed

**Test 1: Check API Health**
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/subtitle/health" -UseBasicParsing
```
Expected: Status 200 OK

**Test 2: Access Swagger UI**
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/swagger" -UseBasicParsing
```
Expected: Status 200 OK with HTML content

**Test 3: Open in Browser**
- HTTP: http://localhost:5000/swagger
- HTTPS: https://localhost:5001/swagger

---

## 🚀 Quick Start Guide

### 1. Start the API
```powershell
cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi
dotnet run
```

### 2. Verify It's Running
```powershell
# Check if port 5001 is listening
Get-NetTCPConnection -LocalPort 5001 -ErrorAction SilentlyContinue

# Test health endpoint
Invoke-WebRequest -Uri "http://localhost:5000/api/subtitle/health"
```

### 3. Access the API

**Landing Page (Root):**
- http://localhost:5000/
- https://localhost:5001/

**Swagger Documentation:**
- http://localhost:5000/swagger
- https://localhost:5001/swagger

**Health Check:**
- http://localhost:5000/api/subtitle/health
- https://localhost:5001/api/subtitle/health

---

## 🐛 Common Issues & Solutions

### Issue 1: Port Already in Use

**Symptoms:**
- API fails to start
- Error: "Address already in use"

**Solution:**
```powershell
# Find process using port 5001
Get-NetTCPConnection -LocalPort 5001 | Select-Object OwningProcess

# Kill the process
Stop-Process -Id <ProcessID> -Force

# Restart the API
dotnet run
```

### Issue 2: SSL Certificate Error in Browser

**Symptoms:**
- Browser shows "Your connection is not private"
- NET::ERR_CERT_AUTHORITY_INVALID

**Solution:**
```powershell
# Trust the development certificate
dotnet dev-certs https --trust

# Restart browser and try again
```

### Issue 3: 404 on All Endpoints

**Symptoms:**
- All API endpoints return 404
- Swagger returns 404

**Solution:**
```powershell
# Check if controllers are being mapped
# In Program.cs, ensure this line exists:
app.MapControllers();

# Rebuild and restart
dotnet build
dotnet run
```

### Issue 4: Swagger Shows Empty/No Endpoints

**Symptoms:**
- Swagger UI loads but shows no endpoints
- Says "No operations defined in spec!"

**Solution:**
```powershell
# Ensure XML documentation is generated
# Check CaptionFlowApi.csproj has:
<GenerateDocumentationFile>true</GenerateDocumentationFile>

# Rebuild
dotnet clean
dotnet build
dotnet run
```

### Issue 5: Large File Upload Fails

**Symptoms:**
- Upload fails with "Request body too large"
- Timeout on large files

**Solution:**
Already configured! Max 2GB, 30-minute timeout.
If still failing, check:
```powershell
# Verify in appsettings.json
"FileStorage": {
  "MaxFileSizeBytes": 2147483648
}

# Verify disk space
Get-PSDrive C | Select-Object Used,Free
```

### Issue 6: URL Download Fails

**Symptoms:**
- Status: "Failed"
- Error: "Failed to download or process video"

**Common Causes & Solutions:**
1. **Invalid URL** - Ensure URL is HTTP/HTTPS
2. **Network issue** - Check internet connection
3. **Server blocks downloads** - Some servers block automated downloads
4. **Timeout** - Very large files may timeout (increase in Program.cs)

---

## 📊 Verify API Status

### Quick Health Check Script
```powershell
# Save as CheckApi.ps1
$baseUrl = "http://localhost:5000"

Write-Host "🔍 Checking CaptionFlow API..." -ForegroundColor Cyan
Write-Host ""

try {
    $health = Invoke-RestMethod -Uri "$baseUrl/api/subtitle/health"
    Write-Host "✅ API Status: $($health.status)" -ForegroundColor Green
    Write-Host "   Service: $($health.service)" -ForegroundColor Gray
    Write-Host "   Version: $($health.version)" -ForegroundColor Gray
    Write-Host "   Max File Size: $($health.limits.maxFileSize)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "📖 Endpoints Available:" -ForegroundColor Cyan
    $health.endpoints.PSObject.Properties | ForEach-Object {
        Write-Host "   - $($_.Name): $($_.Value)" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ API is not responding" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}
```

### Run It
```powershell
.\CheckApi.ps1
```

---

## 🔍 Debug Logging

### Enable Detailed Logging

Edit `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

### View Logs in Real-Time
```powershell
dotnet run --verbosity detailed
```

---

## 🧪 Test All Endpoints

### Complete Test Script
```powershell
$base = "http://localhost:5000"

Write-Host "Testing all endpoints..." -ForegroundColor Cyan

# 1. Health
Write-Host "`n1. Health Check..."
Invoke-RestMethod -Uri "$base/api/subtitle/health" | Select-Object status, version

# 2. List Jobs
Write-Host "`n2. List Jobs..."
$jobs = Invoke-RestMethod -Uri "$base/api/subtitle/jobs"
Write-Host "   Found $($jobs.Count) jobs"

# 3. Root Page
Write-Host "`n3. Root Page..."
$root = Invoke-WebRequest -Uri "$base/" -UseBasicParsing
Write-Host "   Status: $($root.StatusCode)"

# 4. Swagger
Write-Host "`n4. Swagger UI..."
$swagger = Invoke-WebRequest -Uri "$base/swagger" -UseBasicParsing
Write-Host "   Status: $($swagger.StatusCode)"

Write-Host "`n✅ All endpoints working!" -ForegroundColor Green
```

---

## 📞 Getting Help

### Check API is Running
```powershell
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}
Get-NetTCPConnection -LocalPort 5000,5001
```

### View Application Output
The console window shows:
- Request logs
- Error messages
- Processing status
- Job information

### Still Having Issues?

1. **Stop all instances:**
   ```powershell
   Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"} | Stop-Process -Force
   ```

2. **Clean build:**
   ```powershell
   cd E:\IRANEXPEDIA\Subedit\subtitleedit\src\CaptionFlowApi
   dotnet clean
   dotnet build
   ```

3. **Start fresh:**
   ```powershell
   dotnet run
   ```

4. **Check ports:**
   ```powershell
   netstat -ano | findstr ":5000"
   netstat -ano | findstr ":5001"
   ```

---

## ✅ Current Status

**API Status:** ✅ Running  
**Swagger URL:** https://localhost:5001/swagger  
**Root Page:** https://localhost:5001/  
**Health Check:** https://localhost:5001/api/subtitle/health  

**All Issues:** ✅ **RESOLVED**

---

## 📖 Quick Links

- **Swagger Documentation:** https://localhost:5001/swagger
- **Landing Page:** https://localhost:5001/
- **Health Endpoint:** https://localhost:5001/api/subtitle/health
- **Test Scripts:** `TestNewEndpoints.ps1`
- **API Guide:** `LARGE_FILE_GUIDE.md`
- **Quick Ref:** `QUICK_REFERENCE.md`

---

**Made with ❤️ by the CaptionFlow Team**
