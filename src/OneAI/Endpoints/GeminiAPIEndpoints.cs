using System.Text.Json;
using OneAI.Services;
using OneAI.Services.AI;
using OneAI.Services.AI.Gemini;
using OneAI.Services.AI.Models.Gemini.Input;

namespace OneAI.Endpoints;

/// <summary>
/// Gemini API 端点 - 处理 Gemini 原生 API 请求
/// </summary>
public static class GeminiAPIEndpoints
{
    /// <summary>
    /// 映射 Gemini API 端点
    /// </summary>
    public static void MapGeminiAPIEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Gemini 非流式内容生成 API
        endpoints.MapPost("/v1beta/models/{model}:generateContent", HandleGenerateContent)
            .WithName("GeminiGenerateContent")
            .WithSummary("Gemini 内容生成（非流式）")
            .WithDescription("使用 Gemini 模型生成内容（非流式响应）");

        // Gemini 流式内容生成 API
        endpoints.MapPost("/v1beta/models/{model}:streamGenerateContent", HandleStreamGenerateContent)
            .WithName("GeminiStreamGenerateContent")
            .WithSummary("Gemini 内容生成（流式）")
            .WithDescription("使用 Gemini 模型生成内容（流式响应）");
    }

    /// <summary>
    /// 读取请求体
    /// </summary>
    private static async Task<byte[]> ReadRequestBodyAsync(HttpContext context)
    {
        if (context.Request.ContentLength == 0)
        {
            return Array.Empty<byte>();
        }

        await using var buffer = context.Request.ContentLength is > 0 and <= int.MaxValue
            ? new MemoryStream((int)context.Request.ContentLength.Value)
            : new MemoryStream();

        await context.Request.Body.CopyToAsync(buffer, context.RequestAborted);

        return buffer.ToArray();
    }

    /// <summary>
    /// 处理 Gemini 非流式内容生成请求
    /// </summary>
    private static async Task HandleGenerateContent(
        string model,
        HttpContext context,
        GeminiAPIService geminiService,
        GeminiInput input,
        AIAccountService aiAccountService)
    {
        // 提取 conversation_id 用于会话粘性
        var conversationId = context.Request.Headers.TryGetValue("conversation_id", out var convId)
            ? convId.ToString()
            : null;

        await geminiService.ExecuteGenerateContent(context, input, model, conversationId, aiAccountService);
    }

    /// <summary>
    /// 处理 Gemini 流式内容生成请求
    /// </summary>
    private static async Task HandleStreamGenerateContent(
        string model,
        HttpContext context,
        GeminiAPIService geminiService,
        GeminiInput input,
        AIAccountService aiAccountService)
    {
        // 提取 conversation_id 用于会话粘性
        var conversationId = context.Request.Headers.TryGetValue("conversation_id", out var convId)
            ? convId.ToString()
            : null;

        await geminiService.ExecuteStreamGenerateContent(context, input, model, conversationId, aiAccountService);
    }
}
