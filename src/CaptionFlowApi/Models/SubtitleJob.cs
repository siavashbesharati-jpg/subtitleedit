namespace CaptionFlowApi.Models;

/// <summary>
/// Model representing a subtitle job
/// </summary>
public class SubtitleJob
{
    public string JobId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string VideoFilePath { get; set; } = string.Empty;
    public string? SrtFilePath { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public int SubtitleCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
