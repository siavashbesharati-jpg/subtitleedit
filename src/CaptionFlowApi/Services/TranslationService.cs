using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;

namespace CaptionFlowApi.Services;

/// <summary>
/// Service for translating subtitles using Google Translate V1 (free, no API key required)
/// </summary>
public class TranslationService
{
    private readonly ILogger<TranslationService> _logger;
    private readonly GoogleTranslateV1 _translator;

    public TranslationService(ILogger<TranslationService> logger)
    {
        _logger = logger;
        _translator = new GoogleTranslateV1();
        _translator.Initialize();
    }

    /// <summary>
    /// Translate subtitle to target language
    /// </summary>
    public async Task<Subtitle> TranslateSubtitle(
        Subtitle subtitle,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Translating subtitle from {sourceLanguage} to {targetLanguage}");

        var translated = new Subtitle();
        var totalParagraphs = subtitle.Paragraphs.Count;
        var translatedCount = 0;

        foreach (var paragraph in subtitle.Paragraphs)
        {
            try
            {
                var translatedText = await _translator.Translate(
                    paragraph.Text,
                    sourceLanguage,
                    targetLanguage,
                    cancellationToken);

                var newParagraph = new Paragraph(paragraph)
                {
                    Text = translatedText
                };

                translated.Paragraphs.Add(newParagraph);
                translatedCount++;

                _logger.LogDebug($"Translated {translatedCount}/{totalParagraphs}: {paragraph.Text} -> {translatedText}");

                // Small delay to avoid rate limiting
                if (translatedCount < totalParagraphs)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to translate paragraph: {paragraph.Text}");
                
                // Keep original text if translation fails
                translated.Paragraphs.Add(new Paragraph(paragraph));
            }
        }

        translated.Renumber();
        _logger.LogInformation($"Translation complete: {translatedCount}/{totalParagraphs} paragraphs");

        return translated;
    }

    /// <summary>
    /// Get list of supported languages
    /// </summary>
    public List<(string Name, string Code)> GetSupportedLanguages()
    {
        var pairs = GoogleTranslateV1.GetTranslationPairs();
        return pairs
            .Select(p => (p.Name, p.Code))
            .OrderBy(p => p.Name)
            .ToList();
    }

    /// <summary>
    /// Detect language of text
    /// </summary>
    public string DetectLanguage(string text)
    {
        // Use LibSE's language detection
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 1000));
        var detected = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        _logger.LogInformation($"Detected language: {detected}");
        return detected ?? "en";
    }
}
