namespace ZnaykoAI.Services.OpenRouter;

public class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    public string Model { get; set; } = "google/gemini-2.5-flash-preview";

    public double Temperature { get; set; } = 0.7;

    public int MaxTokens { get; set; } = 4096;

    public string? HttpReferer { get; set; }

    public string? SiteTitle { get; set; }

    /// <summary>
    /// Relative path (from content root) or absolute path to the quiz prompt markdown file.
    /// </summary>
    public string PromptFilePath { get; set; } = "Prompts/quiz-generation.md";

    /// <summary>
    /// Optional reasoning effort for supported models (e.g. "low", "medium", "high").
    /// </summary>
    public string? ReasoningEffort { get; set; }
}
