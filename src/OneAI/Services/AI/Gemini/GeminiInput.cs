using System.Text.Json.Serialization;

namespace OneAI.Services.AI.Gemini;

public class GeminiInput
{
    [JsonPropertyName("contents")] public GeminiContents[] Contents { get; set; }

    [JsonPropertyName("generationConfig")] public object? GenerationConfig { get; set; }

    [JsonPropertyName("system_instruction")]
    public SystemInstruction? SystemInstruction { get; set; }

    [JsonPropertyName("tools")]
    public object[] Tools { get; set; }
    
    [JsonPropertyName("toolConfig")]
    public object? ToolConfig { get; set; }
    
    [JsonPropertyName("safetySettings")] public object[]? SafetySettings { get; set; }

    [JsonPropertyName("cachedContent")]
    public string? CachedContent { get; set; }
}

public class SystemInstruction
{
    public object[]? Parts { get; set; }
}

public class GeminiContents
{
    [JsonPropertyName("parts")] public object[] Parts { get; set; }

    [JsonPropertyName("role")] public string Role { get; set; } = "user";
}
