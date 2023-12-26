namespace hz_discord_bot.DectalkService
{
    public interface IDectalkService
    {
        public string WaveOut(string text);
        public Task HandleShutdown();
    }
}
