using Discord;
using Discord.WebSocket;
using hz_discord_bot.DiscordService.Singleton;
using hz_discord_bot.DiscordService.SlashCommand;

namespace hz_discord_bot.DiscordService
{
    internal class SlashCommandsService : ISlashCommandsService
    {
        private readonly ILogger _logger;
        private readonly IClientGetter _client;
        private readonly List<ulong> _guild_ids;
        private readonly IEnumerable<ISlashCommand> _commands;
        private readonly Dictionary<string, Func<SocketSlashCommand, Task>> commandHandlers;

        public SlashCommandsService(ILogger<SlashCommandsService> logger, IClientGetter client, IConfiguration configuration, IEnumerable<ISlashCommand> commands)
        {
            _logger = logger;
            _client = client;
            _guild_ids = configuration.GetSection("guildIds").Get<List<ulong>>() ?? new List<ulong>();
            _commands = commands;

            commandHandlers = new Dictionary<string, Func<SocketSlashCommand, Task>>();

            if (!_guild_ids.Any())
            {
                _logger.LogWarning("No Guild Ids found");
            }
        }

        public async Task AddSlashCommands()
        {
            var client = _client.GetClient();
            await AddCommands(client);
            client.SlashCommandExecuted += SlashCommandHandler;
        }

        private async Task AddCommands(DiscordSocketClient client)
        {
            foreach (var command in _commands)
            {
                if (commandHandlers.ContainsKey(command.GetName()))
                {
                    _logger.LogWarning("Command with {CommandName} already exists, skipping.", command.GetName());
                }
                else
                {
                    commandHandlers.Add(command.GetName(), command.HandleCommand);
                    foreach (var guildId in _guild_ids)
                    {
                        try
                        {
                            var guild = client.GetGuild(guildId);

                            await command.AddCommand(guild);
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError(exception, "Failed to Add Command: {CommandName} for Guild: {GuildId}", command.GetName(), guildId);
                        }
                    }
                }
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            _logger.LogInformation("Handling Slash Command {CommandName}", command.CommandName);
            if (commandHandlers.TryGetValue(command.CommandName, out var handler))
            {
                await handler(command);
            }
            else
            {
                _logger.LogWarning("Unknown command name {CommandName}", command.CommandName);
            }
        }
    }
}
