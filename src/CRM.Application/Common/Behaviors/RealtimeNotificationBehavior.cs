using CRM.Application.Common.Constants;
using CRM.Application.Interfaces.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using INotificationPublisher = CRM.Application.Interfaces.Notifications.INotificationPublisher;
namespace CRM.Application.Common.Behaviors;

public class RealtimeNotificationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<RealtimeNotificationBehavior<TRequest, TResponse>> _logger;

    public RealtimeNotificationBehavior(
        INotificationPublisher notificationPublisher,
        ILogger<RealtimeNotificationBehavior<TRequest, TResponse>> logger)
    {
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    
    private static readonly Dictionary<string, (string EventName, string[] Roles)> CommandEventMap = new()
    {
        ["CreateCustomerCommand"] = ("customer:created", new[] { Roles.Sale, Roles.Manager }),
        ["UpdateCustomerCommand"] = ("customer:updated", new[] { Roles.Sale, Roles.Manager }),
        ["DeleteCustomerCommand"] = ("customer:deleted", new[] { Roles.Sale, Roles.Manager }),
        ["RestoreCustomerCommand"] = ("customer:restored", new[] { Roles.Sale, Roles.Manager }),

        ["CreateLeadCommand"] = ("lead:created", new[] { Roles.Sale, Roles.Manager }),
       
        ["UpdateLeadCommand"] = ("lead:updated", new[] { Roles.Sale, Roles.Manager }),
        ["DeleteLeadCommand"] = ("lead:deleted", new[] { Roles.Sale, Roles.Manager }),
        ["RestoreLeadCommand"] = ("lead:restored", new[] { Roles.Sale, Roles.Manager }),
        ["AssignLeadCommand"] = ("lead:assigned", new[] { Roles.Sale, Roles.Manager }),
        ["ConvertLeadCommand"] = ("lead:converted", new[] { Roles.Sale, Roles.Manager }),

        ["ChangeOpportunityStageCommand"] = ("opportunity:stage_changed", new[] { Roles.Sale, Roles.Manager }),
        ["DeleteOpportunityCommand"] = ("opportunity:deleted", new[] { Roles.Sale, Roles.Manager }),

        ["CreateQuoteCommand"] = ("quote:created", new[] { Roles.Sale, Roles.Manager }),
        ["UpdateQuoteCommand"] = ("quote:updated", new[] { Roles.Sale, Roles.Manager }),
        ["DeleteQuoteCommand"] = ("quote:deleted", new[] { Roles.Sale, Roles.Manager }),
        ["SendQuoteCommand"] = ("quote:sent", new[] { Roles.Sale, Roles.Manager }),
        ["AcceptQuoteCommand"] = ("quote:accepted", new[] { Roles.Sale, Roles.Manager, Roles.Accountant }),
        ["RejectQuoteCommand"] = ("quote:rejected", new[] { Roles.Sale, Roles.Manager }),

        ["CreateContractFromQuoteCommand"] = ("contract:created", new[] { Roles.Sale, Roles.Manager, Roles.Accountant }),
        ["CreateRenewalContractCommand"] = ("contract:created", new[] { Roles.Sale, Roles.Manager, Roles.Accountant }),
        ["UpdateContractStatusCommand"] = ("contract:status_changed", new[] { Roles.Sale, Roles.Manager, Roles.Accountant }),
        ["DeleteContractCommand"] = ("contract:deleted", new[] { Roles.Manager, Roles.Accountant }),
        ["CreateMilestoneCommand"] = ("contract:milestone_changed", new[] { Roles.Sale, Roles.Manager }),
        ["UpdateMilestoneCommand"] = ("contract:milestone_changed", new[] { Roles.Sale, Roles.Manager }),
        ["DeleteMilestoneCommand"] = ("contract:milestone_changed", new[] { Roles.Sale, Roles.Manager }),
        ["CreateLicenseCommand"] = ("contract:license_changed", new[] { Roles.Sale, Roles.Manager }),
        ["RenewLicenseCommand"] = ("contract:license_changed", new[] { Roles.Sale, Roles.Manager }),
        ["ToggleLicenseLockCommand"] = ("contract:license_changed", new[] { Roles.Sale, Roles.Manager }),

        ["CreateInvoiceCommand"] = ("invoice:created", new[] { Roles.Accountant, Roles.Manager }),

        ["CreatePhieuThuChiCommand"] = ("phieuthuchi:created", new[] { Roles.Accountant, Roles.Manager }),

        ["CreateProductCommand"] = ("product:created", new[] { Roles.Sale, Roles.Manager, Roles.Admin }),
        ["UpdateProductCommand"] = ("product:updated", new[] { Roles.Sale, Roles.Manager, Roles.Admin }),
        ["DeleteProductCommand"] = ("product:deleted", new[] { Roles.Sale, Roles.Manager, Roles.Admin }),
        ["UpdateStockCommand"] = ("product:stock_changed", new[] { Roles.Sale, Roles.Manager, Roles.Admin }),

        ["CreateTicketCommand"] = ("ticket:created", new[] { Roles.Sale, Roles.Manager }),
        ["UpdateTicketCommand"] = ("ticket:updated", new[] { Roles.Sale, Roles.Manager }),
        ["DeleteTicketCommand"] = ("ticket:deleted", new[] { Roles.Sale, Roles.Manager }),
        ["AssignTicketCommand"] = ("ticket:assigned", new[] { Roles.Sale, Roles.Manager }),
        ["CloseTicketCommand"] = ("ticket:closed", new[] { Roles.Sale, Roles.Manager }),
        ["AddPhanHoiCommand"] = ("ticket:reply_added", new[] { Roles.Sale, Roles.Manager }),

        ["RedeemVoucherCommand"] = ("loyalty:voucher_redeemed", new[] { Roles.Sale, Roles.Manager }),

        ["CreateUserCommand"] = ("user:created", new[] { Roles.Admin }),
        ["UpdateUserCommand"] = ("user:updated", new[] { Roles.Admin }),
        ["DeleteUserCommand"] = ("user:deleted", new[] { Roles.Admin }),
        ["ToggleUserStatusCommand"] = ("user:status_changed", new[] { Roles.Admin }),
    };

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        var requestName = typeof(TRequest).Name;
        if (CommandEventMap.TryGetValue(requestName, out var mapping))
        {
            try
            {
                foreach (var role in mapping.Roles)
                    await _notificationPublisher.NotifyRoleAsync(role, mapping.EventName, new { }, cancellationToken);
            }
            catch (Exception ex)
            {
              
                _logger.LogWarning(ex, "Realtime notify failed for {Request}", requestName);
            }
        }

        return response;
    }
}