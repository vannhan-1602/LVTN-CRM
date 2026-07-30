using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Addresses.DTOs;
using CRM.Application.Interfaces.Addresses;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Features.Addresses.Commands.CreateAddress;

public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, AddressDto>
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUser;

    public CreateAddressCommandHandler(IAddressRepository addressRepository,
        ICustomerRepository customerRepository, ICurrentUserService currentUser)
    {
        _addressRepository = addressRepository;
        _customerRepository = customerRepository;
        _currentUser = currentUser;
    }

    public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var khachHang = await _customerRepository.GetByIdAsync(request.KhachHangId, cancellationToken)
            ?? throw new NotFoundException("Khách hàng", request.KhachHangId);
        if (_currentUser.Role == Roles.Sale && khachHang.NhanVienPhuTrachId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền thêm địa chỉ cho khách hàng của nhân viên khác.");

        return await _addressRepository.AddAsync(
            request.KhachHangId,
            request.DiaChiChiTiet?.Trim(),
            request.TinhThanhId,
            request.PhuongXaId,
            request.LoaiDiaChi,
            request.IsDefault,
            cancellationToken);
    }
}