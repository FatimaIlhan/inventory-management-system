using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
	public void Configure(EntityTypeBuilder<InventoryMovement> builder)
	{
		builder.ToTable("inventory_movements");

		builder.HasKey(movement => movement.InventoryMovementId);

		builder.Property(movement => movement.InventoryMovementId)
			.HasColumnName("inventory_movement_id");

		builder.Property(movement => movement.ProductId)
			.HasColumnName("product_id")
			.IsRequired();

		builder.HasOne(movement => movement.Product)
			.WithMany()
			.HasForeignKey(movement => movement.ProductId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Property(movement => movement.PerformedByUserId)
			.HasColumnName("performed_by_user_id")
			.IsRequired();

		builder.HasOne(movement => movement.PerformedByUser)
			.WithMany()
			.HasForeignKey(movement => movement.PerformedByUserId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Property(movement => movement.MovementType)
			.HasColumnName("movement_type")
			.HasColumnType("int")
			.IsRequired();

		builder.Property(movement => movement.Quantity)
			.HasColumnName("quantity")
			.IsRequired();

		builder.Property(movement => movement.PreviousStock)
			.HasColumnName("previous_stock")
			.IsRequired();

		builder.Property(movement => movement.NewStock)
			.HasColumnName("new_stock")
			.IsRequired();

		builder.Property(movement => movement.Reason)
			.HasColumnName("reason")
			.HasMaxLength(500)
			.IsRequired();

		builder.Property(movement => movement.CreatedAtUtc)
			.HasColumnName("created_at")
			.IsRequired();
	}
}
