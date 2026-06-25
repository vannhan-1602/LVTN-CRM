using CRM.Application.Features.Addresses.DTOs;
using MediatR;

namespace CRM.Application.Features.Addresses.Commands.CreateAddress;

public record CreateAddressCommand(
    ulong KhachHangId,
    string? DiaChiChiTiet,
    string? TinhThanh,
    string? QuanHuyen,
    string? PhuongXa,
    string LoaiDiaChi,
    bool IsDefault
) : IRequest<AddressDto>;