using hz_discord_bot.DectalkService;
using hz_discord_bot.DiscordService;

namespace hz_discord_bot
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDectalkService _dectalkService;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IDectalkService dectalkService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _dectalkService = dectalkService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(Exit);
            _logger.LogInformation("Starting Worker");
            using var scope = _serviceProvider.CreateScope();
            var discordClientService = scope.ServiceProvider.GetRequiredService<IClientService>();
            await discordClientService.Main();
        }

        private async void Exit()
        {
            _logger.LogInformation("Shutting down");
            var shutdownTasks = new List<Task>
            {
                _dectalkService.HandleShutdown()
            };
            await Task.WhenAll(shutdownTasks);
        }
    }
}