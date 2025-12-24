using System.Text.Json.Serialization;
using Thor.Abstractions.Dtos;

namespace OneAI.Services.AI.Models.Dtos;

public record OneAIBaseResponse
{
    /// <summary>
    ///     对象类型
    /// </summary>
    [JsonPropertyName("object")]
    public string? ObjectTypeName { get; set; }

    /// <summary>
    /// </summary>
    public bool Successful => Error == null;

    /// <summary>
    /// </summary>
    [JsonPropertyName("error")]
    public ThorError? Error { get; set; }
}