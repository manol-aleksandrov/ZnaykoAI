namespace ZnaykoAI.Services.OpenRouter;

public interface IQuizPromptProvider
{
    QuizPrompts BuildPrompts(
        int grade,
        string subject,
        string subTopic,
        int questionCount,
        int answersPerQuestion);
}

public sealed record QuizPrompts(string System, string User);
