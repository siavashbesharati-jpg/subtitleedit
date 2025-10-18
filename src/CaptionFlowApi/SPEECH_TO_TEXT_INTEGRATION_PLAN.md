# 🎤 Speech-to-Text Integration Plan for CaptionFlow API

## 📋 Executive Summary

**Good News!** Subtitle Edit already has **Whisper** and **Vosk** speech recognition engines built into LibSE! We just need to **integrate them into the API**.

---

## 🔍 What's Already Available in LibSE

### **Existing Speech-to-Text Engines:**

| Engine | Status | Performance | Quality | Notes |
|--------|--------|-------------|---------|-------|
| **Whisper.cpp** | ✅ Available | Fast (GPU/CPU) | Excellent | OpenAI's Whisper (C++ port) |
| **Whisper Python** | ✅ Available | Medium | Excellent | OpenAI official |
| **Whisper ConstMe** | ✅ Available | Very Fast (GPU) | Excellent | Optimized C++ version |
| **Faster-Whisper** | ✅ Available | Very Fast | Excellent | CTranslate2 optimized |
| **Vosk** | ✅ Available | Fast | Good | Offline, lightweight |
| **WhisperX** | ✅ Available | Fast | Excellent | With word-level timestamps |

### **Already Implemented in LibSE:**

```
src/libse/AudioToText/
├── WhisperHelper.cs           ← Core Whisper functionality
├── WhisperChoice.cs           ← Engine selection
├── WhisperModel.cs            ← Model definitions
├── WhisperLanguage.cs         ← Language support
├── ResultText.cs              ← Transcription result format
├── AudioToTextPostProcessor.cs ← Fix timing, casing, etc.
├── WhisperTimingFixer.cs      ← Improve subtitle timing
├── VoskModel.cs               ← Vosk engine support
└── IWhisperModel.cs           ← Interface for all engines
```

---

## 🎯 Integration Strategy

### **Phase 1: Add Speech-to-Text Service** (2-3 hours)

1. Create `TranscriptionService.cs` - Wraps LibSE's audio-to-text engines
2. Add `TranscriptionController.cs` - New API endpoints
3. Update models for transcription requests/responses
4. Configure FFmpeg for audio extraction

### **Phase 2: API Endpoints** (1-2 hours)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/transcription/engines` | GET | List available engines |
| `/api/transcription/models` | GET | List available models per engine |
| `/api/transcription/transcribe` | POST | Generate subtitles from video/audio |
| `/api/transcription/status/{jobId}` | GET | Check transcription progress |
| `/api/transcription/download/{jobId}` | GET | Download generated SRT |

### **Phase 3: Testing & Documentation** (1-2 hours)

1. Test with various video files
2. Update Swagger documentation
3. Create usage examples
4. Update README

---

## 💻 Implementation Plan

### **Step 1: Create Transcription Models**

```csharp
// Models/TranscriptionRequest.cs
public class TranscriptionRequest
{
    public string Engine { get; set; } = "whisper-cpp"; // whisper-cpp, vosk, whisper-python
    public string Model { get; set; } = "base"; // tiny, base, small, medium, large
    public string Language { get; set; } = "auto"; // auto, en, es, fr, etc.
    public bool TranslateToEnglish { get; set; } = false;
    public bool UsePostProcessing { get; set; } = true;
    public int AudioTrackNumber { get; set; } = 0;
}

// Models/TranscriptionResponse.cs
public class TranscriptionResponse
{
    public string JobId { get; set; }
    public string Status { get; set; } // Queued, Transcribing, Completed, Failed
    public string Engine { get; set; }
    public string Model { get; set; }
    public int Progress { get; set; } // 0-100
    public string OutputPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<string> Messages { get; set; } // Progress messages
}

// Models/TranscriptionResult.cs
public class TranscriptionResult
{
    public string JobId { get; set; }
    public string Text { get; set; } // Full transcript
    public List<SubtitleSegment> Segments { get; set; }
    public string SrtContent { get; set; }
    public TimeSpan Duration { get; set; }
    public int WordCount { get; set; }
}

public class SubtitleSegment
{
    public int Number { get; set; }
    public double StartSeconds { get; set; }
    public double EndSeconds { get; set; }
    public string Text { get; set; }
    public double Confidence { get; set; }
}
```

