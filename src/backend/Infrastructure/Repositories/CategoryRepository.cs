using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CategoryRepository(InventoryDbContext dbContext) : ICategoryRepository
{
    public async Task<(IReadOnlyList<Category> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken)
    {
        var query = dbContext.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            var loweredSearch = normalizedSearch.ToLower();
            query = query.Where(category =>
                category.Name.ToLower().Contains(loweredSearch) ||
                (category.Description != null && category.Description.ToLower().Contains(loweredSearch)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(category => category.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Category?> GetByIdAsync(long categoryId, CancellationToken cancellationToken)
        => await dbContext.Categories.AsNoTracking()
            .FirstOrDefaultAsync(item => item.CategoryId == categoryId, cancellationToken);

    public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken)
    {
        await dbContext.Categories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long categoryId, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(item => item.CategoryId == categoryId, cancellationToken);
        if (category is null)
        {
            return;
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, long? excludedCategoryId, CancellationToken cancellationToken)
    {
        var normalizedNameKey = name.ToUpperInvariant();
        return await dbContext.Categories.AnyAsync(category =>
            category.Name.ToUpper() == normalizedNameKey &&
            (!excludedCategoryId.HasValue || category.CategoryId != excludedCategoryId.Value),
            cancellationToken);
    }
}
