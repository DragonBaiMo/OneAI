using System.Text.Json.Serialization;
using OneAI.Services.AI.Models.Dtos;
using OneAI.Services.AI.Models.ObjectModels.ObjectModels.SharedModels;
using Thor.Abstractions.Dtos;
using Thor.Abstractions.ObjectModels.ObjectModels.SharedModels;

namespace Thor.Abstractions.ObjectModels.ObjectModels.ResponseModels.FileResponseModels;

public record FileListResponse : OneAIBaseResponse
{
    [JsonPropertyName("data")] public List<FileResponse> Data { get; set; }
}