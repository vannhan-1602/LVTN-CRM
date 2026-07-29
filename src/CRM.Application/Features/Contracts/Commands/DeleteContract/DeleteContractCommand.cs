using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Invoices;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Contracts.Commands.DeleteContract;

public record DeleteContractCommand(ulong Id) : IRequest<bool>;

public class DeleteContractCommandValidator : AbstractValidator<DeleteContractCommand>
{
    public DeleteContractCommandValidator() => RuleFor(x => x.Id).GreaterThan(0UL);
}

public class DeleteContractCommandHandler : IRequestHandler<DeleteContractCommand, bool>
{
    private const string AuditTable = "HD_HopDong";
    private readonly IContractRepository _contractRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IContractMilestoneRepository _milestoneRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<DeleteContractCommandHandler> _logger;

    public DeleteContractCommandHandler(
        IContractRepository contractRepository, IInvoiceRepository invoiceRepository,
        IContractMilestoneRepository milestoneRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ILogger<DeleteContractCommandHandler> logger)
    {
        _contractRepository = contractRepository;
        _invoiceRepository = invoiceRepository;
        _milestoneRepository = milestoneRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteContractCommand request, CancellationToken ct)
    {
        var contract = await _contractRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.Id);

        // ── Chặn xóa nếu hợp đồng đã có dữ liệu nghiệp vụ phụ thuộc ─────────
        // Không dùng transaction/DB constraint để báo lỗi (HD_MocTrienKhai, fk_hopdong_goc
        // là RESTRICT, sẽ ném DbUpdateException thô 500; còn KT_HoaDon là SET NULL, xóa sẽ
        // "chạy êm" nhưng làm hóa đơn — kể cả đã thu tiền — mất dấu vết hợp đồng gốc). Kiểm
        // tra tường minh ở đây để trả về thông báo nghiệp vụ rõ ràng thay vì lỗi hệ thống.
        if (await _invoiceRepository.ExistsForHopDongAsync(request.Id, ct))
            throw new BusinessRuleException(
                $"Không thể xóa hợp đồng {contract.MaHopDong} vì đã có hóa đơn phát sinh. " +
                "Vui lòng chuyển trạng thái hợp đồng sang 'Thanh lý' thay vì xóa.");

        var mocTrienKhais = await _milestoneRepository.GetByHopDongAsync(request.Id, ct);
        if (mocTrienKhais.Count > 0)
            throw new BusinessRuleException(
                $"Không thể xóa hợp đồng {contract.MaHopDong} vì đã có mốc triển khai được ghi nhận. " +
                "Vui lòng xóa các mốc triển khai trước, hoặc chuyển trạng thái hợp đồng sang 'Thanh lý' thay vì xóa.");

        var renewalLinks = await _contractRepository.GetRenewalLinksAsync(request.Id, ct);
        if (renewalLinks.Count > 0)
            throw new BusinessRuleException(
                $"Không thể xóa hợp đồng {contract.MaHopDong} vì đã có hợp đồng gia hạn được tạo từ hợp đồng này.");

        var deleted = await _contractRepository.DeleteAsync(request.Id, ct);
        if (!deleted) throw new NotFoundException(nameof(HopDong), request.Id);

        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "DELETE",
                oldData: contract, newData: null, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for contract {Id}", request.Id); }

        return true;
    }
}
