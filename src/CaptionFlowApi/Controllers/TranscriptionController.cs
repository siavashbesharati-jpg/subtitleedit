using CaptionFlowApi.Models;
using CaptionFlowApi.Services;
using Microsoft.AspNetCore.Mvc;
using Nikse.SubtitleEdit.Core.AudioToText;

namespace CaptionFlowApi.Controllers;

/// <summary>
/// Speech-to-text transcription endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TranscriptionController : ControllerBase
{
    private readonly TranscriptionService _transcriptionService;
    private readonly TranslationService _translationService;
    private readonly ILogger<TranscriptionController> _logger;

    public TranscriptionController(
        TranscriptionService transcriptionService,
        TranslationService translationService,
        ILogger<TranscriptionController> logger)
    {
        _transcriptionService = transcriptionService;
        _translationService = translationService;
        _logger = logger;
    }

    /// <summary>
    /// Generate subtitles from video/audio using speech recognition
    /// </summary>
    /// <param name="file">Video or audio file to transcribe</param>
    /// <param name="engine">Speech recognition engine (whisper-cpp, whisper-python, etc.)</param>
    /// <param name="model">Model size (tiny, base, small, medium, large)</param>
    /// <param name="language">Language code (auto, en, es, fr, etc.)</param>
    /// <param name="translateToEnglish">Translate to English</param>
    /// <param name="usePostProcessing">Apply post-processing for better quality</param>
    /// <param name="audioTrackNumber">Audio track number (0 for default)</param>
    /// <returns>Transcription job details</returns>
    [HttpPost("transcribe")]
    [RequestSizeLimit(2_147_483_648)] // 2GB
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
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
            return BadRequest(new
            {
                error = "No file uploaded",
                development = true,
                message = "Please provide a video or audio file to transcribe"
            });
        }

        _logger.LogInformation($"Received transcription request: {file.FileName}, engine: {engine}, model: {model}");

        var request = new TranscriptionRequest
        {
            Engine = engine,
            Model = model,
            Language = language,
            TranslateToEnglish = translateToEnglish,
            UsePostProcessing = usePostProcessing,
            AudioTrackNumber = audioTrackNumber
        };

        try
        {
            var job = await _transcriptionService.CreateTranscriptionJobAsync(file, request);

            return Ok(new
            {
                jobId = job.JobId,
                status = job.Status,
                engine = job.Engine,
                model = job.Model,
                language = job.Language,
                fileName = job.FileName,
                uploadedFileSize = file.Length,
                createdAt = job.CreatedAt,
                development = true,
                message = "Transcription started. Use GET /api/transcription/status/{jobId} to check progress.",
                endpoints = new
                {
                    status = $"/api/transcription/status/{job.JobId}",
                    download = $"/api/transcription/download/{job.JobId}",
                    result = $"/api/transcription/result/{job.JobId}"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create transcription job");
            return StatusCode(500, new
            {
                error = "Failed to create transcription job",
                details = ex.Message,
                development = true
            });
        }
    }

    /// <summary>
    /// Check transcription job status and progress
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <returns>Job status and progress</returns>
    [HttpGet("status/{jobId}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetStatus(string jobId)
    {
        var job = _transcriptionService.GetJob(jobId);

        if (job == null)
        {
            return NotFound(new
            {
                error = "Job not found",
                jobId,
                development = true,
                message = "The specified transcription job does not exist"
            });
        }

        return Ok(new
        {
            jobId = job.JobId,
            status = job.Status,
            progress = job.Progress,
            engine = job.Engine,
            model = job.Model,
            language = job.Language,
            fileName = job.FileName,
            createdAt = job.CreatedAt,
            completedAt = job.CompletedAt,
            error = job.Error,
            development = true,
            statusDescription = job.Status switch
            {
                "Queued" => "Waiting to start transcription",
                "Extracting Audio" => "Extracting audio from video file",
                "Transcribing" => "Running speech recognition",
                "Post-Processing" => "Improving subtitle quality",
                "Completed" => "Transcription complete! Ready to download",
                "Failed" => "Transcription failed. Check error message",
                _ => job.Status
            }
        });
    }

    /// <summary>
    /// Download transcription result as SRT file
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <returns>SRT file</returns>
    [HttpGet("download/{jobId}")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult Download(string jobId)
    {
        var job = _transcriptionService.GetJob(jobId);

        if (job == null)
        {
            return NotFound(new
            {
                error = "Job not found",
                jobId,
                development = true
            });
        }

        if (job.Status != "Completed")
        {
            return BadRequest(new
            {
                error = $"Job not completed. Current status: {job.Status}",
                jobId,
                status = job.Status,
                progress = job.Progress,
                development = true,
                message = "Please wait for transcription to complete before downloading"
            });
        }

        if (string.IsNullOrEmpty(job.OutputPath) || !System.IO.File.Exists(job.OutputPath))
        {
            return NotFound(new
            {
                error = "Output file not found",
                jobId,
                development = true
            });
        }

        var fileBytes = System.IO.File.ReadAllBytes(job.OutputPath);
        var fileName = Path.GetFileNameWithoutExtension(job.FileName) + ".srt";

        _logger.LogInformation($"Downloading SRT file for job: {jobId}, size: {fileBytes.Length} bytes");

        return File(fileBytes, "application/x-subrip", fileName);
    }

    /// <summary>
    /// Get complete transcription result with all details
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <returns>Full transcription result including text, segments, and metadata</returns>
    [HttpGet("result/{jobId}")]
    [ProducesResponseType(typeof(TranscriptionResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetResult(string jobId)
    {
        var job = _transcriptionService.GetJob(jobId);

        if (job == null)
        {
            return NotFound(new
            {
                error = "Job not found",
                jobId,
                development = true
            });
        }

        if (job.Status != "Completed")
        {
            return BadRequest(new
            {
                error = $"Job not completed. Current status: {job.Status}",
                jobId,
                status = job.Status,
                progress = job.Progress,
                development = true
            });
        }

        if (job.Result == null)
        {
            return NotFound(new
            {
                error = "Result not available",
                jobId,
                development = true
            });
        }

        return Ok(new
        {
            job.Result.JobId,
            job.Result.Text,
            job.Result.Segments,
            job.Result.SrtContent,
            job.Result.Duration,
            job.Result.WordCount,
            job.Result.SegmentCount,
            metadata = new
            {
                job.Engine,
                job.Model,
                job.Language,
                job.FileName,
                job.CreatedAt,
                job.CompletedAt,
                processingTime = job.CompletedAt.HasValue 
                    ? (job.CompletedAt.Value - job.CreatedAt).TotalSeconds 
                    : 0
            },
            development = true
        });
    }

    /// <summary>
    /// List available speech recognition engines
    /// </summary>
    /// <returns>Available engines and their capabilities</returns>
    [HttpGet("engines")]
    [ProducesResponseType(typeof(object), 200)]
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
                    recommended = true,
                    website = "https://github.com/ggerganov/whisper.cpp",
                    notes = "Best performance, requires Whisper.cpp installation"
                },
                new
                {
                    id = "whisper-python",
                    name = "Whisper Python",
                    description = "Official OpenAI Whisper implementation",
                    supportsGpu = true,
                    recommended = false,
                    website = "https://github.com/openai/whisper",
                    notes = "Requires Python and PyTorch"
                },
                new
                {
                    id = "faster-whisper",
                    name = "Faster Whisper",
                    description = "Optimized Whisper with CTranslate2",
                    supportsGpu = true,
                    recommended = true,
                    website = "https://github.com/guillaumekln/faster-whisper",
                    notes = "Up to 4x faster than original Whisper"
                },
                new
                {
                    id = "vosk",
                    name = "Vosk",
                    description = "Offline speech recognition",
                    supportsGpu = false,
                    recommended = false,
                    website = "https://alphacephei.com/vosk/",
                    notes = "Lightweight, works offline, lower quality"
                }
            },
            development = true,
            note = "whisper-cpp is recommended for best performance"
        });
    }

    /// <summary>
    /// List available models for speech recognition engines
    /// </summary>
    /// <param name="engine">Engine name (whisper-cpp, etc.)</param>
    /// <returns>Available models and their specifications</returns>
    [HttpGet("models")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult GetModels([FromQuery] string engine = "whisper-cpp")
    {
        // Get available models (all possible)
        var allWhisperModels = new[]
        {
            new
            {
                name = "tiny",
                size = "75 MB",
                speed = "~32x realtime",
                quality = "Fair",
                description = "Fastest, lowest quality. Good for testing.",
                recommended = false,
                downloaded = false
            },
            new
            {
                name = "base",
                size = "142 MB",
                speed = "~16x realtime",
                quality = "Good",
                description = "Fast and decent quality. Recommended for most use cases.",
                recommended = true,
                downloaded = false
            },
            new
            {
                name = "small",
                size = "465 MB",
                speed = "~6x realtime",
                quality = "Very Good",
                description = "Balanced speed and quality.",
                recommended = false,
                downloaded = false
            },
            new
            {
                name = "medium",
                size = "1.42 GB",
                speed = "~2x realtime",
                quality = "Excellent",
                description = "High quality for professional use.",
                recommended = false,
                downloaded = false
            },
            new
            {
                name = "large",
                size = "2.87 GB",
                speed = "~1x realtime",
                quality = "Best",
                description = "Highest quality, slowest. Use only when quality is critical.",
                recommended = false,
                downloaded = false
            }
        };
        
        // Check which models are actually downloaded
        var whisperModel = WhisperHelper.GetWhisperModel();
        var downloadedModels = new List<string>();
        
        if (Directory.Exists(whisperModel.ModelFolder))
        {
            var modelFiles = Directory.GetFiles(whisperModel.ModelFolder, "*.bin");
            downloadedModels = modelFiles
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(n => !n.EndsWith("$$$")) // Exclude incomplete downloads
                .ToList();
        }
        
        // Mark downloaded models
        var modelsWithStatus = allWhisperModels.Select(m => new
        {
            m.name,
            m.size,
            m.speed,
            m.quality,
            m.description,
            recommended = downloadedModels.Contains(m.name) ? m.recommended : false,
            downloaded = downloadedModels.Contains(m.name) || 
                         downloadedModels.Contains(m.name + ".en") ||
                         downloadedModels.Any(d => d.StartsWith(m.name + "-"))
        }).ToList();

        return Ok(new
        {
            engine,
            models = modelsWithStatus,
            downloaded = downloadedModels,
            modelFolder = whisperModel.ModelFolder,
            development = true,
            note = downloadedModels.Any() 
                ? $"You have {downloadedModels.Count} model(s) downloaded. Only downloaded models can be used."
                : "No models downloaded yet. Download from: https://huggingface.co/ggerganov/whisper.cpp/tree/main"
        });
    }

    /// <summary>
    /// List all transcription jobs
    /// </summary>
    /// <returns>List of all jobs</returns>
    [HttpGet("jobs")]
    [ProducesResponseType(typeof(object), 200)]
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
                language = j.Language,
                createdAt = j.CreatedAt,
                completedAt = j.CompletedAt,
                hasError = !string.IsNullOrEmpty(j.Error)
            }),
            development = true
        });
    }

    /// <summary>
    /// Health check for transcription service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult Health()
    {
        // Check if Whisper is installed
        bool whisperInstalled = false;
        string? whisperPath = null;
        
        try
        {
            whisperPath = Nikse.SubtitleEdit.Core.AudioToText.WhisperHelper.GetWhisperPathAndFileName();
            whisperInstalled = !string.IsNullOrEmpty(whisperPath) && System.IO.File.Exists(whisperPath);
        }
        catch
        {
            // Ignore errors
        }

        return Ok(new
        {
            status = "healthy",
            service = "Transcription Service",
            timestamp = DateTime.UtcNow,
            whisper = new
            {
                installed = whisperInstalled,
                path = whisperPath,
                ready = whisperInstalled
            },
            capabilities = new
            {
                maxFileSize = "2 GB",
                supportedFormats = new[] { "mp4", "mkv", "avi", "mov", "mp3", "wav", "m4a" },
                engines = new[] { "whisper-cpp", "whisper-python", "faster-whisper" },
                models = new[] { "tiny", "base", "small", "medium", "large" }
            },
            development = true,
            warning = !whisperInstalled ? "Whisper is not installed. Please install Whisper to use transcription features." : null
        });
    }

    /// <summary>
    /// Get supported translation languages (Google Translate V1 - Free, no API key required)
    /// </summary>
    /// <returns>List of supported languages for translation</returns>
    [HttpGet("languages")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult GetLanguages()
    {
        var languages = _translationService.GetSupportedLanguages();

        return Ok(new
        {
            count = languages.Count,
            languages = languages.Select(l => new
            {
                name = l.Name,
                code = l.Code
            }),
            translator = "Google Translate V1 (Free)",
            note = "Use the 'code' value for translateTo parameter. Example: 'en' for English, 'es' for Spanish, 'ar' for Arabic",
            development = true
        });
    }
}
