using System.Diagnostics;

namespace SpotBot.Configs
{
    public class LavaLink
    {

        private static ProcessStartInfo Info => new ProcessStartInfo()
        {
            FileName = SpotBotClient.Config.JavaLocation,
            Arguments = "-jar Lavalink.jar",
            RedirectStandardOutput = true,
        };

        public static void RunLava()
        {
            var p = Process.Start(Info);
            if (SpotBotClient.Config.LogLevelDebug)
            {
                p.OutputDataReceived += (sen, e) => System.Console.WriteLine(e.Data);
                p.BeginOutputReadLine();
            }
        }

    }
}
