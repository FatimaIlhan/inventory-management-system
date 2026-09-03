using Domain.Enums;

namespace Api.DTOs.Products;

public sealed record UpdateProductsRequest(
    string Sku,
    string Name,
    string? Description,
    decimal UnitPrice,
    int ReorderLevel,
    ProductStatus Status,
    long CategoryId,
    long SupplierId);