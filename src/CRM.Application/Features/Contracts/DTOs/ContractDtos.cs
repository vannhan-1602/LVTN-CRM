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

    /// <summary>Tổng SoTienDaThu cộng dồn từ mọi hóa đơn của hợp đồng — dùng để cảnh báo "đã thu đủ, có thể thanh lý".</summary>
    public decimal TongDaThu { get; set; }
    public DateOnly? NgayKy { get; set; }
    public int? ThoiHan { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public string? HinhThucThanhToan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>ChinhThuc | GiaHan | BaoTri</summary>
    public string LoaiHopDong { get; set; } = "ChinhThuc";
    public ulong? HopDongGocId { get; set; }
    public string? MaHopDongGoc { get; set; }
    public DateOnly? NgayNhacGiaHanCuoi { get; set; }

    /// <summary>Các hợp đồng gia hạn/bảo trì được tạo ra TỪ hợp đồng này (badge liên kết 2 chiều) — chỉ populate khi xem chi tiết.</summary>
    public List<ContractRenewalLinkDto> HopDongLienKet { get; set; } = new();
}

public class ContractRenewalLinkDto
{
    public ulong Id { get; set; }
    public string MaHopDong { get; set; } = string.Empty;
    public string LoaiHopDong { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}

public class CreateRenewalContractRequestDto
{
    public DateOnly? NgayKy { get; set; }
}

// ── Mốc triển khai (Đào tạo / Bàn giao / Nghiệm thu) ─────────────────────────

public class MocTrienKhaiDto
{
    public ulong Id { get; set; }
    public ulong HopDongId { get; set; }

    /// <summary>DaoTao | BanGiao | NghiemThu</summary>
    public string LoaiMoc { get; set; } = string.Empty;
    public string? NoiDung { get; set; }
    public DateTime? NgayThucHien { get; set; }
    public uint? NhanVienThucHienId { get; set; }
    public string? TenNhanVienThucHien { get; set; }
    public string? NguoiXacNhanKhach { get; set; }
    public string? FileBienBan { get; set; }

    /// <summary>ChuaThucHien | DaThucHien | DaXacNhan</summary>
    public string TrangThai { get; set; } = "ChuaThucHien";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateMocTrienKhaiRequestDto
{
    public string LoaiMoc { get; set; } = string.Empty;
    public string? NoiDung { get; set; }
    public DateTime? NgayThucHien { get; set; }
    public uint? NhanVienThucHienId { get; set; }
}

public class UpdateMocTrienKhaiRequestDto
{
    public string? NoiDung { get; set; }
    public DateTime? NgayThucHien { get; set; }
    public uint? NhanVienThucHienId { get; set; }
    public string? NguoiXacNhanKhach { get; set; }
    public string? FileBienBan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
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

public class LichThanhToanDto
{
    public ulong Id { get; set; }
    public ulong HopDongId { get; set; }
    public int SoDot { get; set; }
    public decimal SoTien { get; set; }
    public DateOnly HanThanhToan { get; set; }
    public string TrangThai { get; set; } = "ChuaDenHan";

    /// <summary>Đợt này đã có hóa đơn nào trỏ tới chưa — dùng để ẩn khỏi dropdown chọn đợt khi tạo hóa đơn mới.</summary>
    public bool DaCoHoaDon { get; set; }
}

public class UpdateContractStatusRequestDto
{
    public string TrangThai { get; set; } = string.Empty;
}