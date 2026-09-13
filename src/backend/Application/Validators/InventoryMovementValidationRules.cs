using Application.Exceptions;

namespace Application.Validation;

public static class InventoryMovementValidationRules
{
    public const int ReasonMaxLength = 500;

    public static int ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new AppValidationException(
                "Quantity must be greater than zero.");
        }

        return quantity;
    }

    public static string ValidateReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new AppValidationException(
                "Reason is required.");
        }

        var normalizedReason = reason.Trim();

        if (normalizedReason.Length > ReasonMaxLength)
        {
            throw new AppValidationException(
                $"Reason cannot exceed {ReasonMaxLength} characters.");
        }

        return normalizedReason;
    }
}