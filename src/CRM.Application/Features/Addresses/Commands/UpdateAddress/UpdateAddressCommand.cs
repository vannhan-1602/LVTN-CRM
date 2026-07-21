using CRM.Application.Features.Addresses.DTOs;
using MediatR;

namespace CRM.Application.Features.Addresses.Commands.UpdateAddress;

public record UpdateAddressCommand : IRequest<AddressDto>
{
    public ulong Id { get; set; }
    public string? DiaChiChiTiet { get; set; }
    public uint? TinhThanhId { get; set; }
    public uint? PhuongXaId { get; set; }
    public string LoaiDiaChi { get; set; } = "Office";
    public bool IsDefault { get; set; }
}