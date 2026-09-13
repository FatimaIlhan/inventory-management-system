using Domain.Enums;

namespace Api.DTOs.InventoryMovements;

public sealed record CreateInventoryMovementRequest(
    long ProductId,
    StockMovementType MovementType,
    int Quantity,
    string Reason);