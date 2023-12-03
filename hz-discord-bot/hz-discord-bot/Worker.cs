using hz_discord_bot.DiscordService;

namespace hz_discord_bot
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Worker");
            using var scope = _serviceProvider.CreateScope();
            var discordClientService = scope.ServiceProvider.GetRequiredService<IClientService>();
            await discordClientService.Main();
        }
    }
}