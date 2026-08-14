namespace Application.DTOs;

public sealed record UpdateSupplierDto(
    string CompanyName,
    string ContactPerson,
    string Phone,
    string Email,
    string Address);