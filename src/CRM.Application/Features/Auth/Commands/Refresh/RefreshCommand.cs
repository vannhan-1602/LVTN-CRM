using CRM.Application.Features.Auth.DTOs;
using MediatR;

namespace CRM.Application.Features.Auth.Commands.Refresh;

public record RefreshCommand(string RefreshToken, string? RequestIp) : IRequest<RefreshResultDto>;
