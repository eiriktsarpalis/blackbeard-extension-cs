namespace Microsoft.Extensions.AI;

public sealed class OpenAISseCompletionResult(IAsyncEnumerable<StreamingChatCompletionUpdate> streamingUpdates) : IResult
{
    public async Task ExecuteAsync(HttpContext ctx)
    {
        ctx.Response.StatusCode = StatusCodes.Status200OK;
        ctx.Response.ContentType = "application/json";
        await OpenAISerializationHelpers.SerializeStreamingAsync(ctx.Response.Body, streamingUpdates, cancellationToken: ctx.RequestAborted).ConfigureAwait(false);
    }
}
