using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Addresses.DTOs;
using CRM.Application.Interfaces.Addresses;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Features.Addresses.Commands.UpdateAddress;

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, AddressDto>
{
    private readonly IAddressRepository _addressRepository;

    public UpdateAddressCommandHandler(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<AddressDto> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        _ = await _addressRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("DiaChi", request.Id);

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