using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class InventoryMovementService(
    IInventoryMovementRepository inventoryMovementRepository,
    IProductRepository productRepository,
    ICurrentUserService currentUserService,
  
    TimeProvider timeProvider) : IInventoryMovementService
{
    public async Task<InventoryMovementDto> CreateAsync(
        CreateInventoryMovementDto createInventoryMovementDto, 
        CancellationToken cancellationToken)
    {
        if(createInventoryMovementDto.Quantity <= 0)
        {
            throw new AppValidationException("Quantity must be greater than 0.");
        }
        if(string.IsNullOrWhiteSpace(createInventoryMovementDto.Reason))
        {
            throw new AppValidationException("Reason must be provided.");
        }
        //load the products
        var product = await productRepository.GetByIdAsync(
            createInventoryMovementDto.ProductId, 
            cancellationToken);
            if (product is null)
{
           throw new NotFoundException(
        $"Product with ID {createInventoryMovementDto.ProductId} was not found.");
}
        var previousStock = product.CurrentStock;
        var newStock = createInventoryMovementDto.MovementType switch
{
    StockMovementType.StockIn =>
        previousStock + createInventoryMovementDto.Quantity,

    StockMovementType.StockOut =>
        previousStock - createInventoryMovementDto.Quantity,

    StockMovementType.Adjustment =>
        createInventoryMovementDto.Quantity,

    _ => throw new AppValidationException(
        "Invalid movement type.")
        
};
if (newStock < 0)
{
    throw new AppValidationException(
        "Stock cannot be negative.");
}
        product.CurrentStock = newStock;
        var inventoryMovement = new InventoryMovement
{
    ProductId = product.ProductId,
    MovementType = createInventoryMovementDto.MovementType,
    Quantity = createInventoryMovementDto.Quantity,
    PreviousStock = previousStock,
    NewStock = newStock,
    Reason = createInventoryMovementDto.Reason.Trim(),
    CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,

    PerformedByUserId = currentUserService.UserId
};
        await productRepository.UpdateAsync(product, cancellationToken);

var createdMovement = await inventoryMovementRepository.CreateAsync(
    inventoryMovement,
    cancellationToken);

        return ToDto(createdMovement);
    }

    public async Task<InventoryMovementDto?> GetByIdAsync(
        long inventoryMovementId,
        CancellationToken cancellationToken)
    {
        var inventoryMovement = await inventoryMovementRepository.GetByIdAsync(
            inventoryMovementId,
            cancellationToken);

        return inventoryMovement is null ? null : ToDto(inventoryMovement);
    }

    public async Task<PagedResultDto<InventoryMovementDto>> GetPagedAsync(
        int page,
         int pageSize, 
         string? search, 
         string? sortBy, 
         bool descending,
          CancellationToken cancellationToken)
    {

        var validatedPage = ValidatePage(page);
        var validatedPageSize = ValidatePageSize(pageSize);

        var (items, totalCount) = await inventoryMovementRepository.GetPagedAsync(
            validatedPage,
            validatedPageSize,
            search,
            sortBy,
            descending,
            cancellationToken);

        return new PagedResultDto<InventoryMovementDto>(
            items.Select(ToDto).ToList(),
            validatedPage,
            validatedPageSize,
            totalCount);
    }
    private static int ValidatePage(int page)
    {
        if (page < 1)
        {
            throw new AppValidationException("Page must be greater than or equal to 1.");
        }

        return page;
    }

    private static int ValidatePageSize(int pageSize)
    {
        if (pageSize < 1 || pageSize > 100)
        {
            throw new AppValidationException("Page size must be between 1 and 100.");
        }

        return pageSize;
    }
        private static InventoryMovementDto ToDto(InventoryMovement inventoryMovement) =>
        new(
            inventoryMovement.InventoryMovementId,
            inventoryMovement.ProductId,
            inventoryMovement.Product.Name,
            inventoryMovement.PerformedByUserId,
            inventoryMovement.PerformedByUser.UserName ?? string.Empty,
            inventoryMovement.MovementType,
            inventoryMovement.Quantity,
            inventoryMovement.PreviousStock,
            inventoryMovement.NewStock,
            inventoryMovement.Reason,
            inventoryMovement.CreatedAtUtc);
}
    
