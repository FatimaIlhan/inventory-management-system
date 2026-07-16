namespace Domain.Enums;

public static class UserRole
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public static readonly IReadOnlyCollection<string> All = new[] { Admin, Manager, Employee };
}
