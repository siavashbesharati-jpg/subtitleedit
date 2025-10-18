using CaptionFlowApi.Models;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace CaptionFlowApi.Services;

/// <summary>
/// Service for transcribing audio/video to subtitles using Whisper/Vosk
/// </summary>
public class TranscriptionService
{
    private readonly ILogger<TranscriptionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, TranscriptionJob> _jobs = new();
    private readonly string _tempDirectory;
    private readonly string _outputDirectory;
    private readonly string _ffmpegPath;

    public TranscriptionService(
        ILogger<TranscriptionService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        _tempDirectory = configuration["Transcription:TempDirectory"] 
            ?? Path.Combine(Path.GetTempPath(), "captionflow", "transcription", "temp");
        _outputDirectory = configuration["Transcription:OutputDirectory"] 
            ?? Path.Combine(Path.GetTempPath(), "captionflow", "transcription", "output");
        _ffmpegPath = configuration["General:FFmpegLocation"] 
            ?? configuration.GetValue<string>("FFmpegPath") 
            ?? "ffmpeg";

        Directory.CreateDirectory(_tempDirectory);
        Directory.CreateDirectory(_outputDirectory);

        _logger.LogInformation("TranscriptionService initialized");
        _logger.LogInformation($"Temp directory: {_tempDirectory}");
        _logger.LogInformation($"Output directory: {_outputDirectory}");
    }

