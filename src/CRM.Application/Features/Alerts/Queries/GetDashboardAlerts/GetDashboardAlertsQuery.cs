using CRM.Application.Features.Alerts.DTOs;
using MediatR;

namespace CRM.Application.Features.Alerts.Queries.GetDashboardAlerts;

public record GetDashboardAlertsQuery : IRequest<DashboardAlertsDto>;
