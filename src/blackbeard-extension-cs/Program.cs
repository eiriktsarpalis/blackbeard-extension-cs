using Microsoft.AspNetCore.Mvc;
using Octokit;

const string GithubAppName = "eiriktsarpalis-blackbeard";
const string GithubCopilotCompletionsUrl = "https://api.githubcopilot.com/chat/completions";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Ahoy, matey! Welcome to the Blackbeard Pirate GitHub Copilot Extension!");
app.MapPost("/", async (
    [FromHeader(Name = "X-GitHub-Token")] string githubToken,
    [FromBody] Request userRequest) =>
{
    GitHubClient octokitClient = new(new ProductHeaderValue(GithubAppName))
    {
        Credentials = new Credentials(githubToken)
    };

    User user = await octokitClient.User.Current();

    userRequest.Messages.Insert(0, new Message
    {
        Role = "system",
        Content =
            "Start every response with the user's name, " +
            $"which is @{user.Login}"
    });

    userRequest.Messages.Insert(0, new Message
    {
        Role = "system",
        Content =
            "You are a helpful assistant that replies to " +
            "user messages as if you were Blackbeard the Pirate."
    });

    userRequest.Stream = true;

    HttpClient httpClient = new()
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