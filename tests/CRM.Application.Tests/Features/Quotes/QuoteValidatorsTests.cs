using CRM.Application.Features.Quotes.Commands.CreateQuote;
using CRM.Application.Features.Quotes.Commands.UpdateQuote;
using CRM.Application.Features.Quotes.DTOs;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.Application.Tests.Features.Quotes;

/// <summary>
/// CreateQuoteCommandValidator vốn đã chặn DonGia âm; UpdateQuoteCommandValidator từng THIẾU
/// đúng rule này (đã fix). Test dưới đây khóa hành vi đúng cho cả 2, để nếu ai vô tình xóa rule
/// đi lần nữa thì CI sẽ đỏ ngay thay vì phải rà tay lại từ đầu.
/// </summary>
public class QuoteValidatorsTests
{
    private static QuoteItemRequestDto Item(uint sanPhamId = 1, int soLuong = 1, decimal? donGia = null) =>
        new() { SanPhamId = sanPhamId, SoLuong = soLuong, DonGia = donGia };

    [Fact]
    public void CreateQuote_DonGiaAm_BiTuChoi()
    {
        var cmd = new CreateQuoteCommand(1, new() { Item(donGia: -1000) });
        var result = new CreateQuoteCommandValidator().TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("ChiTiet[0].DonGia");
    }

    [Fact]
    public void CreateQuote_DonGiaDuong_HopLe()
    {
        var cmd = new CreateQuoteCommand(1, new() { Item(donGia: 500_000) });
        var result = new CreateQuoteCommandValidator().TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor("ChiTiet[0].DonGia");
    }

    [Fact]
    public void CreateQuote_DonGiaBoTrong_KhongLoi_VìSeLayGiaNiemYet()
    {
        // DonGia = null nghĩa là lấy GiaBan hiện tại của sản phẩm — không phải lỗi.
        var cmd = new CreateQuoteCommand(1, new() { Item(donGia: null) });
        var result = new CreateQuoteCommandValidator().TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor("ChiTiet[0].DonGia");
    }

    [Fact]
    public void CreateQuote_ChiTietRong_BiTuChoi()
    {
        var cmd = new CreateQuoteCommand(1, new());
        var result = new CreateQuoteCommandValidator().TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("ChiTiet");
    }

    [Fact]
    public void UpdateQuote_DonGiaAm_BiTuChoi()
    {
        // Đây là rule từng bị thiếu ở UpdateQuoteCommandValidator (có ở Create, không có ở
        // Update) — bài test này khóa lại để không tái diễn.
        var cmd = new UpdateQuoteCommand(1, new() { Item(donGia: -1) });
        var result = new UpdateQuoteCommandValidator().TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("ChiTiet[0].DonGia");
    }

    [Fact]
    public void UpdateQuote_DonGiaDuong_HopLe()
    {
        var cmd = new UpdateQuoteCommand(1, new() { Item(donGia: 1) });
        var result = new UpdateQuoteCommandValidator().TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor("ChiTiet[0].DonGia");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void SoLuong_KhongDuong_BiTuChoi(int soLuong)
    {
        var cmd = new CreateQuoteCommand(1, new() { Item(soLuong: soLuong) });
        var result = new CreateQuoteCommandValidator().TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("ChiTiet[0].SoLuong");
    }
}
