using Discord.WebSocket;

namespace hz_discord_bot.DiscordService.Singleton
{
    internal interface IClientGetter
    {
        public DiscordSocketClient GetClient();
    }
}
