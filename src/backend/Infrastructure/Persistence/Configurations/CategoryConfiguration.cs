using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.CategoryId);

        builder.Property(category => category.CategoryId)
            .HasColumnName("category_id");

        builder.Property(category => category.Name)
            .HasColumnName("category_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(category => category.Name)
            .IsUnique();

        builder.Property(category => category.Description)
            .HasColumnName("category_description")
            .HasMaxLength(500);

        builder.Property(category => category.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(category => category.UpdatedAtUtc)
            .HasColumnName("updated_at");
    }
}
