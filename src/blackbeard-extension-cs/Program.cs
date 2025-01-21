using Microsoft.AspNetCore.Mvc;

const string GithubCopilotCompletionsUrl = "https://api.githubcopilot.com/chat/completions";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

app.MapGet("/", () => "Ahoy, matey! Welcome to the Blackbeard Pirate GitHub Copilot Extension!");
app.MapPost("/", async (
    [FromHeader(Name = "X-GitHub-Token")] string githubToken,
    ChatCompletionRequest chatCompletionRequest,
    HttpClient httpClient,
    CancellationToken cancellationToken) =>
{
    chatCompletionRequest.Stream = true;
    chatCompletionRequest.Messages.Insert(0, new ChatMessage
    {
        Role = "system",
        Content = "Concisely reply as if you were Blackbeard the friendly Pirate."
    });

    HttpRequestMessage httpRequest = new(HttpMethod.Post, GithubCopilotCompletionsUrl)
    {
        Headers = { Authorization = new("Bearer", githubToken) },
        Content = JsonContent.Create(chatCompletionRequest),
    };

    var copilotLLMResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
    return Results.Stream(await copilotLLMResponse.Content.ReadAsStreamAsync(), "application/json");
});

app.Run();

public record ChatCompletionRequest
{
    public List<ChatMessage> Messages { get; set; } = [];
    public bool Stream { get; set; }
}

public record ChatMessage
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}