using Domain.Enums;
namespace Domain.Entities;

public sealed class Product
{
    public long ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
     public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public ProductStatus Status { get; set; }
    public long CategoryId { get; set; }
    public long SupplierId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}