using System.Net.WebSockets;
using System.Text;
using System.Web;

namespace WsProxyClient;

public class WebSocketsSupervisor : IHostedService
{
    private readonly Dictionary<string, (WebSocket WebSocket, Task Chatting)> _webSockets = new();
    private readonly MessageBus _bus;
    private readonly ILogger<WebSocketsSupervisor> _logger;
    private readonly IHttpClientFactory _http;
    private readonly CancellationTokenSource _stopping = new();

    private Task _listeningTask;
    
    public WebSocketsSupervisor(MessageBus bus,
        ILogger<WebSocketsSupervisor> logger,
        IHttpClientFactory http)
    {
        _bus = bus;
        _logger = logger;
        _http = http;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _listeningTask = ListeningAsync();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _stopping.CancelAsync();
        try
        {
            await _listeningTask;
        }
        catch(OperationCanceledException) when(_stopping.IsCancellationRequested)
        {
            // it's ok
        }
    }

    private async Task ListeningAsync()
    {
        var webSocketServerUrl = Environment.GetEnvironmentVariable("WS_SERVER_URL");
        var clusterUrl = Environment.GetEnvironmentVariable("WS_PROXY_CLUSTER_URL");
        
        while (!_stopping.IsCancellationRequested)
        {
            var command = await _bus.ReadCommandAsync(_stopping.Token);

            if (command.StartsWith(Commands.AddPrefix))
            {
                var countToAdd = Commands.ExtractCountToAdd(command);
                var startFrom = _webSockets.Count;
                var finishOn = _webSockets.Count + countToAdd;
                for (var i = startFrom; i < finishOn; i++)
                {
                    var connectionId = ConnectionNameResolver.GetName(i);
                    try
                    {
                        var podLocation = await _http.CreateClient()
                            .GetFromJsonAsync<LocationResponse>($"{clusterUrl}/{connectionId}/info", _stopping.Token);

                        var webSocket = new ClientWebSocket();
                        await webSocket.ConnectAsync(new Uri(
                                $"ws://{podLocation.IpAddress}:{podLocation.Port}/{connectionId}/.ws?webSocketServerUrl=" +
                                HttpUtility.UrlEncode(webSocketServerUrl)),
                            _stopping.Token);

                        _webSockets[connectionId] = (webSocket,
                            ChattingAsync(connectionId, webSocket, _stopping.Token));

                        _logger.LogInformation("New connection added {ConnectionId}", connectionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed starting connection {ConnectionId} : {Error}", connectionId, ex.Message);
                    }
                }

                continue;
            }

            if (command == Commands.KillAll)
            {
                var connectionsCount = _webSockets.Count;
                foreach (var webSocket in _webSockets)
                {
                    webSocket.Value.WebSocket.Dispose();
                }
                await Task.WhenAll(_webSockets.Select(ws => ws.Value.Chatting));
                _webSockets.Clear();
                
                _logger.LogInformation("Killed all {ConnectionsCount} connections", connectionsCount);
            }
        }
        
    }

    // todo use our own cancellation token
    // todo disconnection protocol
    // todo restore connection on fail
    private Task ChattingAsync(string connectionId, WebSocket webSocket, CancellationToken cancellationToken)
    {
        var sending = SendingAsync(connectionId, webSocket, cancellationToken);
        var receiving = ReceivingAsync(webSocket, cancellationToken);
        return Task.WhenAll(sending, receiving);
    }
    
    private async Task SendingAsync(string connectionId, WebSocket webSocket, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2) + TimeSpan.FromSeconds(Random.Shared.Next(0, 10)),
                    cancellationToken);

                var message = $"{connectionId}: {DateTime.UtcNow}";
                webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true,
                    cancellationToken);
                _logger.LogInformation(">> {Message}", message);
            }
        }
        catch
        {
        }
    }
    
    private async Task ReceivingAsync(WebSocket webSocket, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var message in webSocket.StartListeningAsync(cancellationToken))
            {
                if (message.Close)
                {
                    break;
                }
                
                _logger.LogInformation("<< {Message}", message.Message);
            }
        }
        catch
        {
        }
    }

    private record LocationResponse(string IpAddress, int Port);
}