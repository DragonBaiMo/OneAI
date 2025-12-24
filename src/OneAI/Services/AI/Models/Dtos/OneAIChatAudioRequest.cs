using System.Text.Json.Serialization;

namespace OneAI.Services.AI.Models.Dtos;

public sealed class OneAIChatAudioRequest
{
    [JsonPropertyName("voice")] public string? Voice { get; set; }

    [JsonPropertyName("format")] public string? Format { get; set; }
}