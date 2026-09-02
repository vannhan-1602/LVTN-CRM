using MediatR;

namespace CRM.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string? RefreshToken, string? RequestIp) : IRequest<Unit>;
