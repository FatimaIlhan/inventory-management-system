using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Validators;
using Domain.Entities;

namespace Application.Services;

public sealed class CategoryService(ICategoryRepository categoryRepository, TimeProvider timeProvider) : ICategoryService
{
    public async Task<PagedResultDto<CategoryDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var validatedPage = CategoryValidationRules.ValidatePage(page);
        var validatedPageSize = CategoryValidationRules.ValidatePageSize(pageSize);

        var (items, totalCount) = await categoryRepository.GetPagedAsync(validatedPage, validatedPageSize, search, cancellationToken);

        return new PagedResultDto<CategoryDto>(
            items.Select(ToDto).ToList(),
            validatedPage,
            validatedPageSize,
            totalCount);
    }

    public async Task<CategoryDto> GetByIdAsync(long categoryId, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);

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

        var createdCategory = await categoryRepository.CreateAsync(entity, cancellationToken);

        return ToDto(createdCategory);
    }

    public async Task<CategoryDto> UpdateAsync(long categoryId, UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
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

        await categoryRepository.UpdateAsync(category, cancellationToken);

        return ToDto(category);
    }

    public async Task DeleteAsync(long categoryId, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category was not found.");
        }

        await categoryRepository.DeleteAsync(categoryId, cancellationToken);
    }

    private async Task EnsureNameIsUniqueAsync(string name, long? excludedCategoryId, CancellationToken cancellationToken)
    {
        var isNameTaken = await categoryRepository.ExistsByNameAsync(name, excludedCategoryId, cancellationToken);

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
