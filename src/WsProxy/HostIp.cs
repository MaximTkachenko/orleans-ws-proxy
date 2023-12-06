using System.Net;

namespace WsProxy;

public static class Host
{
    static Host()
    {
        var podIp = Environment.GetEnvironmentVariable("POD_IP");
        if (!IPAddress.TryParse(podIp, out IpAddress))
        {
            IpAddress = IPAddress.Loopback;
        }
    }
    
    public static readonly IPAddress IpAddress; 
}