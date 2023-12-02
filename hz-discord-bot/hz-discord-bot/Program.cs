using hz_discord_bot;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .UseSerilog((host, config) =>
    {
        config.ReadFrom.Configuration(host.Configuration);
    })
    .Build();

await host.RunAsync();
