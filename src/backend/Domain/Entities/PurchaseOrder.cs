

namespace Domain.Entities;

public sealed class PurchaseOrder
{
    public long PurchaseOrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public long SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public PurchaseOrderStatus Status { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public DateTime? ReceivedAtUtc { get; set; }
}