using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OpenRouter.NET;
using OpenRouter.NET.Models;
using ZnaykoAI.Models;
using ZnaykoAI.Services.Exceptions;

namespace ZnaykoAI.Services.OpenRouter;

public class OpenRouterQuizService(
    IOptions<OpenRouterOptions> options,
    IQuizPromptProvider promptProvider,
    ILogger<OpenRouterQuizService> logger) : IQuizGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly OpenRouterOptions _options = options.Value;

    public async Task<TestSheet> GenerateQuizAsync(
        string userId,
        int grade,
        string subject,
        string subTopic,
        int questionCount,
        int answersPerQuestion,
        CancellationToken cancellationToken = default)
    {
        ValidateParameters(userId, grade, subject, subTopic, questionCount, answersPerQuestion);

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new QuizGenerationException(
                "OpenRouter API key is not configured. Set OpenRouter:ApiKey in user secrets or environment variables.");
        }

        var rawContent = await CallOpenRouterAsync(
            grade,
            subject,
            subTopic,
            questionCount,
            answersPerQuestion,
            cancellationToken);

        var quizDto = DeserializeQuiz(rawContent);
        ValidateQuizDto(quizDto, questionCount, answersPerQuestion);

        return MapToTestSheet(quizDto, userId, grade, subject, subTopic, answersPerQuestion);
    }

    private async Task<string> CallOpenRouterAsync(
        int grade,
        string subject,
        string subTopic,
        int questionCount,
        int answersPerQuestion,
        CancellationToken cancellationToken)
    {
        using var client = CreateClient();

        var prompts = promptProvider.BuildPrompts(
            grade,
            subject,
            subTopic,
            questionCount,
            answersPerQuestion);

        var request = new ChatCompletionRequest
        {
            Model = _options.Model,
            Temperature = (float)_options.Temperature,
            MaxTokens = _options.MaxTokens,
            Messages =
            [
                Message.FromSystem(prompts.System),
                Message.FromUser(prompts.User)
            ]
        };

        if (!string.IsNullOrWhiteSpace(_options.ReasoningEffort))
        {
            request.Reasoning = new ReasoningConfig
            {
                Effort = _options.ReasoningEffort,
                Enabled = true,
                Exclude = true
            };
        }

        ChatCompletionResponse response;
        try
        {
            response = await client.CreateChatCompletionAsync(request, cancellationToken);
        }
        catch (OpenRouterException ex)
        {
            logger.LogWarning(ex, "OpenRouter API call failed.");
            throw new QuizGenerationException(
                "The AI service returned an error. Please try again later.",
                ex);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "OpenRouter HTTP request failed.");
            throw new QuizGenerationException(
                "Unable to reach the AI service. Please check your connection and try again.",
                ex);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogDebug("OpenRouter request was cancelled.");
            throw;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "OpenRouter request timed out.");
            throw new QuizGenerationException("The AI service timed out. Please try again.", ex);
        }

        var content = response.Choices?
            .FirstOrDefault()?.Message?.Content?
            .ToString();

        if (string.IsNullOrWhiteSpace(content))
        {
            logger.LogWarning("OpenRouter returned an empty chat completion.");
            throw new QuizGenerationException(
                "The AI service returned an unexpected response. Please try again.");
        }

        return StripMarkdownFences(content);
    }

    private OpenRouterClient CreateClient() =>
        new(new OpenRouterClientOptions
        {
            ApiKey = _options.ApiKey,
            BaseUrl = _options.BaseUrl.TrimEnd('/'),
            SiteUrl = _options.HttpReferer,
            SiteName = _options.SiteTitle
        });

    private QuizResponseDto DeserializeQuiz(string rawContent)
    {
        try
        {
            return JsonSerializer.Deserialize<QuizResponseDto>(rawContent, JsonOptions)
                ?? throw new JsonException("Deserialized quiz was null.");
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "LLM returned invalid JSON. Raw: {Raw}", rawContent);
            throw new QuizGenerationException(
                "The AI response could not be parsed. Please try again.",
                ex);
        }
    }

    private static void ValidateParameters(
        string userId,
        int grade,
        string subject,
        string subTopic,
        int questionCount,
        int answersPerQuestion)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        if (grade is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(grade), "Grade must be between 1 and 12.");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject is required.", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(subTopic))
        {
            throw new ArgumentException("Sub-topic is required.", nameof(subTopic));
        }

        if (questionCount is < 1 or > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(questionCount), "Question count must be between 1 and 50.");
        }

        if (answersPerQuestion is < 2 or > 6)
        {
            throw new ArgumentOutOfRangeException(
                nameof(answersPerQuestion),
                "Answers per question must be between 2 and 6.");
        }
    }

    private static void ValidateQuizDto(QuizResponseDto quiz, int questionCount, int answersPerQuestion)
    {
        if (string.IsNullOrWhiteSpace(quiz.Title))
        {
            throw new QuizGenerationException("The AI response did not include a quiz title.");
        }

        if (quiz.Questions is null || quiz.Questions.Count != questionCount)
        {
            throw new QuizGenerationException(
                $"The AI response contained {quiz.Questions?.Count ?? 0} questions, but {questionCount} were requested.");
        }

        for (var i = 0; i < quiz.Questions.Count; i++)
        {
            var question = quiz.Questions[i];

            if (string.IsNullOrWhiteSpace(question.Text))
            {
                throw new QuizGenerationException($"Question {i + 1} is missing text.");
            }

            if (question.Options is null || question.Options.Count != answersPerQuestion)
            {
                throw new QuizGenerationException(
                    $"Question {i + 1} must have exactly {answersPerQuestion} answer options.");
            }

            if (question.Options.Any(string.IsNullOrWhiteSpace))
            {
                throw new QuizGenerationException($"Question {i + 1} contains empty answer options.");
            }

            if (question.CorrectIndex < 0 || question.CorrectIndex >= answersPerQuestion)
            {
                throw new QuizGenerationException(
                    $"Question {i + 1} has an invalid correctIndex ({question.CorrectIndex}).");
            }
        }
    }

    private static TestSheet MapToTestSheet(
        QuizResponseDto quiz,
        string userId,
        int grade,
        string subject,
        string subTopic,
        int answersPerQuestion)
    {
        var testSheetId = Guid.NewGuid();
        var testSheet = new TestSheet
        {
            Id = testSheetId,
            UserId = userId,
            Grade = grade,
            Subject = subject.Trim(),
            SubTopic = subTopic.Trim(),
            Title = quiz.Title.Trim(),
            AnswerCount = answersPerQuestion,
            CreatedOn = DateTime.UtcNow
        };

        foreach (var questionDto in quiz.Questions!)
        {
            var questionId = Guid.NewGuid();
            var question = new Question
            {
                Id = questionId,
                TestSheetId = testSheetId,
                QuestionText = questionDto.Text.Trim(),
                Points = 1
            };

            for (var i = 0; i < questionDto.Options!.Count; i++)
            {
                question.Answers.Add(new Answer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = questionId,
                    AnswerText = questionDto.Options[i].Trim(),
                    IsCorrect = i == questionDto.CorrectIndex,
                    SortOrder = i
                });
            }

            testSheet.Questions.Add(question);
        }

        return testSheet;
    }

    private static string StripMarkdownFences(string content)
    {
        var trimmed = content.Trim();

        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewLine = trimmed.IndexOf('\n');
        if (firstNewLine < 0)
        {
            return trimmed;
        }

        var withoutOpening = trimmed[(firstNewLine + 1)..];
        var closingFence = withoutOpening.LastIndexOf("```", StringComparison.Ordinal);

        return closingFence < 0
            ? withoutOpening.Trim()
            : withoutOpening[..closingFence].Trim();
    }

    private sealed class QuizResponseDto
    {
        public string Title { get; set; } = string.Empty;

        public List<QuizQuestionDto>? Questions { get; set; }
    }

    private sealed class QuizQuestionDto
    {
        public string Text { get; set; } = string.Empty;

        public List<string>? Options { get; set; }

        [JsonPropertyName("correctIndex")]
        public int CorrectIndex { get; set; }
    }
}
