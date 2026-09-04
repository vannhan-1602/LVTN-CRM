using CRM.Application.Common.Constants;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Leads;
using CRM.Application.Interfaces.Notifications;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using INotificationPublisher = CRM.Application.Interfaces.Notifications.INotificationPublisher;

namespace CRM.Application.Features.Leads.Commands.CreatePublicLead;


public class CreatePublicLeadCommandHandler : IRequestHandler<CreatePublicLeadCommand, ulong>
{
    private const string AuditTable = "KH_Lead";
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<CreatePublicLeadCommandHandler> _logger;

    public CreatePublicLeadCommandHandler(
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        INotificationPublisher notificationPublisher,
        ILogger<CreatePublicLeadCommandHandler> logger)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task<ulong> Handle(CreatePublicLeadCommand request, CancellationToken ct)
    {
        var lead = new Lead
        {
            TenLead = request.TenLead.Trim(),
            TenCongTy = request.TenCongTy?.Trim(),
            SoDienThoai = request.SoDienThoai?.Trim(),
            Email = request.Email?.Trim(),
            NguonLead = "Website",
            TinhTrang = LeadTinhTrang.Moi,
            NhanVienPhuTrachId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _leadRepository.AddAsync(lead, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: created, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for public lead {Id}", created.Id); }

       
        var notification = new { LeadId = created.Id, created.TenLead, created.SoDienThoai };
        try
        {
            await _notificationPublisher.NotifyRoleAsync(Roles.Sale, "lead:new", notification, ct);
            await _notificationPublisher.NotifyRoleAsync(Roles.Manager, "lead:new", notification, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Realtime notify failed for public lead {Id}", created.Id); }

        return created.Id;
    }
}