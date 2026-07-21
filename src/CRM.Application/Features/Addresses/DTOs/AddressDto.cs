namespace CRM.Application.Features.Addresses.DTOs;

public class AddressDto
{
    public ulong Id { get; set; }
    public ulong KhachHangId { get; set; }
    public string? DiaChiChiTiet { get; set; }
    public uint? TinhThanhId { get; set; }
    public string? TinhThanh { get; set; }
    public uint? PhuongXaId { get; set; }
    public string? PhuongXa { get; set; }
    public string? LoaiDiaChi { get; set; }
    public bool IsDefault { get; set; }
}