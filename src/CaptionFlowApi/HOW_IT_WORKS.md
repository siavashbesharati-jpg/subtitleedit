# 🎯 What Does CaptionFlow API Actually Do?

## 📋 Executive Summary

**Yes! The API uses the main Subtitle Edit engine (LibSE) to extract subtitles.**

The CaptionFlow API is a **RESTful wrapper** around Subtitle Edit's powerful core library (`libse.dll`). It doesn't generate subtitles from scratch - it **extracts embedded subtitles** that already exist inside video/audio files.

---

## 🔍 How It Works - The Complete Picture

### **Architecture Overview**

```
┌─────────────────────────────────────────────────────────────┐
│                    CaptionFlow API Layer                     │
│  (ASP.NET Core 8.0 - Your Published EXE)                   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Controllers/SubtitleController.cs                   │  │
│  │  - Receives HTTP requests                            │  │
│  │  - Handles file uploads (2GB max)                    │  │
│  │  - Downloads videos from URLs                        │  │
│  │  - Returns JSON responses                            │  │
│  └──────────────────────────────────────────────────────┘  │
│                         ↓                                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Services/SubtitleProcessingService.cs               │  │
│  │  - Manages background jobs                           │  │
│  │  - Saves uploaded files                              │  │
│  │  - Calls LibSE to extract subtitles                  │  │
│  │  - Generates SRT files                               │  │
│  └──────────────────────────────────────────────────────┘  │
│                         ↓                                    │
└──────────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────────┐
│            Subtitle Edit Core Library (LibSE)                │
│       (The REAL Engine - Same as Desktop App!)              │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Nikse.SubtitleEdit.Core.ContainerFormats            │  │
│  │                                                       │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  Matroska/MatroskaFile.cs                      │  │  │
│  │  │  - Parses MKV files                            │  │  │
│  │  │  - Extracts embedded SRT, ASS, SSA subtitles   │  │  │
│  │  │  - Handles multiple subtitle tracks           │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │                                                       │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  Mp4/MP4Parser.cs                              │  │  │
│  │  │  - Parses MP4/M4V files                        │  │  │
│  │  │  - Extracts WebVTT, TX3G subtitles             │  │  │
│  │  │  - Supports closed captions (CEA-608)          │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
│                         ↓                                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Nikse.SubtitleEdit.Core.SubtitleFormats             │  │
│  │  - SubRip (.srt) - Most common format               │  │
│  │  - SubStationAlpha (.ass/.ssa) - Advanced styling   │  │
│  │  - WebVTT (.vtt) - Web standard                     │  │
│  │  - 300+ other subtitle formats                      │  │
│  └──────────────────────────────────────────────────────┘  │
│                         ↓                                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Nikse.SubtitleEdit.Core.Common.Subtitle             │  │
│  │  - Manages subtitle paragraphs                       │  │
│  │  - Handles timing (start/end times)                  │  │
│  │  - Renumbers subtitles                               │  │
│  │  - Converts between formats                          │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

## 🎬 What the API Actually Does

### **1. Extracts EXISTING Subtitles** ✅

The API **extracts subtitles that are already embedded** in video files:

#### Supported Container Formats:
- **MKV (Matroska)** - Most common for embedded subtitles
- **MP4/M4V** - Common for streaming content
- **AVI** - Older format
- **MOV** - QuickTime format

#### Subtitle Formats It Can Extract:
- **SRT** (SubRip) - Plain text with timing
- **ASS/SSA** (SubStation Alpha) - Advanced styling
- **WebVTT** - Web video standard
- **TX3G** - MP4 native subtitle format
- **CEA-608/708** - Closed captions
- **VobSub** - DVD subtitles (image-based)
- **PGS** - Blu-ray subtitles (image-based)

---

### **2. Does NOT Generate/Create Subtitles** ❌

**Important:** The API does **NOT**:
- ❌ Generate subtitles from audio (no speech recognition)
- ❌ Transcribe video content
- ❌ Use AI/ML to create subtitles
- ❌ Auto-translate subtitles
- ❌ Create subtitles from scratch

If a video has **no embedded subtitles**, the API will return:
```json
{
  "status": "Failed",
  "error": "No embedded subtitles found in video file"
}
```

---

## 💻 Code Deep Dive - How It Uses LibSE

### **Step 1: API Receives Video Upload**

```csharp
// Controllers/SubtitleController.cs
[HttpPost("extract")]
public async Task<IActionResult> ExtractSubtitles([FromForm] IFormFile videoFile)
{
    // Save uploaded video to disk
    var job = await _processingService.CreateJobAsync(videoFile, request);
    return Ok(new { jobId = job.JobId, status = "Processing" });
}
```

### **Step 2: Processing Service Calls LibSE**

```csharp
// Services/SubtitleProcessingService.cs
private async Task<Subtitle> ExtractSubtitlesFromVideo(string videoFilePath)
{
    var subtitle = new Subtitle(); // LibSE class!
    
    // Try Matroska (MKV) format
    var matroskaFile = new MatroskaFile(videoFilePath); // LibSE!
    
    if (matroskaFile.IsValid)
    {
        var tracks = matroskaFile.GetTracks();
        foreach (var track in tracks)
        {
            if (track.IsSubtitle)
            {
                // Extract subtitle data
                var matroskaSubtitles = matroskaFile.GetSubtitle(track.TrackNumber, null);
                
                foreach (var mkSub in matroskaSubtitles)
                {
                    var text = mkSub.GetText(track); // Decode subtitle text
                    var p = new Paragraph(text, mkSub.Start, mkSub.End);
                    subtitle.Paragraphs.Add(p); // LibSE paragraph!
                }
                
                subtitle.Renumber(); // LibSE method!
                return subtitle;
            }
        }
    }
}
```

### **Step 3: Convert to SRT Format**

```csharp
// Services/SubtitleProcessingService.cs
var srtFormat = new SubRip(); // LibSE format!
var srtContent = srtFormat.ToText(subtitle, string.Empty);

