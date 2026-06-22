using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ZnaykoAI.Models;
using ZnaykoAI.Services;

namespace ZnaykoAI.Pages.Quizzes;

[Authorize]
public class DetailsModel(ITestSheetService testSheets) : PageModel
{
    public TestSheet? TestSheet { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        TestSheet = await testSheets.GetByIdForUserAsync(
            id,
            userId,
            HttpContext.RequestAborted);

        if (TestSheet is null)
        {
            return NotFound();
        }

        return Page();
    }
}
