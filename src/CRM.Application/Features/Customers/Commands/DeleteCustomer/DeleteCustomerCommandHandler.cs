using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Customers.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Customers;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private const string AuditTableName = "KH_KhachHang";
    private const string AuditActionDelete = "DELETE";

    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<DeleteCustomerCommandHandler> _logger;

    public DeleteCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ILogger<DeleteCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(KhachHang), request.Id);

        var oldDto = CustomerMapper.ToDto(customer);

        var deleted = await _customerRepository.SoftDeleteAsync(request.Id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException(nameof(KhachHang), request.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _auditLogPublisher.PublishAsync(
                AuditTableName,
                request.Id,
                AuditActionDelete,
                oldData: oldDto,
                newData: new { IsDeleted = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish audit log for deleted customer {CustomerId}", request.Id);
        }

        return true;
    }
}
