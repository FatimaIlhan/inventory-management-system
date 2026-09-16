namespace Application.DTOs;

public sealed record PurchaseOrderItemDto(
    long PurchaseOrderItemId,
    long ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);