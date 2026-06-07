using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
            return await next();

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            _logger.LogDebug("Begin transaction for {Request}", requestName);
            var response = await next();
            _logger.LogDebug("Committed transaction for {Request}", requestName);
            return response;
        }, cancellationToken);
    }
}