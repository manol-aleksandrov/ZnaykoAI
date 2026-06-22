using Microsoft.Extensions.Options;
using ZnaykoAI.Services.Exceptions;

namespace ZnaykoAI.Services.OpenRouter;

public class QuizPromptProvider(
    IWebHostEnvironment environment,
    IOptions<OpenRouterOptions> options,
    ILogger<QuizPromptProvider> logger) : IQuizPromptProvider
{
    private const string SystemSectionMarker = "## System Prompt";
    private const string UserSectionMarker = "## User Prompt";

    private readonly OpenRouterOptions _options = options.Value;

    public QuizPrompts BuildPrompts(
        int grade,
        string subject,
        string subTopic,
        int questionCount,
        int answersPerQuestion)
    {
        var template = LoadTemplate();
        var (systemTemplate, userTemplate) = ParseSections(template);

        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Grade"] = grade.ToString(),
            ["Subject"] = subject.Trim(),
            ["SubTopic"] = subTopic.Trim(),
            ["QuestionCount"] = questionCount.ToString(),
            ["AnswersPerQuestion"] = answersPerQuestion.ToString(),
            ["MaxCorrectIndex"] = (answersPerQuestion - 1).ToString()
        };

        return new QuizPrompts(
            ApplyReplacements(systemTemplate, replacements),
            ApplyReplacements(userTemplate, replacements));
    }

    private string LoadTemplate()
    {
        var relativePath = string.IsNullOrWhiteSpace(_options.PromptFilePath)
            ? "Prompts/quiz-generation.md"
            : _options.PromptFilePath;

        var fullPath = Path.IsPathRooted(relativePath)
            ? relativePath
            : Path.Combine(environment.ContentRootPath, relativePath);

        if (!File.Exists(fullPath))
        {
            logger.LogError("Quiz prompt file not found at {Path}", fullPath);
            throw new QuizGenerationException(
                $"Quiz prompt file was not found at '{relativePath}'. Check OpenRouter:PromptFilePath in configuration.");
        }

        return File.ReadAllText(fullPath);
    }

    private static (string System, string User) ParseSections(string template)
    {
        var systemIndex = template.IndexOf(SystemSectionMarker, StringComparison.OrdinalIgnoreCase);
        var userIndex = template.IndexOf(UserSectionMarker, StringComparison.OrdinalIgnoreCase);

        if (systemIndex < 0 || userIndex < 0 || userIndex <= systemIndex)
        {
            throw new QuizGenerationException(
                $"Quiz prompt file must contain '{SystemSectionMarker}' and '{UserSectionMarker}' sections.");
        }

        var systemTemplate = template[(systemIndex + SystemSectionMarker.Length)..userIndex].Trim();
        var userTemplate = template[(userIndex + UserSectionMarker.Length)..].Trim();

        if (string.IsNullOrWhiteSpace(systemTemplate) || string.IsNullOrWhiteSpace(userTemplate))
        {
            throw new QuizGenerationException("Quiz prompt file contains empty system or user sections.");
        }

        return (systemTemplate, userTemplate);
    }

    private static string ApplyReplacements(string template, IReadOnlyDictionary<string, string> replacements)
    {
        var result = template;

        foreach (var (key, value) in replacements)
        {
            result = result.Replace($"{{{{{key}}}}}", value, StringComparison.OrdinalIgnoreCase);
        }

        return result.Trim();
    }
}
