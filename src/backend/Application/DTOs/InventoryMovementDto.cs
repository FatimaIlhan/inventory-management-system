using Domain.Enums;

public record InventoryMovementDto(
    long InventoryMovementId,
    long ProductId,
    string ProductName,
    long PerformedByUserId,
    string PerformedByUserName,
    StockMovementType MovementType,
    decimal Quantity,
    decimal PreviousStock,
    decimal NewStock,
    string Reason,
    DateTime CreatedAtUtc
);