using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.Commands.UpdateContractStatus;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Customers;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Application.Tests.Features.Contracts;

/// <summary>
/// Kiểm tra riêng đúng điểm từng bị thiếu (Sale đổi được trạng thái hợp đồng của khách hàng
/// KHÔNG phải mình phụ trách) — nay đã fix bằng cách tra cứu người phụ trách qua
/// ICustomerRepository trước khi cho phép ghi.
/// </summary>
public class UpdateContractStatusCommandHandlerTests
{
    private readonly Mock<IContractRepository> _contractRepo = new();
    private readonly Mock<ICustomerRepository> _customerRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IAuditLogPublisher> _audit = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly UpdateContractStatusCommandHandler _sut;

    public UpdateContractStatusCommandHandlerTests()
    {
        _sut = new UpdateContractStatusCommandHandler(
            _contractRepo.Object, _customerRepo.Object, _uow.Object, _audit.Object,
            _currentUser.Object, new Mock<ILogger<UpdateContractStatusCommandHandler>>().Object);

        _contractRepo.Setup(r => r.GetByIdEnrichedAsync(It.IsAny<ulong>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractDto { Id = 1 });
        _audit.Setup(a => a.PublishAsync(
                It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static HopDong Contract(ulong khachHangId, string trangThai = "DangThucHien") => new()
    {
        Id = 1,
        MaHopDong = "HD-001",
        KhachHangId = khachHangId,
        TrangThai = trangThai,
    };

    [Fact]
    public async Task Sale_DoiTrangThaiHopDongCuaKhachKhongPhaiMinh_BiTuChoi()
    {
        _currentUser.Setup(c => c.Role).Returns(Roles.Sale);
        _currentUser.Setup(c => c.UserId).Returns(10u);
        _contractRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Contract(khachHangId: 5));
        _customerRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KhachHang { Id = 5, NhanVienPhuTrachId = 99 }); // Sale khác phụ trách

        var act = () => _sut.Handle(new UpdateContractStatusCommand(1, "TamDung"), default);

        await act.Should().ThrowAsync<ForbiddenException>();
        _contractRepo.Verify(r => r.UpdateStatusAsync(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Sale_DoiTrangThaiHopDongCuaKhachMinhPhuTrach_ThanhCong()
    {
        _currentUser.Setup(c => c.Role).Returns(Roles.Sale);
        _currentUser.Setup(c => c.UserId).Returns(10u);
        _contractRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Contract(khachHangId: 5));
        _customerRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KhachHang { Id = 5, NhanVienPhuTrachId = 10 }); // đúng Sale này phụ trách

        await _sut.Handle(new UpdateContractStatusCommand(1, "TamDung"), default);

        _contractRepo.Verify(r => r.UpdateStatusAsync(1, "TamDung", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Manager_DoiTrangThaiBatKyHopDongNao_KhongBiChan()
    {
        _currentUser.Setup(c => c.Role).Returns(Roles.Manager);
        _contractRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Contract(khachHangId: 5));

        await _sut.Handle(new UpdateContractStatusCommand(1, "TamDung"), default);

        // Manager không giới hạn theo người phụ trách nên không cần gọi tới ICustomerRepository.
        _customerRepo.Verify(r => r.GetByIdAsync(It.IsAny<ulong>(), It.IsAny<CancellationToken>()), Times.Never);
        _contractRepo.Verify(r => r.UpdateStatusAsync(1, "TamDung", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HopDongDaThanhLy_KhongDoiTrangThaiTiepDuoc()
    {
        _currentUser.Setup(c => c.Role).Returns(Roles.Manager);
        _contractRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Contract(khachHangId: 5, trangThai: ContractStatus.ThanhLy));

        var act = () => _sut.Handle(new UpdateContractStatusCommand(1, "DangThucHien"), default);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
