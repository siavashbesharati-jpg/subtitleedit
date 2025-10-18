namespace CaptionFlowApi.Models;

/// <summary>
/// Request model for speech-to-text transcription
/// </summary>
public class TranscriptionRequest
{
    /// <summary>
    /// Speech recognition engine to use
    /// </summary>
    public string Engine { get; set; } = "whisper-cpp";

    /// <summary>
    /// Model size (tiny, base, small, medium, large)
    /// </summary>
    public string Model { get; set; } = "base";

    /// <summary>
    /// Language code (auto, en, es, fr, etc.)
    /// </summary>
    public string Language { get; set; } = "auto";

    /// <summary>
    /// Translate to English
    /// </summary>
    public bool TranslateToEnglish { get; set; } = false;

    /// <summary>
    /// Translate subtitle to this language code (empty = no translation)
    /// Examples: en, es, fr, de, ar, zh-CN, etc.
    /// </summary>
    public string TranslateTo { get; set; } = "";

    /// <summary>
    /// Apply post-processing (timing fixes, punctuation, casing)
    /// </summary>
    public bool UsePostProcessing { get; set; } = true;

    /// <summary>
    /// Audio track number (0 for default)
    /// </summary>
    public int AudioTrackNumber { get; set; } = 0;
}
