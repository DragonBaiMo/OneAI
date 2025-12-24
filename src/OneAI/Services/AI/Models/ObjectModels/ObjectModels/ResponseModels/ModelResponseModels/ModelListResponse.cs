using System.Text.Json.Serialization;
using OneAI.Services.AI.Models.Dtos;
using Thor.Abstractions.Dtos;

namespace Thor.Abstractions.ObjectModels.ObjectModels.ResponseModels.ModelResponseModels;

public record ModelListResponse : OneAIBaseResponse
{
    [JsonPropertyName("data")] public List<ModelResponse> Models { get; set; }
}