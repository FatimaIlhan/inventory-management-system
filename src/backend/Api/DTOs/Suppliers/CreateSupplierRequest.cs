namespace Api.DTOs.Suppliers;

public sealed record CreateSupplierRequest(
    string CompanyName,
    string ContactPerson,
    string Phone,
    string Email,
    string Address);