namespace Application.DTOs;

public sealed record UpdatePurchaseOrderDto(
    string OrderNumber,
    long SupplierId,
    IReadOnlyList<CreatePurchaseOrderItemDto> Items);