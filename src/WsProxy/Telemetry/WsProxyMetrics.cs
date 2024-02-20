using System.Diagnostics.Metrics;

namespace WsProxy.Telemetry;

public class WsProxyMetrics
{
    public const string MeterName = nameof(WsProxyMetrics);
    
    private readonly UpDownCounter<long> _proxyGrainCounter;
    
    public WsProxyMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _proxyGrainCounter = meter.CreateUpDownCounter<long>("proxy-grain-counter");
    }

    public void ProxyGrainActivated() => _proxyGrainCounter.Add(1);
    
    public void ProxyGrainDeactivated() => _proxyGrainCounter.Add(-1);
}