    /// <summary>
    /// Create a new transcription job from uploaded file
    /// </summary>
    public async Task<TranscriptionJob> CreateTranscriptionJobAsync(
        IFormFile file,
        TranscriptionRequest request)
    {
        var jobId = Guid.NewGuid().ToString();
        var fileName = $"{jobId}_{file.FileName}";
        var filePath = Path.Combine(_tempDirectory, fileName);

        _logger.LogInformation($"Creating transcription job: {jobId}");

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

    /// <summary>
    /// Main transcription workflow
    /// </summary>
    private async Task TranscribeAsync(
        string jobId,
        string filePath,
        TranscriptionRequest request)
    {
        var job = _jobs[jobId];
        string? wavFile = null;

        try
        {
            _logger.LogInformation($"Starting transcription for job: {jobId}");

            // Step 1: Extract audio
            job.Status = "Extracting Audio";
            job.Progress = 10;
            wavFile = await ExtractAudioToWavAsync(filePath, request.AudioTrackNumber);
            job.Progress = 30;

            // Step 2: Transcribe
            job.Status = "Transcribing";
            var subtitle = await TranscribeWithEngineAsync(
                wavFile,
                request.Engine,
                request.Model,
                request.Language,
                request.TranslateToEnglish,
                (progress) => job.Progress = 30 + (int)(progress * 0.5)); // 30-80%
            
            job.Progress = 80;

            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                throw new Exception("No subtitles were generated. The audio may not contain speech.");
            }

            // Step 3: Post-process
            if (request.UsePostProcessing)
            {
                job.Status = "Post-Processing";
                subtitle = PostProcessSubtitle(subtitle, request.Engine, request.Language);
            }

            job.Progress = 90;

            // Step 4: Translation (if requested)
            if (!string.IsNullOrWhiteSpace(request.TranslateTo) && 
                request.TranslateTo.ToLowerInvariant() != "none")
            {
                job.Status = "Translating";
                job.StatusDescription = $"Translating to {request.TranslateTo}...";
                _logger.LogInformation($"Translating subtitle to: {request.TranslateTo}");

                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var translationLogger = loggerFactory.CreateLogger<Services.TranslationService>();
                var translationService = new Services.TranslationService(translationLogger);

                // Detect source language or use the transcription language
                var sourceLanguage = request.Language.ToLowerInvariant();
                if (sourceLanguage == "auto" && subtitle.Paragraphs.Any())
                {
                    sourceLanguage = translationService.DetectLanguage(
                        string.Join(" ", subtitle.Paragraphs.Take(5).Select(p => p.Text)));
                }

                subtitle = await translationService.TranslateSubtitle(
                    subtitle,
                    sourceLanguage,
                    request.TranslateTo,
                    CancellationToken.None);

                job.Progress = 95;
                _logger.LogInformation($"Translation complete");
            }

            // Step 5: Save as SRT
            var srtPath = Path.Combine(_outputDirectory, $"{jobId}.srt");
            var srtFormat = new SubRip();
            var srtContent = srtFormat.ToText(subtitle, string.Empty);
            await File.WriteAllTextAsync(srtPath, srtContent, Encoding.UTF8);

            // Step 6: Complete
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
                WordCount = subtitle.Paragraphs.Sum(p => p.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length),
                SegmentCount = subtitle.Paragraphs.Count,
                Segments = subtitle.Paragraphs.Select(p => new SubtitleSegment
                {
                    Number = p.Number,
                    StartSeconds = p.StartTime.TotalSeconds,
                    EndSeconds = p.EndTime.TotalSeconds,
                    Text = p.Text,
                    Confidence = 0.95 // Whisper doesn't provide per-segment confidence
                }).ToList()
            };

            _logger.LogInformation($"Transcription completed for job: {jobId}");
            _logger.LogInformation($"Generated {subtitle.Paragraphs.Count} subtitle segments");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Transcription failed for job: {jobId}");
            job.Status = "Failed";
            job.Error = ex.Message;
            job.Progress = 0;
        }
        finally
        {
            // Clean up temporary WAV file
            if (wavFile != null && File.Exists(wavFile))
            {
                try
                {
                    File.Delete(wavFile);
                    _logger.LogInformation($"Deleted temporary WAV file: {wavFile}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to delete temporary WAV file: {wavFile}");
                }
            }
        }
    }

    /// <summary>
    /// Extract audio from video as 16kHz mono WAV for Whisper
    /// </summary>
    private async Task<string> ExtractAudioToWavAsync(string videoPath, int audioTrack)
    {
        var wavPath = Path.Combine(_tempDirectory, $"{Guid.NewGuid()}.wav");

        _logger.LogInformation($"Extracting audio to: {wavPath}");

        // FFmpeg command to extract audio as 16kHz mono WAV
        var ffmpegArgs = $"-i \"{videoPath}\" -ar 16000 -ac 1 -map 0:a:{audioTrack} \"{wavPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var errorOutput = new StringBuilder();
        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                errorOutput.AppendLine(args.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (!File.Exists(wavPath) || process.ExitCode != 0)
        {
            var error = errorOutput.ToString();
            _logger.LogError($"FFmpeg failed: {error}");
            throw new Exception($"Failed to extract audio. FFmpeg error: {error}");
        }

        _logger.LogInformation($"Audio extracted successfully: {new FileInfo(wavPath).Length / 1024 / 1024} MB");

        return wavPath;
    }

    /// <summary>
    /// Transcribe audio using specified engine
    /// </summary>
    private async Task<Subtitle> TranscribeWithEngineAsync(
        string wavFile,
        string engine,
        string model,
        string language,
        bool translate,
        Action<double>? progressCallback = null)
    {
        _logger.LogInformation($"Transcribing with engine: {engine}, model: {model}, language: {language}");

        if (engine.Contains("whisper", StringComparison.OrdinalIgnoreCase))
        {
            return await TranscribeWithWhisperAsync(wavFile, model, language, translate, progressCallback);
        }
        else if (engine.Contains("vosk", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotImplementedException("Vosk support coming soon");
        }

        throw new NotSupportedException($"Engine not supported: {engine}");
    }

    /// <summary>
    /// Transcribe using Whisper (uses LibSE's Whisper helper)
    /// </summary>
    private async Task<Subtitle> TranscribeWithWhisperAsync(
        string wavFile,
        string model,
        string language,
        bool translate,
        Action<double>? progressCallback = null)
    {
        // Check if Whisper is installed
        var whisperPath = WhisperHelper.GetWhisperPathAndFileName();

        if (string.IsNullOrEmpty(whisperPath) || !File.Exists(whisperPath))
        {
            throw new Exception(
                "Whisper is not installed. Please install Whisper first. " +
                "Visit: https://github.com/ggerganov/whisper.cpp/releases");
        }

        _logger.LogInformation($"Using Whisper at: {whisperPath}");
        
        // Find the model file (try different variations)
        var whisperModel = WhisperHelper.GetWhisperModel();
        var modelPath = FindWhisperModelFile(whisperModel.ModelFolder, model);
        
        if (modelPath == null)
        {
            var availableModels = new List<string>();
            if (Directory.Exists(whisperModel.ModelFolder))
            {
                var modelFiles = Directory.GetFiles(whisperModel.ModelFolder, "*.bin")
                    .Where(f => !f.EndsWith(".$$$")); // Exclude incomplete downloads
                availableModels = modelFiles
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .ToList();
            }
            
            var availableList = availableModels.Any() 
                ? $"Available models: {string.Join(", ", availableModels)}" 
                : "No models downloaded yet.";
            
            throw new Exception(
                $"Model '{model}' not found.\n\n" +
                $"{availableList}\n\n" +
                $"Download models from: https://huggingface.co/ggerganov/whisper.cpp/tree/main\n" +
                $"Save to: {whisperModel.ModelFolder}");
        }
        
        _logger.LogInformation($"Using model: {modelPath}");

        // Normalize language code
        if (language.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            language = "auto";
        }
        else if (language.Equals("english", StringComparison.OrdinalIgnoreCase))
        {
            language = "en";
        }

        // Build Whisper arguments
        var translateArg = translate && !language.Equals("en", StringComparison.OrdinalIgnoreCase) 
            ? " --task translate" 
            : "";
        
        var languageArg = language.Equals("auto", StringComparison.OrdinalIgnoreCase) 
            ? "" 
            : $" --language {language}";

        // Use the full path to the model file (not just the model name)
        var args = $"--model \"{modelPath}\"{languageArg} --output-srt{translateArg} --print-progress \"{wavFile}\"";

        _logger.LogInformation($"Whisper command: {whisperPath} {args}");

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
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(whisperPath)
            }
        };

        var outputData = new StringBuilder();
        var errorData = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputData.AppendLine(e.Data);
                _logger.LogDebug($"Whisper output: {e.Data}");

                // Try to parse progress
                if (e.Data.Contains("progress"))
                {
                    progressCallback?.Invoke(0.5); // Rough estimate
                }
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorData.AppendLine(e.Data);
                _logger.LogDebug($"Whisper stderr: {e.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = errorData.ToString();
            _logger.LogError($"Whisper failed with exit code {process.ExitCode}: {error}");
            throw new Exception($"Whisper transcription failed: {error}");
        }

        progressCallback?.Invoke(1.0);

        // Read generated SRT file
        var srtFile = wavFile + ".srt";
        if (!File.Exists(srtFile))
        {
            // Try alternative naming (without .wav extension)
            srtFile = Path.ChangeExtension(wavFile, ".srt");
        }

        if (!File.Exists(srtFile))
        {
            _logger.LogError($"Whisper did not generate SRT file. Expected: {srtFile}");
            throw new Exception("Whisper did not generate SRT file");
        }

        _logger.LogInformation($"Reading SRT file: {srtFile}");

        // Parse SRT using LibSE
        var subtitle = new Subtitle();
        var srtFormat = new SubRip();
        var srtLines = await File.ReadAllLinesAsync(srtFile);
        srtFormat.LoadSubtitle(subtitle, srtLines.ToList(), srtFile);

        // Clean up generated SRT file
        try
        {
            File.Delete(srtFile);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to delete temporary SRT file: {srtFile}");
        }

        _logger.LogInformation($"Parsed {subtitle.Paragraphs.Count} subtitle paragraphs");

        return subtitle;
    }

    /// <summary>
    /// Apply post-processing to improve subtitle quality
    /// </summary>
    private Subtitle PostProcessSubtitle(Subtitle subtitle, string engine, string language)
    {
        _logger.LogInformation("Applying post-processing");

        // Get two-letter language code
        var twoLetterCode = GetTwoLetterLanguageCode(language);
        
        var processor = new AudioToTextPostProcessor(twoLetterCode);

        var engineType = engine.Contains("whisper", StringComparison.OrdinalIgnoreCase)
            ? AudioToTextPostProcessor.Engine.Whisper
            : AudioToTextPostProcessor.Engine.Vosk;

        var processed = processor.Fix(
            engineType,
            subtitle,
            usePostProcessing: true,
            addPeriods: true,
            mergeLines: true,
            fixCasing: true,
            fixShortDuration: true,
            splitLines: false);

        _logger.LogInformation("Post-processing completed");

        return processed;
    }

    /// <summary>
    /// Convert language code to two-letter ISO code
    /// </summary>
    private string GetTwoLetterLanguageCode(string language)
    {
        // Common mappings
        var languageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "auto", "en" },
            { "english", "en" },
            { "spanish", "es" },
            { "french", "fr" },
            { "german", "de" },
            { "italian", "it" },
            { "portuguese", "pt" },
            { "russian", "ru" },
            { "japanese", "ja" },
            { "chinese", "zh" },
            { "korean", "ko" },
            { "arabic", "ar" },
            { "danish", "da" },
            { "dutch", "nl" },
            { "norwegian", "no" }
        };

        if (languageMap.TryGetValue(language, out var code))
        {
            return code;
        }

        // If already a two-letter code, return as-is
        if (language.Length == 2)
        {
            return language.ToLowerInvariant();
        }

        // Default to English
        return "en";
    }

    /// <summary>
    /// Get job by ID
    /// </summary>
    public TranscriptionJob? GetJob(string jobId)
    {
        return _jobs.TryGetValue(jobId, out var job) ? job : null;
    }

    /// <summary>
    /// Get all jobs
    /// </summary>
    public List<TranscriptionJob> GetAllJobs()
    {
        return _jobs.Values.OrderByDescending(j => j.CreatedAt).ToList();
    }

    /// <summary>
    /// Clean up old jobs and files
    /// </summary>
    public void CleanupOldJobs(TimeSpan maxAge)
    {
        var cutoffTime = DateTime.UtcNow - maxAge;
        var oldJobs = _jobs.Values.Where(j => j.CreatedAt < cutoffTime).ToList();

        foreach (var job in oldJobs)
        {
            try
            {
                // Delete input file
                if (File.Exists(job.FilePath))
                {
                    File.Delete(job.FilePath);
                }

                // Delete output file
                if (!string.IsNullOrEmpty(job.OutputPath) && File.Exists(job.OutputPath))
                {
                    File.Delete(job.OutputPath);
                }

                // Remove from dictionary
                _jobs.TryRemove(job.JobId, out _);

                _logger.LogInformation($"Cleaned up old job: {job.JobId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to cleanup job: {job.JobId}");
            }
        }
    }

    /// <summary>
    /// Find Whisper model file with fallback logic
    /// Tries: model.bin, model.en.bin, model-q5_0.bin, model-q5_1.bin
    /// </summary>
    private string? FindWhisperModelFile(string modelFolder, string modelName)
    {
        if (!Directory.Exists(modelFolder))
        {
            return null;
        }

        // Try different model file variations
        var variations = new[]
        {
            $"{modelName}.bin",              // tiny.bin
            $"{modelName}.en.bin",           // tiny.en.bin (English-only)
            $"{modelName}-q5_0.bin",         // tiny-q5_0.bin (quantized)
            $"{modelName}-q5_1.bin",         // tiny-q5_1.bin (quantized)
            $"{modelName}.en-q5_0.bin",      // tiny.en-q5_0.bin
            $"{modelName}.en-q5_1.bin",      // tiny.en-q5_1.bin
        };

        foreach (var variation in variations)
        {
            var fullPath = Path.Combine(modelFolder, variation);
            if (File.Exists(fullPath))
            {
                _logger.LogInformation($"Found model variant: {variation}");
                return fullPath;
            }
        }

        // Try wildcards for other variations
        var pattern = $"{modelName}*.bin";
        var matchingFiles = Directory.GetFiles(modelFolder, pattern)
            .Where(f => !f.EndsWith(".$$$")) // Exclude incomplete downloads
            .OrderBy(f => f.Length) // Prefer shorter names (less suffix)
            .FirstOrDefault();

        if (matchingFiles != null)
        {
            _logger.LogInformation($"Found model match: {Path.GetFileName(matchingFiles)}");
            return matchingFiles;
        }

        return null;
    }
}
