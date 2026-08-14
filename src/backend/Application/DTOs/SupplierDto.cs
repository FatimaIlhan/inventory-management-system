namespace Application.DTOs;

public sealed record SupplierDto(
    long SupplierId,
    string CompanyName,
    string ContactPerson,
    string Phone,
    string Email,
    string Address,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);