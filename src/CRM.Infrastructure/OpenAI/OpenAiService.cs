using CRM.Domain.Interfaces.Services;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace CRM.Infrastructure.OpenAI;

public class OpenAiService : IOpenAiService
{
    private readonly OpenAiSettings _settings;
    private readonly ChatClient _chatClient;

    public OpenAiService(IOptions<OpenAiSettings> settings)
    {
        _settings = settings.Value;
        _chatClient = new ChatClient(_settings.Model, _settings.ApiKey);
    }

    public async Task<string> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var completion = await _chatClient.CompleteChatAsync(
            [ChatMessage.CreateUserMessage(prompt)],
            cancellationToken: cancellationToken);

        return completion.Value.Content[0].Text ?? string.Empty;
    }
}
