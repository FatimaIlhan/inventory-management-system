using Domain.Enums;

namespace Api.DTOs.Products;

public sealed record CreateProductsRequest(
    string Sku,
    string Name,
    string? Description,
    decimal UnitPrice,
    int CurrentStock,
    int ReorderLevel,
    ProductStatus Status,
    long CategoryId,
    long SupplierId);