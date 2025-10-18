# How to Download Whisper Models

## 📍 Model Location

Models are stored in:
```
C:\Users\MOHAMMAD\AppData\Roaming\Subtitle Edit\Whisper\Cpp\Models
```

## ✅ Currently Downloaded

You currently have these models:
- ✅ **tiny.en** (74 MB) - English only, fastest
- ✅ **medium-q5_0** (514 MB) - High quality, quantized

## 📥 How to Download More Models

### Option 1: Download via Subtitle Edit GUI (Recommended)

1. Open **Subtitle Edit** desktop application
2. Go to **Video → Audio to text (Whisper...)**
3. Select your preferred model from the dropdown
4. If not downloaded, it will prompt you to download
5. Click "Yes" and it will download automatically

### Option 2: Manual Download from HuggingFace

Download models from: https://huggingface.co/ggerganov/whisper.cpp/tree/main

**Model Files to Download:**

1. **Tiny** (75 MB) - Fastest
   ```
   https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-tiny.bin
   ```
   Save as: `tiny.bin`

2. **Base** (142 MB) - Recommended
   ```
   https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin
   ```
   Save as: `base.bin`

3. **Small** (465 MB) - Better quality
   ```
   https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.bin
   ```
   Save as: `small.bin`

4. **Medium** (1.42 GB) - High quality
   ```
   https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-medium.bin
   ```
   Save as: `medium.bin`

5. **Large** (2.87 GB) - Best quality
   ```
   https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-large-v3.bin
   ```
   Save as: `large.bin`

### PowerShell Download Script

```powershell
# Download Base model (recommended)
$modelFolder = "$env:APPDATA\Subtitle Edit\Whisper\Cpp\Models"
$modelUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin"
$modelPath = Join-Path $modelFolder "base.bin"

Write-Host "Downloading Base model (142 MB)..." -ForegroundColor Yellow
Invoke-WebRequest -Uri $modelUrl -OutFile $modelPath -UseBasicParsing
Write-Host "✅ Downloaded: $modelPath" -ForegroundColor Green
```

## 📊 Model Comparison

| Model | Size | Speed | Quality | Best For |
|-------|------|-------|---------|----------|
| **tiny** | 75 MB | 32x realtime | Fair | Quick tests, English-only |
| **base** | 142 MB | 16x realtime | Good | Most use cases ⭐ |
| **small** | 465 MB | 6x realtime | Very Good | Better accuracy |
| **medium** | 1.42 GB | 2x realtime | Excellent | Professional work |
| **large** | 2.87 GB | 1x realtime | Best | Maximum quality |

## 🎯 Recommendations

- **For Testing:** Use `tiny` (already have `tiny.en`)
- **For Production:** Use `base` (need to download)
- **For Best Quality:** Use `medium` (already have `medium-q5_0`)
- **For Maximum Accuracy:** Use `large` (need to download)

## 🔍 Verify Downloaded Models

### Via PowerShell:
```powershell
Get-ChildItem "$env:APPDATA\Subtitle Edit\Whisper\Cpp\Models" -Filter "*.bin" | 
    Where-Object { $_.Name -notlike "*.$$$" } |
    Select-Object Name, @{N='Size';E={"{0:N2} MB" -f ($_.Length/1MB)}}
```

### Via API:
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/transcription/models" | 
    Select-Object -ExpandProperty downloaded
```

## ⚠️ Important Notes

1. **File Extensions:** Models must have `.bin` extension
2. **Incomplete Downloads:** Files ending with `.$$$` are incomplete
3. **Naming:** Use simple names like `tiny.bin`, `base.bin`, `medium.bin`
4. **Quantized Models:** Models with `-q5_0` suffix are compressed but still high quality
5. **English-only:** Models with `.en` suffix are English-only but faster

## 🚀 Quick Start

Since you have `tiny.en` and `medium-q5_0`:

- ✅ **Ready to use:** `tiny` model
- ✅ **Ready to use:** `medium` model
- ❌ **Need to download:** `base`, `small`, `large`

The test page now defaults to **`tiny`** which you already have! 🎉
