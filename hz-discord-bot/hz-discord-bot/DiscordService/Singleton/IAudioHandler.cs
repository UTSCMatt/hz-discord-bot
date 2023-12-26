using Discord.WebSocket;

namespace hz_discord_bot.DiscordService.Singleton
{
    internal interface IAudioHandler
    {
        public bool AddToQueue(ulong guildId, string fileName);
        public bool IsQueueEmpty(ulong guildId);
        public SocketVoiceChannel? GetCurrentChannel(ulong guildId);
        public void EmptyQueue(ulong guildId);
        public Task<bool> ConnectAsync(ulong guildId, SocketVoiceChannel voiceChannel);
        public Task<bool> Disconnect(ulong guildId);
    }
}
