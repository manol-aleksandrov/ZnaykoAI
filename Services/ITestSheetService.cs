using ZnaykoAI.Models;

namespace ZnaykoAI.Services;

public interface ITestSheetService
{
    Task<TestSheet> SaveAsync(TestSheet sheet, CancellationToken cancellationToken = default);

    Task<TestSheet?> GetByIdForUserAsync(
        Guid id,
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestSheet>> GetAllForUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteForUserAsync(
        Guid id,
        string userId,
        CancellationToken cancellationToken = default);
}
