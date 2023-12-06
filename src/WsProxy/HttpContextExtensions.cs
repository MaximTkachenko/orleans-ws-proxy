namespace WsProxy;

public static class HttpContextExtensions
{
    public static void Put<TData>(this HttpContext httpContext, string key, TData data)
    {
        httpContext.Items[key] = data;
    }
    
    public static TData Get<TData>(this HttpContext httpContext, string key)
    {
        return (TData)httpContext.Items[key];
    }
}