namespace Api.DTOs.PurchaseOrders;

public sealed record UpdatePurchaseOrderRequest(
    string OrderNumber,
    long SupplierId,
    IReadOnlyList<PurchaseOrderItemRequest> Items);