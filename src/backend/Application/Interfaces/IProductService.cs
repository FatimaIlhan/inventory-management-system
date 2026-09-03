using Application.DTOs;

namespace Application.Interfaces;

public interface IProductService
{
    Task<PagedResultDto<ProductDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);

    Task<ProductDto> GetByIdAsync(
        long productId,
        CancellationToken cancellationToken);

    Task<ProductDto> CreateAsync(
        CreateProductDto createProductDto,
        CancellationToken cancellationToken);

    Task<ProductDto> UpdateAsync(
        long productId,
        UpdateProductDto updateProductDto,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        long productId,
        CancellationToken cancellationToken);
}
