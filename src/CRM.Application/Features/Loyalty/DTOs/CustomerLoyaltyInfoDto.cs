namespace CRM.Application.Features.Loyalty.DTOs;

public class CustomerLoyaltyInfoDto
{
    public ulong KhachHangId { get; set; }

    // ── Hạng hiện tại ────────────────────────────────────────────────────────
    public ushort? HangHienTaiId { get; set; }
    public string? TenHangHienTai { get; set; }
    public byte? ThuTuHangHienTai { get; set; }
    public decimal? PhanTramGiamVoucher { get; set; }
    public string? MoTaQuyenLoi { get; set; }

    // ── Tích lũy 12 tháng gần nhất (rolling window) ─────────────────────────
    public int TongDiem12Thang { get; set; }
    public int SoLanThu12Thang { get; set; }

    // ── Hạng kế tiếp (null nếu đã ở hạng cao nhất) ──────────────────────────
    public string? TenHangTiepTheo { get; set; }
    public int? SoDiemCanThemDeLenHang { get; set; }

    public List<DiemThuongDto> LichSuDiem { get; set; } = new();
    public List<LichSuHangDto> LichSuHang { get; set; } = new();
    public List<VoucherDto> Vouchers { get; set; } = new();
}

public class DiemThuongDto
{
    public ulong Id { get; set; }
    public int SoDiem { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public DateOnly NgayPhatSinh { get; set; }
    public string? GhiChu { get; set; }
}

public class LichSuHangDto
{
    public ulong Id { get; set; }
    public string? TenHangCu { get; set; }
    public string TenHangMoi { get; set; } = string.Empty;
    public string LyDo { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}

public class VoucherDto
{
    public ulong Id { get; set; }
    public string MaVoucher { get; set; } = string.Empty;
    public string LoaiGiamGia { get; set; } = string.Empty;
    public decimal GiaTriGiam { get; set; }
    public decimal? GiaTriGiamToiDa { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayHetHan { get; set; }
    public string LyDoPhatHanh { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    /// <summary>"ConHieuLuc" | "DaSuDung" | "HetHan" — tính sẵn ở backend cho frontend hiển thị badge.</summary>
    public string TrangThai { get; set; } = string.Empty;
}
