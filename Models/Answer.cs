namespace ZnaykoAI.Models;

public class Answer
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public string AnswerText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int SortOrder { get; set; }

    public Question Question { get; set; } = null!;
}
