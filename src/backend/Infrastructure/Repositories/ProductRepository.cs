using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ProductRepository(
    InventoryDbContext dbContext) : IProductRepository
{
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            var loweredSearch = normalizedSearch.ToLower();

            query = query.Where(product =>
                product.Sku.ToLower().Contains(loweredSearch) ||
                product.Name.ToLower().Contains(loweredSearch) ||
                (product.Description != null && product.Description.ToLower().Contains(loweredSearch)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "sku" => descending
                ? query.OrderByDescending(product => product.Sku)
                : query.OrderBy(product => product.Sku),

            "name" => descending
                ? query.OrderByDescending(product => product.Name)
                : query.OrderBy(product => product.Name),

            "unitprice" => descending
                ? query.OrderByDescending(product => product.UnitPrice)
                : query.OrderBy(product => product.UnitPrice),

            "currentstock" => descending
                ? query.OrderByDescending(product => product.CurrentStock)
                : query.OrderBy(product => product.CurrentStock),

            "reorderlevel" => descending
                ? query.OrderByDescending(product => product.ReorderLevel)
                : query.OrderBy(product => product.ReorderLevel),

            _ => descending
                ? query.OrderByDescending(product => product.Name)
                : query.OrderBy(product => product.Name)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Product?> GetByIdAsync(
        long productId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.ProductId == productId, cancellationToken);
    }

    public async Task<Product> CreateAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        long productId,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(item => item.ProductId == productId, cancellationToken);

        if (product is null)
        {
            return;
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(
        string sku,
        long? excludedProductId,
        CancellationToken cancellationToken)
    {
        var normalizedSku = sku.ToUpperInvariant();

        return await dbContext.Products.AnyAsync(
            product =>
                product.Sku.ToUpper() == normalizedSku &&
                (!excludedProductId.HasValue || product.ProductId != excludedProductId.Value),
            cancellationToken);
    }
}
