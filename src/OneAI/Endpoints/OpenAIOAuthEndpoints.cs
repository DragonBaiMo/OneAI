using Microsoft.EntityFrameworkCore;
using OneAI.Data;
using OneAI.Models;
using OneAI.Services.OpenAIOAuth;

namespace OneAI.Endpoints;

/// <summary>
/// OpenAI OAuth 相关的 Minimal APIs
/// </summary>
public static class OpenAIOAuthEndpoints
{
    /// <summary>
    /// 映射 OpenAI OAuth 相关的端点
    /// </summary>
    public static void MapOpenAIOAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/openai/oauth")
            .WithTags("OpenAI OAuth")
            .RequireAuthorization();

        // 生成 OpenAI OAuth 授权链接
        group.MapPost("/authorize", GenerateOAuthUrl)
            .WithName("GenerateOpenAIOAuthUrl")
            .WithSummary("生成 OpenAI OAuth 授权链接")
            .WithDescription("生成用于 OpenAI 授权的链接")
            .Produces<ApiResponse<object>>(200)
            .Produces<ApiResponse>(401)
            .Produces<ApiResponse>(500);

        // 处理 OpenAI OAuth 回调
        group.MapPost("/callback", ExchangeOAuthCode)
            .WithName("ExchangeOpenAIOAuthCode")
            .WithSummary("处理 OpenAI OAuth 授权码")
            .WithDescription("交换授权码并创建账户")
            .Produces<ApiResponse<AIAccountDto>>(200)
            .Produces<ApiResponse>(400)
            .Produces<ApiResponse>(401)
            .Produces<ApiResponse>(500);
    }

    /// <summary>
    /// 生成 OpenAI OAuth 授权链接
    /// </summary>
    private static IResult GenerateOAuthUrl(
        GenerateOpenAiOAuthUrlRequest request,
        OpenAiOAuthHelper oAuthHelper,
        IOAuthSessionService sessionService,
        OpenAIOAuthService authService)
    {
        try
        {
            var result = authService.GenerateOpenAIOAuthUrl(request, oAuthHelper, sessionService);
            return Results.Json(ApiResponse<object>.Success(result, "授权链接生成成功"));
        }
        catch (Exception ex)
        {
            return Results.Json(
                ApiResponse.Fail($"生成授权链接失败: {ex.Message}", 500),
                statusCode: 500
            );
        }
    }

    /// <summary>
    /// 处理 OpenAI OAuth 授权码
    /// </summary>
    private static async Task<IResult> ExchangeOAuthCode(
        ExchangeOpenAiOAuthCodeRequest request,
        OpenAiOAuthHelper oAuthHelper,
        IOAuthSessionService sessionService,
        OpenAIOAuthService authService,
        AppDbContext dbContext)
    {
        try
        {
            await authService.ExchangeOpenAIOAuthCode(dbContext, request, oAuthHelper, sessionService);

            return Results.Json(ApiResponse.Success("账户创建成功"));
        }
        catch (ArgumentException ex)
        {
            return Results.Json(
                ApiResponse.Fail(ex.Message, 400),
                statusCode: 400
            );
        }
        catch (Exception ex)
        {
            return Results.Json(
                ApiResponse.Fail($"处理授权码失败: {ex.Message}", 500),
                statusCode: 500
            );
        }
    }
}