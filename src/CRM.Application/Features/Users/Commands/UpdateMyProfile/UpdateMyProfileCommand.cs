using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Users;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Users.Commands.UpdateMyProfile;

public record UpdateMyProfileCommand(string HoTen, string? Email, string? SoDienThoai) : IRequest<UserDto>;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.HoTen).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.SoDienThoai)
            .MaximumLength(20)
            .Matches(@"^[0-9+\-\s()]+$").WithMessage("Số điện thoại không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.SoDienThoai));
    }
}

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, UserDto>
{
    private readonly IUserManagementRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateMyProfileCommandHandler(
        IUserManagementRepository repository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<UserDto> Handle(UpdateMyProfileCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Không xác định được người dùng.");

        // Email trùng với người khác thì chặn — trừ chính mình ra khỏi kiểm tra.
        var currentProfile = await _repository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _repository.EmailExistsAsync(request.Email, currentProfile.NhanSuId, ct))
        {
            throw new BusinessRuleException($"Email '{request.Email}' đã được sử dụng bởi nhân sự khác.");
        }

        await _repository.UpdateMyProfileAsync(userId, request.HoTen, request.Email, request.SoDienThoai, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await _repository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);
    }
}