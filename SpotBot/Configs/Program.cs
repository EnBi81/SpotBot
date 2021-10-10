using SpotBot.Services;
using System;
using System.Threading.Tasks;

namespace SpotBot
{
    public class Error
    {
        public static event ErrorHandler ErrorHappened;
        public delegate void ErrorHandler(Exception e);
        public static void RaiseException(Exception e)
        {
            ErrorHandler handler = ErrorHappened;
            handler?.Invoke(e);
        }

    }

    public class Program
    {
        public static SpotBotClient Client { get; } = new SpotBotClient();
        static async Task Main()
        {
            try
            {
                //LavaNodeService.SetLavaSaver();
                Spotify.SBUser.SetTimer();
                PlaylistService.SetTimer();
                QueueService.SetTimer();
                AutoDisconnect.SetTimer();

                await Client.InitializeAsync();
                await Client.Start();

                await Task.Delay(-1);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }
    }
}
