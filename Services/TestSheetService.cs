using Microsoft.EntityFrameworkCore;
using ZnaykoAI.Data;
using ZnaykoAI.Models;

namespace ZnaykoAI.Services;

public class TestSheetService(ApplicationDbContext db) : ITestSheetService
{
    public async Task<TestSheet> SaveAsync(TestSheet sheet, CancellationToken cancellationToken = default)
    {
        db.TestSheets.Add(sheet);
        await db.SaveChangesAsync(cancellationToken);
        return sheet;
    }

    public async Task<TestSheet?> GetByIdForUserAsync(
        Guid id,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await db.TestSheets
            .AsNoTracking()
            .Include(ts => ts.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(
                ts => ts.Id == id && ts.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<TestSheet>> GetAllForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await db.TestSheets
            .AsNoTracking()
            .Include(ts => ts.Questions)
            .Where(ts => ts.UserId == userId)
            .OrderByDescending(ts => ts.CreatedOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteForUserAsync(
        Guid id,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var sheet = await db.TestSheets
            .FirstOrDefaultAsync(
                ts => ts.Id == id && ts.UserId == userId,
                cancellationToken);

        if (sheet is null)
        {
            return false;
        }

        db.TestSheets.Remove(sheet);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
