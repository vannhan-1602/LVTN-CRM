using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using CRM.Domain.Entities.Sales;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Contracts.Commands.CreateMilestone;

// Tạo 1 mốc triển khai cho hợp đồng. Cho phép nhiều dòng cùng LoaiMoc (VD: DaoTao)
// để đối chiếu "đã đào tạo x/y buổi" theo SoLuong đã bán trong HD_BaoGia_ChiTiet.
public record CreateMilestoneCommand(
    ulong HopDongId, string LoaiMoc, string? NoiDung,
    DateTime? NgayThucHien, uint? NhanVienThucHienId) : IRequest<MocTrienKhaiDto>;

public class CreateMilestoneCommandValidator : AbstractValidator<CreateMilestoneCommand>
{
    private static readonly string[] LoaiMocHopLe = { "DaoTao", "BanGiao", "NghiemThu" };

    public CreateMilestoneCommandValidator()
    {
        RuleFor(x => x.HopDongId).GreaterThan(0UL);
        RuleFor(x => x.LoaiMoc).NotEmpty().Must(x => LoaiMocHopLe.Contains(x))
            .WithMessage("LoaiMoc phải là DaoTao, BanGiao hoặc NghiemThu.");
        RuleFor(x => x.NoiDung).MaximumLength(255);
    }
}

public class CreateMilestoneCommandHandler : IRequestHandler<CreateMilestoneCommand, MocTrienKhaiDto>
{
    private readonly IContractRepository _contractRepository;
    private readonly IContractMilestoneRepository _milestoneRepository;

    public CreateMilestoneCommandHandler(
        IContractRepository contractRepository, IContractMilestoneRepository milestoneRepository)
    {
        _contractRepository = contractRepository;
        _milestoneRepository = milestoneRepository;
    }

    public async Task<MocTrienKhaiDto> Handle(CreateMilestoneCommand request, CancellationToken ct)
    {
        _ = await _contractRepository.GetByIdAsync(request.HopDongId, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.HopDongId);

        return await _milestoneRepository.AddAsync(
            request.HopDongId, request.LoaiMoc, request.NoiDung?.Trim(),
            request.NgayThucHien, request.NhanVienThucHienId, ct);
    }
}
