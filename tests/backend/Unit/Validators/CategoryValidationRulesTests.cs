using Application.Exceptions;
using Application.Validators;

namespace Unit.Validators;

public sealed class CategoryValidationRulesTests
{
    [Fact]
    public void ValidateAndNormalizeName_ShouldThrow_WhenNameIsMissing()
    {
        var action = () => CategoryValidationRules.ValidateAndNormalizeName("   ");

        var exception = Assert.Throws<AppValidationException>(action);
        Assert.Equal("Category name is required.", exception.Message);
    }

    [Fact]
    public void ValidateAndNormalizeName_ShouldThrow_WhenNameExceedsLimit()
    {
        var tooLongName = new string('a', CategoryValidationRules.NameMaxLength + 1);

        var action = () => CategoryValidationRules.ValidateAndNormalizeName(tooLongName);

        var exception = Assert.Throws<AppValidationException>(action);
        Assert.Equal($"Category name cannot exceed {CategoryValidationRules.NameMaxLength} characters.", exception.Message);
    }

    [Fact]
    public void ValidateAndNormalizeName_ShouldTrim_WhenNameIsValid()
    {
        var normalizedName = CategoryValidationRules.ValidateAndNormalizeName("  Electronics  ");

        Assert.Equal("Electronics", normalizedName);
    }

    [Fact]
    public void ValidateAndNormalizeDescription_ShouldReturnNull_WhenDescriptionIsEmpty()
    {
        var normalizedDescription = CategoryValidationRules.ValidateAndNormalizeDescription("   ");

        Assert.Null(normalizedDescription);
    }

    [Fact]
    public void ValidateAndNormalizeDescription_ShouldTrim_WhenDescriptionIsValid()
    {
        var normalizedDescription = CategoryValidationRules.ValidateAndNormalizeDescription("  Office supplies and paper goods  ");

        Assert.Equal("Office supplies and paper goods", normalizedDescription);
    }
}
