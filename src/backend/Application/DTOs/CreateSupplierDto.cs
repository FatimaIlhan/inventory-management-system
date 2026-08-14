namespace Application.DTOs;

public sealed record CreateSupplierDto(
    string CompanyName,
    string ContactPerson,
    string Phone,
    string Email,
    string Address);