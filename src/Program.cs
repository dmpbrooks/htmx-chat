using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCors(options
        => options.AddPolicy(
            name: "cors",
            configurePolicy: policyBuilder =>
            {
                policyBuilder.AllowAnyHeader();
                policyBuilder.AllowAnyMethod();
                policyBuilder.AllowAnyOrigin();
            }));

builder.Services.AddSingleton<ChatService>();

await using var app = builder.Build();

app.UseCors("cors");
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.MapGet("/register", (ChatService chatService)
    => $"<input id='clientId' name='clientId' value='{chatService.Register()}' type='hidden' >");

app.MapGet("/messages", async (HttpContext ctx, ChatService chatService, CancellationToken cancellationToken) =>
{
    ctx.Response.Headers.Add("Content-Type", "text/event-stream");

    var lastMessageCheck = DateTime.Now;

    while (!cancellationToken.IsCancellationRequested)
    {
        var latestMessages = chatService.GetLatestMessages(lastMessageCheck);
        foreach (var message in latestMessages)
        {
            var output = $"data: <div class='msg'><label>Received At: </label><span>{message.timeReceived}</span><label>Message from client {message.clientId}: </label><span>{message.message}</span></div>\n\n";
            await ctx.Response.WriteAsync(output);
            await ctx.Response.Body.FlushAsync();
        }
        lastMessageCheck = DateTime.Now;
        await Task.Delay(500);
    }
});

app.MapPost("/add", (ChatService chatService, [FromForm] string clientId, [FromForm] string message)
    => chatService.AddMessage(clientId, message))
    .DisableAntiforgery();

await app.RunAsync();
