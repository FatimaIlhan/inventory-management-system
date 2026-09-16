using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasKey(po => po.PurchaseOrderId);

        builder.Property(po => po.OrderNumber)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasOne(po => po.Supplier)
               .WithMany()
               .HasForeignKey(po => po.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Items)
               .WithOne(poi => poi.PurchaseOrder)
               .HasForeignKey(poi => poi.PurchaseOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(po => po.Status)
               .IsRequired();

        builder.Property(po => po.CreatedAtUtc)
               .IsRequired();

        builder.Property(po => po.SubmittedAtUtc);

        builder.Property(po => po.ReceivedAtUtc);
    }
}