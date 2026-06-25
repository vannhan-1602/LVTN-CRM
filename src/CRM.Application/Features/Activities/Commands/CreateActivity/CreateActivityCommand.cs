using CRM.Application.Features.Activities.DTOs;
using MediatR;

namespace CRM.Application.Features.Activities.Commands.CreateActivity;

public record CreateActivityCommand(
    ulong? KhachHangId,
    ulong? LeadId,
    string LoaiHoatDong,
    string? NoiDung,
    DateTime ThoiGianThucHien
) : IRequest<ActivityDto>;