### **Step 2: Create Transcription Service**

```csharp
// Services/TranscriptionService.cs
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

public class TranscriptionService
{
    private readonly ILogger<TranscriptionService> _logger;
    private readonly ConcurrentDictionary<string, TranscriptionJob> _jobs = new();
    private readonly string _tempDirectory;
    private readonly string _outputDirectory;

    public async Task<TranscriptionJob> CreateTranscriptionJobAsync(
        IFormFile file, 
        TranscriptionRequest request)
    {
        var jobId = Guid.NewGuid().ToString();
        var fileName = $"{jobId}_{file.FileName}";
        var filePath = Path.Combine(_tempDirectory, fileName);

        // Save uploaded file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var job = new TranscriptionJob
        {
            JobId = jobId,
            FileName = file.FileName,
            FilePath = filePath,
            Status = "Queued",
            Engine = request.Engine,
            Model = request.Model,
            Language = request.Language,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;

        // Start transcription in background
        _ = Task.Run(() => TranscribeAsync(jobId, filePath, request));

        return job;
    }

    private async Task TranscribeAsync(
        string jobId, 
        string filePath, 
        TranscriptionRequest request)
    {
        var job = _jobs[jobId];
        
        try
        {
            job.Status = "Extracting Audio";
            job.Progress = 10;

            // Extract audio to WAV (16kHz, mono) using FFmpeg
            var wavFile = await ExtractAudioToWavAsync(filePath, request.AudioTrackNumber);
            job.Progress = 30;

            job.Status = "Transcribing";
            
            // Use LibSE's Whisper or Vosk engine
            var subtitle = await TranscribeWithEngineAsync(
                wavFile, 
                request.Engine, 
                request.Model, 
                request.Language,
                request.TranslateToEnglish);
            
            job.Progress = 80;

            if (request.UsePostProcessing)
            {
                job.Status = "Post-Processing";
                subtitle = PostProcessSubtitle(subtitle, request.Engine);
            }

            job.Progress = 90;

            // Save as SRT
            var srtPath = Path.Combine(_outputDirectory, $"{jobId}.srt");
            var srtFormat = new SubRip();
            var srtContent = srtFormat.ToText(subtitle, string.Empty);
            await File.WriteAllTextAsync(srtPath, srtContent, Encoding.UTF8);

            job.Status = "Completed";
            job.Progress = 100;
            job.OutputPath = srtPath;
            job.CompletedAt = DateTime.UtcNow;
            job.Result = new TranscriptionResult
            {
                JobId = jobId,
                Text = string.Join(" ", subtitle.Paragraphs.Select(p => p.Text)),
                SrtContent = srtContent,
                Duration = subtitle.Paragraphs.LastOrDefault()?.EndTime.TimeSpan ?? TimeSpan.Zero,
                WordCount = subtitle.Paragraphs.Sum(p => p.Text.Split(' ').Length),
                Segments = subtitle.Paragraphs.Select(p => new SubtitleSegment
                {
                    Number = p.Number,
                    StartSeconds = p.StartTime.TotalSeconds,
                    EndSeconds = p.EndTime.TotalSeconds,
                    Text = p.Text
                }).ToList()
            };

            _logger.LogInformation($"Transcription completed: {jobId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Transcription failed: {jobId}");
            job.Status = "Failed";
            job.Error = ex.Message;
        }
    }

    private async Task<string> ExtractAudioToWavAsync(string videoPath, int audioTrack)
    {
        var wavPath = Path.Combine(_tempDirectory, $"{Guid.NewGuid()}.wav");

        // FFmpeg command to extract audio as 16kHz mono WAV
        var ffmpegArgs = $"-i \"{videoPath}\" -ar 16000 -ac 1 -map 0:a:{audioTrack} \"{wavPath}\"";
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (!File.Exists(wavPath))
        {
            throw new Exception("Failed to extract audio");
        }

        return wavPath;
    }

    private async Task<Subtitle> TranscribeWithEngineAsync(
        string wavFile,
        string engine,
        string model,
        string language,
        bool translate)
    {
        // Use LibSE's existing engines!
        
        if (engine.StartsWith("whisper"))
        {
            return await TranscribeWithWhisperAsync(wavFile, model, language, translate);
        }
        else if (engine == "vosk")
        {
            return await TranscribeWithVoskAsync(wavFile, model, language);
        }

        throw new NotSupportedException($"Engine not supported: {engine}");
    }

    private async Task<Subtitle> TranscribeWithWhisperAsync(
        string wavFile,
        string model,
        string language,
        bool translate)
    {
        // This is where we call LibSE's Whisper code
        // Similar to WhisperAudioToText.TranscribeViaWhisper()
        
        var resultList = new List<ResultText>();
        
        // Get Whisper executable path
        var whisperPath = WhisperHelper.GetWhisperPathAndFileName();
        
        if (string.IsNullOrEmpty(whisperPath))
        {
            throw new Exception("Whisper not installed. Please install Whisper first.");
        }

        // Build command line arguments
        var args = $"--model {model} --language {language} --output-srt \"{wavFile}\"";
        if (translate)
        {
            args += " --task translate";
        }

        // Run Whisper process
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = whisperPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        // Read generated SRT file
        var srtFile = wavFile + ".srt";
        if (!File.Exists(srtFile))
        {
            throw new Exception("Whisper did not generate SRT file");
        }

        // Parse SRT using LibSE
        var subtitle = new Subtitle();
        var srtFormat = new SubRip();
        var srtLines = await File.ReadAllLinesAsync(srtFile);
        srtFormat.LoadSubtitle(subtitle, srtLines.ToList(), srtFile);

        return subtitle;
    }

    private async Task<Subtitle> TranscribeWithVoskAsync(
        string wavFile,
        string model,
        string language)
    {
        // Use LibSE's Vosk implementation
        // Similar to VoskAudioToText.TranscribeViaVosk()
        
        throw new NotImplementedException("Vosk support coming soon");
    }

    private Subtitle PostProcessSubtitle(Subtitle subtitle, string engine)
    {
        // Use LibSE's AudioToTextPostProcessor
        var processor = new AudioToTextPostProcessor();
        
        var engineType = engine.StartsWith("whisper") 
            ? AudioToTextPostProcessor.Engine.Whisper 
            : AudioToTextPostProcessor.Engine.Vosk;

        return processor.Fix(
            engineType,
            subtitle,
            addPeriods: true,
            mergeShortLines: true,
            fixCasing: true,
            fixShortDuration: true,
            splitLines: false);
    }

    public TranscriptionJob GetJob(string jobId)
    {
        return _jobs.TryGetValue(jobId, out var job) ? job : null;
    }

    public List<TranscriptionJob> GetAllJobs()
    {
        return _jobs.Values.OrderByDescending(j => j.CreatedAt).ToList();
    }
}
```

