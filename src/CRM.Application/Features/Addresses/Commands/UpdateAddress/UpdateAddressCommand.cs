using CRM.Application.Features.Addresses.DTOs;
using MediatR;

namespace CRM.Application.Features.Addresses.Commands.UpdateAddress;

public record UpdateAddressCommand(
    ulong Id,
    string? DiaChiChiTiet,
    string? TinhThanh,
    string? QuanHuyen,
    string? PhuongXa,
    string LoaiDiaChi,
    bool IsDefault
) : IRequest<AddressDto>;