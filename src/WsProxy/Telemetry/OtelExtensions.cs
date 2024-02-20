using System.Diagnostics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace WsProxy.Telemetry;

public static class OtelExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry().WithMetrics(meterBuilder =>
        {
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddAttributes(new Dictionary<string, object>
                {
                    //["service.version"] = FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion ?? throw new InvalidOperationException(),
                    ["service.instance.id"] = Guid.NewGuid().ToString()
                });
            meterBuilder.SetResourceBuilder(resourceBuilder)
                .AddMeter(WsProxyMetrics.MeterName)
                .AddRuntimeInstrumentation()
                .AddOtlpExporter();
            
            // if (builder.Environment.IsDevelopment())
            // {
            //     meterBuilder.AddConsoleExporter();
            // }
        });

        return builder;
    }
}