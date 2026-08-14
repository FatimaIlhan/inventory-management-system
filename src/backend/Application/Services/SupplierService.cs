using Application.Interfaces;
using Application.DTOs;
using Application.Validators;
using Domain.Entities;
using Application.Exceptions;
namespace Application.Services;

public sealed class SupplierService(
    ISupplierRepository supplierRepository,
    TimeProvider timeProvider) : ISupplierService
{
    
    public async Task<PagedResultDto<SupplierDto>> GetPagedAsync(
    int page,
    int pageSize,
    string? search,
    CancellationToken cancellationToken)
{
    var validatedPage = SupplierValidationRules.ValidatePage(page);
    var validatedPageSize = SupplierValidationRules.ValidatePageSize(pageSize);

    var (items, totalCount) = await supplierRepository.GetPagedAsync(
        validatedPage,
        validatedPageSize,
        search,
        cancellationToken);

    return new PagedResultDto<SupplierDto>(
        items.Select(ToDto).ToList(),
        validatedPage,
        validatedPageSize,
        totalCount);
}


public async Task<SupplierDto> GetByIdAsync(
    long supplierId,
    CancellationToken cancellationToken)
{
    var supplier = await supplierRepository.GetByIdAsync(
        supplierId,
        cancellationToken);

    if (supplier is null)
    {
        throw new NotFoundException("Supplier was not found.");
    }

    return ToDto(supplier);
}
public async Task<SupplierDto> CreateAsync(
    CreateSupplierDto createSupplierDto,
    CancellationToken cancellationToken)
{
    var normalizedCompanyName =
        SupplierValidationRules.ValidateAndNormalizeCompanyName(
            createSupplierDto.CompanyName);

    var normalizedContactPerson =
        SupplierValidationRules.ValidateAndNormalizeContactPerson(
            createSupplierDto.ContactPerson);

    var normalizedPhone =
        SupplierValidationRules.ValidateAndNormalizePhone(
            createSupplierDto.Phone);

    var normalizedEmail =
        SupplierValidationRules.ValidateAndNormalizeEmail(
            createSupplierDto.Email);

    var normalizedAddress =
        SupplierValidationRules.ValidateAndNormalizeAddress(
            createSupplierDto.Address);

    await EnsureCompanyNameIsUniqueAsync(
        normalizedCompanyName,
        null,
        cancellationToken);

    var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

    var supplier = new Supplier
    {
        CompanyName = normalizedCompanyName,
        ContactPerson = normalizedContactPerson,
        Phone = normalizedPhone,
        Email = normalizedEmail,
        Address = normalizedAddress,
        CreatedAtUtc = nowUtc,
        UpdatedAtUtc = null
    };

    var createdSupplier = await supplierRepository.CreateAsync(
        supplier,
        cancellationToken);

    return ToDto(createdSupplier);
}
private async Task EnsureCompanyNameIsUniqueAsync(
    string companyName,
    long? excludedSupplierId,
    CancellationToken cancellationToken)
{
    var companyNameTaken =
        await supplierRepository.ExistsByCompanyNameAsync(
            companyName,
            excludedSupplierId,
            cancellationToken);

    if (companyNameTaken)
    {
        throw new ConflictException(
            "Supplier company name already exists.");
    }
}


public async Task<SupplierDto> UpdateAsync(
    long supplierId,
    UpdateSupplierDto updateSupplierDto,
    CancellationToken cancellationToken)
{
    var supplier = await supplierRepository.GetByIdAsync(
        supplierId,
        cancellationToken);

    if (supplier is null)
    {
        throw new NotFoundException("Supplier was not found.");
    }

    var normalizedCompanyName =
        SupplierValidationRules.ValidateAndNormalizeCompanyName(
            updateSupplierDto.CompanyName);

    var normalizedContactPerson =
        SupplierValidationRules.ValidateAndNormalizeContactPerson(
            updateSupplierDto.ContactPerson);

    var normalizedPhone =
        SupplierValidationRules.ValidateAndNormalizePhone(
            updateSupplierDto.Phone);

    var normalizedEmail =
        SupplierValidationRules.ValidateAndNormalizeEmail(
            updateSupplierDto.Email);

    var normalizedAddress =
        SupplierValidationRules.ValidateAndNormalizeAddress(
            updateSupplierDto.Address);

    await EnsureCompanyNameIsUniqueAsync(
        normalizedCompanyName,
        supplierId,
        cancellationToken);

    supplier.CompanyName = normalizedCompanyName;
    supplier.ContactPerson = normalizedContactPerson;
    supplier.Phone = normalizedPhone;
    supplier.Email = normalizedEmail;
    supplier.Address = normalizedAddress;
    supplier.UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

    await supplierRepository.UpdateAsync(
        supplier,
        cancellationToken);

    return ToDto(supplier);
}

public async Task DeleteAsync(
    long supplierId,
    CancellationToken cancellationToken)
{
    var supplier = await supplierRepository.GetByIdAsync(
        supplierId,
        cancellationToken);

    if (supplier is null)
    {
        throw new NotFoundException("Supplier was not found.");
    }

    await supplierRepository.DeleteAsync(
        supplierId,
        cancellationToken);
}
private static SupplierDto ToDto(Supplier supplier) =>
    new(
        supplier.SupplierId,
        supplier.CompanyName,
        supplier.ContactPerson,
        supplier.Phone,
        supplier.Email,
        supplier.Address,
        supplier.CreatedAtUtc,
        supplier.UpdatedAtUtc);
}

