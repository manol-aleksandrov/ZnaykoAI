using System.ComponentModel.DataAnnotations;

namespace ZnaykoAI.Pages.Quizzes;

public class GenerateQuizInput
{
    [Display(Name = "Grade")]
    [Range(1, 12, ErrorMessage = "Grade must be between 1 and 12.")]
    public int Grade { get; set; } = 5;

    [Display(Name = "Subject")]
    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Display(Name = "Sub-topic")]
    [Required(ErrorMessage = "Sub-topic is required.")]
    [StringLength(200)]
    public string SubTopic { get; set; } = string.Empty;

    [Display(Name = "Number of questions")]
    [Range(1, 50, ErrorMessage = "Question count must be between 1 and 50.")]
    public int QuestionCount { get; set; } = 5;

    [Display(Name = "Answers per question")]
    [Range(2, 6, ErrorMessage = "Answers per question must be between 2 and 6.")]
    public int AnswersPerQuestion { get; set; } = 4;
}
