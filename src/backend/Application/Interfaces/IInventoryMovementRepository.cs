using Domain.Entities;

namespace Application.Interfaces;

public interface IInventoryMovementRepository
{
    Task<(IReadOnlyList<InventoryMovement> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
         string? sortBy,
          bool descending,
        CancellationToken cancellationToken);

    Task<InventoryMovement?> GetByIdAsync(
        long inventoryMovementId,
        CancellationToken cancellationToken);

    Task<InventoryMovement> CreateAsync(
        InventoryMovement inventoryMovement,
        CancellationToken cancellationToken);
}