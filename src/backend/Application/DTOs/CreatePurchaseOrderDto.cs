namespace Application.DTOs;

public sealed record CreatePurchaseOrderDto(
    string OrderNumber,
    long SupplierId,
    IReadOnlyList<CreatePurchaseOrderItemDto> Items);