// Save to file
File.WriteAllText(outputFilePath, srtContent, Encoding.UTF8);
```

### **Final SRT Output**

```srt
1
00:00:01,000 --> 00:00:04,000
Hello, welcome to the video!

2
00:00:05,000 --> 00:00:08,000
This subtitle was extracted from the video.

3
00:00:09,000 --> 00:00:12,000
The API used LibSE to parse it!
```

---

## 🔧 LibSE Components Used by the API

### **1. Container Format Parsers**

| LibSE Component | Purpose | Used By API |
|----------------|---------|-------------|
| `MatroskaFile` | Parse MKV files | ✅ Yes |
| `MP4Parser` | Parse MP4 files | ✅ Yes |
| `MatroskaSubtitle` | Decode subtitle data | ✅ Yes |

### **2. Subtitle Format Handlers**

| LibSE Component | Purpose | Used By API |
|----------------|---------|-------------|
| `SubRip` | SRT format | ✅ Yes (output) |
| `SubStationAlpha` | ASS format | ✅ Yes (parsing) |
| `AdvancedSubStationAlpha` | ASS format | ✅ Yes (parsing) |
| `WebVTT` | VTT format | ✅ Yes (parsing) |

### **3. Core Classes**

| LibSE Component | Purpose | Used By API |
|----------------|---------|-------------|
| `Subtitle` | Main subtitle container | ✅ Yes |
| `Paragraph` | Individual subtitle entry | ✅ Yes |
| `TimeCode` | Timing information | ✅ Yes |
| `Utilities` | Helper functions | ✅ Yes |

---

## 📊 Example Workflow

### **User Request:**
```bash
curl -X POST "http://localhost:5000/api/subtitle/extract" \
  -F "videoFile=@movie.mkv"
```

### **What Happens Inside:**

1. **API receives `movie.mkv`** (1.5 GB)
2. **Saves to:** `uploads/abc123_movie.mkv`
3. **Creates job:** `{ jobId: "abc123", status: "Processing" }`
4. **Background processing starts:**
   - Opens MKV file using **LibSE MatroskaFile**
   - Finds subtitle tracks using **LibSE GetTracks()**
   - Extracts subtitle data using **LibSE GetSubtitle()**
   - Decodes text using **LibSE GetText()**
   - Creates **LibSE Paragraph** objects
   - Builds **LibSE Subtitle** object
   - Converts to SRT using **LibSE SubRip.ToText()**
5. **Saves SRT to:** `outputs/abc123.srt`
6. **Updates job:** `{ status: "Completed", outputPath: "..." }`

### **User Downloads:**
```bash
curl "http://localhost:5000/api/subtitle/download/abc123/srt" \
  -o movie.srt
