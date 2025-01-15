using Microsoft.AspNetCore.Mvc;

const string GithubCopilotCompletionsUrl = "https://api.githubcopilot.com/chat/completions";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Ahoy, matey! Welcome to the Blackbeard Pirate GitHub Copilot Extension!");
app.MapPost("/", async (
    [FromHeader(Name = "X-GitHub-Token")] string githubToken,
    [FromBody] Request userRequest) =>
{
    userRequest.Stream = true;
    userRequest.Messages.Insert(0, new Message
    {
        Role = "system",
        Content = "Concisely reply as if you were Blackbeard the Pirate."
    });

    using HttpClient httpClient = new()
    {
        DefaultRequestHeaders = { Authorization = new("Bearer", githubToken) }
    };

    var copilotLLMResponse = await httpClient.PostAsJsonAsync(GithubCopilotCompletionsUrl, userRequest);
    return Results.Stream(await copilotLLMResponse.Content.ReadAsStreamAsync(), "application/json");
});

app.Run();

public record Request
{
    public bool Stream { get; set; }
    public List<Message> Messages { get; set; } = [];
}

public record Message
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}