using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotBot.Services
{
    public class AutoDisconnect
    {
        public static Dictionary<IVoiceChannel, DateTime> Disconnect { get; } = new Dictionary<IVoiceChannel, DateTime>();
        

        public static void SetTimer()
        {
            _timer.Start();
            _timer.Elapsed += async (sen, e) =>
            {
                try
                {
                    if (Program.Client.Disconnecting || !Program.Client.WatchWebSocketDisconnect) return;

                    var disconnects = from disc in Disconnect
                                      let elapsed = DateTime.Now - disc.Value
                                      where elapsed.TotalMinutes > 2
                                      select disc;

                    foreach (var (vc, dt) in disconnects)
                    {
                        try
                        {
                            var node = LavaNodeService.GetNode(vc.Guild);
                            await node.LeaveAsync(vc);
                        }
                        catch { }
                    }
                }
                catch { }

            };
        }
        private static readonly System.Timers.Timer _timer = new System.Timers.Timer(60 * 1000) { AutoReset = true };
    }
}
