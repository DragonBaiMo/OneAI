using System.Text.Json.Serialization;
using OneAI.Services.AI.Models.Dtos;
using Thor.Abstractions.Dtos;
using Thor.Abstractions.ObjectModels.ObjectModels;
using Thor.Abstractions.ObjectModels.ObjectModels.SharedModels;

namespace OneAI.Services.AI.Models.ObjectModels.ObjectModels.SharedModels;

public record FileResponse : OneAIBaseResponse, IOpenAiModels.IId, IOpenAiModels.ICreatedAt
{
    [JsonPropertyName("bytes")] public int? Bytes { get; set; }
    [JsonPropertyName("filename")] public string FileName { get; set; }
    public UploadFilePurposes.UploadFilePurpose PurposeEnum => UploadFilePurposes.ToEnum(Purpose);
    [JsonPropertyName("purpose")] public string Purpose { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; }
    [JsonPropertyName("created_at")] public int CreatedAt { get; set; }
    [JsonPropertyName("id")] public string Id { get; set; }
}