using MediatR;

namespace CRM.Application.Features.Addresses.Commands.DeleteAddress;

public record DeleteAddressCommand(ulong Id) : IRequest<bool>;