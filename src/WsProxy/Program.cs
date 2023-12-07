using WsProxy;
using Host = WsProxy.Host;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddOrleans();
builder.Services.AddSingleton<HttpContextResolver>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseWebSockets();

app.MapGet("/{id}/info", async (string id, IClusterClient clusterClient) =>
    {
        return await clusterClient.GetGrain<IProxyGrain>(id).GetInfoAsync();
    })
    .WithName("GetProxyInfo")
    .WithOpenApi();

app.MapGet("/{id}/.ws", async (string id, string? webSocketServerUrl, HttpContext httpContext, 
        HttpContextResolver httpContextResolver, IClusterClient clusterClient) =>
    {
        webSocketServerUrl ??= "ws://localhost:10010/.ws";
        
        if (!httpContext.WebSockets.IsWebSocketRequest)
            return Results.BadRequest("Not a websocket request.");

        try
        {
            if (!httpContextResolver.TryPut(id, httpContext))
                return Results.BadRequest("We already have an HttpContext for this id.");

            // return result, probably Try
            await clusterClient.GetGrain<IProxyGrain>(id)
                .TryStartWebSocketChatAsync(Host.IpAddress.ToString(), webSocketServerUrl);

            await httpContext.Get<TaskCompletionSource>(id).Task;

            return Results.Ok();
        }
        finally
        {
            // remove httpContext from resolver anyway
            httpContextResolver.TryExtract(id, out _);
        }
    })
    .WithName("StartWebSocketChat")
    .WithOpenApi();

app.Run();