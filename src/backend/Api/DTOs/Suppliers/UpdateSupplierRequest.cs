namespace Api.DTOs.Suppliers;

public sealed record UpdateSupplierRequest(
    string CompanyName,
    string ContactPerson,
    string Phone,
    string Email,
    string Address);