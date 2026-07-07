namespace CRM.Infrastructure.OpenAI;

public class OpenAiSettings
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";

    // Để trống = dùng OpenAI thật (trả phí). Điền vào đây để dùng miễn phí qua provider
    // tương thích OpenAI API, ví dụ:
    //   Groq:    https://api.groq.com/openai/v1   (model: "llama-3.3-70b-versatile")
    //   Ollama:  http://localhost:11434/v1         (model: "llama3.1", ApiKey: "ollama" - bất kỳ giá trị nào)
    // Xem hướng dẫn chi tiết trong SECURITY-SETUP.md / README phần "AI miễn phí".
    public string? BaseUrl { get; set; }
}
