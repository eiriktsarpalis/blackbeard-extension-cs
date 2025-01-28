using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.AI;

public static class AspNetCoreExtensions
{
    public static IResult ToOpenAISseResult(this IAsyncEnumerable<StreamingChatCompletionUpdate> streamingUpdates)
    {
        return new OpenAISseCompletionResult(streamingUpdates);
    }

    public static ChatClientBuilder AddChatClient<TChatClient>(this IServiceCollection services)
        where TChatClient : class, IChatClient
    {
        ChatClientBuilder builder = new(provider => ActivatorUtilities.CreateInstance<TChatClient>(provider));
        services.AddScoped(builder.Build);
        services.AddHttpContextAccessor();
        return builder;
    }
}