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
            
                siloBuilder
                    .UseDashboard(options =>
                    {
                        options.Port = 5223;
                    });
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
        });

        return builder;
    }
}