### **Step 3: Create API Controller**

```csharp
// Controllers/TranscriptionController.cs
[ApiController]
[Route("api/[controller]")]
public class TranscriptionController : ControllerBase
{
    private readonly TranscriptionService _transcriptionService;
    private readonly ILogger<TranscriptionController> _logger;

    public TranscriptionController(
        TranscriptionService transcriptionService,
        ILogger<TranscriptionController> logger)
    {
        _transcriptionService = transcriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Generate subtitles from video/audio using speech recognition
    /// </summary>
    [HttpPost("transcribe")]
    [RequestSizeLimit(2_147_483_648)] // 2GB
    public async Task<IActionResult> Transcribe(
        [FromForm] IFormFile file,
        [FromForm] string engine = "whisper-cpp",
        [FromForm] string model = "base",
        [FromForm] string language = "auto",
        [FromForm] bool translateToEnglish = false,
        [FromForm] bool usePostProcessing = true,
        [FromForm] int audioTrackNumber = 0)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        var request = new TranscriptionRequest
        {
            Engine = engine,
            Model = model,
            Language = language,
            TranslateToEnglish = translateToEnglish,
            UsePostProcessing = usePostProcessing,
            AudioTrackNumber = audioTrackNumber
        };

        var job = await _transcriptionService.CreateTranscriptionJobAsync(file, request);

        return Ok(new
        {
            jobId = job.JobId,
            status = job.Status,
            engine = job.Engine,
            model = job.Model,
            createdAt = job.CreatedAt,
            message = "Transcription started. Use /api/transcription/status/{jobId} to check progress."
        });
    }

    /// <summary>
    /// Check transcription job status
    /// </summary>
    [HttpGet("status/{jobId}")]
    public IActionResult GetStatus(string jobId)
    {
        var job = _transcriptionService.GetJob(jobId);
        
        if (job == null)
        {
            return NotFound(new { error = "Job not found" });
        }

        return Ok(new
        {
            jobId = job.JobId,
            status = job.Status,
            progress = job.Progress,
            engine = job.Engine,
            model = job.Model,
            fileName = job.FileName,
            createdAt = job.CreatedAt,
            completedAt = job.CompletedAt,
            error = job.Error
        });
    }

    /// <summary>
    /// Download transcription result as SRT
    /// </summary>
    [HttpGet("download/{jobId}")]
    public IActionResult Download(string jobId)
    {
        var job = _transcriptionService.GetJob(jobId);
        
        if (job == null)
        {
            return NotFound(new { error = "Job not found" });
        }

        if (job.Status != "Completed")
        {
            return BadRequest(new { error = $"Job status: {job.Status}" });
        }

        if (!System.IO.File.Exists(job.OutputPath))
        {
            return NotFound(new { error = "Output file not found" });
        }

        var fileBytes = System.IO.File.ReadAllBytes(job.OutputPath);
        var fileName = Path.GetFileNameWithoutExtension(job.FileName) + ".srt";

        return File(fileBytes, "application/x-subrip", fileName);
    }

    /// <summary>
    /// Get transcription result with all details
    /// </summary>
    [HttpGet("result/{jobId}")]
    public IActionResult GetResult(string jobId)
    {
        var job = _transcriptionService.GetJob(jobId);
        
        if (job == null)
        {
            return NotFound(new { error = "Job not found" });
        }

        if (job.Status != "Completed")
        {
            return BadRequest(new { error = $"Job not completed. Status: {job.Status}" });
        }

        return Ok(job.Result);
    }

    /// <summary>
    /// List available speech recognition engines
    /// </summary>
    [HttpGet("engines")]
    public IActionResult GetEngines()
    {
        return Ok(new
        {
            engines = new[]
            {
                new
                {
                    id = "whisper-cpp",
                    name = "Whisper C++",
                    description = "Fast C++ implementation of OpenAI Whisper",
                    supportsGpu = true,
                    recommended = true
                },
                new
                {
                    id = "whisper-python",
                    name = "Whisper Python",
                    description = "Official OpenAI Whisper implementation",
                    supportsGpu = true,
                    recommended = false
                },
                new
                {
                    id = "faster-whisper",
                    name = "Faster Whisper",
                    description = "Optimized Whisper with CTranslate2",
                    supportsGpu = true,
                    recommended = true
                },
                new
                {
                    id = "vosk",
                    name = "Vosk",
                    description = "Offline speech recognition",
                    supportsGpu = false,
                    recommended = false
                }
            }
        });
    }

    /// <summary>
    /// List available models for each engine
    /// </summary>
    [HttpGet("models")]
    public IActionResult GetModels([FromQuery] string engine = "whisper-cpp")
    {
        var whisperModels = new[]
        {
            new { name = "tiny", size = "75 MB", description = "Fastest, lowest quality" },
            new { name = "base", size = "142 MB", description = "Fast, good quality" },
            new { name = "small", size = "465 MB", description = "Balanced" },
            new { name = "medium", size = "1.42 GB", description = "High quality" },
            new { name = "large", size = "2.87 GB", description = "Best quality, slowest" }
        };

        return Ok(new { engine, models = whisperModels });
    }

    /// <summary>
    /// List all transcription jobs
    /// </summary>
    [HttpGet("jobs")]
    public IActionResult GetJobs()
    {
        var jobs = _transcriptionService.GetAllJobs();
        
        return Ok(new
        {
            count = jobs.Count,
            jobs = jobs.Select(j => new
            {
                jobId = j.JobId,
                fileName = j.FileName,
                status = j.Status,
                progress = j.Progress,
                engine = j.Engine,
                model = j.Model,
                createdAt = j.CreatedAt,
                completedAt = j.CompletedAt
            })
        });
    }
}
```

