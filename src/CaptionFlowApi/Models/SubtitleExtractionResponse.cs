namespace CaptionFlowApi.Models;

/// <summary>
/// Response model for subtitle extraction
/// </summary>
public class SubtitleExtractionResponse
{
    /// <summary>
    /// Unique identifier for the subtitle job
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Status of the subtitle extraction (Pending, Processing, Completed, Failed)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Message describing the current status or any errors
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Original filename of the uploaded video
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Number of subtitle entries extracted
    /// </summary>
    public int SubtitleCount { get; set; }

    /// <summary>
    /// URL to download the SRT file (when status is Completed)
    /// </summary>
    public string? DownloadUrl { get; set; }
}
