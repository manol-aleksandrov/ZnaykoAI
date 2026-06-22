using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ZnaykoAI.Models;
using ZnaykoAI.Services;
using ZnaykoAI.Services.Exceptions;

namespace ZnaykoAI.Pages.Quizzes;

[Authorize]
public class GenerateModel(IQuizGenerationService quizGeneration) : PageModel
{
    [BindProperty]
    public GenerateQuizInput Input { get; set; } = new();

    public TestSheet? Result { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        try
        {
            Result = await quizGeneration.GenerateQuizAsync(
                userId,
                Input.Grade,
                Input.Subject,
                Input.SubTopic,
                Input.QuestionCount,
                Input.AnswersPerQuestion,
                HttpContext.RequestAborted);
        }
        catch (QuizGenerationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected (navigation away, tab closed) — not an application error.
        }

        return Page();
    }
}
