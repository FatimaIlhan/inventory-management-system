using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class DashboardRepository(InventoryDbContext dbContext) : IDashboardRepository
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken cancellationToken)
    {
        var totalProducts = await dbContext.Products.CountAsync(cancellationToken);
        var totalCategories = await dbContext.Categories.CountAsync(cancellationToken);
        var totalSuppliers = await dbContext.Suppliers.CountAsync(cancellationToken);
        var totalPurchaseOrders = await dbContext.PurchaseOrders.CountAsync(cancellationToken);
        var lowStockItemCount = await dbContext.Products.CountAsync(
            product => product.Status == ProductStatus.Active && product.CurrentStock <= product.ReorderLevel,
            cancellationToken);

        var movements = dbContext.InventoryMovements
            .AsNoTracking()
            .Where(movement =>
                movement.CreatedAtUtc >= fromUtc &&
                movement.CreatedAtUtc < toUtcExclusive);

        var recentActivityCount = await movements.CountAsync(cancellationToken);

var movementData = await movements
    .Select(movement => new
    {
        movement.CreatedAtUtc,
        movement.MovementType,
        movement.Quantity,
        movement.NewStock,
        movement.PreviousStock
    })
    .ToListAsync(cancellationToken);

var movementTrends = movementData
    .GroupBy(movement => movement.CreatedAtUtc.Date)
    .Select(group => new DashboardMovementTrendDto(
        group.Key,
        group.Where(movement => movement.MovementType == StockMovementType.StockIn)
            .Sum(movement => movement.Quantity),

        group.Where(movement => movement.MovementType == StockMovementType.StockOut)
            .Sum(movement => movement.Quantity),

        group.Where(movement => movement.MovementType == StockMovementType.Adjustment)
            .Sum(movement => Math.Abs(
                movement.NewStock - movement.PreviousStock))
    ))
    .OrderBy(trend => trend.DateUtc)
    .ToList();

        var inventoryByCategory = await dbContext.Categories
            .AsNoTracking()
            .Select(category => new
            {
                category.CategoryId,
                category.Name,
                CurrentStock = dbContext.Products
                    .Where(product => product.CategoryId == category.CategoryId)
                    .Sum(product => (int?)product.CurrentStock) ?? 0
            })
            .OrderByDescending(category => category.CurrentStock)
            .Select(category => new DashboardCategoryInventoryDto(
                category.CategoryId,
                category.Name,
                category.CurrentStock))
            .ToListAsync(cancellationToken);

        var topMovingProducts = await movements
            .GroupBy(movement => new
            {
                movement.ProductId,
                movement.Product.Name,
                movement.Product.Sku
            })
            .Select(group => new
            {
                group.Key.ProductId,
                group.Key.Name,
                group.Key.Sku,
                TotalQuantityMoved = group.Sum(movement => movement.MovementType == StockMovementType.Adjustment
                    ? Math.Abs(movement.NewStock - movement.PreviousStock)
                    : movement.Quantity)
            })
            .OrderByDescending(product => product.TotalQuantityMoved)
            .ThenBy(product => product.Name)
            .Take(5)
            .Select(product => new DashboardTopMovingProductDto(
                product.ProductId,
                product.Name,
                product.Sku,
                product.TotalQuantityMoved))
            .ToListAsync(cancellationToken);

        var recentActivities = await movements
            .OrderByDescending(movement => movement.CreatedAtUtc)
            .Take(5)
            .Select(movement => new DashboardRecentActivityDto(
                movement.InventoryMovementId,
                movement.ProductId,
                movement.Product.Name,
                movement.PerformedByUserId,
                movement.PerformedByUser.UserName ?? string.Empty,
                movement.MovementType,
                movement.Quantity,
                movement.PreviousStock,
                movement.NewStock,
                movement.Reason,
                movement.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        var lowStockProducts = await dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.Status == ProductStatus.Active &&
                product.CurrentStock <= product.ReorderLevel)
            .OrderBy(product => product.CurrentStock)
            .ThenBy(product => product.Name)
            .Take(5)
            .Select(product => new DashboardLowStockProductDto(
                product.ProductId,
                product.Sku,
                product.Name,
                product.CurrentStock,
                product.ReorderLevel))
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto(
            totalProducts,
            totalCategories,
            totalSuppliers,
            lowStockItemCount,
            totalPurchaseOrders,
            recentActivityCount,
            movementTrends,
            inventoryByCategory,
            topMovingProducts,
            recentActivities,
            lowStockProducts);
    }
}