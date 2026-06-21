namespace CRM.Application.Common.Constants;

public static class Policies
{
    public const string AdminOnly = "AdminOnly"; // Admin only
    public const string ManagerOnly = "ManagerOnly"; // Manager only
    public const string SaleOnly = "SaleOnly"; // Sale only
    public const string AccountantOnly = "AccountantOnly"; // Accountant only
    public const string AdminOrManager = "AdminOrManager";  // Admin + Manager
    public const string SalesTeam = "SalesTeam";       // Sale + Manager 
    public const string FinanceTeam = "FinanceTeam";   // Accountant + Manager
    public const string CustomerReadAccess = "CustomerReadAccess"; // Sale + Manager + Accountant
}
