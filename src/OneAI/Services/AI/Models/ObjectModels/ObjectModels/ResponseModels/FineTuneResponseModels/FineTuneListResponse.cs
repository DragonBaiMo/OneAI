using System.Text.Json.Serialization;
using OneAI.Services.AI.Models.Dtos;
using Thor.Abstractions.Dtos;

namespace Thor.Abstractions.ObjectModels.ObjectModels.ResponseModels.FineTuneResponseModels;

public record FineTuneListResponse : OneAIBaseResponse
{
    [JsonPropertyName("data")] public List<FineTuneResponse> Data { get; set; }
}