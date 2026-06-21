using Microsoft.AspNetCore.Identity;

namespace ZnaykoAI.Models;

public class TestSheet
{
    public Guid Id { get; set; }

    public int Grade { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string SubTopic { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public string UserId { get; set; } = string.Empty;

    public IdentityUser User { get; set; } = null!;

    public ICollection<Question> Questions { get; set; } = [];
}
