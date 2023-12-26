using Discord;
using Discord.WebSocket;
using hz_discord_bot.DectalkService;
using hz_discord_bot.DiscordService.Singleton;

namespace hz_discord_bot.DiscordService.SlashCommand
{
    internal class TtsCommand : ISlashCommand
    {
        private const string COMMAND_NAME = "tts";
        private const string OPTION_NAME = "input-text";

        private readonly ILogger _logger;
        private IAudioHandler _audioHandler;
        private IDectalkService _dectalkService;

        public TtsCommand(ILogger<TtsCommand> logger, IDectalkService dectalkService, IAudioHandler audioHandler)
        {
            _logger = logger;
            _dectalkService = dectalkService;
            _audioHandler = audioHandler;
        }

        public async Task AddCommand(SocketGuild guild)
        {
            var commandBuilder = new SlashCommandBuilder();
            commandBuilder.WithName(GetName());
            commandBuilder.WithDescription("Play TTS");
            commandBuilder.AddOption(OPTION_NAME, ApplicationCommandOptionType.String, "The text you want to play tts", isRequired: true);

            await guild.CreateApplicationCommandAsync(commandBuilder.Build());
            _logger.LogInformation("Creating TTS Command with name {CommandName} for Guild: {GuildId}", COMMAND_NAME, guild.Id);
        }

        public string GetName()
        {
            return COMMAND_NAME;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            var text = (string)(command.Data.Options.Where(o => o.Name == OPTION_NAME).FirstOrDefault()?.Value ?? "");
            var user = command.User;
            if (user is not SocketGuildUser guildUser)
            {
                await command.RespondAsync("Command not being used in server");
                return;
            }
            var channel = guildUser.VoiceChannel;
            if (channel == null)
            {
                await command.RespondAsync("User must be in a voice channel");
                return;
            }
            var existingChannel = _audioHandler.GetCurrentChannel(guildUser.Guild.Id);
            if (!_audioHandler.IsQueueEmpty(guildUser.Guild.Id) && existingChannel != null)
            {
                if (existingChannel.Id != channel.Id)
                {
                    await command.RespondAsync("Bot is busy in another channel");
                    return;
                }
            }
            else if (existingChannel?.Id != channel.Id)
            {
                await _audioHandler.Disconnect(guildUser.Guild.Id);
                existingChannel = null;
            }
            _ = Task.Run(async () =>
            {
                await command.DeferAsync();
                if (existingChannel != null || await _audioHandler.ConnectAsync(guildUser.Guild.Id, channel))
                {
                    var fileName = _dectalkService.WaveOut(text);
                    if (!_audioHandler.AddToQueue(guildUser.Guild.Id, fileName))
                    {
                        await command.ModifyOriginalResponseAsync((x) => {
                            x.Content = "Unable to add tts";
                        });
                    }
                    else
                    {
                        await command.ModifyOriginalResponseAsync((x) => {
                            x.Content = text;
                        });
                    }
                }
                else
                {
                    await command.ModifyOriginalResponseAsync((x) => {
                        x.Content = "Unable to connect";
                    });
                }
            });
        }
    }
}
