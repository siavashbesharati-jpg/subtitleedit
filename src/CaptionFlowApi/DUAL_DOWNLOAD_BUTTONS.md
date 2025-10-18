# Dual Download Buttons Feature

## Overview
Added **TWO separate download buttons** for transcription with translation:
1. **📄 Download Original** - Downloads the original transcription
2. **🌐 Download Translation** - Downloads the translated subtitle (e.g., Farsi)

## How It Works

### User Interface
When translation is selected (e.g., Farsi):
- **Both buttons are shown** side by side
- Original button downloads the source language subtitle
- Translation button shows language name (e.g., "🌐 Download 🇮🇷 Farsi (Persian)")

When NO translation is selected:
- **Only the original button is shown**
- Translation button is hidden

### API Endpoints

#### 1. Download Original
```
GET /api/transcription/download/{jobId}/original
```
Downloads the original transcription as SRT file.

**Response:**
- Content-Type: `application/x-subrip`
- Filename: `{videoname}_original.srt`

#### 2. Download Translated
```
GET /api/transcription/download/{jobId}/translated
```
Downloads the translated subtitle as SRT file.

**Response:**
- Content-Type: `application/x-subrip`
- Filename: `{videoname}_translated.srt`
- Returns 404 if translation was not requested

### Data Model Changes

**TranscriptionJob.cs** now includes:
```csharp
public string? OriginalOutputPath { get; set; }     // Path to original SRT
public string? TranslatedOutputPath { get; set; }   // Path to translated SRT
public string OutputPath { get; set; }              // Main output (translated if available, else original)
```

### TranscriptionService Changes

The workflow now:
1. **Saves original** subtitle immediately after transcription
2. **Translates** (if requested) and saves to separate file
3. **Stores both paths** in job object:
   - `job.OriginalOutputPath` = original subtitle
   - `job.TranslatedOutputPath` = translated subtitle (or null)

### UI Implementation

**HTML Structure:**
```html
<div id="downloadButtons" style="display: flex; gap: 15px;">
    <button id="downloadOriginalBtn">
        📄 Download Original
    </button>
    <button id="downloadTranslatedBtn" style="display: none;">
        🌐 Download Translation
    </button>
</div>
```

**JavaScript Logic:**
```javascript
if (translateTo && translateTo !== 'none') {
    // Show both buttons
    downloadOriginalBtn.style.display = 'block';
    downloadTranslatedBtn.style.display = 'block';
    
    downloadOriginalBtn.onclick = () => {
        window.open(`/api/transcription/download/${jobId}/original`, '_blank');
    };
    
    downloadTranslatedBtn.onclick = () => {
        window.open(`/api/transcription/download/${jobId}/translated`, '_blank');
    };
} else {
    // Show only original
    downloadOriginalBtn.style.display = 'block';
    downloadTranslatedBtn.style.display = 'none';
}
```

## Example Usage

### Test Page Workflow
1. Go to http://localhost:5000/transcription-test.html
2. Upload a video file
3. Select translation language (e.g., **🇮🇷 Farsi (Persian)**)
4. Click "🚀 Start Transcription"
5. Wait for completion
6. See **TWO buttons**:
   - **📄 Download Original** - Gets source language
   - **🌐 Download 🇮🇷 Farsi (Persian)** - Gets Farsi translation
7. Click either button to download that specific file

### API Usage (cURL)

```bash
# Upload and transcribe with Farsi translation
curl -X POST http://localhost:5000/api/transcription/transcribe \
  -F "file=@video.mp4" \
  -F "engine=whisper-cpp" \
  -F "model=tiny" \
  -F "language=auto" \
  -F "translateTo=fa"

# Response: { "jobId": "abc123", ... }

# Download original English transcription
curl -O http://localhost:5000/api/transcription/download/abc123/original
# Downloads: video_original.srt

# Download Farsi translation
curl -O http://localhost:5000/api/transcription/download/abc123/translated
# Downloads: video_translated.srt
```

### JavaScript Example

```javascript
// After transcription completes with translation
const jobId = 'abc123';

// Download original
fetch(`/api/transcription/download/${jobId}/original`)
    .then(response => response.blob())
    .then(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'original.srt';
        a.click();
    });

// Download translated
fetch(`/api/transcription/download/${jobId}/translated`)
    .then(response => response.blob())
    .then(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'translated.srt';
        a.click();
    });
```

## Benefits

✅ **User Control** - Download exactly what you need
✅ **No ZIP Confusion** - Direct file downloads
✅ **Clear Labels** - Know what each button does
✅ **Flexible** - Get one or both files
✅ **Simple** - Just click the button you want

## File Locations

Both files are saved in:
```
C:\Users\MOHAMMAD\AppData\Local\Temp\captionflow\transcription\output\
```

Files:
- `{jobId}_original.srt` - Original language
- `{jobId}_translated_{langCode}.srt` - Translation (e.g., `translated_fa.srt`)

## Supported Languages

All 144 languages from Google Translate V1:
- **fa** - 🇮🇷 Farsi (Persian) - **Default**
- **ar** - 🇸🇦 Arabic
- **en** - 🇬🇧 English
- **es** - 🇪🇸 Spanish
- **fr** - 🇫🇷 French
- And 139 more...

See `/api/transcription/languages` endpoint for full list.

## Error Handling

**No translation requested:**
- Translation button is hidden
- GET `/download/{jobId}/translated` returns 404

**Translation failed:**
- Both paths stored as null
- Only original button shown
- Translation button returns 404

**File not found:**
- API returns 404 with error message
- User sees browser download error
