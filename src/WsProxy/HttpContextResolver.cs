using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace WsProxy;

public class HttpContextResolver
{
    private readonly ConcurrentDictionary<string, HttpContext> _contexts = new(StringComparer.OrdinalIgnoreCase );
    
    public bool TryPut(string id, HttpContext context)
    {
        return _contexts.TryAdd(id, context);
    }
    
    public bool TryExtract(string id, [NotNullWhen(true)] out HttpContext? context)
    {
        return _contexts.TryRemove(id, out context);
    }
}