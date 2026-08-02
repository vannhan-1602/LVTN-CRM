using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Contracts.Commands.CreateLicense;

// Cấp License mới cho 1 hợp đồng. Chỉ ManagerOnly (enforce ở Controller) — giống mốc triển khai.
// Ràng buộc nghiệp vụ (đã thống nhất, không đổi DB):
//   - Chỉ hợp đồng LoaiHopDong=ChinhThuc, đang DangThucHien.
//   - SanPham phải thuộc LoaiSanPham.HinhThuc = "License".
//   - Phải đã có ít nhất 1 mốc triển khai LoaiMoc=BanGiao, TrangThai=DaXacNhan — license chỉ
//     cấp sau khi khách đã xác nhận nhận bàn giao, không cấp khống trước đó.
public record CreateLicenseCommand(
    ulong HopDongId, uint SanPhamId, int SoLuongUser,
    string? PhienBan, string MoiTruongTrienKhai) : IRequest<LicenseDto>;

public class CreateLicenseCommandValidator : AbstractValidator<CreateLicenseCommand>
{
    private static readonly string[] MoiTruongHopLe = { "Cloud", "OnPremise" };

    public CreateLicenseCommandValidator()
    {
        RuleFor(x => x.HopDongId).GreaterThan(0UL);
        RuleFor(x => x.SanPhamId).GreaterThan(0U);
        RuleFor(x => x.SoLuongUser).GreaterThan(0);
        RuleFor(x => x.PhienBan).MaximumLength(50);
        RuleFor(x => x.MoiTruongTrienKhai).NotEmpty().Must(x => MoiTruongHopLe.Contains(x))
            .WithMessage("MoiTruongTrienKhai phải là Cloud hoặc OnPremise.");
    }
}

public class CreateLicenseCommandHandler : IRequestHandler<CreateLicenseCommand, LicenseDto>
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractMilestoneRepository _milestoneRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILicenseRepository _licenseRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLicenseCommandHandler(
        IContractRepository contractRepository, IContractMilestoneRepository milestoneRepository,
        IProductRepository productRepository, ILicenseRepository licenseRepository,
        ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _milestoneRepository = milestoneRepository;
        _productRepository = productRepository;
        _licenseRepository = licenseRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<LicenseDto> Handle(CreateLicenseCommand request, CancellationToken ct)
    {
        var hopDong = await _contractRepository.GetByIdAsync(request.HopDongId, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.HopDongId);

        if (hopDong.TrangThai != "DangThucHien")
            throw new BusinessRuleException(
                $"Chỉ cấp License cho hợp đồng đang thực hiện. Hợp đồng {hopDong.MaHopDong} đang ở trạng thái '{hopDong.TrangThai}'.");

        if (hopDong.LoaiHopDong != "ChinhThuc")
            throw new BusinessRuleException(
                "Chỉ hợp đồng Chính thức mới được cấp License mới. Hợp đồng Gia hạn dùng chức năng " +
                "'Gia hạn License', hợp đồng Bảo trì không cấp License.");

        var sanPham = await _productRepository.GetByIdEnrichedAsync(request.SanPhamId, ct)
            ?? throw new NotFoundException("Sản phẩm", request.SanPhamId);

        if (sanPham.HinhThuc != "License")
            throw new BusinessRuleException(
                $"Sản phẩm '{sanPham.TenSP}' không phải sản phẩm dạng License (đang là '{sanPham.HinhThuc}').");

        var mocTrienKhais = await _milestoneRepository.GetByHopDongAsync(request.HopDongId, ct);
        var daBanGiao = mocTrienKhais.Any(m => m.LoaiMoc == "BanGiao" && m.TrangThai == "DaXacNhan");
        if (!daBanGiao)
            throw new BusinessRuleException(
                "Chưa thể cấp License: hợp đồng chưa có mốc Bàn giao được khách xác nhận.");

        // Đơn vị tồn kho cho sản phẩm License = 1 license key cấp ra (khớp với đơn giá đang
        // niêm yết "đ/License" và lịch sử giao dịch kho hiện có), KHÔNG phải theo SoLuongUser —
        // SoLuongUser chỉ là thông số kỹ thuật của license, không phải số lượng trừ kho.
        var tonHienTai = await _productRepository.GetCurrentStockAsync(request.SanPhamId, ct);
        if (tonHienTai < 1)
            throw new BusinessRuleException(
                $"Sản phẩm '{sanPham.TenSP}' đã hết tồn kho License, không thể cấp thêm.");

        var ngayKichHoat = DateOnly.FromDateTime(DateTime.UtcNow);
        // QUAN TRỌNG: License phải hết hạn CÙNG LÚC với hợp đồng (HopDong.NgayKetThuc = NgayKy +
        // ThoiHan, đã tính sẵn khi tạo hợp đồng) — KHÔNG được tính lại "NgayKichHoat + ThoiHan",
        // vì License thường được cấp trễ hơn NgayKy (sau khi đã Bàn giao xong), nên tính từ
        // NgayKichHoat sẽ cho ra ngày hết hạn License TRỄ HƠN ngày hết hạn hợp đồng — vô lý vì
        // license không thể còn hiệu lực sau khi hợp đồng đã kết thúc/thanh lý.
        var ngayHetHan = hopDong.NgayKetThuc;

        // Bọc trong 1 transaction: nếu AdjustStockAsync thất bại (VD: 2 Manager cùng cấp License
        // cho sản phẩm chỉ còn đúng 1 tồn kho — race condition giữa lúc check tonHienTai ở trên
        // và lúc trừ kho thật), License vừa tạo phải bị rollback theo, không để lại 1 dòng
        // License "ma" không có tồn kho đứng sau.
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var license = await _licenseRepository.AddAsync(
                request.HopDongId, request.SanPhamId, request.SoLuongUser, request.PhienBan?.Trim(),
                request.MoiTruongTrienKhai, ngayKichHoat, ngayHetHan, ct);

            // Trừ kho ngay khi cấp license thành công — không để nhân viên phải tự đi ghi tay ở
            // trang Sản phẩm nữa. MaChungTu dùng MaHopDong thật (có thể tra cứu lại), thay vì
            // để trống hoặc để người dùng gõ tay như trước.
            await _productRepository.AdjustStockAsync(
                request.SanPhamId, StockTransactionType.XuatBan, -1,
                maChungTu: hopDong.MaHopDong,
                ghiChu: $"Cấp license {sanPham.TenSP} ({license.MaLicenseKey}) cho hợp đồng {hopDong.MaHopDong}",
                nguoiThucHienId: _currentUser.UserId, ct: ct);

            return license;
        }, ct);
    }
}