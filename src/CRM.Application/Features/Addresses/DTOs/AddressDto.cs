namespace CRM.Application.Features.Addresses.DTOs;

public class AddressDto
{
    public ulong Id { get; set; }
    public ulong KhachHangId { get; set; }
    public string? DiaChiChiTiet { get; set; }
    public string? TinhThanh { get; set; }
    public string? QuanHuyen { get; set; }
    public string? PhuongXa { get; set; }
    public string? LoaiDiaChi { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateAddressRequestDto
{
    public string? DiaChiChiTiet { get; set; }
    public string? TinhThanh { get; set; }
    public string? QuanHuyen { get; set; }
    public string? PhuongXa { get; set; }
    public string LoaiDiaChi { get; set; } = "Office";
    public bool IsDefault { get; set; }
}

public class UpdateAddressRequestDto
{
    public string? DiaChiChiTiet { get; set; }
    public string? TinhThanh { get; set; }
    public string? QuanHuyen { get; set; }
    public string? PhuongXa { get; set; }
    public string LoaiDiaChi { get; set; } = "Office";
    public bool IsDefault { get; set; }
}