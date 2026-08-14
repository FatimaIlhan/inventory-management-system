using Domain.Entities;

namespace Application.Interfaces;

public interface ISupplierRepository
{
    Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken);

    Task<Supplier?> GetByIdAsync(
        long supplierId,
        CancellationToken cancellationToken);

    Task<Supplier> CreateAsync(
        Supplier supplier,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Supplier supplier,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        long supplierId,
        CancellationToken cancellationToken);

    Task<bool> ExistsByCompanyNameAsync(
        string companyName,
        long? excludedSupplierId,
        CancellationToken cancellationToken);
}