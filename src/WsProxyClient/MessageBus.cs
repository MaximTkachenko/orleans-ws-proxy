using System.Threading.Channels;

namespace WsProxyClient;

public class MessageBus
{
    private readonly Channel<string> _incomingMessages = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public async Task AddCommandAsync(string command)
    {
        await _incomingMessages.Writer.WriteAsync(command);
    }

    public async Task<string> ReadCommandAsync(CancellationToken cancellationToken)
    {
        return await _incomingMessages.Reader.ReadAsync(cancellationToken);
    }
}