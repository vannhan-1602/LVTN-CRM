using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Contracts;
using CRM.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Contracts.Commands.DeleteMilestone;

public record DeleteMilestoneCommand(ulong Id) : IRequest<bool>;

public class DeleteMilestoneCommandValidator : AbstractValidator<DeleteMilestoneCommand>
{
    public DeleteMilestoneCommandValidator() => RuleFor(x => x.Id).GreaterThan(0UL);
}

public class DeleteMilestoneCommandHandler : IRequestHandler<DeleteMilestoneCommand, bool>
{
    private readonly IContractMilestoneRepository _milestoneRepository;
    private readonly IContractRepository _contractRepository;

    public DeleteMilestoneCommandHandler(
        IContractMilestoneRepository milestoneRepository, IContractRepository contractRepository)
    {
        _milestoneRepository = milestoneRepository;
        _contractRepository = contractRepository;
    }

    public async Task<bool> Handle(DeleteMilestoneCommand request, CancellationToken ct)
    {
        var existing = await _milestoneRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("HD_MocTrienKhai", request.Id);

        // Đồng bộ với CreateMilestone/UpdateMilestone: không xóa được mốc triển khai — vốn là
        // bằng chứng đã đào tạo/bàn giao/nghiệm thu — khỏi 1 hợp đồng đã thanh lý xong.
        var hopDong = await _contractRepository.GetByIdAsync(existing.HopDongId, ct);
        if (hopDong?.TrangThai == ContractStatus.ThanhLy)
            throw new BusinessRuleException(
                $"Hợp đồng {hopDong.MaHopDong} đã thanh lý, không thể xóa mốc triển khai.");

        var ok = await _milestoneRepository.DeleteAsync(request.Id, ct);
        if (!ok) throw new NotFoundException("HD_MocTrienKhai", request.Id);
        return true;
    }
}
