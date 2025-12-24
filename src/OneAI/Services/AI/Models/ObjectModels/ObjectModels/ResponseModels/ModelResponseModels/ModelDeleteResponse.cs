using System.Text.Json.Serialization;
using OneAI.Services.AI.Models.Dtos;
using Thor.Abstractions.Dtos;

namespace Thor.Abstractions.ObjectModels.ObjectModels.ResponseModels.ModelResponseModels;

public record ModelDeleteResponse : OneAIBaseResponse
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("deleted")] public bool Deleted { get; set; }
}