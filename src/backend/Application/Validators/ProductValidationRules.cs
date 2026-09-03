using Application.Exceptions;
using Domain.Enums;
namespace Application.Validators;

public static class ProductValidationRules
{
    public const int SkuMaxLength = 50;
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    public static string ValidateAndNormalizeSku(string? sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new AppValidationException("Product SKU is required.");
        }

        var normalizedSku = sku.Trim();

        if (normalizedSku.Length > SkuMaxLength)
        {
            throw new AppValidationException(
                $"Product SKU cannot exceed {SkuMaxLength} characters.");
        }

        return normalizedSku;
    }

    public static string ValidateAndNormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new AppValidationException("Product name is required.");
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > NameMaxLength)
        {
            throw new AppValidationException(
                $"Product name cannot exceed {NameMaxLength} characters.");
        }

        return normalizedName;
    }

    public static string? ValidateAndNormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length > DescriptionMaxLength)
        {
            throw new AppValidationException(
                $"Product description cannot exceed {DescriptionMaxLength} characters.");
        }

        return normalizedDescription;
    }
    public static decimal ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
        {
            throw new AppValidationException(
                "Product unit price must be greater than zero.");
        }

        return unitPrice;
    }
    public static int ValidateCurrentStock(int currentStock)
    {
        if (currentStock < 0)
        {
            throw new AppValidationException(
                "Product current stock cannot be negative.");
        }

        return currentStock;
    }
    public static int ValidateReorderLevel(int reorderLevel)
    {
        if (reorderLevel < 0)
        {
            throw new AppValidationException(
                "Product reorder level cannot be negative.");
        }

        return reorderLevel;
    }
    public static long ValidateCategoryId(long categoryId)
    {
        if (categoryId <= 0)
        {
            throw new AppValidationException(
                "Product category is required.");
        }

        return categoryId;
    }

    public static long ValidateSupplierId(long supplierId)
    {
        if (supplierId <= 0)
        {
            throw new AppValidationException(
                "Product supplier is required.");
        }

        return supplierId;
    }
    public static ProductStatus ValidateStatus(ProductStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new AppValidationException(
                "Product status is invalid.");
        }

        return status;
    }

}