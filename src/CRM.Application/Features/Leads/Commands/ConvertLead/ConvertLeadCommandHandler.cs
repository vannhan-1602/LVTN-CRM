using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Leads.Commands.ConvertLead
{
    public class ConvertLeadCommandHandler : IRequestHandler<ConvertLeadCommand, CustomerDto>
    {
        private readonly ILeadRepository _leadRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ConvertLeadCommandHandler> _logger;

        public ConvertLeadCommandHandler(
            ILeadRepository leadRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher,
            ICurrentUserService currentUser,
            ILogger<ConvertLeadCommandHandler> logger)
        {
            _leadRepository = leadRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<CustomerDto> Handle(ConvertLeadCommand request, CancellationToken cancellationToken)
        {
            var lead = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken)
                ?? throw new NotFoundException(nameof(Lead), request.LeadId);

            // Chặn Sale chuyển đổi Lead không phải mình phụ trách
            if (_currentUser.Role == Roles.Sale && lead.NhanVienPhuTrachId != _currentUser.NhanSuId)
                throw new ForbiddenException("Bạn không có quyền thao tác trên dữ liệu của nhân viên khác.");

            if (lead.TinhTrang == LeadTinhTrang.DaChuyenDoi)
                throw new BusinessRuleException("Lead này đã được chuyển đổi thành khách hàng.");

            var maKhachHang = await _customerRepository.GenerateMaKhachHangAsync(cancellationToken);
            var customer = new KhachHang
            {
                MaKhachHang = maKhachHang,
                TenKhachHang = lead.TenCongTy ?? lead.TenLead,
                Email = lead.Email,
                SoDienThoai = lead.SoDienThoai,
                LoaiKhachHangId = request.LoaiKhachHangId,
                TinhTrangId = request.TinhTrangKhachHangId,
                NhanVienPhuTrachId = lead.NhanVienPhuTrachId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            lead.TinhTrang = LeadTinhTrang.DaChuyenDoi;
            lead.UpdatedAt = DateTime.UtcNow;

            var createdCustomer = await _customerRepository.AddAsync(customer, cancellationToken);
            await _leadRepository.UpdateAsync(lead, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            createdCustomer = await _customerRepository.GetByMaKhachHangAsync(maKhachHang, cancellationToken)
                ?? createdCustomer;

            var dto = CustomerMapper.ToDto(createdCustomer);

            try
            {
                await _auditLogPublisher.PublishAsync("KH_KhachHang", createdCustomer.Id, "INSERT",
                    oldData: null, newData: dto, cancellationToken);
                await _auditLogPublisher.PublishAsync("KH_Lead", lead.Id, "UPDATE",
                    oldData: new { TinhTrang = "Cũ" }, newData: new { lead.TinhTrang }, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for ConvertLead {Id}", request.LeadId); }

            return dto;
        }
    }
}
