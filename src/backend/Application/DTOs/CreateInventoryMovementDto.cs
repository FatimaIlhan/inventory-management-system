using Domain.Enums;

public record CreateInventoryMovementDto(
    long ProductId,
    StockMovementType MovementType,
    int  Quantity,
    string Reason
);