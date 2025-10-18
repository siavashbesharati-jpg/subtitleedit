# Dual-Download Feature for Translation

## Overview
When translation is requested, the API now saves **BOTH** the original transcription and the translated version, packaging them together in a **ZIP file** for convenient download.

## How It Works

### 1. **Workflow Steps**
The transcription service now follows these steps:

1. **Extract Audio** - Convert video to 16kHz mono WAV
2. **Transcribe** - Generate subtitle using Whisper
3. **Post-process** - Apply AudioToTextPostProcessor
4. **Save Original** - Save original transcription as `{jobId}_original.srt`
5. **Translate** (if requested) - Translate to target language using Google Translate V1
6. **Save Translated** - Save translation as `{jobId}_translated_{langCode}.srt`
7. **Package** - Create ZIP file containing both files: `{jobId}_subtitles.zip`
8. **Complete** - Set OutputPath to ZIP file

### 2. **Download Behavior**

#### With Translation (e.g., Farsi selected)
- **Downloads**: ZIP file named `{originalFilename}_subtitles.zip`
- **Contains**:
  - `{jobId}_original.srt` - Original language transcription
  - `{jobId}_translated_fa.srt` - Farsi translation
- **Content Type**: `application/zip`

#### Without Translation (Language = "None")
- **Downloads**: Single SRT file named `{originalFilename}.srt`
- **Contains**: Original transcription only
- **Content Type**: `application/x-subrip`

### 3. **User Interface Updates**

The test page (`transcription-test.html`) now shows:

**Before Download:**
```
📦 ZIP package includes:
  • Original subtitle file
  • Farsi translation
```

**Download Button:**
- Text changed from "💾 Download Subtitle File (.srt)" to "💾 Download Subtitles"
- Clearly indicates both files are included when translation is used

## API Changes

### TranscriptionService.cs
```csharp
// Step 4: Save original subtitle first (before translation)
var originalSrtPath = Path.Combine(_outputDirectory, $"{jobId}_original.srt");
await File.WriteAllTextAsync(originalSrtPath, originalSrtContent, Encoding.UTF8);

// Step 5: Translation (if requested)
if (translation requested) {
    translatedSubtitle = await translationService.TranslateSubtitle(...);
    translatedSrtPath = Path.Combine(_outputDirectory, $"{jobId}_translated_{langCode}.srt");
    await File.WriteAllTextAsync(translatedSrtPath, translatedSrtContent, Encoding.UTF8);
}

// Step 6: Create ZIP package
if (translated) {
    using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
        zip.CreateEntryFromFile(originalSrtPath, Path.GetFileName(originalSrtPath));
        zip.CreateEntryFromFile(translatedSrtPath, Path.GetFileName(translatedSrtPath));
    }
    job.OutputPath = zipPath;
} else {
    job.OutputPath = originalSrtPath;
}
```

### TranscriptionController.cs
```csharp
// Download endpoint now handles both ZIP and SRT
var fileExtension = Path.GetExtension(job.OutputPath).ToLowerInvariant();

if (fileExtension == ".zip") {
    contentType = "application/zip";
    fileName = $"{originalName}_subtitles.zip";
} else {
    contentType = "application/x-subrip";
    fileName = $"{originalName}.srt";
}

return File(fileBytes, contentType, fileName);
```

## Example Usage

### cURL Example
```bash
# Upload and transcribe with Farsi translation
curl -X POST http://localhost:5000/api/transcription/transcribe \
  -F "file=@video.mp4" \
  -F "engine=whisper-cpp" \
  -F "model=tiny" \
  -F "language=auto" \
  -F "translateTo=fa"

# Response: { "jobId": "abc123", ... }

# Download ZIP with both files
curl -O http://localhost:5000/api/transcription/download/abc123
# Downloads: video_subtitles.zip containing:
#   - abc123_original.srt (English/detected language)
#   - abc123_translated_fa.srt (Farsi)
```

### JavaScript Example
```javascript
// In transcription-test.html
const formData = new FormData();
formData.append('file', videoFile);
formData.append('engine', 'whisper-cpp');
formData.append('model', 'tiny');
formData.append('language', 'auto');
formData.append('translateTo', 'fa'); // Request Farsi translation

const response = await fetch('/api/transcription/transcribe', {
    method: 'POST',
    body: formData
});

const { jobId } = await response.json();

// Wait for completion...

// Download both files
window.open(`/api/transcription/download/${jobId}`, '_blank');
// Downloads ZIP with original + Farsi translation
```

## Benefits

1. **No Data Loss** - Original transcription is always preserved
2. **Convenience** - Single download gets both versions
3. **Workflow Efficiency** - Users don't need to choose between original OR translated
4. **Quality Control** - Can compare translation against original
5. **Flexibility** - Has both languages for different use cases

## File Naming Convention

| Scenario | File Names |
|----------|-----------|
| No translation | `{jobId}_original.srt` |
| With translation | `{jobId}_original.srt`<br>`{jobId}_translated_{langCode}.srt` |
| ZIP package | `{jobId}_subtitles.zip` |
| Downloaded name | `{originalVideoName}_subtitles.zip` |

## Supported Languages

All 144 languages supported by Google Translate V1:
- Farsi (fa) - **Default in test page**
- Arabic (ar)
- Spanish (es)
- French (fr)
- German (de)
- Chinese (zh)
- Japanese (ja)
- Korean (ko)
- And 136 more...

See `/api/transcription/languages` for complete list.

## Testing

1. **Open Test Page**: http://localhost:5000/transcription-test.html
2. **Select File**: Choose a video/audio file
3. **Select Translation**: Choose "Farsi (Persian)" or any language
4. **Upload**: Click "🚀 Start Transcription"
5. **Wait**: Monitor progress bar
6. **Download**: Click "💾 Download Subtitles"
7. **Verify**: Unzip and check both files are present

## Notes

- ZIP creation uses `System.IO.Compression.ZipFile`
- Original subtitle is saved **before** translation to preserve exact transcription
- Translation shows in preview but ZIP contains both versions
- If translation fails, only original is returned (as single SRT, not ZIP)
- ZIP file is created only when translation succeeds
