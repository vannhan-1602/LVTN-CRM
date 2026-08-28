using CRM.Application.Features.Leads.Commands.UpdateLead;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Enums;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace CRM.Application.Tests.Features.Leads;

/// <summary>
/// Trạng thái "Đã chuyển đổi" (DaChuyenDoi) chỉ được phép đạt được qua ConvertLeadCommand — nơi
/// duy nhất thực sự tạo bản ghi KH_KhachHang tương ứng. UpdateLeadCommandValidator từng cho set
/// trực tiếp trạng thái này (đã fix) — bài test khóa lại hành vi đúng.
/// </summary>
public class UpdateLeadCommandValidatorTests
{
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly UpdateLeadCommandValidator _sut;

    public UpdateLeadCommandValidatorTests()
    {
        _leadRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<ulong?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _sut = new UpdateLeadCommandValidator(_leadRepo.Object);
    }

    private static UpdateLeadCommand Cmd(string? tinhTrang) =>
        new(1, "Nguyễn Văn A", "Công ty ABC", "0900000000", "a@example.com", tinhTrang, null);

    [Fact]
    public async Task Set_TinhTrang_DaChuyenDoi_TrucTiep_BiTuChoi()
    {
        var result = await _sut.TestValidateAsync(Cmd(LeadTinhTrang.DaChuyenDoi));

        result.ShouldHaveValidationErrorFor(x => x.TinhTrang);
    }

    [Theory]
    [InlineData(LeadTinhTrang.Moi)]
    [InlineData(LeadTinhTrang.DangChamSoc)]
    [InlineData(LeadTinhTrang.ThatBai)]
    public async Task Set_CacTinhTrangKhac_HopLe(string tinhTrang)
    {
        var result = await _sut.TestValidateAsync(Cmd(tinhTrang));

        result.ShouldNotHaveValidationErrorFor(x => x.TinhTrang);
    }

    [Fact]
    public async Task BoTrong_TinhTrang_KhongLoi_VìKhongDoi()
    {
        var result = await _sut.TestValidateAsync(Cmd(null));

        result.ShouldNotHaveValidationErrorFor(x => x.TinhTrang);
    }
}
