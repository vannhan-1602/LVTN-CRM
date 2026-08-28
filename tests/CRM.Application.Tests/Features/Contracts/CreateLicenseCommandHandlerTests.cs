using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.Commands.CreateLicense;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Features.Products.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Sales;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Application.Tests.Features.Contracts;

/// <summary>
/// Trọng tâm: hợp đồng tạo mà KHÔNG nhập Thời hạn (ThoiHan là trường optional) sẽ có
/// NgayKetThuc = NULL mãi mãi — nếu không chặn, License được cấp ra sẽ có NgayHetHan = NULL và
/// biến mất khỏi mọi cơ chế theo dõi/nhắc gia hạn (LicenseLifecycleJobHostedService lọc bỏ các
/// License có NgayHetHan null). Test dưới đây khóa lại rule chặn này.
/// </summary>
public class CreateLicenseCommandHandlerTests
{
    private readonly Mock<IContractRepository> _contractRepo = new();
    private readonly Mock<IContractMilestoneRepository> _milestoneRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<ILicenseRepository> _licenseRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly CreateLicenseCommandHandler _sut;

    public CreateLicenseCommandHandlerTests()
    {
        _sut = new CreateLicenseCommandHandler(
            _contractRepo.Object, _milestoneRepo.Object, _productRepo.Object,
            _licenseRepo.Object, _currentUser.Object, _uow.Object);

        // Mọi điều kiện phụ đều cho pass — chỉ tập trung vào điều kiện đang test.
        _productRepo.Setup(p => p.GetByIdEnrichedAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductDto { Id = 1, TenSP = "CRM Pro", HinhThuc = "License" });
        _productRepo.Setup(p => p.GetCurrentStockAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        _milestoneRepo.Setup(m => m.GetByHopDongAsync(It.IsAny<ulong>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MocTrienKhaiDto>
            {
                new() { LoaiMoc = "BanGiao", TrangThai = "DaXacNhan" },
            });
        _uow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<LicenseDto>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<LicenseDto>>, CancellationToken>((op, _) => op());
    }

    private static HopDong ValidContract(DateOnly? ngayKetThuc) => new()
    {
        Id = 1,
        MaHopDong = "HD-001",
        TrangThai = "DangThucHien",
        LoaiHopDong = "ChinhThuc",
        NgayKetThuc = ngayKetThuc,
    };

    private static CreateLicenseCommand Cmd() => new(1, 1, 5, "1.0", "Cloud");

    [Fact]
    public async Task HopDongChuaXacDinhNgayKetThuc_BiTuChoi()
    {
        _contractRepo.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidContract(ngayKetThuc: null));

        var act = () => _sut.Handle(Cmd(), default);

        var ex = await act.Should().ThrowAsync<BusinessRuleException>();
        ex.Which.Message.Should().Contain("Thời hạn");
        _licenseRepo.Verify(l => l.AddAsync(
            It.IsAny<ulong>(), It.IsAny<uint>(), It.IsAny<int>(), It.IsAny<string?>(),
            It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HopDongDaXacDinhNgayKetThuc_CapLicenseThanhCong()
    {
        var ngayKetThuc = new DateOnly(2027, 1, 1);
        _contractRepo.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidContract(ngayKetThuc));
        _licenseRepo.Setup(l => l.AddAsync(
                1, 1, 5, "1.0", "Cloud", It.IsAny<DateOnly>(), ngayKetThuc, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LicenseDto { Id = 1, MaLicenseKey = "LIC-001", NgayHetHan = ngayKetThuc });

        var result = await _sut.Handle(Cmd(), default);

        result.NgayHetHan.Should().Be(ngayKetThuc);
        _productRepo.Verify(p => p.AdjustStockAsync(
            1, It.IsAny<string>(), -1, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<uint?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChuaCoMocBanGiaoXacNhan_BiTuChoi()
    {
        _contractRepo.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidContract(new DateOnly(2027, 1, 1)));
        _milestoneRepo.Setup(m => m.GetByHopDongAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MocTrienKhaiDto>()); // chưa có mốc bàn giao nào

        var act = () => _sut.Handle(Cmd(), default);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Bàn giao*");
    }

    [Fact]
    public async Task SanPhamKhongPhaiLicenseHoacSubscription_BiTuChoi()
    {
        _contractRepo.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidContract(new DateOnly(2027, 1, 1)));
        _productRepo.Setup(p => p.GetByIdEnrichedAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductDto { Id = 1, TenSP = "Chuột không dây", HinhThuc = "VatLy" });

        var act = () => _sut.Handle(Cmd(), default);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task SanPhamSubscription_CapLicenseDuocGiongNhuLicense()
    {
        // Subscription dùng chung luồng cấp phát với License (cùng bản chất, không phải hàng
        // vật lý) — không được để lệch riêng ra nữa.
        var ngayKetThuc = new DateOnly(2027, 1, 1);
        _contractRepo.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidContract(ngayKetThuc));
        _productRepo.Setup(p => p.GetByIdEnrichedAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductDto { Id = 1, TenSP = "CRM Subscription", HinhThuc = "Subscription" });
        _licenseRepo.Setup(l => l.AddAsync(
                It.IsAny<ulong>(), It.IsAny<uint>(), It.IsAny<int>(), It.IsAny<string?>(),
                It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LicenseDto { Id = 1, MaLicenseKey = "LIC-002" });

        var act = () => _sut.Handle(Cmd(), default);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HopDongKhongPhaiChinhThuc_BiTuChoi()
    {
        var contract = ValidContract(new DateOnly(2027, 1, 1));
        contract.LoaiHopDong = "GiaHan";
        _contractRepo.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(contract);

        var act = () => _sut.Handle(Cmd(), default);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Gia hạn License*");
    }

    [Fact]
    public async Task HetTonKhoLicense_BiTuChoi()
    {
        _contractRepo.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidContract(new DateOnly(2027, 1, 1)));
        _productRepo.Setup(p => p.GetCurrentStockAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var act = () => _sut.Handle(Cmd(), default);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*hết tồn kho*");
    }
}
