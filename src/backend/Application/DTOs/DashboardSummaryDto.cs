using Domain.Enums;

namespace Application.DTOs;

public sealed record DashboardSummaryDto(
    int TotalProducts,
    int TotalCategories,
    int TotalSuppliers,
    int LowStockItemCount,
    int TotalPurchaseOrders,
    int RecentActivityCount,
    IReadOnlyCollection<DashboardMovementTrendDto> MovementTrends,
    IReadOnlyCollection<DashboardCategoryInventoryDto> InventoryByCategory,
    IReadOnlyCollection<DashboardTopMovingProductDto> TopMovingProducts,
    IReadOnlyCollection<DashboardRecentActivityDto> RecentActivities,
    IReadOnlyCollection<DashboardLowStockProductDto> LowStockProducts);

public sealed record DashboardMovementTrendDto(
    DateTime DateUtc,
    int StockInQuantity,
    int StockOutQuantity,
    int AdjustmentQuantity);

public sealed record DashboardCategoryInventoryDto(
    long CategoryId,
    string CategoryName,
    int CurrentStock);

public sealed record DashboardTopMovingProductDto(
    long ProductId,
    string ProductName,
    string Sku,
    int TotalQuantityMoved);

public sealed record DashboardRecentActivityDto(
    long InventoryMovementId,
    long ProductId,
    string ProductName,
    long PerformedByUserId,
    string PerformedByUserName,
    StockMovementType MovementType,
    int Quantity,
    int PreviousStock,
    int NewStock,
    string Reason,
    DateTime CreatedAtUtc);

public sealed record DashboardLowStockProductDto(
    long ProductId,
    string Sku,
    string ProductName,
    int CurrentStock,
    int ReorderLevel);