```

---

## 🎯 What Videos Work?

### ✅ **Videos WITH Embedded Subtitles**

These will work perfectly:
- Movies/TV shows with built-in subtitles
- MKV files with multiple subtitle tracks
- MP4 files with closed captions
- Streaming content with WebVTT subtitles

**Example:** A movie downloaded from streaming services often has:
```
movie.mkv
├── Video Track (H.264)
├── Audio Track (AAC)
├── Subtitle Track 1 (English SRT)
├── Subtitle Track 2 (Spanish SRT)
└── Subtitle Track 3 (French ASS)
```

### ❌ **Videos WITHOUT Embedded Subtitles**

These will return "No subtitles found":
- Raw camera recordings
- Screen recordings
- Home videos
- Videos with only burned-in/hardcoded subtitles
- YouTube videos downloaded without subtitle tracks

---

## 🆚 Comparison: Desktop App vs API

| Feature | Desktop App | CaptionFlow API | Shared Engine |
|---------|------------|----------------|---------------|
| Extract subtitles from MKV/MP4 | ✅ Yes | ✅ Yes | **LibSE** |
| Convert subtitle formats | ✅ Yes | 🔄 Planned | **LibSE** |
| Edit subtitle timing | ✅ Yes | ❌ No | N/A |
| Spell check subtitles | ✅ Yes | ❌ No | **LibSE** |
| OCR from images | ✅ Yes | ❌ No | **LibSE** |
| Generate from audio | ❌ No | ❌ No | N/A |
| RESTful API | ❌ No | ✅ Yes | N/A |
| Web interface | ❌ No | ✅ Swagger | N/A |

---

## 🔍 How to Check If a Video Has Subtitles

### **Option 1: Use Your API**
```powershell
# Upload video
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/subtitle/extract" `
  -Method Post -Form @{videoFile = Get-Item "movie.mkv"}

# Check status
Invoke-RestMethod -Uri "http://localhost:5000/api/subtitle/status/$($response.jobId)"
```

### **Option 2: Use Desktop Tools**

**VLC Media Player:**
1. Open video in VLC
2. Click `Subtitle` menu
3. If you see subtitle tracks → ✅ Has subtitles
4. If grayed out → ❌ No subtitles

**MediaInfo:**
```powershell
# Install MediaInfo
choco install mediainfo-cli

# Check video
mediainfo movie.mkv

# Look for:
Text #1
Format: SubRip  ← Has subtitles!
```

**FFmpeg:**
```powershell
ffmpeg -i movie.mkv

# Look for:
Stream #0:2(eng): Subtitle: subrip  ← Has subtitles!
```

---

## 🚀 Future Enhancements (Not Currently Implemented)

### **Potential Future Features:**

1. **Auto-Translation** 
   - Integrate Google Translate API
   - Translate extracted subtitles to other languages

2. **Speech Recognition**
   - Use Whisper AI or Azure Speech
   - Generate subtitles from videos without them

3. **Format Conversion**
   - Convert SRT ↔ ASS ↔ VTT ↔ etc.

4. **Subtitle Editing**
   - Adjust timing
   - Fix spelling errors
   - Merge/split subtitles

5. **Burn-In Subtitles**
   - Use FFmpeg to hardcode subtitles into video

---

## 📚 Summary

### **What CaptionFlow API IS:**
✅ RESTful wrapper around Subtitle Edit's core library (LibSE)
✅ Subtitle **extraction** tool for MKV/MP4 videos
✅ Format converter (to SRT)
✅ Web API for automation and integration

### **What CaptionFlow API IS NOT:**
❌ Speech-to-text transcription service
❌ AI subtitle generator
❌ Video editor
❌ OCR tool (for hardcoded subtitles)

### **The Core Technology:**
- **Same engine as Subtitle Edit desktop app**
- **Uses LibSE.dll** (Nikse.SubtitleEdit.Core)
- **Proven subtitle parsing library**
- **Supports 300+ subtitle formats**
- **Battle-tested by millions of users**

---

## 🎓 Key Takeaway

**The API is NOT creating subtitles - it's EXTRACTING existing subtitles from video files using the same powerful engine that powers Subtitle Edit desktop application.**

Think of it like a **digital librarian** that opens a video file (like opening a book), finds the subtitle tracks (like finding specific chapters), and extracts them into a separate file (like making photocopies).

---

**Questions?** Check:
- `DEPLOYMENT_GUIDE.md` - How to deploy
- `API_DOCUMENTATION.md` - API endpoints
- `TROUBLESHOOTING.md` - Common issues

**Want to add AI subtitle generation?** You would need to integrate:
- OpenAI Whisper
- Google Speech-to-Text
- Azure Cognitive Services
- Amazon Transcribe

(These are separate services, not included in the current API)
