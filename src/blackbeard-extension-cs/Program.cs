using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddChatClient(services => new OllamaChatClient("http://localhost:11434", modelId: "deepseek-r1"));
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
        .SkipWhile(update => update.Text is not "</think>") // Skip all chain-of-thought updates
        .Skip(1) // Skip the </think> update
        .Prepend(new() { Text = "Arr, let me give it a think..."}) // Preface response with a thinking message.
        .ToOpenAISseResult();
});

app.Run();