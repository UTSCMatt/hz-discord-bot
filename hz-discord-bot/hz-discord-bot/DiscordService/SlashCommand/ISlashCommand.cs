using Discord.WebSocket;

namespace hz_discord_bot.DiscordService.SlashCommand
{
    public interface ISlashCommand
    {
        public string GetName();
        public Task AddCommand(SocketGuild guild);
        public Task HandleCommand(SocketSlashCommand command);
    }
}
