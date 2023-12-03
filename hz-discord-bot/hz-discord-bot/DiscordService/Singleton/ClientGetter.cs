using Discord.WebSocket;

namespace hz_discord_bot.DiscordService.Singleton
{
    internal class ClientGetter : IClientGetter
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;

        public ClientGetter(ILogger<ClientGetter> logger)
        {
            _logger = logger;
            _logger.LogInformation("Init DiscordSocketClient");
            _client = new DiscordSocketClient();
        }

        public DiscordSocketClient GetClient()
        {
            return _client;
        }
    }
}
