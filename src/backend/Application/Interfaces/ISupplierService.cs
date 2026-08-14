using Application.DTOs;
namespace Application.Interfaces;

public interface ISupplierService
{
    Task<PagedResultDto<SupplierDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken);

    Task<SupplierDto> GetByIdAsync(
        long supplierId,
        CancellationToken cancellationToken);

    Task<SupplierDto> CreateAsync(
        CreateSupplierDto createSupplierDto,
        CancellationToken cancellationToken);

    Task<SupplierDto> UpdateAsync(
        long supplierId,
        UpdateSupplierDto updateSupplierDto,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        long supplierId,
        CancellationToken cancellationToken);
}