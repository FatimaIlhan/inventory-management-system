using Application.DTOs;

namespace Application.Interfaces;

public interface ICategoryService
{
    Task<PagedResultDto<CategoryDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
    Task<CategoryDto> GetByIdAsync(long categoryId, CancellationToken cancellationToken);
    Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto, CancellationToken cancellationToken);
    Task<CategoryDto> UpdateAsync(long categoryId, UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken);
    Task DeleteAsync(long categoryId, CancellationToken cancellationToken);
}
