using CRM.Application.Features.Opportunities.DTOs;
using MediatR;

namespace CRM.Application.Features.Opportunities.Commands.UpdateOpportunity;

public record UpdateOpportunityCommand(
    ulong Id, string TenThuongVu, ulong? KhachHangId, ulong? LeadId,
    int TyLeThanhCong, decimal? DoanhThuKyVong,
    string? GhiChu, DateOnly? NgayDuKien,
    uint? NhanVienPhuTrachId = null
) : IRequest<OpportunityDto>;