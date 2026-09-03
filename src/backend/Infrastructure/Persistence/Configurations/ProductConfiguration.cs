using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.ProductId);

        builder.Property(product => product.ProductId)
            .HasColumnName("product_id");

        builder.Property(product => product.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(product => product.Sku)
            .IsUnique();

        builder.Property(product => product.Name)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasColumnName("product_description")
            .HasMaxLength(500);

        builder.Property(product => product.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(product => product.CurrentStock)
            .HasColumnName("current_stock")
            .IsRequired();

        builder.Property(product => product.ReorderLevel)
            .HasColumnName("reorder_level")
            .IsRequired();

        builder.Property(product => product.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(product => product.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(product => product.SupplierId)
            .HasColumnName("supplier_id")
            .IsRequired();

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(product => product.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(product => product.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(product => product.UpdatedAtUtc)
            .HasColumnName("updated_at");
    }
}