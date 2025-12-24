namespace OneAI.Services.AI;


public class AIProviderAsyncLocal
{
    private static readonly AsyncLocal<AIProviderHolder> _AIProviderHolder = new();

    public static List<int> AIProviderIds
    {
        get => _AIProviderHolder.Value?.AIProviderIds ?? new List<int>(5);
        set
        {
            _AIProviderHolder.Value ??= new AIProviderHolder();

            _AIProviderHolder.Value.AIProviderIds = value;
        }
    }

    private sealed class AIProviderHolder
    {
        /// <summary>
        /// 已经试用的渠道ID
        /// </summary>
        public List<int> AIProviderIds { get; set; } = new(5);
    }
}