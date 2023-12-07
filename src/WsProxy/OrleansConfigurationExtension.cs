namespace WsProxy;

public static class OrleansConfigurationExtension
{
    public static WebApplicationBuilder AddOrleans(this WebApplicationBuilder builder)
    {
        builder.Host.UseOrleans(siloBuilder =>
        {
            if (builder.Environment.IsDevelopment())
            {
                siloBuilder
                    .UseLocalhostClustering();
            }
            else
            {
                siloBuilder
                    .UseKubernetesHosting()
                    .UseAzureStorageClustering(clusteringOptions =>
                    {
                        clusteringOptions.TableName = "WsProxyClustering";
                        clusteringOptions.ConfigureTableServiceClient(Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING"));
                    });
            }
            
            siloBuilder
                .UseDashboard(options =>
                {
                    options.Port = 5223;
                });
        });

        return builder;
    }
}