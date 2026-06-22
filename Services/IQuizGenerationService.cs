using ZnaykoAI.Models;

namespace ZnaykoAI.Services;

public interface IQuizGenerationService
{
    Task<TestSheet> GenerateQuizAsync(
        string userId,
        int grade,
        string subject,
        string subTopic,
        int questionCount,
        int answersPerQuestion,
        CancellationToken cancellationToken = default);
}
