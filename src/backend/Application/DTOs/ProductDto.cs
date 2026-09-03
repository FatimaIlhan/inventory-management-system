using Domain.Enums;

namespace Application.DTOs;

public sealed record ProductDto(
    long ProductId,
    string Sku,
    string Name,
    string? Description,
    decimal UnitPrice,
    int CurrentStock,
    int ReorderLevel,
    ProductStatus Status,
    long CategoryId,
    long SupplierId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);