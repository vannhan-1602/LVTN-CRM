using CRM.Application.Features.DanhMuc.Commands;
using CRM.Application.Features.DanhMuc.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.Application.Tests.Features.DanhMuc;

/// <summary>
/// Ngày lễ chỉ được lưu ngày CÓ THẬT trong lịch — tháng 4/6/9/11 tối đa 30 ngày, tháng 2 tối đa
/// 29 ngày. Trước fix, validator chỉ giới hạn 1-31 nên chấp nhận được cả những ngày không tồn
/// tại trong bất kỳ năm nào (VD 31/4) — hậu quả là ngày lễ đó âm thầm không bao giờ gửi được.
/// </summary>
public class NgayLeValidatorsTests
{
    private static UpsertNgayLeDto Dto(byte thang, byte ngay) => new()
    {
        TenNgayLe = "Ngày lễ test",
        Thang = thang,
        Ngay = ngay,
    };

    [Theory]
    [InlineData(4, 31)]   // Tháng 4 chỉ có 30 ngày
    [InlineData(6, 31)]   // Tháng 6 chỉ có 30 ngày
    [InlineData(9, 31)]   // Tháng 9 chỉ có 30 ngày
    [InlineData(11, 31)]  // Tháng 11 chỉ có 30 ngày
    [InlineData(2, 30)]   // Tháng 2 tối đa chỉ 29 ngày (kể cả năm nhuận)
    [InlineData(2, 31)]
    public void NgayKhongTonTaiTrongLich_BiTuChoi(byte thang, byte ngay)
    {
        var cmd = new CreateNgayLeCommand(Dto(thang, ngay));
        var result = new CreateNgayLeCommandValidator().TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Dto);
    }

    [Theory]
    [InlineData(4, 30)]   // Đúng ngày cuối cùng hợp lệ của tháng 30 ngày
    [InlineData(2, 29)]   // 29/2 vẫn hợp lệ (có thật vào năm nhuận)
    [InlineData(1, 31)]   // Tháng 1 có 31 ngày — hợp lệ
    [InlineData(12, 25)]  // Giáng sinh — case thông thường
    public void NgayHopLeTrongLich_KhongLoi(byte thang, byte ngay)
    {
        var cmd = new CreateNgayLeCommand(Dto(thang, ngay));
        var result = new CreateNgayLeCommandValidator().TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Dto);
    }

    [Fact]
    public void Update_NgayKhongTonTai_CungBiTuChoi()
    {
        // UpdateNgayLeCommandValidator phải có cùng ràng buộc như Create, không được lệch nhau.
        var cmd = new UpdateNgayLeCommand(1, Dto(4, 31));
        var result = new UpdateNgayLeCommandValidator().TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Dto);
    }
}
