namespace Microsoft.Extensions.AI;

public static class AspNetCoreExtensions
{
    /// <summary>
    /// Registers a chat client of the specified type with the service collection using scoped lifetime.
    /// </summary>
    /// <typeparam name="TChatClient">The implementation type of the chat client.</typeparam>
    public static ChatClientBuilder AddChatClient<TChatClient>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TChatClient : class, IChatClient
    {
        return services.AddChatClient(provider => ActivatorUtilities.CreateInstance<TChatClient>(provider), lifetime);
    }

    /// <summary>
    /// Registers a chat client of the specified type with the service collection using scoped lifetime.
    /// </summary>
    /// <typeparam name="TChatClient">The implementation type of the chat client.</typeparam>
    public static ChatClientBuilder AddChatClient(this IServiceCollection services, Func<IServiceProvider, IChatClient> innerClientFactory, ServiceLifetime lifetime)
    {
        ChatClientBuilder builder = new(innerClientFactory);
        services.Add(new ServiceDescriptor(typeof(IChatClient), builder.Build, lifetime));
        return builder;
    }

    /// <summary>
    /// Converts the <see cref="IAsyncEnumerable{T}"/> of <see cref="StreamingChatCompletionUpdate"/> to an <see cref="IResult"/>
    /// that streams the updates using server-sent events in the OpenAI wire format.
    /// </summary>
    public static IResult ToOpenAISseResult(this IAsyncEnumerable<StreamingChatCompletionUpdate> streamingUpdates)
    {
        ArgumentNullException.ThrowIfNull(streamingUpdates);
        return new OpenAISseCompletionResult(streamingUpdates);
    }

    private sealed class OpenAISseCompletionResult(IAsyncEnumerable<StreamingChatCompletionUpdate> streamingUpdates) : IResult
    {
        public async Task ExecuteAsync(HttpContext ctx)
        {
            ctx.Response.ContentType = "text/event-stream";
            streamingUpdates = streamingUpdates.Select(update =>
            {
                update.ModelId ??= "<unspecified>";
                return update;
            });

            await OpenAISerializationHelpers.SerializeStreamingAsync(ctx.Response.Body, streamingUpdates, cancellationToken: ctx.RequestAborted).ConfigureAwait(false);
        }
    }
}