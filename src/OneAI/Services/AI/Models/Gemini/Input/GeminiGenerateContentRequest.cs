namespace OneAI.Services.AI.Models.Gemini.Input;

/// <summary>
/// Gemini API 内容生成请求
/// </summary>
public class GeminiGenerateContentRequest
{
    /// <summary>
    /// 对话内容
    /// </summary>
    public List<GeminiContent>? Contents { get; set; }

    /// <summary>
    /// 生成配置
    /// </summary>
    public GeminiGenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// 系统指令
    /// </summary>
    public string? SystemInstruction { get; set; }

    /// <summary>
    /// 对话 ID（用于会话粘性）
    /// </summary>
    public string? ConversationId { get; set; }
}

/// <summary>
/// Gemini 内容块
/// </summary>
public class GeminiContent
{
    /// <summary>
    /// 角色（user 或 model）
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// 内容部分列表
    /// </summary>
    public List<GeminiPart>? Parts { get; set; }
}

/// <summary>
/// Gemini 内容部分
/// </summary>
public class GeminiPart
{
    /// <summary>
    /// 文本内容
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 内联数据（如图片）
    /// </summary>
    public GeminiInlineData? InlineData { get; set; }
}

/// <summary>
/// 内联数据（二进制内容）
/// </summary>
public class GeminiInlineData
{
    /// <summary>
    /// MIME 类型
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Base64 编码的数据
    /// </summary>
    public string? Data { get; set; }
}

/// <summary>
/// Gemini 生成配置
/// </summary>
public class GeminiGenerationConfig
{
    /// <summary>
    /// 最大输出令牌数
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// 温度参数
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// Top P 参数
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// Top K 参数
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// 停止序列
    /// </summary>
    public List<string>? StopSequences { get; set; }
}