### **Step 4: Register Services in Program.cs**

```csharp
// Program.cs
builder.Services.AddSingleton<TranscriptionService>();
builder.Services.AddSingleton<SubtitleProcessingService>(); // Existing
```

---

## 📝 API Usage Examples

### **Example 1: Transcribe Video File**

```powershell
# Upload video and start transcription
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/transcription/transcribe" `
  -Method Post `
  -Form @{
    file = Get-Item "C:\Videos\movie.mp4"
    engine = "whisper-cpp"
    model = "base"
    language = "en"
  }

Write-Host "Job ID: $($response.jobId)"
```

### **Example 2: Check Progress**

```powershell
# Check transcription progress
$jobId = "abc-123-def-456"
$status = Invoke-RestMethod -Uri "http://localhost:5000/api/transcription/status/$jobId"

Write-Host "Status: $($status.status)"
Write-Host "Progress: $($status.progress)%"
```

### **Example 3: Download SRT**

```powershell
# Download completed transcript
Invoke-RestMethod -Uri "http://localhost:5000/api/transcription/download/$jobId" `
  -OutFile "transcript.srt"
```

### **Example 4: Get Full Result**

```powershell
# Get detailed results
$result = Invoke-RestMethod -Uri "http://localhost:5000/api/transcription/result/$jobId"

