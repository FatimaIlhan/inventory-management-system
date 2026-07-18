using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Validators;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CategoryService(InventoryDbContext dbContext, TimeProvider timeProvider) : ICategoryService
{
    public async Task<PagedResultDto<CategoryDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var validatedPage = CategoryValidationRules.ValidatePage(page);
        var validatedPageSize = CategoryValidationRules.ValidatePageSize(pageSize);
        var normalizedSearch = search?.Trim();

        var query = dbContext.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            var loweredSearch = normalizedSearch.ToLower();
            query = query.Where(category =>
                category.Name.ToLower().Contains(loweredSearch) ||
                (category.Description != null && category.Description.ToLower().Contains(loweredSearch)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(category => category.Name)
            .Skip((validatedPage - 1) * validatedPageSize)
            .Take(validatedPageSize)
            .Select(category => ToDto(category))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CategoryDto>(items, validatedPage, validatedPageSize, totalCount);
    }

    public async Task<CategoryDto> GetByIdAsync(long categoryId, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.AsNoTracking()
            .FirstOrDefaultAsync(item => item.CategoryId == categoryId, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category was not found.");
        }

        return ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto, CancellationToken cancellationToken)
    {
        var normalizedName = CategoryValidationRules.ValidateAndNormalizeName(createCategoryDto.Name);
        var normalizedDescription = CategoryValidationRules.ValidateAndNormalizeDescription(createCategoryDto.Description);

        await EnsureNameIsUniqueAsync(normalizedName, null, cancellationToken);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        var entity = new Category
        {
            Name = normalizedName,
            Description = normalizedDescription,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = null
        };

        await dbContext.Categories.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<CategoryDto> UpdateAsync(long categoryId, UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(item => item.CategoryId == categoryId, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category was not found.");
        }

        var normalizedName = CategoryValidationRules.ValidateAndNormalizeName(updateCategoryDto.Name);
        var normalizedDescription = CategoryValidationRules.ValidateAndNormalizeDescription(updateCategoryDto.Description);

        await EnsureNameIsUniqueAsync(normalizedName, categoryId, cancellationToken);

        category.Name = normalizedName;
        category.Description = normalizedDescription;
        category.UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(category);
    }

    public async Task DeleteAsync(long categoryId, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(item => item.CategoryId == categoryId, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category was not found.");
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNameIsUniqueAsync(string name, long? excludedCategoryId, CancellationToken cancellationToken)
    {
        var normalizedNameKey = name.ToUpperInvariant();
        var isNameTaken = await dbContext.Categories.AnyAsync(category =>
                category.Name.ToUpper() == normalizedNameKey &&
                (!excludedCategoryId.HasValue || category.CategoryId != excludedCategoryId.Value),
            cancellationToken);

        if (isNameTaken)
        {
            throw new AppValidationException("Category name must be unique.");
        }
    }

    private static CategoryDto ToDto(Category category) =>
        new(
            category.CategoryId,
            category.Name,
            category.Description,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);
}
