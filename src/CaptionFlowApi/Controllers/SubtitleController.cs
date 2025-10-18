using CaptionFlowApi.Models;
using CaptionFlowApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CaptionFlowApi.Controllers;

/// <summary>
/// API Controller for subtitle extraction and export operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubtitleController : ControllerBase
{
    private readonly SubtitleProcessingService _processingService;
    private readonly ILogger<SubtitleController> _logger;

    public SubtitleController(SubtitleProcessingService processingService, ILogger<SubtitleController> logger)
    {
        _processingService = processingService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a video file and extract subtitles
    /// </summary>
    /// <param name="videoFile">The video file to process (MP4, MKV, AVI, etc.)</param>
    /// <param name="sourceLanguage">Optional source language code</param>
    /// <param name="useOcr">Whether to use OCR for subtitle extraction</param>
    /// <returns>Job information with status and job ID</returns>
    /// <response code="200">Returns the newly created job</response>
    /// <response code="400">If the video file is invalid</response>
    [HttpPost("extract")]
    [ProducesResponseType(typeof(SubtitleExtractionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubtitleExtractionResponse>> ExtractSubtitles(
        IFormFile videoFile,
        [FromForm] string? sourceLanguage = null,
        [FromForm] bool useOcr = false)
    {
        if (videoFile == null || videoFile.Length == 0)
        {
            return BadRequest(new { error = "No video file provided or file is empty" });
        }

        // Validate file extension
        var allowedExtensions = new[] { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".ts" };
        var fileExtension = Path.GetExtension(videoFile.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { error = $"Unsupported video format. Allowed formats: {string.Join(", ", allowedExtensions)}" });
        }

        // Validate file size (max 500MB)
        const long maxFileSize = 500 * 1024 * 1024;
        if (videoFile.Length > maxFileSize)
        {
            return BadRequest(new { error = "Video file is too large. Maximum size is 500MB" });
        }

        try
        {
            var request = new SubtitleExtractionRequest
            {
                SourceLanguage = sourceLanguage,
                UseOcr = useOcr,
                OutputFormat = "srt"
            };

            var job = await _processingService.CreateJobAsync(videoFile, request);

            var response = new SubtitleExtractionResponse
            {
                JobId = job.JobId,
                Status = job.Status,
                Message = "Video uploaded successfully. Processing subtitles...",
                FileName = job.FileName,
                SubtitleCount = job.SubtitleCount,
                DownloadUrl = job.Status == "Completed" ? Url.Action(nameof(DownloadSrt), new { jobId = job.JobId }) : null
            };

            _logger.LogInformation("Created subtitle extraction job {JobId} for file {FileName}", job.JobId, videoFile.FileName);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subtitle extraction job");
            return StatusCode(500, new { error = "An error occurred while processing the video file", details = ex.Message });
        }
    }

    /// <summary>
    /// Get the status of a subtitle extraction job
    /// </summary>
    /// <param name="jobId">The job ID returned from the extract endpoint</param>
    /// <returns>Current job status and information</returns>
    /// <response code="200">Returns the job status</response>
    /// <response code="404">If the job ID is not found</response>
    [HttpGet("status/{jobId}")]
    [ProducesResponseType(typeof(SubtitleExtractionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<SubtitleExtractionResponse> GetStatus(string jobId)
    {
        var job = _processingService.GetJob(jobId);

        if (job == null)
        {
            return NotFound(new { error = "Job not found" });
        }

        var response = new SubtitleExtractionResponse
        {
            JobId = job.JobId,
            Status = job.Status,
            Message = job.Status == "Failed" ? job.ErrorMessage ?? "Processing failed" : 
                     job.Status == "Completed" ? "Subtitles extracted successfully" : 
                     "Processing in progress...",
            FileName = job.FileName,
            SubtitleCount = job.SubtitleCount,
            DownloadUrl = job.Status == "Completed" ? Url.Action(nameof(DownloadSrt), new { jobId = job.JobId }) : null
        };

        return Ok(response);
    }

    /// <summary>
    /// Download the extracted SRT subtitle file
    /// </summary>
    /// <param name="jobId">The job ID</param>
    /// <returns>The SRT file</returns>
    /// <response code="200">Returns the SRT file</response>
    /// <response code="404">If the job or file is not found</response>
    [HttpGet("download/{jobId}/srt")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("text/plain", "application/x-subrip")]
    public IActionResult DownloadSrt(string jobId)
    {
        var job = _processingService.GetJob(jobId);

        if (job == null)
        {
            return NotFound(new { error = "Job not found" });
        }

        if (job.Status != "Completed" || string.IsNullOrEmpty(job.SrtFilePath))
        {
            return NotFound(new { error = "SRT file not available. Job status: " + job.Status });
        }

        if (!System.IO.File.Exists(job.SrtFilePath))
        {
            return NotFound(new { error = "SRT file not found on server" });
        }

        var fileBytes = System.IO.File.ReadAllBytes(job.SrtFilePath);
        var fileName = Path.GetFileNameWithoutExtension(job.FileName) + ".srt";

        return File(fileBytes, "application/x-subrip", fileName);
    }

    /// <summary>
    /// Get all jobs (for debugging/monitoring)
    /// </summary>
    /// <returns>List of all jobs</returns>
    [HttpGet("jobs")]
    [ProducesResponseType(typeof(IEnumerable<SubtitleJob>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<SubtitleJob>> GetAllJobs()
    {
        var jobs = _processingService.GetAllJobs();
        return Ok(jobs);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow, service = "CaptionFlow API" });
    }
}
