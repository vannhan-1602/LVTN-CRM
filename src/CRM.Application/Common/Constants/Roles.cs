namespace CRM.Application.Common.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Sale = "Sale";
    public const string Accountant = "Accountant";

    public static readonly IReadOnlyList<string> All = [Admin, Manager, Sale, Accountant];
}
