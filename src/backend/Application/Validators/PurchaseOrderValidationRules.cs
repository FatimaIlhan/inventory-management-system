using Application.DTOs;
using Application.Exceptions;

namespace Application.Validators;

public static class PurchaseOrderValidationRules
{
    public const int OrderNumberMaxLength = 50;

    public static string ValidateAndNormalizeOrderNumber(string? orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new AppValidationException("Purchase order number is required.");
        }

        var normalizedOrderNumber = orderNumber.Trim();

        if (normalizedOrderNumber.Length > OrderNumberMaxLength)
        {
            throw new AppValidationException(
                $"Purchase order number cannot exceed {OrderNumberMaxLength} characters.");
        }

        return normalizedOrderNumber;
    }

    public static long ValidateSupplierId(long supplierId)
    {
        if (supplierId <= 0)
        {
            throw new AppValidationException("Purchase order supplier is required.");
        }

        return supplierId;
    }

    public static int ValidatePage(int page)
    {
        if (page < 1)
        {
            throw new AppValidationException("Page must be greater than or equal to 1.");
        }

        return page;
    }

    public static int ValidatePageSize(int pageSize)
    {
        if (pageSize < 1 || pageSize > 100)
        {
            throw new AppValidationException("Page size must be between 1 and 100.");
        }

        return pageSize;
    }

    public static IReadOnlyList<CreatePurchaseOrderItemDto> ValidateItems(
        IReadOnlyList<CreatePurchaseOrderItemDto>? items)
    {
        if (items is null || items.Count == 0)
        {
            throw new AppValidationException("Purchase order must contain at least one item.");
        }

        if (items.Any(item => item.ProductId <= 0))
        {
            throw new AppValidationException("Purchase order item product is required.");
        }

        if (items.Any(item => item.Quantity <= 0))
        {
            throw new AppValidationException("Purchase order item quantity must be greater than zero.");
        }

        if (items.Any(item => item.UnitPrice <= 0))
        {
            throw new AppValidationException("Purchase order item unit price must be greater than zero.");
        }

        if (items.GroupBy(item => item.ProductId).Any(group => group.Count() > 1))
        {
            throw new AppValidationException("A product can appear only once in a purchase order.");
        }

        return items;
    }
}