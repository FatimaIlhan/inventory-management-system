using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class PurchaseOrderRepository(InventoryDbContext dbContext) : IPurchaseOrderRepository
{
    public async Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        IQueryable<PurchaseOrder> query = dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(purchaseOrder => purchaseOrder.Supplier)
            .Include(purchaseOrder => purchaseOrder.Items)
                .ThenInclude(item => item.Product);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            var loweredSearch = normalizedSearch.ToLower();

            query = query.Where(purchaseOrder =>
                purchaseOrder.OrderNumber.ToLower().Contains(loweredSearch) ||
                purchaseOrder.Supplier.CompanyName.ToLower().Contains(loweredSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLower() switch
        {
            "ordernumber" => descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.OrderNumber)
                : query.OrderBy(purchaseOrder => purchaseOrder.OrderNumber),

            "suppliername" => descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.Supplier.CompanyName)
                : query.OrderBy(purchaseOrder => purchaseOrder.Supplier.CompanyName),

            "status" => descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.Status)
                : query.OrderBy(purchaseOrder => purchaseOrder.Status),

            "submittedatutc" => descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.SubmittedAtUtc)
                : query.OrderBy(purchaseOrder => purchaseOrder.SubmittedAtUtc),

            "receivedatutc" => descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.ReceivedAtUtc)
                : query.OrderBy(purchaseOrder => purchaseOrder.ReceivedAtUtc),

            _ => descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.CreatedAtUtc)
                : query.OrderBy(purchaseOrder => purchaseOrder.CreatedAtUtc)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<PurchaseOrder?> GetByIdAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        return await dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(purchaseOrder => purchaseOrder.Supplier)
            .Include(purchaseOrder => purchaseOrder.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(
                purchaseOrder => purchaseOrder.PurchaseOrderId == purchaseOrderId,
                cancellationToken);
    }

    public async Task<PurchaseOrder> CreateAsync(
        PurchaseOrder purchaseOrder,
        CancellationToken cancellationToken)
    {
        await dbContext.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return purchaseOrder;
    }

    public async Task UpdateAsync(
        PurchaseOrder purchaseOrder,
        CancellationToken cancellationToken)
    {
        var existingPurchaseOrder = await dbContext.PurchaseOrders
            .Include(item => item.Items)
            .FirstOrDefaultAsync(
                item => item.PurchaseOrderId == purchaseOrder.PurchaseOrderId,
                cancellationToken);

        if (existingPurchaseOrder is null)
        {
            return;
        }

        existingPurchaseOrder.OrderNumber = purchaseOrder.OrderNumber;
        existingPurchaseOrder.SupplierId = purchaseOrder.SupplierId;
        existingPurchaseOrder.Status = purchaseOrder.Status;
        existingPurchaseOrder.CreatedAtUtc = purchaseOrder.CreatedAtUtc;
        existingPurchaseOrder.SubmittedAtUtc = purchaseOrder.SubmittedAtUtc;
        existingPurchaseOrder.ReceivedAtUtc = purchaseOrder.ReceivedAtUtc;

        dbContext.PurchaseOrderItems.RemoveRange(existingPurchaseOrder.Items);
        existingPurchaseOrder.Items = purchaseOrder.Items.Select(item => new PurchaseOrderItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList();

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await dbContext.PurchaseOrders
            .FirstOrDefaultAsync(
                item => item.PurchaseOrderId == purchaseOrderId,
                cancellationToken);

        if (purchaseOrder is null)
        {
            return;
        }

        dbContext.PurchaseOrders.Remove(purchaseOrder);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var normalizedOrderNumber = orderNumber.ToUpperInvariant();

        return await dbContext.PurchaseOrders.AnyAsync(
            purchaseOrder => purchaseOrder.OrderNumber.ToUpper() == normalizedOrderNumber,
            cancellationToken);
    }
}