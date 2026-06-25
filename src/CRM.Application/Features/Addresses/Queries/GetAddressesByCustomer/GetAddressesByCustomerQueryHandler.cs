using CRM.Application.Features.Addresses.DTOs;
using CRM.Application.Interfaces.Addresses;
using MediatR;

namespace CRM.Application.Features.Addresses.Queries.GetAddressesByCustomer;

public class GetAddressesByCustomerQueryHandler : IRequestHandler<GetAddressesByCustomerQuery, List<AddressDto>>
{
    private readonly IAddressRepository _addressRepository;

    public GetAddressesByCustomerQueryHandler(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public Task<List<AddressDto>> Handle(GetAddressesByCustomerQuery request, CancellationToken cancellationToken) =>
        _addressRepository.GetByKhachHangAsync(request.KhachHangId, cancellationToken);
}