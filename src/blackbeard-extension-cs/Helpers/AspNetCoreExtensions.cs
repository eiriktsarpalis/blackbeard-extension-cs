namespace Microsoft.Extensions.AI;

public static class AspNetCoreExtensions
{
    /// <summary>
    /// Registers a chat client of the specified type with the service collection using scoped lifetime.
    /// </summary>
    /// <typeparam name="TChatClient">The implementation type of the chat client.</typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static ChatClientBuilder AddScopedChatClient<TChatClient>(this IServiceCollection services)
        where TChatClient : class, IChatClient
    {
        ChatClientBuilder builder = new(provider => ActivatorUtilities.CreateInstance<TChatClient>(provider));
        services.AddScoped(builder.Build);
        return builder;
    }

    /// <summary>
    /// Converts the <see cref="IAsyncEnumerable{T}"/> of <see cref="StreamingChatCompletionUpdate"/> to an <see cref="IResult"/>
    /// that streams the updates using server-sent events in the OpenAI wire format.
    /// </summary>
    /// <param name="streamingUpdates"></param>
    /// <returns></returns>
    public static IResult ToOpenAISseResult(this IAsyncEnumerable<StreamingChatCompletionUpdate> streamingUpdates)
    {
        ArgumentNullException.ThrowIfNull(streamingUpdates);
        return new OpenAISseCompletionResult(streamingUpdates);
    }

    private sealed class OpenAISseCompletionResult(IAsyncEnumerable<StreamingChatCompletionUpdate> streamingUpdates) : IResult
    {
        public async Task ExecuteAsync(HttpContext ctx)
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
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