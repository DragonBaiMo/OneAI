using OneAI.Services.AI.Models.Dtos;
using Thor.Abstractions.Dtos;

namespace Thor.Abstractions.ObjectModels.ObjectModels.ResponseModels;

/// <summary>
///     File content response
/// </summary>
/// <typeparam name="T"></typeparam>
public record AudioCreateSpeechResponse<T> : OneAiDataBaseResponse<T>
{
}