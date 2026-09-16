namespace Api.DTOs.PurchaseOrders;

public sealed record PurchaseOrderItemRequest(
    long ProductId,
    int Quantity,
    decimal UnitPrice);