using Discord;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SpotBot.Services
{
    public class LogService
    {
        private static string FileName => "botlog.txt";
        private static StreamWriter Writer => new StreamWriter(FileName, true);
        private readonly SemaphoreSlim _semaphoreSlim;

        public LogService()
        {
            _semaphoreSlim = new SemaphoreSlim(1);
        }

        internal async Task LogAsync(LogMessage arg)
        {
            await _semaphoreSlim.WaitAsync();

            var timeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            const string format = "{0,-10} {1,10}";
            string text = $"[{timeStamp}] {string.Format(format, arg.Source, $": {arg.Message}")}";
            Console.WriteLine(text);

            using var writer = Writer;
            await writer.WriteLineAsync(text);

            _semaphoreSlim.Release();
        }
        internal async Task LogAsync(string message)
        {
            await _semaphoreSlim.WaitAsync();

            var timeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string text = $"[{timeStamp}] {message}";
            Console.WriteLine(text);

            using var writer = Writer;
            await writer.WriteLineAsync(text);

            _semaphoreSlim.Release();
        }

    }
}