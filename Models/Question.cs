namespace ZnaykoAI.Models;

public class Question
{
    public Guid Id { get; set; }

    public Guid TestSheetId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public int Points { get; set; }

    public TestSheet TestSheet { get; set; } = null!;
}
