using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IChatClient, GithubChatClient>();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

app.MapGet("/", () => "Ahoy, matey! Welcome to the Blackbeard Pirate GitHub Copilot Extension!");
app.MapPost("/", (
    OpenAIChatCompletionRequest chatCompletionRequest,
    IChatClient chatClient) =>
{
    chatCompletionRequest.Messages.Insert(0, new ChatMessage
    {
        Role = ChatRole.System,
        Contents = [new TextContent("Concisely reply as if you were Blackbeard the Pirate.")]
    });

    var streamingResponse = chatClient.CompleteStreamingAsync(chatCompletionRequest.Messages, chatCompletionRequest.Options);
    return new OpenAISseCompletionResult(streamingResponse);
});

app.Run();