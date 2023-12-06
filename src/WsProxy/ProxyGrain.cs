using System.Net.WebSockets;

namespace WsProxy;

public interface IProxyGrain : IGrainWithStringKey
{
    Task<string> GetInfoAsync();
    Task TryStartWebSocketChatAsync(string sourceIpAddress, string webSocketServerUrl);
}

public class ProxyGrain : Grain, IProxyGrain
{
    private readonly HttpContextResolver _httpContextResolver;
    private readonly string _hostIp;

    private string _id;

    private WebSocket? _serverWebSocket;
    private Task? _serverListening;
    
    private WebSocket? _clientWebSocket;
    private Task? _clientListening;
    private TaskCompletionSource? _clientListeningTcs;

    private CancellationTokenSource? _stopChatting;
    
    public ProxyGrain(HttpContextResolver httpContextResolver)
    {
        _httpContextResolver = httpContextResolver;
        _hostIp = Host.IpAddress.ToString();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _id = this.GetPrimaryKeyString();
        
        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        // todo kill all tasks
        
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    public Task<string> GetInfoAsync()
    {
        return Task.FromResult(Host.IpAddress.ToString());
    }

    public async Task TryStartWebSocketChatAsync(string sourceIpAddress, string webSocketServerUrl)
    {
        if (sourceIpAddress != _hostIp)
            return;

        if (!_httpContextResolver.TryExtract(_id, out var httpContext))
            return;

        var serverWebSocket = new ClientWebSocket();
        await serverWebSocket.ConnectAsync(new Uri(webSocketServerUrl), default);
        _serverWebSocket = serverWebSocket;
        
        // todo kill existing client connection
        
        _clientWebSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
        _clientListeningTcs = new TaskCompletionSource();
        httpContext.Put(_id, _clientListeningTcs);

        _stopChatting = new CancellationTokenSource();
        _serverListening = ServerListeningAsync();
        _clientListening = ClientListeningAsync();

        // todo metrics
        // todo handle exceptions handling
    }

    private async Task ServerListeningAsync()
    {
        // todo handle when other one is disconnected
        
        await foreach (var message in _serverWebSocket.StartListeningAsync(_stopChatting.Token))
        {
            if (message.Close)
            {
                break;
            }
            
            _clientWebSocket.SendAsync(message.Data, WebSocketMessageType.Text, message.EndOfMessage, _stopChatting.Token);
        }
    }

    private async Task ClientListeningAsync()
    {
        // todo handle when other one is disconnected        
        
        await foreach (var message in _clientWebSocket.StartListeningAsync(_stopChatting.Token))
        {
            if (message.Close)
            {
                break;
            }
            
            _serverWebSocket.SendAsync(message.Data, WebSocketMessageType.Text, message.EndOfMessage, _stopChatting.Token);
        }
    }
}