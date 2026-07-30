using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Addresses.DTOs;
using CRM.Application.Interfaces.Addresses;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Features.Addresses.Commands.UpdateAddress;

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, AddressDto>
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUser;

    public UpdateAddressCommandHandler(IAddressRepository addressRepository,
        ICustomerRepository customerRepository, ICurrentUserService currentUser)
    {
        _addressRepository = addressRepository;
        _customerRepository = customerRepository;
        _currentUser = currentUser;
    }

    public async Task<AddressDto> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var existing = await _addressRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("DiaChi", request.Id);

        if (_currentUser.Role == Roles.Sale)
        {
            var khachHang = await _customerRepository.GetByIdAsync(existing.KhachHangId, cancellationToken)
                ?? throw new NotFoundException("Khách hàng", existing.KhachHangId);
            if (khachHang.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền sửa địa chỉ của khách hàng do nhân viên khác phụ trách.");
        }

        return await _addressRepository.UpdateAsync(
            request.Id,
            request.DiaChiChiTiet?.Trim(),
            request.TinhThanhId,
            request.PhuongXaId,
            request.LoaiDiaChi,
            request.IsDefault,
            cancellationToken);
    }
}