using MediatR;

namespace CRM.Application.Features.Customers.Commands.RestoreCustomer;

public record RestoreCustomerCommand(ulong Id) : IRequest<bool>;
