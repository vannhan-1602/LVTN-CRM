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

        // BaseUrl trống → dùng OpenAI thật. BaseUrl có giá trị → trỏ sang provider
        // tương thích OpenAI API (Groq/Ollama/OpenRouter...) để dùng MIỄN PHÍ, không
        // cần đổi code nơi khác — chỉ đổi cấu hình trong appsettings/user-secrets.
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
            // ngắn, tránh model trả lời lan man (tốn token/tiền, chậm) mà không kiểm soát được.
            MaxOutputTokenCount = 1200
        };

        var completion = await _chatClient.CompleteChatAsync(
            [ChatMessage.CreateUserMessage(prompt)],
            options,
            cancellationToken: cancellationToken);

        return completion.Value.Content[0].Text ?? string.Empty;
    }
}
