namespace CRM.Domain.Interfaces.Services;

public interface IOpenAiService
{
    Task<string> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default);
}
