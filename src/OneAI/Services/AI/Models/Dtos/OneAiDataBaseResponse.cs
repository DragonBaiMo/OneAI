using System.Text.Json.Serialization;

namespace OneAI.Services.AI.Models.Dtos;

/// <summary>
/// </summary>
/// <typeparam name="T"></typeparam>
public record OneAiDataBaseResponse<T> : OneAIBaseResponse
{
    /// <summary>
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}