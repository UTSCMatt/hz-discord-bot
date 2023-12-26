using Discord.Audio;
using Discord.WebSocket;
using hz_discord_bot.DiscordService.Singleton;
using System.Collections.Concurrent;
using System.Diagnostics;
using System;

namespace hz_discord_bot.DiscordService.SlashCommand
{
    internal class AudioHandler : IAudioHandler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly List<ulong> _guild_ids;
        private readonly Dictionary<ulong, BlockingCollection<string>> _queues;
        private readonly Dictionary<ulong, IAudioClient?> _audioClients;
        private readonly Dictionary<ulong, SocketVoiceChannel?> _audioClientChannels;
        private readonly Dictionary<ulong, AudioOutStream?> _audioStreams;

        public AudioHandler(IConfiguration configuration, ILogger<AudioHandler> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _guild_ids = configuration.GetSection("guildIds").Get<List<ulong>>() ?? new List<ulong>();

            if (!_guild_ids.Any())
            {
                _logger.LogWarning("No Guild Ids found");
            }

            _queues = new Dictionary<ulong, BlockingCollection<string>>();
            _audioClients = new Dictionary<ulong, IAudioClient?>();
            _audioClientChannels = new Dictionary<ulong, SocketVoiceChannel?>();
            _audioStreams = new Dictionary<ulong, AudioOutStream?>();
            foreach (var guildId in _guild_ids)
            {
                _queues.Add(guildId, new BlockingCollection<string>());
                _audioClients.Add(guildId, null);
                _audioClientChannels.Add(guildId, null);
                _audioStreams.Add(guildId, null);
            }
            _ = QueueHandler();
        }
        public bool AddToQueue(ulong guildId, string fileName)
        {
            if (_queues.TryGetValue(guildId, out var queue) && _audioClients.TryGetValue(guildId, out var audioClient))
            {
                if (audioClient == null)
                {
                    _logger.LogWarning("Tried to add audio before connecitng to voice channel");
                    return false;
                }
                queue.Add(fileName);
                return true;
            }
            else
            {
                _logger.LogWarning("Tried to add audio to non configured guild {guildId}", guildId);
            }
            return false;
        }

        public bool IsQueueEmpty(ulong guildId)
        {
            if (_queues.TryGetValue(guildId, out var queue))
            {
                return queue.Count == 0;
            }
            return true;
        }

        public void EmptyQueue(ulong guildId)
        {
            if (_queues.TryGetValue(guildId, out var queue))
            {
                while (queue.TryTake(out _)) { }
            }
        }

        private Process? CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task QueueHandler()
        {
            var tasks = new List<Task>();
            foreach (var guildId in _guild_ids)
            {
                tasks.Add(Task.Run(async () =>
                {
                    if (_queues.TryGetValue(guildId, out var queue))
                    {
                        while (true)
                        {
                            string file = "";
                            try
                            {
                                file = queue.Take();
                                PlayAudio(guildId, file);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Exception encountered during audio handler for guild {guildId}, will reconnect and try again", guildId);
                                var lastChannel = GetCurrentChannel(guildId);
                                try
                                {
                                    await Disconnect(guildId);
                                }
                                catch (Exception subEx)
                                {
                                    _logger.LogError(subEx, "Exception encountered during disconnect portion of retry");
                                }
                                try
                                {
                                    await ConnectAsync(guildId, lastChannel);
                                }
                                catch (Exception subEx)
                                {
                                    _logger.LogError(subEx, "Exception encountered during reconnect portion of retry");
                                }
                                try
                                {
                                    PlayAudio(guildId, file);
                                }
                                catch (Exception subEx)
                                {
                                    _logger.LogError(subEx, "Exception encountered during reattempt of play audio");
                                }
                            }
                        }
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }

        private void PlayAudio(ulong guildId, string file)
        {
            if (_audioClients.TryGetValue(guildId, out var audioClient) && _audioStreams.TryGetValue(guildId, out var audioStream))
            {
                if (audioClient != null && audioStream != null)
                {
                    _ = SendAsync(audioClient, file, audioStream);
                }
            }
        }

        private async Task SendAsync(IAudioClient client, string path, AudioOutStream discord)
        {
            // Create FFmpeg using the previous example
            using var ffmpeg = CreateStream(path);
            if (ffmpeg == null)
            {
                _logger.LogWarning("Creating ffmpeg stream returned null");
            }
            else
            {
                using var output = ffmpeg.StandardOutput.BaseStream;
                try
                {
                    await discord.FlushAsync();
                    await output.CopyToAsync(discord);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception encountered during audio sender");
                }
                finally 
                { 
                    await discord.FlushAsync();
                    _logger.LogInformation("Succesfully discord.FlushAsync()");
                    File.Delete(path);
                }
            }
        }

        public SocketVoiceChannel? GetCurrentChannel(ulong guildId)
        {
            if (_audioClientChannels.TryGetValue(guildId, out var channel))
            {
                return channel;
            }
            return null;
        }

        public async Task<bool> ConnectAsync(ulong guildId, SocketVoiceChannel voiceChannel)
        {
            try
            {
                var audioClient = await voiceChannel.ConnectAsync();
                _audioClients[guildId] = audioClient;
                _audioClientChannels[guildId] = voiceChannel;
                if (_audioStreams[guildId] != null)
                {
                    await _audioStreams[guildId].DisposeAsync();
                }
                _audioStreams[guildId] = audioClient.CreatePCMStream(AudioApplication.Voice, 128 * 1024, 200);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception encountered during audio connect");
            }
            return false;
        }

        public async Task<bool> Disconnect(ulong guildId)
        {
            try
            {
                if (_audioClientChannels.TryGetValue(guildId, out var channel) && channel != null)
                {
                    _audioClientChannels[guildId] = null;
                    _audioClients[guildId] = null;
                    if (_audioStreams[guildId] != null)
                    {
                        await _audioStreams[guildId].DisposeAsync();
                    }
                    _audioStreams[guildId] = null;
                    await channel.DisconnectAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception encountered during audio disconnect");
            }
            return false;
        }
    }
}
