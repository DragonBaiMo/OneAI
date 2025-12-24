using System.Text.Json.Serialization;
using OneAI.Services.AI.Models.Dtos;
using Thor.Abstractions.Dtos;
using Thor.Abstractions.ObjectModels.ObjectModels.SharedModels;

namespace Thor.Abstractions.ObjectModels.ObjectModels.ResponseModels.FineTuningJobResponseModels;

public record FineTuningJobListEventsResponse : OneAIBaseResponse
{
    [JsonPropertyName("data")] public List<EventResponse> Data { get; set; }

    [JsonPropertyName("has_more")] public bool HasMore { get; set; }
}