using Discord;
using Discord.WebSocket;

namespace hz_discord_bot.DiscordService.SlashCommand
{
    public class TestCommand : ISlashCommand
    {
        private const string FIRST_TEST_COMMAND = "first-test-command";

        private readonly ILogger _logger;

        public TestCommand(ILogger<TestCommand> logger)
        {
            _logger = logger;
        }

        public async Task AddCommand(SocketGuild guild)
        {
            var commandBuilder = new SlashCommandBuilder();
            commandBuilder.WithName(GetName());
            commandBuilder.WithDescription("This is my first test slash command");

            await guild.CreateApplicationCommandAsync(commandBuilder.Build());
            _logger.LogInformation("Creating Test Command with name {CommandName} for Guild: {GuildId}", FIRST_TEST_COMMAND, guild.Id);
        }

        public string GetName()
        {
            return FIRST_TEST_COMMAND;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.RespondAsync($"Hello World");
        }
    }
}
