using CRM.Application.Features.Auth.DTOs;
using MediatR;

namespace CRM.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponseDto>;
