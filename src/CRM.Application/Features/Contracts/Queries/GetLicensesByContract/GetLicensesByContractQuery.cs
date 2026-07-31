using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using MediatR;

namespace CRM.Application.Features.Contracts.Queries.GetLicensesByContract;

public record GetLicensesByContractQuery(ulong HopDongId) : IRequest<List<LicenseDto>>;

public class GetLicensesByContractQueryHandler
    : IRequestHandler<GetLicensesByContractQuery, List<LicenseDto>>
{
    private readonly ILicenseRepository _licenseRepository;
    public GetLicensesByContractQueryHandler(ILicenseRepository licenseRepository) =>
        _licenseRepository = licenseRepository;

    public Task<List<LicenseDto>> Handle(GetLicensesByContractQuery request, CancellationToken ct) =>
        _licenseRepository.GetByHopDongAsync(request.HopDongId, ct);
}
