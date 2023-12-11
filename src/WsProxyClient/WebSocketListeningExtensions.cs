using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace WsProxyClient;

public static class WebSocketListeningExtensions
{
    public static async IAsyncEnumerable<(string? Message, bool Close)> StartListeningAsync(this WebSocket webSocket,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Memory<byte> buffer = new byte[2 * 1024];
        var messageSize = 0;
        var finalBuffer = new MemoryStream();
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await webSocket.ReceiveAsync(buffer, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                yield return (default, true);
                yield break;
            }

            await finalBuffer.WriteAsync(buffer[..result.Count], cancellationToken);
            messageSize += result.Count;
            
            if (result.EndOfMessage)
            {
                yield return  (Encoding.UTF8.GetString(finalBuffer.GetBuffer().AsSpan().Slice(0, messageSize)), false);
                
                finalBuffer.Position = 0;
                messageSize = 0;
            }
        }
    }
}