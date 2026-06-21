using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Users;
using MediatR;

namespace CRM.Application.Features.Users.Queries.GetLookups;

public record GetUserLookupsQuery : IRequest<UserLookupsDto>;

public class GetUserLookupsQueryHandler : IRequestHandler<GetUserLookupsQuery, UserLookupsDto>
{
    private readonly IUserManagementRepository _repository;
    public GetUserLookupsQueryHandler(IUserManagementRepository repository) => _repository = repository;

    public Task<UserLookupsDto> Handle(GetUserLookupsQuery request, CancellationToken ct) =>
        _repository.GetLookupsAsync(ct);
}
