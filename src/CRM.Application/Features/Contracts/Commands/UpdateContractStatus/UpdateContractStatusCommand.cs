using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Customers;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Contracts.Commands.UpdateContractStatus;

public record UpdateContractStatusCommand(ulong Id, string TrangThai) : IRequest<ContractDto>;

public class UpdateContractStatusCommandValidator : AbstractValidator<UpdateContractStatusCommand>
{
    public UpdateContractStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
        RuleFor(x => x.TrangThai)
            .NotEmpty()
            .Must(v => ContractStatus.All.Contains(v))
            .WithMessage("Trạng thái hợp đồng không hợp lệ.");
    }
}

public class UpdateContractStatusCommandHandler : IRequestHandler<UpdateContractStatusCommand, ContractDto>
{
    private const string AuditTable = "HD_HopDong";
    private readonly IContractRepository _contractRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateContractStatusCommandHandler> _logger;

    public UpdateContractStatusCommandHandler(
        IContractRepository contractRepository, ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork, IAuditLogPublisher auditLogPublisher,
        ICurrentUserService currentUser, ILogger<UpdateContractStatusCommandHandler> logger)
    {
        _contractRepository = contractRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ContractDto> Handle(UpdateContractStatusCommand request, CancellationToken ct)
    {
        var contract = await _contractRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.Id);

        // Sale chỉ đổi trạng thái hợp đồng của khách hàng mình phụ trách — đồng bộ với check
        // đã có sẵn ở CreateContractFromQuoteCommandHandler. HopDong không lưu người phụ trách
        // trực tiếp nên phải tra qua KhachHang (giống pattern CreateQuoteCommandHandler).
        if (_currentUser.Role == Roles.Sale)
        {
            var khachHang = await _customerRepository.GetByIdAsync(contract.KhachHangId, ct);
            if (khachHang?.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException(
                    "Bạn không có quyền thay đổi trạng thái hợp đồng của khách hàng do nhân viên khác phụ trách.");
        }

        if (contract.TrangThai == ContractStatus.ThanhLy)
            throw new BusinessRuleException("Hợp đồng đã thanh lý, không thể thay đổi trạng thái.");

        var oldDto = await _contractRepository.GetByIdEnrichedAsync(request.Id, ct);

        await _contractRepository.UpdateStatusAsync(request.Id, request.TrangThai, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var newDto = await _contractRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.Id);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: oldDto, newData: newDto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for contract {Id}", request.Id); }

        return newDto;
    }
}
