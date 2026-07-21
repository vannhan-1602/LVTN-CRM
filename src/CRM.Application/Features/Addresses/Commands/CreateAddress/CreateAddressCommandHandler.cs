using CRM.Application.Features.Addresses.DTOs;
using CRM.Application.Interfaces.Addresses;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Features.Addresses.Commands.CreateAddress;

public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, AddressDto>
{
    private readonly IAddressRepository _addressRepository;

    public CreateAddressCommandHandler(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken) =>
        _addressRepository.AddAsync(
            request.KhachHangId,
            request.DiaChiChiTiet?.Trim(),
            request.TinhThanhId,
            request.PhuongXaId,
            request.LoaiDiaChi,
            request.IsDefault,
            cancellationToken);
}