using CRM.Application.Interfaces.Loyalty;
using CRM.Application.Services;
using CRM.Domain.Entities.Loyalty;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class LoyaltyRepository : ILoyaltyRepository
{
    private readonly CrmDbContext _context;
    private const decimal TY_LE_DIEM = 100_000m; // 100.000 VNĐ = 1 điểm

    public LoyaltyRepository(CrmDbContext context) => _context = context;

    // ══════════════════════════════════════════════════════════════════════════
    // ĐIỂM TÍCH LŨY
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<int> CongDiemAsync(
        ulong khachHangId, decimal soTienThu,
        ulong hoaDonId, ulong phieuThuChiId,
        CancellationToken ct = default)
    {
        // Idempotent: nếu phiếu thu này đã sinh điểm rồi thì bỏ qua
        // (unique constraint uq_diemthuong_phieuthu trong DB cũng bảo vệ tầng này)
        bool daConem = await _context.KhDiemThuongs
            .AnyAsync(x => x.PhieuThuChi_Id == phieuThuChiId, ct);
        if (daConem) return 0;

        int diem = (int)Math.Floor(soTienThu / TY_LE_DIEM);
        if (diem <= 0) return 0;

        _context.KhDiemThuongs.Add(new KhDiemThuongEntity
        {
            KhachHang_Id = khachHangId,
            SoDiem = diem,
            LoaiGiaoDich = "MuaHang",
            HoaDon_Id = hoaDonId,
            PhieuThuChi_Id = phieuThuChiId,
            NgayPhatSinh = DateOnly.FromDateTime(DateTime.UtcNow),
            GhiChu = $"Tự động từ phiếu thu PT-{phieuThuChiId}",
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(ct);
        return diem;
    }

    public async Task<(int TongDiem, int SoLanThu)> GetTichLuy12ThangAsync(
        ulong khachHangId, CancellationToken ct = default)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var rows = await _context.KhDiemThuongs
            .AsNoTracking()
            .Where(x => x.KhachHang_Id == khachHangId
                     && x.LoaiGiaoDich == "MuaHang"
                     && x.NgayPhatSinh >= from)
            .ToListAsync(ct);

        return (rows.Sum(x => x.SoDiem), rows.Count);
    }

    public async Task<List<DiemThuong>> GetLichSuDiemAsync(
        ulong khachHangId, int pageSize = 20, CancellationToken ct = default)
    {
        var rows = await _context.KhDiemThuongs
            .AsNoTracking()
            .Where(x => x.KhachHang_Id == khachHangId)
            .OrderByDescending(x => x.Id)
            .Take(pageSize)
            .ToListAsync(ct);

        return rows.Select(e => new DiemThuong
        {
            Id = e.Id,
            KhachHangId = e.KhachHang_Id,
            SoDiem = e.SoDiem,
            LoaiGiaoDich = e.LoaiGiaoDich,
            HoaDonId = e.HoaDon_Id,
            PhieuThuChiId = e.PhieuThuChi_Id,
            NgayPhatSinh = e.NgayPhatSinh,
            GhiChu = e.GhiChu,
            NguoiTaoId = e.NguoiTao_Id,
            CreatedAt = e.CreatedAt,
        }).ToList();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // XẾP HẠNG
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<(ushort HangMoiId, bool DaThayDoi, ushort? HangCuId)> TinhLaiHangAsync(
        ulong khachHangId, CancellationToken ct = default)
    {
        var (tongDiem, soLanThu) = await GetTichLuy12ThangAsync(khachHangId, ct);

        // Lấy hạng cao nhất mà khách đủ điều kiện
        var hangMoi = await _context.KhXepHangs
            .AsNoTracking()
            .Where(x => x.IsActive
                     && x.DiemToiThieu <= tongDiem
                     && x.SoLanThuToiThieu <= soLanThu)
            .OrderByDescending(x => x.ThuTu)
            .FirstOrDefaultAsync(ct);

        // Mặc định hạng Đồng (ThuTu = 1) nếu chưa đủ tiêu chí nào
        if (hangMoi is null)
        {
            hangMoi = await _context.KhXepHangs
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.ThuTu)
                .FirstOrDefaultAsync(ct);
        }
        if (hangMoi is null) return (1, false, null); // fallback cứng

        var kh = await _context.KhKhachHangs
            .FirstOrDefaultAsync(x => x.Id == khachHangId, ct);
        if (kh is null) return ((ushort)hangMoi.Id, false, null);

        ushort hangCuId = kh.HangKhachHang_Id ?? 0;
        bool daThayDoi = hangMoi.Id != hangCuId;

        if (daThayDoi)
        {
            kh.HangKhachHang_Id = hangMoi.Id;
            _context.KhLichSuHangs.Add(new KhLichSuHangEntity
            {
                KhachHang_Id = khachHangId,
                HangCu_Id = hangCuId == 0 ? null : hangCuId,
                HangMoi_Id = hangMoi.Id,
                LyDo = hangMoi.ThuTu > (hangCuId == 0 ? 0 : hangCuId)
                                   ? "TuDongDuDieuKien" : "TuDongXuongHang",
                GhiChu = $"Điểm 12T: {tongDiem}, Số lần thu: {soLanThu}",
                CreatedAt = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync(ct);
        }

        return ((ushort)hangMoi.Id, daThayDoi, hangCuId == 0 ? null : hangCuId);
    }

    public async Task<List<LichSuHang>> GetLichSuHangAsync(
        ulong khachHangId, int pageSize = 10, CancellationToken ct = default)
    {
        var rows = await _context.KhLichSuHangs
            .AsNoTracking()
            .Where(x => x.KhachHang_Id == khachHangId)
            .OrderByDescending(x => x.Id)
            .Take(pageSize)
            .ToListAsync(ct);

        return rows.Select(e => new LichSuHang
        {
            Id = e.Id,
            KhachHangId = e.KhachHang_Id,
            HangCuId = e.HangCu_Id,
            HangMoiId = e.HangMoi_Id,
            LyDo = e.LyDo,
            GhiChu = e.GhiChu,
            NguoiThucHienId = e.NguoiThucHien_Id,
            CreatedAt = e.CreatedAt,
        }).ToList();
    }

    public async Task<ushort?> GetHangHienTaiAsync(ulong khachHangId, CancellationToken ct = default) =>
        await _context.KhKhachHangs
            .AsNoTracking()
            .Where(x => x.Id == khachHangId)
            .Select(x => x.HangKhachHang_Id)
            .FirstOrDefaultAsync(ct);

    public async Task<(string TenKhachHang, string? Email)?> GetTenVaEmailAsync(ulong khachHangId, CancellationToken ct = default)
    {
        var kh = await _context.KhKhachHangs
            .AsNoTracking()
            .Where(x => x.Id == khachHangId)
            .Select(x => new { x.TenKhachHang, x.Email })
            .FirstOrDefaultAsync(ct);

        return kh is null ? null : (kh.TenKhachHang, kh.Email);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // VOUCHER
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<Voucher> PhatVoucherAsync(
        ulong khachHangId, ushort xepHangId,
        string lyDo, ulong? lichSuHangId,
        CancellationToken ct = default)
    {
        var hang = await _context.KhXepHangs.FindAsync([xepHangId], ct)
            ?? throw new InvalidOperationException($"Không tìm thấy hạng Id={xepHangId}");

        // Không phát voucher nếu % giảm = 0
        if (hang.PhanTramGiamVoucher <= 0)
            throw new InvalidOperationException($"Hạng '{hang.TenHang}' không có voucher (% giảm = 0).");

        var ngayHetHan = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)); // voucher có hiệu lực 90 ngày

        var entity = new KhVoucherEntity
        {
            MaVoucher = GenerateMaVoucher(),
            KhachHang_Id = khachHangId,
            LoaiGiamGia = "PhanTram",
            GiaTriGiam = hang.PhanTramGiamVoucher,
            GiaTriGiamToiDa = 5_000_000m,  // giới hạn tối đa 5 triệu để tránh lạm dụng
            NgayBatDau = DateOnly.FromDateTime(DateTime.UtcNow),
            NgayHetHan = ngayHetHan,
            LyDoPhatHanh = lyDo,
            LichSuHang_Id = lichSuHangId,
            TrangThaiYeuCau = "ChuaYeuCau",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.KhVouchers.Add(entity);
        await _context.SaveChangesAsync(ct);

        // Tạo token bảo mật cho link email
        var token = new KhVoucherTokenEntity
        {
            Voucher_Id = entity.Id,
            Token = GenerateSecureToken(),
            NgayHetHanToken = ngayHetHan.ToDateTime(TimeOnly.MinValue),
            DaSuDung = false,
            CreatedAt = DateTime.UtcNow,
        };
        _context.KhVoucherTokens.Add(token);
        await _context.SaveChangesAsync(ct);

        return MapVoucherToDomain(entity);
    }

    public async Task<(Voucher? Voucher, VoucherToken? Token)> GetVoucherByTokenAsync(
        string token, CancellationToken ct = default)
    {
        var tokenEntity = await _context.KhVoucherTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Token == token, ct);

        if (tokenEntity is null) return (null, null);

        var voucher = await _context.KhVouchers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tokenEntity.Voucher_Id, ct);

        return (voucher is null ? null : MapVoucherToDomain(voucher),
                new VoucherToken
                {
                    Id = tokenEntity.Id,
                    VoucherId = tokenEntity.Voucher_Id,
                    Token = tokenEntity.Token,
                    NgayHetHanToken = tokenEntity.NgayHetHanToken,
                    DaSuDung = tokenEntity.DaSuDung,
                    CreatedAt = tokenEntity.CreatedAt,
                });
    }

    public async Task<bool> DanhDauTokenDaSuDungAsync(ulong tokenId, CancellationToken ct = default)
    {
        var rows = await _context.KhVoucherTokens
            .Where(x => x.Id == tokenId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.DaSuDung, true), ct);
        return rows > 0;
    }

    public async Task<bool> GanTicketVaoVoucherAsync(ulong voucherId, ulong ticketId, CancellationToken ct = default)
    {
        var rows = await _context.KhVouchers
            .Where(x => x.Id == voucherId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Ticket_Id, ticketId)
                .SetProperty(x => x.TrangThaiYeuCau, "DaYeuCau")
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
        return rows > 0;
    }

    public async Task<bool> DanhDauDaYeuCauAsync(ulong voucherId, CancellationToken ct = default)
    {
        var rows = await _context.KhVouchers
            .Where(x => x.Id == voucherId && x.TrangThaiYeuCau == "ChuaYeuCau")
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.TrangThaiYeuCau, "DaYeuCau")
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
        return rows > 0;
    }

    public async Task<List<Voucher>> GetVouchersByKhachHangAsync(
        ulong khachHangId, CancellationToken ct = default)
    {
        var rows = await _context.KhVouchers
            .AsNoTracking()
            .Where(x => x.KhachHang_Id == khachHangId)
            .OrderByDescending(x => x.Id)
            .ToListAsync(ct);

        return rows.Select(MapVoucherToDomain).ToList();
    }

    public async Task ApDungVoucherAsync(
        ulong voucherId, ulong baoGiaId, uint nguoiApDungId,
        CancellationToken ct = default)
    {
        await _context.KhVouchers
            .Where(x => x.Id == voucherId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsUsed, true)
                .SetProperty(x => x.AppliedTo_BaoGia_Id, baoGiaId)
                .SetProperty(x => x.NgaySuDung, DateTime.UtcNow)
                .SetProperty(x => x.NguoiApDung_Id, nguoiApDungId)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // EMAIL LOG
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<bool> DaGuiEmailTrongNamAsync(
        ulong khachHangId, string loaiEmail, int nam,
        CancellationToken ct = default)
    {
        var from = new DateTime(nam, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddYears(1);

        return await _context.KhEmailLogs.AnyAsync(x =>
            x.KhachHang_Id == khachHangId &&
            x.LoaiEmail == loaiEmail &&
            x.TrangThaiGui == "ThanhCong" &&
            x.CreatedAt >= from &&
            x.CreatedAt < to, ct);
    }

    public async Task GhiEmailLogAsync(
        ulong khachHangId, string loaiEmail, string emailDen,
        string tieuDe, bool thanhCong,
        ulong? voucherId = null, string? loiChiTiet = null,
        CancellationToken ct = default)
    {
        _context.KhEmailLogs.Add(new KhEmailLogEntity
        {
            KhachHang_Id = khachHangId,
            LoaiEmail = loaiEmail,
            Voucher_Id = voucherId,
            EmailDen = emailDen,
            TieuDe = tieuDe,
            TrangThaiGui = thanhCong ? "ThanhCong" : "ThatBai",
            LoiChiTiet = loiChiTiet,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<(string LoaiEmail, bool ThanhCong, string EmailDen, string? LoiChiTiet)>> ThongKeEmailLogTuAsync(
        DateTime tuThoiDiem, CancellationToken ct = default)
    {
        var rows = await _context.KhEmailLogs
            .AsNoTracking()
            .Where(x => x.CreatedAt >= tuThoiDiem)
            .Select(x => new { x.LoaiEmail, x.TrangThaiGui, x.EmailDen, x.LoiChiTiet })
            .ToListAsync(ct);

        return rows.Select(x => (x.LoaiEmail, x.TrangThaiGui == "ThanhCong", x.EmailDen, x.LoiChiTiet)).ToList();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // BACKGROUND JOB
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<List<KhachHangNgayDacBiet>> GetKhachHangNgayDacBietAsync(
        int soNgayToi, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var targetM = new List<int>();
        var targetD = new List<int>();

        for (int i = 0; i <= soNgayToi; i++)
        {
            var d = today.AddDays(i);
            targetM.Add(d.Month);
            targetD.Add(d.Day);
        }

        // Dùng danh sách ngày cụ thể để tìm sinh nhật / ngày thành lập sắp tới
        var result = new List<KhachHangNgayDacBiet>();

        var khList = await (
            from kh in _context.KhKhachHangs
            where kh.Email != null && (kh.NgaySinh != null || kh.NgayThanhLap != null)
            join lkh in _context.KhLoaiKhachHangs
                on kh.LoaiKhachHangId equals lkh.Id into lkhJoin
            from lkh in lkhJoin.DefaultIfEmpty()
            select new
            {
                kh.Id,
                kh.TenKhachHang,
                kh.Email,
                kh.NgaySinh,
                kh.NgayThanhLap,
                kh.HangKhachHang_Id,
                LoaiKH = lkh != null ? lkh.TenLoai : null
            }
        ).AsNoTracking().ToListAsync(ct);

        foreach (var kh in khList)
        {
            // Kiểm tra sinh nhật
            if (kh.NgaySinh is DateOnly ns)
            {
                for (int i = 0; i <= soNgayToi; i++)
                {
                    var target = today.AddDays(i);
                    if (ns.Month == target.Month && ns.Day == target.Day)
                    {
                        result.Add(new KhachHangNgayDacBiet
                        {
                            KhachHangId = kh.Id,
                            TenKhachHang = kh.TenKhachHang,
                            Email = kh.Email,
                            LoaiNgay = "SinhNhat",
                            TenLoaiKH = kh.LoaiKH ?? "",
                            HangHienTaiId = kh.HangKhachHang_Id,
                        });
                        break;
                    }
                }
            }

            // Kiểm tra ngày thành lập
            if (kh.NgayThanhLap is DateOnly ntl)
            {
                for (int i = 0; i <= soNgayToi; i++)
                {
                    var target = today.AddDays(i);
                    if (ntl.Month == target.Month && ntl.Day == target.Day)
                    {
                        result.Add(new KhachHangNgayDacBiet
                        {
                            KhachHangId = kh.Id,
                            TenKhachHang = kh.TenKhachHang,
                            Email = kh.Email,
                            LoaiNgay = "NgayThanhLap",
                            TenLoaiKH = kh.LoaiKH ?? "",
                            HangHienTaiId = kh.HangKhachHang_Id,
                        });
                        break;
                    }
                }
            }
        }

        return result;
    }

    public async Task<List<ulong>> GetAllKhachHangIdsAsync(CancellationToken ct = default) =>
        await _context.KhKhachHangs
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(ct);

    public async Task<List<KhachHangNgayDacBiet>> GetKhachHangChoNgayLeAsync(
        string apDungChoLoaiKH, ushort? hangToiThieuApDung, CancellationToken ct = default)
    {
        // Thứ tự hạng tối thiểu (ThuTu) để so sánh "hạng >= hạng tối thiểu áp dụng"
        byte? thuTuToiThieu = null;
        if (hangToiThieuApDung.HasValue)
        {
            thuTuToiThieu = await _context.Set<KhXepHangEntity>()
                .Where(h => h.Id == hangToiThieuApDung.Value)
                .Select(h => (byte?)h.ThuTu)
                .FirstOrDefaultAsync(ct);
        }

        var query =
            from kh in _context.KhKhachHangs.AsNoTracking()
            where !kh.IsDeleted && kh.Email != null
            join lkh in _context.KhLoaiKhachHangs on kh.LoaiKhachHangId equals lkh.Id into lkhJoin
            from lkh in lkhJoin.DefaultIfEmpty()
            join hang in _context.Set<KhXepHangEntity>() on kh.HangKhachHang_Id equals hang.Id into hangJoin
            from hang in hangJoin.DefaultIfEmpty()
            select new
            {
                kh.Id,
                kh.TenKhachHang,
                kh.Email,
                kh.HangKhachHang_Id,
                LoaiKH = lkh != null ? lkh.TenLoai : null,
                ThuTuHang = hang != null ? hang.ThuTu : (byte?)null
            };

        if (apDungChoLoaiKH != "TatCa")
            query = query.Where(x => x.LoaiKH == apDungChoLoaiKH);

        if (thuTuToiThieu.HasValue)
            query = query.Where(x => x.ThuTuHang != null && x.ThuTuHang >= thuTuToiThieu.Value);

        var list = await query.ToListAsync(ct);

        return list.Select(x => new KhachHangNgayDacBiet
        {
            KhachHangId = x.Id,
            TenKhachHang = x.TenKhachHang,
            Email = x.Email,
            LoaiNgay = "NgayLe",
            TenLoaiKH = x.LoaiKH ?? "",
            HangHienTaiId = x.HangKhachHang_Id
        }).ToList();
    }


    public async Task<List<XepHangInfo>> GetXepHangInfoAsync(
        IEnumerable<ushort> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await _context.KhXepHangs
            .AsNoTracking()
            .Where(x => idList.Contains(x.Id))
            .Select(x => new XepHangInfo
            {
                Id = x.Id,
                TenHang = x.TenHang,
                ThuTu = x.ThuTu,
                DiemToiThieu = x.DiemToiThieu,
                PhanTramGiamVoucher = x.PhanTramGiamVoucher,
                MoTaQuyenLoi = x.MoTaQuyenLoi,
            })
            .ToListAsync(ct);
    }

    public async Task<string?> GetLatestTokenByVoucherAsync(
        ulong voucherId, CancellationToken ct = default) =>
        await _context.KhVoucherTokens
            .AsNoTracking()
            .Where(x => x.Voucher_Id == voucherId && !x.DaSuDung)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Token)
            .FirstOrDefaultAsync(ct);

    // ══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    private static string GenerateMaVoucher()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var rand = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"VC-{date}-{rand}";
    }

    private static string GenerateSecureToken() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

    private static Voucher MapVoucherToDomain(KhVoucherEntity e) => new()
    {
        Id = e.Id,
        MaVoucher = e.MaVoucher,
        KhachHangId = e.KhachHang_Id,
        LoaiGiamGia = e.LoaiGiamGia,
        GiaTriGiam = e.GiaTriGiam,
        GiaTriGiamToiDa = e.GiaTriGiamToiDa,
        NgayBatDau = e.NgayBatDau,
        NgayHetHan = e.NgayHetHan,
        LyDoPhatHanh = e.LyDoPhatHanh,
        LichSuHangId = e.LichSuHang_Id,
        TrangThaiYeuCau = e.TrangThaiYeuCau,
        TicketId = e.Ticket_Id,
        IsUsed = e.IsUsed,
        AppliedToBaoGiaId = e.AppliedTo_BaoGia_Id,
        NgaySuDung = e.NgaySuDung,
        NguoiApDungId = e.NguoiApDung_Id,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };
}