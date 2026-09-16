namespace Application.DTOs;

public sealed record CreatePurchaseOrderItemDto(
    long ProductId,
    int Quantity,
    decimal UnitPrice);