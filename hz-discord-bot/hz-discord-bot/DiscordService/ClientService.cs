using Discord.WebSocket;
using Discord;
using hz_discord_bot.DiscordService.Singleton;

namespace hz_discord_bot.DiscordService
{
    internal class ClientService : IClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ISlashCommandsService _slashCommandsService;
        private readonly List<ulong> _guild_ids;

        public ClientService(ILogger<ClientService> logger, IConfiguration configuration, ISlashCommandsService slashCommandsService, IClientGetter client)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client.GetClient();
            _slashCommandsService = slashCommandsService;

            _guild_ids = configuration.GetSection("guildIds").Get<List<ulong>>() ?? new List<ulong>();

            if (!_guild_ids.Any())
            {
                _logger.LogWarning("No Guild Ids found");
            }

            _client.Log += Log;
        }

        public async Task Main()
        {
            var token = _configuration.GetValue<string>("discordBotToken");

            _logger.LogInformation("Logging into Discord");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Ready += ReadyBoot;
            _client.Ready += _slashCommandsService.AddSlashCommands;
        }

        private async Task ReadyBoot()
        {
            try
            {
                var tasks = new List<Task>
                {
                    CleanUpGlobalCommands(),
                    CleanUpGuildCommands()
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to do all Boot Tasks");
            }
        }

        private async Task CleanUpGlobalCommands()
        {
            try
            {
                var tasks = new List<Task>();
                var globalCommands = await _client.GetGlobalApplicationCommandsAsync();
                foreach (var globalCommand in globalCommands)
                {
                    tasks.Add(globalCommand.DeleteAsync());
                }
                await Task.WhenAll(tasks);
                _logger.LogInformation("Successfully cleaned up global commands");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up Global Commands");
            }
        }

        private async Task CleanUpGuildCommands()
        {
            try
            {
                var tasks = new List<Task>();

                foreach (var guildId in _guild_ids)
                {
                    var guild = _client.GetGuild(guildId);
                    var guildCommands = await guild.GetApplicationCommandsAsync();
                    foreach (var guildCommand in guildCommands)
                    {
                        tasks.Add(guildCommand.DeleteAsync());
                    }

                }
                await Task.WhenAll(tasks);
                _logger.LogInformation("Successfully cleaned up guild commands");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up Guild Commands");
            }
        }

        private Task Log(LogMessage msg)
        {
            switch(msg.Severity)
            {
                case LogSeverity.Error:
                    _logger.LogError(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Critical:
                    _logger.LogCritical(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(msg.Exception, msg.Message);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
