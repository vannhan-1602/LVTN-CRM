using CRM.Application.Common.Constants;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private const string AuditTableName = "KH_KhachHang";
    private const string AuditActionInsert = "INSERT";

    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ICurrentUserService currentUser,
        ILogger<CreateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var maKhachHang = await _customerRepository.GenerateMaKhachHangAsync(cancellationToken);

        // Sale tạo Customer luôn tự gán cho chính mình; Manager chỉ định tùy ý.
        var nhanVienPhuTrachId = _currentUser.Role == Roles.Sale
            ? _currentUser.UserId
            : request.NhanVienPhuTrachId;

        var customer = new KhachHang
        {
            MaKhachHang = maKhachHang,
            TenKhachHang = request.TenKhachHang.Trim(),
            LoaiKhachHangId = request.LoaiKhachHangId,
            TinhTrangId = request.TinhTrangId,
            Email = request.Email?.Trim(),
            SoDienThoai = request.SoDienThoai?.Trim(),
            MaSoThue = request.MaSoThue?.Trim(),
            NhanVienPhuTrachId = nhanVienPhuTrachId,
            NgaySinh = request.NgaySinh,
            NgayThanhLap = request.NgayThanhLap,
            HangKhachHangId = request.HangKhachHangId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        created = await _customerRepository.GetByMaKhachHangAsync(maKhachHang, cancellationToken)
            ?? created;

        var dto = CustomerMapper.ToDto(created);

        try
        {
            await _auditLogPublisher.PublishAsync(
                AuditTableName,
                created.Id,
                AuditActionInsert,
                oldData: null,
                newData: dto,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish audit log for customer {CustomerId}", created.Id);
        }

        return dto;
    }
}