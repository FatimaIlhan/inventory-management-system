using Domain.Entities;

public interface IPurchaseOrderRepository
{
    Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);

    Task<PurchaseOrder?> GetByIdAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken);

    Task<PurchaseOrder> CreateAsync(
        PurchaseOrder purchaseOrder,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        PurchaseOrder purchaseOrder,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        long purchaseOrderId,
        CancellationToken cancellationToken);

    Task<bool> ExistsByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken);
}