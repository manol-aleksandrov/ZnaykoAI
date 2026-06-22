namespace ZnaykoAI.Services.Exceptions;

public class QuizGenerationException : Exception
{
    public QuizGenerationException(string message)
        : base(message)
    {
    }

    public QuizGenerationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
