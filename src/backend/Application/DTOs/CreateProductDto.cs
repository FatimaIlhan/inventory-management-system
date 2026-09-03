using Domain.Enums;

namespace Application.DTOs;

public sealed record CreateProductDto(
    string Sku,
    string Name,
    string? Description,
    decimal UnitPrice,
    int CurrentStock,
    int ReorderLevel,
    ProductStatus Status,
    long CategoryId,
    long SupplierId);