using Discord;
using Discord.WebSocket;
using hz_discord_bot.DiscordService.Singleton;

namespace hz_discord_bot.DiscordService.SlashCommand
{
    internal class DisconnectCommand : ISlashCommand
    {
        private const string COMMAND_NAME = "disconnect";

        private readonly ILogger _logger;
        private IAudioHandler _audioHandler;

        public DisconnectCommand(ILogger<DisconnectCommand> logger, IAudioHandler audioHandler)
        {
            _logger = logger;
            _audioHandler = audioHandler;
        }

        public async Task AddCommand(SocketGuild guild)
        {
            var commandBuilder = new SlashCommandBuilder();
            commandBuilder.WithName(GetName());
            commandBuilder.WithDescription("Try to disconnect from existing voice channel");

            await guild.CreateApplicationCommandAsync(commandBuilder.Build());
            _logger.LogInformation("Creating Disconnect Command with name {CommandName} for Guild: {GuildId}", GetName(), guild.Id);
        }

        public string GetName()
        {
            return COMMAND_NAME;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            var user = command.User;
            if (user is not SocketGuildUser guildUser)
            {
                await command.RespondAsync("Command not being used in server");
                return;
            }
            var channel = guildUser.VoiceChannel;
            try
            {
                _ = _audioHandler.Disconnect(guildUser.Guild.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encountered exception when attempting to disconnect via command");
            }
            await command.RespondAsync("Attempting to disconnect");
        }
    }
}
