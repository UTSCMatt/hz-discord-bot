using DectalkNET;
using System.Text;

namespace hz_discord_bot.DectalkService
{
    public class DectalkService : IDectalkService
    {
        private readonly ILogger _logger;

        private const string GENERIC_HEADER = "[:phoneme arpabet on] [:rate 300]";

        public DectalkService(ILogger<DectalkService> logger)
        {
            _logger = logger;
            Dectalk.Startup();
        }

        public async Task HandleShutdown()
        {
            _logger.LogInformation("Handling shutdown for Dectalk");
            try
            {
                await Task.Run(() =>
                {
                    var standardDectalkOut = new DirectoryInfo(Directory.GetCurrentDirectory());
                    var waves = standardDectalkOut.GetFiles("*.wav");
                    foreach (var wave in waves)
                    {
                        _logger.LogInformation("Deleting {file}", wave.Name);
                        wave.Delete();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception encountered during Dectalk shutdown handle");
            }
            finally
            {
                Dectalk.Shutdown();
            }
        }

        public string WaveOut(string text)
        {
            var fileName = $"{Guid.NewGuid()}.wav";
            CheckStatus();
            var textBuilder = new StringBuilder(GENERIC_HEADER);
            textBuilder.Append(text);
            Dectalk.WaveOut(textBuilder.ToString(), fileName);
            if (Dectalk.status == DectalkNET.invokes.MMRESULT.MMSYSERR_NOERROR)
            {
                _logger.LogInformation("Dectalk generated file {fileName}", fileName);
                return fileName;
            }
            else
            {
                throw new IOException($"Dectalk failed WaveOut with status {Dectalk.status}");
            }
        }

        private void CheckStatus()
        {
            if (Dectalk.status != DectalkNET.invokes.MMRESULT.MMSYSERR_NOERROR)
            {
                _logger.LogWarning("Dectalk status is not no error, status is {status}, restarting", Dectalk.status);
                Dectalk.Startup();
            }
        }
    }
}
