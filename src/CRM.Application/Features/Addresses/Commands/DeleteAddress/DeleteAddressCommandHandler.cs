using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Addresses;
using MediatR;

namespace CRM.Application.Features.Addresses.Commands.DeleteAddress;

public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, bool>
{
    private readonly IAddressRepository _addressRepository;

    public DeleteAddressCommandHandler(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<bool> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _addressRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted) throw new NotFoundException("DiaChi", request.Id);
        return true;
    }
}