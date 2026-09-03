using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Customers;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Customers.Commands.RestoreCustomer;

public class RestoreCustomerCommandHandler : IRequestHandler<RestoreCustomerCommand, bool>
{
    private const string AuditTableName = "KH_KhachHang";
    private const string AuditActionRestore = "RESTORE";

    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<RestoreCustomerCommandHandler> _logger;

    public RestoreCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ILogger<RestoreCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(RestoreCustomerCommand request, CancellationToken cancellationToken)
    {
      
        var existing = await _customerRepository.GetByIdEnrichedAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(KhachHang), request.Id);

       
        if (!string.IsNullOrWhiteSpace(existing.Email))
        {
            var duplicates = await _customerRepository.FindDuplicatesAsync(
                existing.Email, null, null, request.Id, cancellationToken);
            if (duplicates.Count > 0)
                throw new BusinessRuleException(
                    $"Không thể khôi phục — email '{existing.Email}' hiện đã thuộc về khách hàng " +
                    $"'{duplicates[0].TenKhachHang}' ({duplicates[0].MaKhachHang}) đang hoạt động.");
        }

        var restored = await _customerRepository.RestoreAsync(request.Id, cancellationToken);
        if (!restored)
        {
           
            throw new NotFoundException(nameof(KhachHang), request.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _auditLogPublisher.PublishAsync(
                AuditTableName,
                request.Id,
                AuditActionRestore,
                oldData: new { IsDeleted = true },
                newData: new { IsDeleted = false },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish audit log for restored customer {CustomerId}", request.Id);
        }

        return true;
    }
}
