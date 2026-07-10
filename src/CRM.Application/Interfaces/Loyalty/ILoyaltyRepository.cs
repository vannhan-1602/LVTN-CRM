using CRM.Domain.Entities.Loyalty;

namespace CRM.Application.Interfaces.Loyalty;

/// <summary>
/// Repository cho toàn bộ nghiệp vụ Loyalty (Tích điểm, Xếp hạng, Voucher).
/// Tất cả logic nằm trong C# — không dùng trigger DB.
/// </summary>
public interface ILoyaltyRepository
{
    // ── Điểm ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Cộng điểm cho khách hàng sau khi phiếu thu được xác nhận.
    /// Tỷ lệ: 100.000 VNĐ = 1 điểm. Chỉ insert nếu chưa có bản ghi cho PhieuThuChiId này
    /// (idempotent — tránh cộng 2 lần nếu request bị gọi lại).
    /// </summary>
    Task<int> CongDiemAsync(ulong khachHangId, decimal soTienThu,
        ulong hoaDonId, ulong phieuThuChiId, CancellationToken ct = default);

    /// <summary>Tổng điểm trong 12 tháng gần nhất (rolling window).</summary>
    Task<(int TongDiem, int SoLanThu)> GetTichLuy12ThangAsync(ulong khachHangId, CancellationToken ct = default);

    Task<List<DiemThuong>> GetLichSuDiemAsync(ulong khachHangId, int pageSize = 20, CancellationToken ct = default);

    // ── Xếp hạng ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Tính lại hạng của khách hàng dựa trên điểm 12 tháng gần nhất.
    /// Nếu hạng thay đổi: UPDATE KH_KhachHang + INSERT KH_LichSuHang.
    /// Trả về (hangMoiId, daThayDoi).
    /// </summary>
    Task<(ushort HangMoiId, bool DaThayDoi, ushort? HangCuId)> TinhLaiHangAsync(
        ulong khachHangId, CancellationToken ct = default);

    Task<List<LichSuHang>> GetLichSuHangAsync(ulong khachHangId, int pageSize = 10, CancellationToken ct = default);

    Task<ushort?> GetHangHienTaiAsync(ulong khachHangId, CancellationToken ct = default);

    /// <summary>Lấy tên và email khách hàng theo Id — dùng cho các job nền cần gửi email.</summary>
    Task<(string TenKhachHang, string? Email)?> GetTenVaEmailAsync(ulong khachHangId, CancellationToken ct = default);

    // ── Voucher ───────────────────────────────────────────────────────────────

    Task<Voucher> PhatVoucherAsync(ulong khachHangId, ushort xepHangId,
        string lyDo, ulong? lichSuHangId, CancellationToken ct = default);

    Task<(Voucher? Voucher, VoucherToken? Token)> GetVoucherByTokenAsync(
        string token, CancellationToken ct = default);

    Task<bool> DanhDauTokenDaSuDungAsync(ulong tokenId, CancellationToken ct = default);

    Task<bool> GanTicketVaoVoucherAsync(ulong voucherId, ulong ticketId, CancellationToken ct = default);

    /// <summary>Khách bấm link redeem trong email — đánh dấu đã yêu cầu dùng (chưa gắn ticket hỗ trợ).</summary>
    Task<bool> DanhDauDaYeuCauAsync(ulong voucherId, CancellationToken ct = default);

    Task<List<Voucher>> GetVouchersByKhachHangAsync(ulong khachHangId, CancellationToken ct = default);

    /// <summary>Áp dụng voucher vào báo giá — đánh dấu IsUsed = true.</summary>
    Task ApDungVoucherAsync(ulong voucherId, ulong baoGiaId, uint nguoiApDungId, CancellationToken ct = default);

    // ── Email Log ─────────────────────────────────────────────────────────────

    /// <summary>Kiểm tra đã gửi email loại này cho khách trong năm/tháng chưa (chống gửi trùng).</summary>
    Task<bool> DaGuiEmailTrongNamAsync(ulong khachHangId, string loaiEmail, int nam, CancellationToken ct = default);

    Task GhiEmailLogAsync(ulong khachHangId, string loaiEmail, string emailDen,
        string tieuDe, bool thanhCong, ulong? voucherId = null, string? loiChiTiet = null,
        CancellationToken ct = default);

    // ── Dùng cho Background Job ───────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách khách hàng có sinh nhật / ngày thành lập trong N ngày tới.
    /// Dùng cho job hàng ngày để gửi email ưu đãi.
    /// </summary>
    Task<List<KhachHangNgayDacBiet>> GetKhachHangNgayDacBietAsync(
        int soNgayToi, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách khách hàng phù hợp với 1 ngày lễ/ưu đãi cụ thể (theo loại KH và hạng tối thiểu).
    /// Dùng cho job hàng ngày gửi email ngày lễ.
    /// </summary>
    Task<List<KhachHangNgayDacBiet>> GetKhachHangChoNgayLeAsync(
        string apDungChoLoaiKH, ushort? hangToiThieuApDung, CancellationToken ct = default);

    /// <summary>Lấy toàn bộ khách hàng để job đầu tháng tính lại hạng.</summary>
    Task<List<ulong>> GetAllKhachHangIdsAsync(CancellationToken ct = default);

    /// <summary>Lấy thông tin XepHang (tên, %, mốc điểm) theo danh sách Id.</summary>
    Task<List<XepHangInfo>> GetXepHangInfoAsync(IEnumerable<ushort> ids, CancellationToken ct = default);

    /// <summary>Lấy token bảo mật mới nhất của voucher (vừa tạo trong PhatVoucherAsync).</summary>
    Task<string?> GetLatestTokenByVoucherAsync(ulong voucherId, CancellationToken ct = default);
}

public class XepHangInfo
{
    public ushort Id { get; set; }
    public string TenHang { get; set; } = string.Empty;
    public byte ThuTu { get; set; }
    public int DiemToiThieu { get; set; }
    public decimal PhanTramGiamVoucher { get; set; }
    public string? MoTaQuyenLoi { get; set; }
}

public class KhachHangNgayDacBiet
{
    public ulong KhachHangId { get; set; }
    public string TenKhachHang { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string LoaiNgay { get; set; } = string.Empty; // "SinhNhat" | "NgayThanhLap"
    public string TenLoaiKH { get; set; } = string.Empty;
    public ushort? HangHienTaiId { get; set; }
}