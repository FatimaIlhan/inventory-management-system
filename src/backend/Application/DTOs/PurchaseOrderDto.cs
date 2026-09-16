using Domain.Enums;

namespace Application.DTOs;

public sealed record PurchaseOrderDto(
    long PurchaseOrderId,
    string OrderNumber,
    long SupplierId,
    string SupplierName,
    PurchaseOrderStatus Status,
    IReadOnlyList<PurchaseOrderItemDto> Items,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc,
    DateTime? ReceivedAtUtc);