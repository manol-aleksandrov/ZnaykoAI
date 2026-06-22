using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ZnaykoAI.Services;
using ZnaykoAI.Services.Exceptions;

namespace ZnaykoAI.Pages.Quizzes;

[Authorize]
public class GenerateModel(
    IQuizGenerationService quizGeneration,
    ITestSheetService testSheets) : PageModel
{
    [BindProperty]
    public GenerateQuizInput Input { get; set; } = new();

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
            var sheet = await quizGeneration.GenerateQuizAsync(
                userId,
                Input.Grade,
                Input.Subject,
                Input.SubTopic,
                Input.QuestionCount,
                Input.AnswersPerQuestion,
                HttpContext.RequestAborted);

            await testSheets.SaveAsync(sheet, HttpContext.RequestAborted);

            TempData["SuccessMessage"] =
                $"Quiz \"{sheet.Title}\" was generated successfully with {sheet.Questions.Count} question(s).";

            return RedirectToPage("Details", new { id = sheet.Id });
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
