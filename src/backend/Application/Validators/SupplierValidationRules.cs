using System.Net.Mail;
using Application.Exceptions;

namespace Application.Validators;

public static class SupplierValidationRules
{
    public const int CompanyNameMaxLength = 200;
    public const int ContactPersonMaxLength = 100;
    public const int PhoneMaxLength = 20;
    public const int EmailMaxLength = 255;
    public const int AddressMaxLength = 300;

    public static string ValidateAndNormalizeCompanyName(string? companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new AppValidationException("Supplier company name is required.");
        }

        var normalizedCompanyName = companyName.Trim();

        if (normalizedCompanyName.Length > CompanyNameMaxLength)
        {
            throw new AppValidationException(
                $"Supplier company name cannot exceed {CompanyNameMaxLength} characters.");
        }

        return normalizedCompanyName;
    }

    public static string ValidateAndNormalizeContactPerson(string? contactPerson)
    {
        if (string.IsNullOrWhiteSpace(contactPerson))
        {
            throw new AppValidationException("Supplier contact person is required.");
        }

        var normalizedContactPerson = contactPerson.Trim();

        if (normalizedContactPerson.Length > ContactPersonMaxLength)
        {
            throw new AppValidationException(
                $"Supplier contact person cannot exceed {ContactPersonMaxLength} characters.");
        }

        return normalizedContactPerson;
    }

    public static string ValidateAndNormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new AppValidationException("Supplier phone number is required.");
        }

        var normalizedPhone = phone.Trim();

        if (normalizedPhone.Length > PhoneMaxLength)
        {
            throw new AppValidationException(
                $"Supplier phone number cannot exceed {PhoneMaxLength} characters.");
        }

        return normalizedPhone;
    }

    public static string ValidateAndNormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new AppValidationException("Supplier email is required.");
        }

        var normalizedEmail = email.Trim();

        if (normalizedEmail.Length > EmailMaxLength)
        {
            throw new AppValidationException(
                $"Supplier email cannot exceed {EmailMaxLength} characters.");
        }

        try
        {
            var mailAddress = new MailAddress(normalizedEmail);

            if (!string.Equals(
                    mailAddress.Address,
                    normalizedEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new AppValidationException("Supplier email must be valid.");
            }
        }
        catch (FormatException)
        {
            throw new AppValidationException("Supplier email must be valid.");
        }

        return normalizedEmail;
    }

    public static string ValidateAndNormalizeAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new AppValidationException("Supplier address is required.");
        }

        var normalizedAddress = address.Trim();

        if (normalizedAddress.Length > AddressMaxLength)
        {
            throw new AppValidationException(
                $"Supplier address cannot exceed {AddressMaxLength} characters.");
        }

        return normalizedAddress;
    }

    public static int ValidatePage(int page)
    {
        if (page < 1)
        {
            throw new AppValidationException(
                "Page must be greater than or equal to 1.");
        }

        return page;
    }

    public static int ValidatePageSize(int pageSize)
    {
        if (pageSize < 1 || pageSize > 100)
        {
            throw new AppValidationException(
                "Page size must be between 1 and 100.");
        }

        return pageSize;
    }
}