using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Contracts.Commands.UpdateMilestone;

// Cập nhật 1 mốc triển khai: ghi nhận đã thực hiện / đã khách xác nhận, đính kèm biên bản.
public record UpdateMilestoneCommand(
    ulong Id, string? NoiDung, DateTime? NgayThucHien, uint? NhanVienThucHienId,
    string? NguoiXacNhanKhach, string? FileBienBan, string TrangThai) : IRequest<MocTrienKhaiDto>;

public class UpdateMilestoneCommandValidator : AbstractValidator<UpdateMilestoneCommand>
{
    private static readonly string[] TrangThaiHopLe = { "ChuaThucHien", "DaThucHien", "DaXacNhan" };

    public UpdateMilestoneCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
        RuleFor(x => x.TrangThai).NotEmpty().Must(x => TrangThaiHopLe.Contains(x))
            .WithMessage("TrangThai phải là ChuaThucHien, DaThucHien hoặc DaXacNhan.");
        RuleFor(x => x.NoiDung).MaximumLength(255);
        RuleFor(x => x.NguoiXacNhanKhach).MaximumLength(255);
        RuleFor(x => x.FileBienBan).MaximumLength(500);
    }
}

public class UpdateMilestoneCommandHandler : IRequestHandler<UpdateMilestoneCommand, MocTrienKhaiDto>
{
    private readonly IContractMilestoneRepository _milestoneRepository;
    private readonly IContractRepository _contractRepository;
    private readonly ICurrentUserService _currentUser;

    public UpdateMilestoneCommandHandler(
        IContractMilestoneRepository milestoneRepository, IContractRepository contractRepository,
        ICurrentUserService currentUser)
    {
        _milestoneRepository = milestoneRepository;
        _contractRepository = contractRepository;
        _currentUser = currentUser;
    }

    public async Task<MocTrienKhaiDto> Handle(UpdateMilestoneCommand request, CancellationToken ct)
    {
        var existing = await _milestoneRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("HD_MocTrienKhai", request.Id);

        // Đồng bộ với CreateMilestoneCommandHandler: hợp đồng đã Thanh lý là hồ sơ lịch sử đã
        // đóng — trước đây chỉ CreateMilestone chặn việc này, UpdateMilestone (sửa trạng thái/
        // xác nhận khách/file biên bản) lại không chặn, nên vẫn "xác nhận" được 1 mốc triển khai
        // trên hợp đồng đã thanh lý xong — không hợp lý về mặt hồ sơ pháp lý.
        var hopDong = await _contractRepository.GetByIdAsync(existing.HopDongId, ct);
        if (hopDong?.TrangThai == ContractStatus.ThanhLy)
            throw new BusinessRuleException(
                $"Hợp đồng {hopDong.MaHopDong} đã thanh lý, không thể cập nhật mốc triển khai.");

        var noiDung = request.NoiDung?.Trim();
        var ngayThucHien = request.NgayThucHien;
        var nhanVienThucHienId = request.NhanVienThucHienId;

        if (_currentUser.Role == Roles.Sale)
        {
            // Sale chỉ được cập nhật mốc triển khai được gán cho chính mình — không được
            // thao tác trên mốc của người khác, và không được đổi nội dung/ngày thực hiện/
            // người phụ trách, chỉ được cập nhật trạng thái thực hiện, người xác nhận khách,
            // và file biên bản. Việc tạo mới hoặc gán lại cho người khác do Manager thực hiện.
            if (existing.NhanVienThucHienId != _currentUser.UserId)
                throw new ForbiddenException("Bạn chỉ có thể cập nhật mốc triển khai được gán cho mình.");

            noiDung = existing.NoiDung;
            ngayThucHien = existing.NgayThucHien;
            nhanVienThucHienId = existing.NhanVienThucHienId;
        }

        return await _milestoneRepository.UpdateAsync(
            request.Id, noiDung, ngayThucHien, nhanVienThucHienId,
            request.NguoiXacNhanKhach?.Trim(), request.FileBienBan, request.TrangThai, ct)
        ?? throw new NotFoundException("HD_MocTrienKhai", request.Id);
    }
}
