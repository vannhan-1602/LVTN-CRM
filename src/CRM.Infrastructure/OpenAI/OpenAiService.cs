using System.ClientModel;
using CRM.Domain.Interfaces.Services;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace CRM.Infrastructure.OpenAI;

public class OpenAiService : IOpenAiService
{
    private readonly OpenAiSettings _settings;
    private readonly ChatClient _chatClient;

    public OpenAiService(IOptions<OpenAiSettings> settings)
    {
        _settings = settings.Value;

        _chatClient = string.IsNullOrWhiteSpace(_settings.BaseUrl)
            ? new ChatClient(_settings.Model, _settings.ApiKey)
            : new ChatClient(
                _settings.Model,
                new ApiKeyCredential(_settings.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(_settings.BaseUrl) });
    }

    public async Task<string> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var options = new ChatCompletionOptions
        {
            // Giới hạn rõ ràng thay vì để mặc định của provider — đủ cho vài đoạn JSON/text
            // ngắn, tránh model trả lời lan man
            MaxOutputTokenCount = 1200
        };

        var completion = await _chatClient.CompleteChatAsync(
            [ChatMessage.CreateUserMessage(prompt)],
            options,
            cancellationToken: cancellationToken);

        return completion.Value.Content[0].Text ?? string.Empty;
    }
}
