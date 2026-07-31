using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Products;
using CRM.Domain.Entities.Sales;
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

    public CreateLicenseCommandHandler(
        IContractRepository contractRepository, IContractMilestoneRepository milestoneRepository,
        IProductRepository productRepository, ILicenseRepository licenseRepository)
    {
        _contractRepository = contractRepository;
        _milestoneRepository = milestoneRepository;
        _productRepository = productRepository;
        _licenseRepository = licenseRepository;
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

        var ngayKichHoat = DateOnly.FromDateTime(DateTime.UtcNow);
        var ngayHetHan = hopDong.ThoiHan.HasValue ? ngayKichHoat.AddMonths(hopDong.ThoiHan.Value) : (DateOnly?)null;

        return await _licenseRepository.AddAsync(
            request.HopDongId, request.SanPhamId, request.SoLuongUser, request.PhienBan?.Trim(),
            request.MoiTruongTrienKhai, ngayKichHoat, ngayHetHan, ct);
    }
}
