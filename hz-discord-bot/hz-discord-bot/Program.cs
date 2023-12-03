using hz_discord_bot;
using hz_discord_bot.DiscordService;
using hz_discord_bot.DiscordService.Singleton;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();

        services.AddSingleton<IClientGetter, ClientGetter>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ISlashCommandsService, SlashCommandsService>();
    })
    .UseSerilog((host, config) =>
    {
        config.ReadFrom.Configuration(host.Configuration);
    })
    .Build();

await host.RunAsync();
