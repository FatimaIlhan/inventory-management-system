using Application.Exceptions;

namespace Application.Validators;

public static class CategoryValidationRules
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    public static string ValidateAndNormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new AppValidationException("Category name is required.");
        }

        var normalizedName = name.Trim();
        if (normalizedName.Length > NameMaxLength)
        {
            throw new AppValidationException($"Category name cannot exceed {NameMaxLength} characters.");
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
            throw new AppValidationException($"Category description cannot exceed {DescriptionMaxLength} characters.");
        }

        return normalizedDescription;
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
}
