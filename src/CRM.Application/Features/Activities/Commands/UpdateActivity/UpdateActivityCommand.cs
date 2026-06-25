using CRM.Application.Features.Activities.DTOs;
using MediatR;

namespace CRM.Application.Features.Activities.Commands.UpdateActivity;

public record UpdateActivityCommand(
    ulong Id,
    string LoaiHoatDong,
    string? NoiDung,
    DateTime ThoiGianThucHien
) : IRequest<ActivityDto>;