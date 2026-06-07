using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Customers;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private const string AuditTableName = "KH_KhachHang";
    private const string AuditActionUpdate = "UPDATE";

    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ILogger<UpdateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(KhachHang), request.Id);

        var oldDto = CustomerMapper.ToDto(customer);

        customer.TenKhachHang = request.TenKhachHang.Trim();
        customer.LoaiKhachHangId = request.LoaiKhachHangId;
        customer.TinhTrangId = request.TinhTrangId;
        customer.Email = request.Email?.Trim();
        customer.SoDienThoai = request.SoDienThoai?.Trim();
        customer.MaSoThue = request.MaSoThue?.Trim();
        customer.NhanVienPhuTrachId = request.NhanVienPhuTrachId;
        customer.UpdatedAt = DateTime.UtcNow;

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var newDto = CustomerMapper.ToDto(customer);

        try
        {
            await _auditLogPublisher.PublishAsync(
                AuditTableName,
                customer.Id,
                AuditActionUpdate,
                oldData: oldDto,
                newData: newDto,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish audit log for customer {CustomerId}", customer.Id);
        }

        return newDto;
    }
}
