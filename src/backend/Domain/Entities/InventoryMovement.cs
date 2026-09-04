using Domain.Enums;

namespace Domain.Entities;

public sealed class InventoryMovement
{
	public long InventoryMovementId { get; set; }
	public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
	public long PerformedByUserId { get; set; }
    public User PerformedByUser { get; set; } = null!;
	public StockMovementType MovementType { get; set; }
	public int Quantity { get; set; }
	public int PreviousStock { get; set; }
	public int NewStock { get; set; }
	public string Reason { get; set; } = string.Empty;
	public DateTime CreatedAtUtc { get; set; }
}