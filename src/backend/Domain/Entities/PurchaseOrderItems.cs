namespace Domain.Entities;

public  class PurchaseOrderItem
{
    public long PurchaseOrderItemId { get; set; }

    public long PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}