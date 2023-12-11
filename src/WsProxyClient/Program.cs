using Microsoft.AspNetCore.Mvc;
using WsProxyClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<WebSocketsSupervisor>();
builder.Services.AddSingleton<MessageBus>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/add", async ([FromQuery]int? count, MessageBus bus) =>
    {
        await bus.AddCommandAsync(Commands.CreateAdd(count ?? 20));
        return Results.Accepted();
    })
    .WithName("EstablishNewWebSocketConnection")
    .WithOpenApi();

app.MapDelete("/kill-all", async (MessageBus bus) =>
    {
        await bus.AddCommandAsync(Commands.KillAll);
        return Results.Accepted();
    })
    .WithName("BreakAllWebSocketConnections")
    .WithOpenApi();

app.Run();