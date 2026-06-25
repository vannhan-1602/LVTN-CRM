using CRM.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CRM.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.AdminOnly, policy =>
                policy.RequireRole(Roles.Admin));

            options.AddPolicy(Policies.ManagerOnly, policy =>
                policy.RequireRole(Roles.Manager));

            options.AddPolicy(Policies.SaleOnly, policy =>
                policy.RequireRole(Roles.Sale));

            options.AddPolicy(Policies.AccountantOnly, policy =>
                policy.RequireRole(Roles.Accountant));

            options.AddPolicy(Policies.AdminOrManager, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager));

           
            options.AddPolicy(Policies.SalesTeam, policy =>
                policy.RequireRole(Roles.Manager, Roles.Sale));

           
            options.AddPolicy(Policies.FinanceTeam, policy =>
                policy.RequireRole(Roles.Manager, Roles.Accountant));

            // Sale + Manager + Accountant: dùng cho API GET (chỉ đọc)
            // ở Customer/Contract. Accountant có quyền xem nhưng không thuộc SalesTeam.
            options.AddPolicy(Policies.CustomerReadAccess, policy =>
                policy.RequireRole(Roles.Manager, Roles.Sale, Roles.Accountant));
        });

        return services;
    }
}
