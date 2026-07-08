using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Loyalty.DTOs;
using CRM.Application.Interfaces.DanhMuc;
using CRM.Application.Interfaces.Loyalty;
using MediatR;

namespace CRM.Application.Features.Loyalty.Queries.GetCustomerLoyaltyInfo;

public class GetCustomerLoyaltyInfoQueryHandler
    : IRequestHandler<GetCustomerLoyaltyInfoQuery, CustomerLoyaltyInfoDto>
{
    private readonly ILoyaltyRepository _loyaltyRepo;
    private readonly IDanhMucRepository _danhMucRepo;

    public GetCustomerLoyaltyInfoQueryHandler(ILoyaltyRepository loyaltyRepo, IDanhMucRepository danhMucRepo)
    {
        _loyaltyRepo = loyaltyRepo;
        _danhMucRepo = danhMucRepo;
    }

    public async Task<CustomerLoyaltyInfoDto> Handle(
        GetCustomerLoyaltyInfoQuery request, CancellationToken ct)
    {
        var khachHangId = request.KhachHangId;

        var thongTinCoBan = await _loyaltyRepo.GetTenVaEmailAsync(khachHangId, ct)
            ?? throw new NotFoundException("KhachHang", khachHangId);

        var hangHienTaiId = await _loyaltyRepo.GetHangHienTaiAsync(khachHangId, ct);
        var (tongDiem, soLanThu) = await _loyaltyRepo.GetTichLuy12ThangAsync(khachHangId, ct);

        // Toàn bộ danh sách hạng (đã sắp theo ThuTu tăng dần) — dùng để suy ra hạng hiện tại
        // đầy đủ thông tin + tìm hạng kế tiếp cần bao nhiêu điểm nữa.
        var tatCaHang = (await _danhMucRepo.GetXepHangActiveAsync(ct))
            .OrderBy(h => h.ThuTu)
            .ToList();

        var hangHienTai = hangHienTaiId.HasValue
            ? tatCaHang.FirstOrDefault(h => h.Id == hangHienTaiId.Value)
            : null;

        var hangTiepTheo = hangHienTai is null
            ? tatCaHang.FirstOrDefault()
            : tatCaHang.FirstOrDefault(h => h.ThuTu > hangHienTai.ThuTu);

        var lichSuDiem = await _loyaltyRepo.GetLichSuDiemAsync(khachHangId, 20, ct);
        var lichSuHangRaw = await _loyaltyRepo.GetLichSuHangAsync(khachHangId, 10, ct);
        var vouchers = await _loyaltyRepo.GetVouchersByKhachHangAsync(khachHangId, ct);

        // Resolve tên hạng cũ/mới cho lịch sử thăng/hạ hạng
        var hangIdsCanTra = lichSuHangRaw
            .SelectMany(x => new[] { x.HangCuId, (ushort?)x.HangMoiId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var tenHangMap = hangIdsCanTra.Count == 0
            ? new Dictionary<ushort, string>()
            : (await _loyaltyRepo.GetXepHangInfoAsync(hangIdsCanTra, ct))
                .ToDictionary(h => h.Id, h => h.TenHang);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return new CustomerLoyaltyInfoDto
        {
            KhachHangId = khachHangId,
            HangHienTaiId = hangHienTai?.Id,
            TenHangHienTai = hangHienTai?.TenHang,
            ThuTuHangHienTai = hangHienTai?.ThuTu,
            PhanTramGiamVoucher = hangHienTai?.PhanTramGiamVoucher,
            MoTaQuyenLoi = hangHienTai?.MoTaQuyenLoi,
            TongDiem12Thang = tongDiem,
            SoLanThu12Thang = soLanThu,
            TenHangTiepTheo = hangTiepTheo is not null && hangTiepTheo.Id != hangHienTai?.Id ? hangTiepTheo.TenHang : null,
            SoDiemCanThemDeLenHang = hangTiepTheo is not null && hangTiepTheo.Id != hangHienTai?.Id
                ? Math.Max(0, hangTiepTheo.DiemToiThieu - tongDiem)
                : null,
            LichSuDiem = lichSuDiem.Select(d => new DiemThuongDto
            {
                Id = d.Id,
                SoDiem = d.SoDiem,
                LoaiGiaoDich = d.LoaiGiaoDich,
                NgayPhatSinh = d.NgayPhatSinh,
                GhiChu = d.GhiChu
            }).ToList(),
            LichSuHang = lichSuHangRaw.Select(h => new LichSuHangDto
            {
                Id = h.Id,
                TenHangCu = h.HangCuId.HasValue && tenHangMap.TryGetValue(h.HangCuId.Value, out var tenCu) ? tenCu : null,
                TenHangMoi = tenHangMap.TryGetValue(h.HangMoiId, out var tenMoi) ? tenMoi : "",
                LyDo = h.LyDo,
                CreatedAt = h.CreatedAt
            }).ToList(),
            Vouchers = vouchers.Select(v => new VoucherDto
            {
                Id = v.Id,
                MaVoucher = v.MaVoucher,
                LoaiGiamGia = v.LoaiGiamGia,
                GiaTriGiam = v.GiaTriGiam,
                GiaTriGiamToiDa = v.GiaTriGiamToiDa,
                NgayBatDau = v.NgayBatDau,
                NgayHetHan = v.NgayHetHan,
                LyDoPhatHanh = v.LyDoPhatHanh,
                IsUsed = v.IsUsed,
                TrangThai = v.IsUsed ? "DaSuDung" : (v.NgayHetHan < today ? "HetHan" : "ConHieuLuc")
            }).OrderByDescending(v => v.Id).ToList()
        };
    }
}
