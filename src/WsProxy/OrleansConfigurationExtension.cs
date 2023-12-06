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
                // todo for production, az table clustering, k8s hosting
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