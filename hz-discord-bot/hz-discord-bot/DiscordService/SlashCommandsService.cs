using Discord;
using Discord.WebSocket;
using hz_discord_bot.DiscordService.Singleton;

namespace hz_discord_bot.DiscordService
{
    internal class SlashCommandsService : ISlashCommandsService
    {
        private const string FIRST_TEST_COMMAND = "first-test-command";

        private readonly ILogger _logger;
        private readonly IClientGetter _client;
        private readonly List<ulong> _guild_ids;

        public SlashCommandsService(ILogger<SlashCommandsService> logger, IClientGetter client, IConfiguration configuration)
        {
            _logger = logger;
            _client = client;
            _guild_ids = configuration.GetSection("guildIds").Get<List<ulong>>() ?? new List<ulong>();

            if (!_guild_ids.Any())
            {
                _logger.LogWarning("No Guild Ids found");
            }
        }

        public async Task AddSlashCommands()
        {
            var client = _client.GetClient();
            await AddTestCommand(client);
            client.SlashCommandExecuted += SlashCommandHandler;
        }

        private async Task AddTestCommand(DiscordSocketClient client)
        {
            foreach (var guildId in _guild_ids)
            { 
                try
                {
                    var guild = client.GetGuild(guildId);

                    var command = new SlashCommandBuilder();
                    command.WithName(FIRST_TEST_COMMAND);
                    command.WithDescription("This is my first test slash command");

                    await guild.CreateApplicationCommandAsync(command.Build());
                    _logger.LogInformation("Creating Test Command with name {CommandName} for Guild: {GuildId}", FIRST_TEST_COMMAND, guildId);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to Add Test Command for Guild: {GuildId}", guildId);
                }
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            _logger.LogInformation("Handlingg Slash Command {CommandName}", command.CommandName);
            switch (command.CommandName)
            {
                case FIRST_TEST_COMMAND:
                    await command.RespondAsync($"Hello World");
                    break;
                default:
                    _logger.LogWarning("Unknown command name {CommandName}", command.CommandName);
                    break;
            }
        }
    }
}
