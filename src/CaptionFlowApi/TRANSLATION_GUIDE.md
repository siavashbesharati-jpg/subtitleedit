# 🌐 Translation Feature - Quick Guide

## ✅ What's Included

- **FREE Google Translate V1** (no API key required!)
- **144 languages** supported
- **Auto language detection** for source language
- **Seamless integration** with transcription workflow

## 🎯 Default Settings

The test page now defaults to:
- **Translation:** 🇮🇷 **Farsi (Persian)** - `fa`
- You can change to any of 144 supported languages
- Or select "None" to keep original language

## 📋 How It Works

### Workflow:
1. **Upload video** → Extract audio
2. **Transcribe** → Generate subtitles in original language (detected automatically)
3. **Translate** → Translate each subtitle segment to target language (if selected)
4. **Save** → Final SRT file with translated text

### Example:
- Upload English video
- Select "Farsi" as translation language
- Get Persian subtitles! 🇮🇷

## 🚀 Using the API

### Via Test Page:
1. Upload your video
2. Select "Farsi (Persian)" from "Translate To" dropdown (already selected by default!)
3. Click "Start Transcription"
4. Download translated subtitles

### Via Swagger:
```json
POST /api/transcription/transcribe
{
  "engine": "whisper-cpp",
  "model": "tiny",
  "language": "auto",
  "translateTo": "fa",  // Farsi/Persian
  "usePostProcessing": true
}
```

### Via cURL:
```bash
curl -X POST "https://localhost:5001/api/transcription/transcribe" \
  -F "file=@video.mp4" \
  -F "engine=whisper-cpp" \
  -F "model=tiny" \
  -F "language=auto" \
  -F "translateTo=fa" \
  -F "usePostProcessing=true"
```

### Via PowerShell:
```powershell
$form = @{
    file = Get-Item "video.mp4"
    engine = "whisper-cpp"
    model = "tiny"
    language = "auto"
    translateTo = "fa"  # Farsi
    usePostProcessing = $true
}

Invoke-RestMethod -Uri "https://localhost:5001/api/transcription/transcribe" `
    -Method Post -Form $form
```

## 🌍 Popular Language Codes

| Language | Code | Example |
|----------|------|---------|
| 🇮🇷 **Farsi (Persian)** | `fa` | **Default!** |
| 🇬🇧 English | `en` | English subtitles |
| 🇸🇦 Arabic | `ar` | العربية |
| 🇪🇸 Spanish | `es` | Español |
| 🇫🇷 French | `fr` | Français |
| 🇩🇪 German | `de` | Deutsch |
| 🇹🇷 Turkish | `tr` | Türkçe |
| 🇵🇰 Urdu | `ur` | اردو |
| 🇮🇳 Hindi | `hi` | हिन्दी |
| 🇨🇳 Chinese (Simplified) | `zh-CN` | 简体中文 |
| 🇯🇵 Japanese | `ja` | 日本語 |
| 🇰🇷 Korean | `ko` | 한국어 |
| 🇷🇺 Russian | `ru` | Русский |
| 🇮🇹 Italian | `it` | Italiano |
| 🇧🇷 Portuguese | `pt` | Português |

## 📡 Get All Languages

### API Endpoint:
```
GET /api/transcription/languages
```

### Response:
```json
{
  "count": 144,
  "translator": "Google Translate V1 (Free)",
  "languages": [
    { "name": "AFRIKAANS", "code": "af" },
    { "name": "ARABIC", "code": "ar" },
    { "name": "PERSIAN", "code": "fa" },
    ...
  ]
}
```

### PowerShell:
```powershell
# Get all languages
$languages = Invoke-RestMethod -Uri "https://localhost:5001/api/transcription/languages"

# Find Farsi
$languages.languages | Where-Object { $_.name -like "*PERSIAN*" }

# Show all
$languages.languages | Format-Table
```

## ⚙️ Advanced Options

### No Translation:
```json
{
  "translateTo": ""  // Empty or "none"
}
```

### Whisper Built-in Translation to English:
```json
{
  "translateToEnglish": true,  // Uses Whisper's built-in translator
  "translateTo": ""            // Leave empty
}
```

### Translate to Multiple Languages:
Run multiple jobs with different `translateTo` values:
1. First job: `translateTo: "fa"` → Get Farsi
2. Second job: `translateTo: "ar"` → Get Arabic
3. Third job: `translateTo: "ur"` → Get Urdu

## 🎬 Real-World Example

### Scenario: English YouTube Video → Farsi Subtitles

1. **Download video**: `yt-dlp https://youtube.com/watch?v=...`
2. **Upload to API**: Select Farsi translation
3. **Get result**: English video with Persian subtitles!

### Progress Updates:
```
Queued → 0%
Extracting Audio → 10%
Transcribing → 30-80%
Post-Processing → 80-90%
Translating → 90-95%  ← NEW STEP!
Completed → 100%
```

## 💡 Pro Tips

1. **Auto Language Detection**: Set `language: "auto"` and let Whisper detect the source language
2. **Post-Processing**: Always enable for better timing and punctuation
3. **Translation Speed**: ~100ms per subtitle segment (small delay to avoid rate limiting)
4. **Quality**: Google Translate V1 provides good quality for most languages
5. **Free Service**: No API key needed, completely free!

## 🔧 Technical Details

### Translation Service:
- **Engine**: Google Translate V1 API
- **Cost**: FREE (no API key required)
- **Rate Limit**: Small delay between segments to avoid blocking
- **Quality**: Production-ready, same engine as google.com/translate

### Process:
```csharp
// 1. Transcribe (Whisper)
var subtitle = TranscribeWithWhisperAsync(wavFile, model, language);

// 2. Translate (Google)
if (translateTo != "")
{
    foreach (var paragraph in subtitle.Paragraphs)
    {
        paragraph.Text = await GoogleTranslate(
            paragraph.Text, 
            sourceLanguage, 
            targetLanguage
        );
    }
}

// 3. Save
SaveAsSrt(subtitle);
```

## 📝 Swagger Documentation

All parameters are documented in Swagger UI:
```
https://localhost:5001/swagger
```

Look for:
- `POST /api/Transcription/transcribe` → See `translateTo` parameter
- `GET /api/Transcription/languages` → Get all supported languages

## 🎉 Quick Start

**Easiest way to test:**

1. Open: `https://localhost:5001/transcription-test.html`
2. Upload a video
3. **Translation is already set to Farsi by default!** 🇮🇷
4. Click "Start Transcription"
5. Download your Persian subtitles!

**That's it!** The API will:
- Detect video language (e.g., English)
- Transcribe to English text
- Translate to Farsi
- Give you a `.srt` file in Persian! 🎬
