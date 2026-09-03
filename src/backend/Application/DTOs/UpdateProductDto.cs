using Domain.Enums;

namespace Application.DTOs;

public sealed record UpdateProductDto(
    string Sku,
    string Name,
    string? Description,
    decimal UnitPrice,
    int ReorderLevel,
    ProductStatus Status,
    long CategoryId,
    long SupplierId);