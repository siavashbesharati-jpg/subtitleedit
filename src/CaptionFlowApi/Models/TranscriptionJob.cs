namespace CaptionFlowApi.Models;

/// <summary>
/// Transcription job status and details
/// </summary>
public class TranscriptionJob
{
    public string JobId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Status { get; set; } = "Queued"; // Queued, Extracting Audio, Transcribing, Post-Processing, Translating, Completed, Failed
    public string StatusDescription { get; set; } = "Waiting to start..."; // Detailed description of current status
    public int Progress { get; set; } = 0; // 0-100
    public string Engine { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty; // Main output (original or translated)
    public string? OriginalOutputPath { get; set; } // Path to original subtitle
    public string? TranslatedOutputPath { get; set; } // Path to translated subtitle (if translation was requested)
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TranscriptionResult? Result { get; set; }
}

/// <summary>
/// Complete transcription result with segments
/// </summary>
public class TranscriptionResult
{
    public string JobId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty; // Full transcript
    public List<SubtitleSegment> Segments { get; set; } = new();
    public string SrtContent { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int WordCount { get; set; }
    public int SegmentCount { get; set; }
}

/// <summary>
/// Individual subtitle segment
/// </summary>
public class SubtitleSegment
{
    public int Number { get; set; }
    public double StartSeconds { get; set; }
    public double EndSeconds { get; set; }
    public string Text { get; set; } = string.Empty;
    public double Confidence { get; set; }
}