Write-Host "Transcript: $($result.text)"
Write-Host "Duration: $($result.duration)"
Write-Host "Word Count: $($result.wordCount)"
Write-Host "Segments: $($result.segments.Count)"
```

---

## 🚀 Next Steps

### **1. Install Whisper** (Required)

The API needs Whisper installed on the server:

```powershell
# Option 1: Download from Subtitle Edit
# The API can use the same Whisper installation as the desktop app

# Option 2: Install Whisper.cpp manually
# Download from: https://github.com/ggerganov/whisper.cpp/releases

# Option 3: Use Docker (easiest for deployment)
```

### **2. Download Whisper Models**

Models need to be downloaded before use:

```powershell
# Models are stored in: %APPDATA%\Subtitle Edit\Whisper
# Sizes:
# - tiny: 75 MB
# - base: 142 MB
# - small: 465 MB
# - medium: 1.42 GB
# - large: 2.87 GB
```

### **3. Configuration**

Add to `appsettings.json`:

```json
{
  "Transcription": {
    "WhisperPath": "C:\\Program Files\\Whisper\\whisper.exe",
    "ModelsPath": "%APPDATA%\\Subtitle Edit\\Whisper",
    "DefaultEngine": "whisper-cpp",
    "DefaultModel": "base",
    "MaxConcurrentJobs": 2,
    "TempDirectory": "C:\\CaptionFlow\\Temp",
    "OutputDirectory": "C:\\CaptionFlow\\Transcriptions"
  }
}
```

---

## 📊 Performance Estimates

| Model | Size | Speed (1h audio) | Quality |
|-------|------|------------------|---------|
| tiny | 75 MB | ~2-3 min | Fair |
| base | 142 MB | ~5-7 min | Good |
| small | 465 MB | ~10-15 min | Very Good |
| medium | 1.42 GB | ~20-30 min | Excellent |
| large | 2.87 GB | ~40-60 min | Best |

*Times are approximate on mid-range CPU. GPU acceleration can be 5-10x faster.*

---

## 🎯 Summary

**What We're Adding:**

✅ Speech-to-text transcription using existing LibSE engines  
✅ Whisper C++, Python, Faster-Whisper support  
✅ Multiple model sizes (tiny to large)  
✅ Auto-language detection  
✅ Translation to English  
✅ Post-processing (timing, casing, punctuation)  
✅ Background job processing  
✅ Progress tracking  
✅ SRT output

**What's Already Done in LibSE:**

✅ Whisper engine integration  
✅ Vosk engine integration  
✅ Audio extraction (FFmpeg)  
✅ Model management  
✅ Result parsing  
✅ Subtitle timing fixes  
✅ Post-processing

**What We Need to Do:**

1. Create `TranscriptionService.cs` - Wrapper around LibSE
2. Create `TranscriptionController.cs` - API endpoints
3. Add transcription models
4. Update Swagger documentation
5. Test and publish

**Total Effort:** ~4-6 hours of development

---

**Ready to implement? Let me know and I'll start creating the files!** 🚀
