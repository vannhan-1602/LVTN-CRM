using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Opportunities.Commands.ChangeOpportunityStage;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Opportunities;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Application.Tests.Features.Opportunities;

public class ChangeOpportunityStageCommandHandlerTests
{
    private readonly Mock<IOpportunityRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IAuditLogPublisher> _audit = new();
    private readonly ChangeOpportunityStageCommandHandler _sut;

    public ChangeOpportunityStageCommandHandlerTests()
    {
        _sut = new ChangeOpportunityStageCommandHandler(
            _repo.Object, _uow.Object, _currentUser.Object, _audit.Object,
            new Mock<ILogger<ChangeOpportunityStageCommandHandler>>().Object);

        // Audit log publish thất bại không được làm hỏng luồng chính — mock trả về Task hoàn tất
        // bình thường cho các test không quan tâm audit.
        _audit.Setup(a => a.PublishAsync(
                It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repo.Setup(r => r.GetByIdEnrichedAsync(It.IsAny<ulong>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OpportunityDto { Id = 1, GiaiDoan = CoHoiGiaiDoan.DeXuat.ToString() });
    }

    private static CoHoiBanHang Opportunity(string giaiDoan, int? nhanVienPhuTrachId = null) => new()
    {
        Id = 1,
        TenThuongVu = "Thương vụ test",
        GiaiDoan = giaiDoan,
        NhanVienPhuTrachId = nhanVienPhuTrachId,
    };

    [Fact]
    public async Task KhongTonTai_NemNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoHoiBanHang?)null);

        var act = () => _sut.Handle(new ChangeOpportunityStageCommand(99, "DeXuat", null), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Sale_DoiCoHoiCuaNguoiKhac_BiTuChoi()
    {
        _currentUser.Setup(c => c.Role).Returns(Roles.Sale);
        _currentUser.Setup(c => c.UserId).Returns(10u);
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Opportunity(CoHoiGiaiDoan.KhaoSat.ToString(), nhanVienPhuTrachId: 20));

        var act = () => _sut.Handle(new ChangeOpportunityStageCommand(1, "DeXuat", null), default);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Sale_DoiCoHoiCuaChinhMinh_ThanhCong()
    {
        _currentUser.Setup(c => c.Role).Returns(Roles.Sale);
        _currentUser.Setup(c => c.UserId).Returns(10u);
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Opportunity(CoHoiGiaiDoan.KhaoSat.ToString(), nhanVienPhuTrachId: 10));

        var result = await _sut.Handle(new ChangeOpportunityStageCommand(1, "DeXuat", null), default);

        result.Should().NotBeNull();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<CoHoiBanHang>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("ThanhCong")]
    [InlineData("ThatBai")]
    public async Task CoHoiDaChot_KhongDoiTiepDuoc(string giaiDoanDaChot)
    {
        _currentUser.Setup(c => c.Role).Returns(Roles.Manager);
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Opportunity(giaiDoanDaChot));

        var act = () => _sut.Handle(new ChangeOpportunityStageCommand(1, "DeXuat", null), default);

        await act.Should().ThrowAsync<BusinessRuleException>();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<CoHoiBanHang>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChuyenSangThanhCong_TuDongSetTyLeThanhCong100()
    {
        _currentUser.Setup(c => c.Role).Returns(Roles.Manager);
        var entity = Opportunity(CoHoiGiaiDoan.ThuongLuong.ToString());
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await _sut.Handle(new ChangeOpportunityStageCommand(1, "ThanhCong", null), default);

        entity.TyLeThanhCong.Should().Be(100);
    }

    [Fact]
    public async Task ChuyenSangThatBai_TuDongSetTyLeThanhCong0()
    {
        _currentUser.Setup(c => c.Role).Returns(Roles.Manager);
        var entity = Opportunity(CoHoiGiaiDoan.ThuongLuong.ToString());
        entity.TyLeThanhCong = 70;
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await _sut.Handle(new ChangeOpportunityStageCommand(1, "ThatBai", null), default);

        entity.TyLeThanhCong.Should().Be(0);
    }

    [Fact]
    public async Task Manager_DoiGiaiDoanTuDoKhongBiEpThuTu()
    {
        // Validator không ép thứ tự — Manager có thể nhảy tự do giữa các giai đoạn chưa chốt,
        // kể cả "lùi" lại (VD Thương lượng -> Khảo sát).
        _currentUser.Setup(c => c.Role).Returns(Roles.Manager);
        var entity = Opportunity(CoHoiGiaiDoan.ThuongLuong.ToString());
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await _sut.Handle(new ChangeOpportunityStageCommand(1, "KhaoSat", null), default);

        entity.GiaiDoan.Should().Be("KhaoSat");
    }
}
