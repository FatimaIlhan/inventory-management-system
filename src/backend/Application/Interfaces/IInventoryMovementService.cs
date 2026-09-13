using Application.DTOs;

public interface IInventoryMovementService
{
      Task<PagedResultDto<InventoryMovementDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);

    Task<InventoryMovementDto?> GetByIdAsync(
        long productId,
        CancellationToken cancellationToken);

    Task<InventoryMovementDto> CreateAsync(
        CreateInventoryMovementDto createProductDto,
        CancellationToken cancellationToken);
}