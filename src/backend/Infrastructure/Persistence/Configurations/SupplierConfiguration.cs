using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(supplier => supplier.SupplierId);

        builder.Property(supplier => supplier.SupplierId)
            .HasColumnName("supplier_id");

        builder.Property(supplier => supplier.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(supplier => supplier.CompanyName)
            .IsUnique();

        builder.Property(supplier => supplier.ContactPerson)
            .HasColumnName("contact_person")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(supplier => supplier.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(supplier => supplier.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(supplier => supplier.Address)
            .HasColumnName("address")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(supplier => supplier.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(supplier => supplier.UpdatedAtUtc)
            .HasColumnName("updated_at");
    }
} 