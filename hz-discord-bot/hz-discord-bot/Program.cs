using hz_discord_bot;
using hz_discord_bot.DectalkService;
using hz_discord_bot.DiscordService;
using hz_discord_bot.DiscordService.Singleton;
using hz_discord_bot.DiscordService.SlashCommand;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();

        services.AddSingleton<IClientGetter, ClientGetter>();
        services.AddSingleton<IDectalkService, DectalkService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ISlashCommandsService, SlashCommandsService>();
        services.AddScoped<ISlashCommand, TestCommand>();
    })
    .UseSerilog((host, config) =>
    {
        config.ReadFrom.Configuration(host.Configuration);
    })
    .Build();

await host.RunAsync();
