using OpenAI;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Defines an <see cref="IChatClient"/> implementation wrapping the Github Copilot API.
/// </summary>
public class GithubChatClient : IChatClient
{
    private readonly IChatClient _openAIChatClient;
    private readonly string _modelId;

    public GithubChatClient(IHttpContextAccessor httpContextAccessor, string? modelId = null)
        : this(httpContextAccessor.HttpContext?.Request.Headers["X-GitHub-Token"]!, modelId)
    {
    }

    public GithubChatClient(string githubToken, string? modelId = null)
    {
        ArgumentNullException.ThrowIfNull(githubToken);

        _modelId = modelId ?? "gpt-4o";
        OpenAIClientOptions options = new() { Endpoint = new("https://api.githubcopilot.com") };
        _openAIChatClient = new OpenAI.Chat.ChatClient(_modelId, credential: new(githubToken), options).AsChatClient();
    }

    public async Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var completion = await _openAIChatClient.CompleteAsync(chatMessages, options, cancellationToken).ConfigureAwait(false);
        completion.ModelId ??= _modelId;
        return completion;
    }

    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var update in _openAIChatClient.CompleteStreamingAsync(chatMessages, options, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            update.ModelId ??= _modelId;
            yield return update;
        }
    }

    object? IChatClient.GetService(Type serviceType, object? serviceKey) => _openAIChatClient.GetService(serviceType, serviceKey);
    void IDisposable.Dispose() => _openAIChatClient.Dispose();
}
