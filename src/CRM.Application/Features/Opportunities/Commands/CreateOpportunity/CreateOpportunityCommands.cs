using CRM.Application.Features.Opportunities.DTOs;
using MediatR;

namespace CRM.Application.Features.Opportunities.Commands.CreateOpportunity;

public record CreateOpportunityCommand(
    string TenThuongVu, ulong? KhachHangId, ulong? LeadId,
    int TyLeThanhCong, decimal? DoanhThuKyVong,
    string? GhiChu, DateOnly? NgayDuKien
) : IRequest<OpportunityDto>;