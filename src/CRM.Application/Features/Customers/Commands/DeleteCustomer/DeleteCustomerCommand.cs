using MediatR;

namespace CRM.Application.Features.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(ulong Id) : IRequest<bool>;
