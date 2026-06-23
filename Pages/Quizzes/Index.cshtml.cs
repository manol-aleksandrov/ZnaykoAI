using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ZnaykoAI.Models;
using ZnaykoAI.Services;

namespace ZnaykoAI.Pages.Quizzes;

[Authorize]
public class IndexModel(ITestSheetService testSheets) : PageModel
{
    public IReadOnlyList<TestSheet> Quizzes { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        Quizzes = await testSheets.GetAllForUserAsync(userId, HttpContext.RequestAborted);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var deleted = await testSheets.DeleteForUserAsync(id, userId, HttpContext.RequestAborted);
        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Quiz deleted successfully.";
        return RedirectToPage();
    }
}
