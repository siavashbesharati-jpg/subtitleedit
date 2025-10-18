using CaptionFlowApi.Models;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Concurrent;
using System.Text;

namespace CaptionFlowApi.Services;

/// <summary>
/// Service for processing video files and extracting subtitles
/// </summary>
public class SubtitleProcessingService
{
    private readonly ConcurrentDictionary<string, SubtitleJob> _jobs = new();
    private readonly string _uploadDirectory;
    private readonly string _outputDirectory;
    private readonly ILogger<SubtitleProcessingService> _logger;

    public SubtitleProcessingService(ILogger<SubtitleProcessingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _uploadDirectory = configuration["FileStorage:UploadDirectory"] ?? Path.Combine(Path.GetTempPath(), "captionflow", "uploads");
        _outputDirectory = configuration["FileStorage:OutputDirectory"] ?? Path.Combine(Path.GetTempPath(), "captionflow", "outputs");

        Directory.CreateDirectory(_uploadDirectory);
        Directory.CreateDirectory(_outputDirectory);
    }

    /// <summary>
    /// Creates a new subtitle extraction job
    /// </summary>
    public async Task<SubtitleJob> CreateJobAsync(IFormFile videoFile, SubtitleExtractionRequest request)
    {
        var jobId = Guid.NewGuid().ToString();
        var videoFileName = $"{jobId}_{videoFile.FileName}";
        var videoFilePath = Path.Combine(_uploadDirectory, videoFileName);

        // Save uploaded video file
        using (var stream = new FileStream(videoFilePath, FileMode.Create))
        {
            await videoFile.CopyToAsync(stream);
        }

        var job = new SubtitleJob
        {
            JobId = jobId,
            FileName = videoFile.FileName,
            VideoFilePath = videoFilePath,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;

        // Start processing in background
        _ = Task.Run(() => ProcessJobAsync(jobId, request));

        return job;
    }

    /// <summary>
    /// Gets the status of a subtitle job
    /// </summary>
    public SubtitleJob? GetJob(string jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return job;
    }

    /// <summary>
    /// Gets all jobs (for debugging/admin purposes)
    /// </summary>
    public IEnumerable<SubtitleJob> GetAllJobs()
    {
        return _jobs.Values.OrderByDescending(j => j.CreatedAt);
    }

    /// <summary>
    /// Processes a subtitle extraction job
    /// </summary>
    private async Task ProcessJobAsync(string jobId, SubtitleExtractionRequest request)
    {
        var job = _jobs[jobId];
        
        try
        {
            job.Status = "Processing";
            _logger.LogInformation("Starting processing for job {JobId}", jobId);

            // Check if video file has embedded subtitles
            var subtitle = await Task.Run(() => ExtractSubtitlesFromVideo(job.VideoFilePath, request));

            if (subtitle != null && subtitle.Paragraphs.Count > 0)
            {
                job.SubtitleCount = subtitle.Paragraphs.Count;

                // Generate SRT file
                var srtFileName = $"{jobId}.srt";
                var srtFilePath = Path.Combine(_outputDirectory, srtFileName);
                
                var format = new SubRip();
                var srtContent = subtitle.ToText(format);
                await File.WriteAllTextAsync(srtFilePath, srtContent, Encoding.UTF8);

                job.SrtFilePath = srtFilePath;
                job.Status = "Completed";
                job.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation("Job {JobId} completed successfully with {Count} subtitles", jobId, job.SubtitleCount);
            }
            else
            {
                job.Status = "Failed";
                job.ErrorMessage = "No subtitles found in the video file. The video may not contain embedded subtitles.";
                _logger.LogWarning("Job {JobId} failed: No subtitles found", jobId);
            }
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Job {JobId} failed with error", jobId);
        }
    }

    /// <summary>
    /// Extracts subtitles from a video file
    /// </summary>
    private Subtitle? ExtractSubtitlesFromVideo(string videoFilePath, SubtitleExtractionRequest request)
    {
        try
        {
            var subtitle = new Subtitle();

            // Try to extract embedded subtitles from Matroska/MKV files
            var matroskaFile = new Nikse.SubtitleEdit.Core.ContainerFormats.Matroska.MatroskaFile(videoFilePath);
            
            if (matroskaFile.IsValid)
            {
                var tracks = matroskaFile.GetTracks(subtitleOnly: true);
                var subtitleTrack = tracks.FirstOrDefault(t => t.IsSubtitle);
                
                if (subtitleTrack != null)
                {
                    var matroskaSubtitles = matroskaFile.GetSubtitle(subtitleTrack.TrackNumber, null);
                    
                    if (matroskaSubtitles != null && matroskaSubtitles.Count > 0)
                    {
                        // Convert MatroskaSubtitle to Subtitle paragraphs
                        foreach (var matSub in matroskaSubtitles)
                        {
                            var data = matSub.GetData(subtitleTrack);
                            if (data != null && data.Length > 0)
                            {
                                var text = Encoding.UTF8.GetString(data).Trim();
                                if (!string.IsNullOrEmpty(text))
                                {
                                    subtitle.Paragraphs.Add(new Paragraph
                                    {
                                        StartTime = new TimeCode(matSub.Start),
                                        EndTime = new TimeCode(matSub.Start + matSub.Duration),
                                        Text = text
                                    });
                                }
                            }
                        }
                        
                        if (subtitle.Paragraphs.Count > 0)
                        {
                            _logger.LogInformation("Found {Count} subtitles in Matroska track", subtitle.Paragraphs.Count);
                            subtitle.Renumber();
                            return subtitle;
                        }
                    }
                }
                
                matroskaFile.Dispose();
            }

            // Try MP4 subtitles
            try
            {
                var mp4 = new Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.MP4Parser(videoFilePath);
                var mp4SubtitleTracks = mp4.GetSubtitleTracks();
                
                if (mp4SubtitleTracks != null && mp4SubtitleTracks.Count > 0)
                {
                    // Get paragraphs from first subtitle track
                    var paragraphs = mp4SubtitleTracks[0].Mdia?.Minf?.Stbl?.GetParagraphs();
                    
                    if (paragraphs != null && paragraphs.Count > 0)
                    {
                        foreach (var para in paragraphs)
                        {
                            subtitle.Paragraphs.Add(para);
                        }
                        
                        _logger.LogInformation("Found {Count} subtitles in MP4", subtitle.Paragraphs.Count);
                        subtitle.Renumber();
                        return subtitle;
                    }
                }
            }
            catch (Exception mp4Ex)
            {
                _logger.LogDebug(mp4Ex, "No MP4 subtitles found");
            }

            _logger.LogWarning("No embedded subtitles found in video file");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting subtitles from video");
            throw;
        }
    }

    /// <summary>
    /// Cleans up old job files
    /// </summary>
    public void CleanupOldJobs(int olderThanHours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-olderThanHours);
        var jobsToRemove = _jobs.Values.Where(j => j.CreatedAt < cutoffTime).ToList();

        foreach (var job in jobsToRemove)
        {
            try
            {
                if (File.Exists(job.VideoFilePath))
                {
                    File.Delete(job.VideoFilePath);
                }

                if (!string.IsNullOrEmpty(job.SrtFilePath) && File.Exists(job.SrtFilePath))
                {
                    File.Delete(job.SrtFilePath);
                }

                _jobs.TryRemove(job.JobId, out _);
                _logger.LogInformation("Cleaned up old job {JobId}", job.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up job {JobId}", job.JobId);
            }
        }
    }
}
