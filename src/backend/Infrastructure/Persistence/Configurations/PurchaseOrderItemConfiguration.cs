using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.HasKey(poi => poi.PurchaseOrderItemId);

        builder.HasOne(poi => poi.PurchaseOrder)
               .WithMany(po => po.Items)
               .HasForeignKey(poi => poi.PurchaseOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(poi => poi.Product)
               .WithMany()
               .HasForeignKey(poi => poi.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
               builder.Property(poi => poi.Quantity)
                      .IsRequired();        
                      builder.Property(poi => poi.UnitPrice)
                             .IsRequired()
                             .HasPrecision(18, 2);
    }
}