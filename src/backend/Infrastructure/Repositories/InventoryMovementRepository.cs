using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly InventoryDbContext dbContext;

    public InventoryMovementRepository(InventoryDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<InventoryMovement> CreateAsync(InventoryMovement inventoryMovement, CancellationToken cancellationToken)
    {
         await dbContext.InventoryMovements.AddAsync(inventoryMovement, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.InventoryMovements
            .Include(inventoryMovement => inventoryMovement.Product)
            .Include(inventoryMovement => inventoryMovement.PerformedByUser)
            .AsNoTracking()
            .SingleAsync(movement => movement.InventoryMovementId == inventoryMovement.InventoryMovementId, cancellationToken);
    }

    public async Task<InventoryMovement?> GetByIdAsync(long inventoryMovementId, CancellationToken cancellationToken)
    {
        return await dbContext.InventoryMovements
            .Include(inventoryMovement => inventoryMovement.Product)
            .Include(inventoryMovement => inventoryMovement.PerformedByUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(inventoryMovement => inventoryMovement.InventoryMovementId == inventoryMovementId, cancellationToken);
    }

    public async Task<(IReadOnlyList<InventoryMovement> Items, int TotalCount)> GetPagedAsync(
        int page,
         int pageSize,
          string? search,
          string? sortBy,
          bool descending,
          CancellationToken cancellationToken)
    {
        var query = dbContext.InventoryMovements
            .Include(inventoryMovement => inventoryMovement.Product)
            .Include(inventoryMovement => inventoryMovement.PerformedByUser)
            .AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            var loweredSearch = normalizedSearch.ToLower();

            query = query.Where(inventoryMovement =>
                inventoryMovement.Reason.ToLower().Contains(loweredSearch));
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        sortBy = sortBy?.Trim().ToLowerInvariant();

query = sortBy switch
{
    "quantity" => descending
        ? query.OrderByDescending(x => x.Quantity)
        : query.OrderBy(x => x.Quantity),

    "previousstock" => descending
        ? query.OrderByDescending(x => x.PreviousStock)
        : query.OrderBy(x => x.PreviousStock),

    "newstock" => descending
        ? query.OrderByDescending(x => x.NewStock)
        : query.OrderBy(x => x.NewStock),

    "movementtype" => descending
        ? query.OrderByDescending(x => x.MovementType)
        : query.OrderBy(x => x.MovementType),

    _ => descending
        ? query.OrderByDescending(x => x.CreatedAtUtc)
        : query.OrderBy(x => x.CreatedAtUtc)
};

        var items = await query
        .OrderByDescending(inventoryMovement => inventoryMovement.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}