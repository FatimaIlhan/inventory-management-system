using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class SupplierRepository(InventoryDbContext dbContext) : ISupplierRepository
{
    public async Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Suppliers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            var loweredSearch = normalizedSearch.ToLower();

            query = query.Where(supplier =>
                supplier.CompanyName.ToLower().Contains(loweredSearch) ||
                supplier.ContactPerson.ToLower().Contains(loweredSearch) ||
                supplier.Phone.ToLower().Contains(loweredSearch) ||
                supplier.Email.ToLower().Contains(loweredSearch) ||
                supplier.Address.ToLower().Contains(loweredSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(supplier => supplier.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Supplier?> GetByIdAsync(
        long supplierId,
        CancellationToken cancellationToken)
        => await dbContext.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                supplier => supplier.SupplierId == supplierId,
                cancellationToken);

    public async Task<Supplier> CreateAsync(
        Supplier supplier,
        CancellationToken cancellationToken)
    {
        await dbContext.Suppliers.AddAsync(supplier, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return supplier;
    }

   public async Task UpdateAsync(
    Supplier supplier,
    CancellationToken cancellationToken)
{
    dbContext.Suppliers.Update(supplier);

    await dbContext.SaveChangesAsync(cancellationToken);
}

    public async Task DeleteAsync(
        long supplierId,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers
            .FirstOrDefaultAsync(
                item => item.SupplierId == supplierId,
                cancellationToken);

        if (supplier is null)
        {
            return;
        }

        dbContext.Suppliers.Remove(supplier);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCompanyNameAsync(
        string companyName,
        long? excludedSupplierId,
        CancellationToken cancellationToken)
    {
        var normalizedCompanyName = companyName.ToUpperInvariant();

        return await dbContext.Suppliers.AnyAsync(
            supplier =>
                supplier.CompanyName.ToUpper() == normalizedCompanyName &&
                (!excludedSupplierId.HasValue ||
                 supplier.SupplierId != excludedSupplierId.Value),
            cancellationToken);
    }
}