using System;
using System.Collections.Generic;

namespace CRM.Application.Features.Contracts.DTOs;

public class ContractDto
{
    public ulong Id { get; set; }
    public string MaHopDong { get; set; } = string.Empty;
    public ulong KhachHangId { get; set; }
    public string? TenKhachHang { get; set; }
    public ulong? BaoGiaId { get; set; }
    public string? MaBaoGia { get; set; }
    public decimal? GiaTri { get; set; }
    public DateOnly? NgayKy { get; set; }
    public int? ThoiHan { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public string? HinhThucThanhToan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateContractFromQuoteRequestDto
{
    public ulong BaoGiaId { get; set; }
    public DateOnly? NgayKy { get; set; }
    public int? ThoiHan { get; set; }
    public string HinhThucThanhToan { get; set; } = "ThanhToanMotLan";
    public List<LichThanhToanInputDto> LichThanhToans { get; set; } = new();
}

public class LichThanhToanInputDto
{
    public int SoDot { get; set; }
    public decimal SoTien { get; set; }
    public DateOnly HanThanhToan { get; set; }
}

public class UpdateContractStatusRequestDto
{
    public string TrangThai { get; set; } = string.Empty;
}