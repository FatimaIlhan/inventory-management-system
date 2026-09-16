namespace Api.DTOs.PurchaseOrders;

public sealed record CreatePurchaseOrderRequest(
    string OrderNumber,
    long SupplierId,
    IReadOnlyList<PurchaseOrderItemRequest> Items);