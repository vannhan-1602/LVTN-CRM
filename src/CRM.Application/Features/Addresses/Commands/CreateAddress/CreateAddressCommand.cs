using CRM.Application.Features.Addresses.DTOs;
using MediatR;

namespace CRM.Application.Features.Addresses.Commands.CreateAddress;

public record CreateAddressCommand : IRequest<AddressDto>
{
    public ulong KhachHangId { get; set; }
    public string? DiaChiChiTiet { get; set; }
    public uint? TinhThanhId { get; set; }
    public uint? PhuongXaId { get; set; }
    public string LoaiDiaChi { get; set; } = "Office";
    public bool IsDefault { get; set; }
}