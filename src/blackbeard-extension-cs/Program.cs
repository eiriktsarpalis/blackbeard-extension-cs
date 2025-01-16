using Microsoft.Extensions.AI;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IChatClient>(services =>
    new GithubChatClient(services, modelId: "gpt-4")
        .AsBuilder()
        .UseFunctionInvocation()
        .Build());
    
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

app.MapGet("/", () => "Ahoy, matey! Welcome to the Blackbeard Pirate GitHub Copilot Extension!");
app.MapPost("/", (
    OpenAIChatCompletionRequest chatCompletionRequest,
    IChatClient chatClient) =>
{
    (chatCompletionRequest.Options.Tools ??= []).Add(AIFunctionFactory.Create(GetNearestPorts));
    chatCompletionRequest.Messages.Insert(0, new ChatMessage
    {
        Role = ChatRole.System,
        Contents = [new TextContent("Concisely reply as if you were Blackbeard the Pirate.")]
    });

    return chatClient.CompleteStreamingAsync(chatCompletionRequest.Messages, chatCompletionRequest.Options)
        .ToOpenAISseResult();
});

app.Run();

[Description("Scouted ports and their treasure.")]
IEnumerable<Port> GetNearestPorts()
{
    yield return new Port("Port Royal", GarrisonSize: 25, Treasure: 2000);
    yield return new Port("Nassau", GarrisonSize: 100, Treasure: 10_000);
    yield return new Port("Tortuga", GarrisonSize: 30, Treasure: 2000);
    yield return new Port("Havana", GarrisonSize: 10, Treasure: 4000);
}

record Port(string Name, int GarrisonSize, int Treasure);