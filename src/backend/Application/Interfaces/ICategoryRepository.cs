using Domain.Entities;

namespace Application.Interfaces;

public interface ICategoryRepository
{
    Task<(IReadOnlyList<Category> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(long categoryId, CancellationToken cancellationToken);
    Task<Category> CreateAsync(Category category, CancellationToken cancellationToken);
    Task UpdateAsync(Category category, CancellationToken cancellationToken);
    Task DeleteAsync(long categoryId, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, long? excludedCategoryId, CancellationToken cancellationToken);
}
