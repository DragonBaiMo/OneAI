using System.Text.Json.Serialization;
using OneAI.Services.AI.Models.Dtos;
using Thor.Abstractions.Dtos;

namespace Thor.Abstractions.ObjectModels.ObjectModels.ResponseModels.FineTuningJobResponseModels;

public record FineTuningJobListResponse : OneAIBaseResponse
{
    [JsonPropertyName("data")] public List<FineTuningJobResponse> Data { get; set; }

    [JsonPropertyName("has_more")] public bool HasMore { get; set; }
}