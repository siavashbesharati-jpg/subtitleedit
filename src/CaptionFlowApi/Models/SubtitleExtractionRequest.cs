namespace CaptionFlowApi.Models;

/// <summary>
/// Request model for subtitle extraction from video
/// </summary>
public class SubtitleExtractionRequest
{
    /// <summary>
    /// Source language code (e.g., "en", "es", "fr")
    /// </summary>
    public string? SourceLanguage { get; set; }

    /// <summary>
    /// Whether to perform OCR on embedded subtitles
    /// </summary>
    public bool UseOcr { get; set; } = false;

    /// <summary>
    /// Target format for subtitle extraction (default: SRT)
    /// </summary>
    public string OutputFormat { get; set; } = "srt";
}
