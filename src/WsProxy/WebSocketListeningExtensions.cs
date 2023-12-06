using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace WsProxy;

public static class WebSocketListeningExtensions
{
    public static async IAsyncEnumerable<(Memory<byte> Data, bool EndOfMessage, bool Close)> StartListeningAsync(this WebSocket webSocket,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Memory<byte> buffer = new byte[2 * 1024];
        while (true)
        {
            var result = await webSocket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                yield return (default, false, true);
                yield break;
            }
            
            yield return (buffer.Slice(0, result.Count), result.EndOfMessage, false);
        }
    }
}