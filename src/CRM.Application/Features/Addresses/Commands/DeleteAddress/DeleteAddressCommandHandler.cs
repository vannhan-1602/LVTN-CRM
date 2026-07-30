using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Addresses;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using MediatR;

namespace CRM.Application.Features.Addresses.Commands.DeleteAddress;

public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, bool>
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUser;

    public DeleteAddressCommandHandler(IAddressRepository addressRepository,
        ICustomerRepository customerRepository, ICurrentUserService currentUser)
    {
        _addressRepository = addressRepository;
        _customerRepository = customerRepository;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var existing = await _addressRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("DiaChi", request.Id);

        if (_currentUser.Role == Roles.Sale)
        {
            var khachHang = await _customerRepository.GetByIdAsync(existing.KhachHangId, cancellationToken)
                ?? throw new NotFoundException("Khách hàng", existing.KhachHangId);
            if (khachHang.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền xóa địa chỉ của khách hàng do nhân viên khác phụ trách.");
        }

        var deleted = await _addressRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted) throw new NotFoundException("DiaChi", request.Id);
        return true;
    }
}