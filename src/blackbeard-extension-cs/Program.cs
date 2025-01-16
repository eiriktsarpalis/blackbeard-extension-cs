using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddChatClient<GithubChatClient>();
var app = builder.Build();

app.MapGet("/", () => "Ahoy, matey! Welcome to the Blackbeard Pirate GitHub Copilot Extension!");
app.MapPost("/", (OpenAIChatCompletionRequest completionRequest, IChatClient chatClient) =>
{
    completionRequest.Messages.Insert(0, new ChatMessage
    {
        Role = ChatRole.System,
        Contents = [new TextContent("Concisely reply as if you were Blackbeard the friendly Pirate.")]
    });

    return chatClient.CompleteStreamingAsync(completionRequest.Messages, completionRequest.Options)
        .ToOpenAISseResult();
});

app.Run();