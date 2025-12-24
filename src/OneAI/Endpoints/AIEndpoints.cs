using OneAI.Services;
using OneAI.Services.AI;
using OneAI.Services.AI.Models.Responses.Input;

namespace OneAI.Endpoints;

public static class AIEndpoints
{
    /// <summary>
    /// 映射 AI 账户相关的端点
    /// </summary>
    public static void MapAIEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/v1/responses", async (ResponsesService responses,
            HttpContext context,
            ResponsesInput input,
            AIAccountService aiAccountService) =>
        {
            await responses.Execute(context, input, aiAccountService);
        });
    }
}