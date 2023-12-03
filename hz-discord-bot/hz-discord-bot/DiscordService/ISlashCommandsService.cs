using Discord.WebSocket;

namespace hz_discord_bot.DiscordService
{
    internal interface ISlashCommandsService
    {
        public Task AddSlashCommands();
    }
}
