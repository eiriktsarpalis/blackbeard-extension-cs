using Microsoft.Extensions.AI;

const string OllamaEndpoint = "http://localhost:11434";
const string OllamaModel = "llama3";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Ahoy, matey! Welcome to the Blackbeard Pirate GitHub Copilot Extension!");
app.MapPost("/", async (HttpContext ctx) =>
{
    var openAiRequest = await OpenAISerializationHelpers.DeserializeChatCompletionRequestAsync(ctx.Request.Body, ctx.RequestAborted);

    openAiRequest.Options.ModelId = null;
    openAiRequest.Messages.Insert(0, new ChatMessage
    {
        Role = ChatRole.System,
        Contents = [new TextContent("Concisely reply as if you were Blackbeard the Pirate.")]
    });

    using IChatClient chatClient = new OllamaChatClient(OllamaEndpoint, OllamaModel);
    var streamingResponse = chatClient.CompleteStreamingAsync(openAiRequest.Messages, openAiRequest.Options, ctx.RequestAborted);

    ctx.Response.StatusCode = StatusCodes.Status200OK;
    ctx.Response.ContentType = "application/json";
    await OpenAISerializationHelpers.SerializeStreamingAsync(ctx.Response.Body, streamingResponse, cancellationToken: ctx.RequestAborted);
});

app.Run();
