namespace Microsoft.Extensions.AI;

using OpenAI;

#pragma warning disable CS8425

public class GithubChatClient : IChatClient
{
    private readonly OpenAIChatClient _openAIChatClient;
    private readonly string _modelId;

    public GithubChatClient(IServiceProvider serviceProvider)
        : this(serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request.Headers["X-GitHub-Token"]!)
    {
    }

    private GithubChatClient(string githubToken, string? modelId = null)
    {
        ArgumentNullException.ThrowIfNull(githubToken);

        OpenAIClientOptions options = new()
        {
            Endpoint = new("https://api.githubcopilot.com"),
        };

        _modelId = modelId ??= "gpt-4o";
        OpenAI.Chat.ChatClient openAIClient = new(model: modelId, credential: new(githubToken), options);
        _openAIChatClient = new(openAIClient);
    }

    public ChatClientMetadata Metadata => _openAIChatClient.Metadata;

    public async Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var completion = await _openAIChatClient.CompleteAsync(chatMessages, options, cancellationToken).ConfigureAwait(false);
        completion.ModelId ??= _modelId;
        return completion;
    }

    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        await foreach (var update in _openAIChatClient.CompleteStreamingAsync(chatMessages, options, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            update.ModelId ??= _modelId;
            yield return update;
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => _openAIChatClient.GetService(serviceType, serviceKey);
    public void Dispose() => ((IDisposable)_openAIChatClient).Dispose();
}
