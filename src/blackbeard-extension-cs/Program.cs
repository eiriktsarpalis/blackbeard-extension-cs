using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddChatClient<GithubChatClient>(ServiceLifetime.Scoped).UseFunctionInvocation();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.MapGet("/", () => "Ahoy, matey! Welcome to the Blackbeard Pirate GitHub Copilot Extension!");
app.MapPost("/", (OpenAIChatCompletionRequest completionRequest, IChatClient chatClient) =>
{
    completionRequest.Options.Tools = [AIFunctionFactory.Create(GetNearestPorts)];
    completionRequest.Messages.Insert(0, new ChatMessage
    {
        Role = ChatRole.System,
        Contents = [new TextContent("Concisely reply as if you were Blackbeard the friendly Pirate.")]
    });

    return chatClient.GetStreamingResponseAsync(completionRequest.Messages, completionRequest.Options)
        .ToOpenAISseResult();
});

app.Run();

IEnumerable<Port> GetNearestPorts()
{
    yield return new Port("Port Royal", GarrisonSize: 25, Treasure: 2000);
    yield return new Port("Nassau", GarrisonSize: 100, Treasure: 10_000);
    yield return new Port("Tortuga", GarrisonSize: 30, Treasure: 2000);
    yield return new Port("Havana", GarrisonSize: 10, Treasure: 4000);
}

record Port(string Name, int GarrisonSize, int Treasure);