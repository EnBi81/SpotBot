using Discord;
using Discord.WebSocket;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

using Victoria.EventArgs;
using Victoria;

using SpotBot.Modules;
using SpotBot.Configs;


namespace SpotBot.Services
{
    public static class LavaNodeService
    {
        public static LavaNode FirstNode { get {
                foreach (var node in Nodes)
                {
                    if (node.IsConnected) return node;
                }
                return null;
            } }
        private static List<LavaNodeSettings> Settings => LavaNodeSettings.GetNodeSettings();
        private static List<LavaNode> Nodes { get; } = new List<LavaNode>();
        internal static Dictionary<ulong, (IVoiceChannel VoiceChannel, ITextChannel TextChannel, LavaTrack Track, TimeSpan Position, bool IsPlaying)> Players { get; } = new Dictionary<ulong, (IVoiceChannel VoiceChannel, ITextChannel TextChannel, LavaTrack Track, TimeSpan Position, bool IsPlaying)>();
        private static readonly object obj = new object();


        public static void SetNodes(DiscordSocketClient client)
        {
            Console.WriteLine("Setting nodes");
            lock (obj)
            {
                try
                {
                    if (Nodes.Any())
                    {
                        foreach (var node in Nodes)
                        {
                            node.OnWebSocketClosed -= Program.Client.WebSocketClosed;
                            node.OnTrackEnded -= AudioModule.OnTrackEnded;
                            node.OnTrackStuck -= AudioModule.OnTrackStuck;
                            node.OnTrackException -= AudioModule.OnTrackException;
                            node.OnTrackStarted -= AudioModule.OnTrackStarted;
                            node.OnPlayerUpdated -= PlayerUpdated;
                            new Thread(async () => { try { await node.DisconnectAsync(); } catch { } }).Start();
                        }
                    }
                }
                catch
                { }

                Nodes.Clear();

                foreach (var setting in Settings)
                {
                    new Thread(async () =>
                    {
                        if (!setting.Connect) return;

                        var config = new LavaConfig()
                        {
                            Port = setting.Port,
                            SelfDeaf = true,
                            ReconnectAttempts = 20,
                            ResumeTimeout = new TimeSpan(0, 10, 0),
                            LogSeverity = SpotBotClient.Config.LogLevelDebug ? LogSeverity.Debug : LogSeverity.Info,
                            ReconnectDelay = new TimeSpan(0, 0, 3),
                            Authorization = setting.Authorization ?? "youshallnotpass",
                            Hostname = setting.Host,

                        };
                        var node = new LavaNode(client, config);

                        node.OnLog += async msg => await SpotBotClient.LogService.LogAsync(msg);
                        while (client.ConnectionState != ConnectionState.Connected) ;
                        await node.ConnectAsync();

                        node.OnTrackStarted += AudioModule.OnTrackStarted;
                        node.OnTrackEnded += AudioModule.OnTrackEnded;
                        node.OnTrackStuck += AudioModule.OnTrackStuck;
                        node.OnTrackException += AudioModule.OnTrackException;
                        node.OnWebSocketClosed += Program.Client.WebSocketClosed;
                        node.OnPlayerUpdated += PlayerUpdated;

                        Nodes.Add(node);

                    }).Start();
                }

                Program.Client.WatchWebSocketDisconnect = true;
            }
        }

        public static async void ReJoin()
        {
            foreach (var node in Nodes)
            {
                if (!node.IsConnected) await node.ConnectAsync();
            }

            try
            {
                while (!Nodes.Any()) ;
            }
            catch { }
            Console.WriteLine("Players to load: " + Players.Count);
            foreach (var  (GuildId, (VoiceChannel, TextChannel, Track, Pos, IsPlaying)) in Players)
            {
                new Thread(async () =>
                {
                    try
                    {
                        var guild = VoiceChannel.Guild;
                        var vc = VoiceChannel;
                        var txt = TextChannel;


                        var pl = Player.GetPlayer(guild);
                        if (pl.CurrentTrack is null) return;

                        var node = GetNode(guild);


                        var p = await node.JoinAsync(vc, txt);
                        await p.PlayAsync(Track);
                        if(!IsPlaying)
                            await p.PauseAsync();
                        await p.UpdateVolumeAsync(100);
                        await p.SeekAsync(Pos);
                    }
                    catch { }

                }).Start();
            }
            Program.Client.WatchWebSocketDisconnect = true;
        }

        public static async void UnSubscribe()
        {
            try
            {
                if (!Nodes.Any()) return;
                Players.Clear();
                Console.WriteLine("Nodes count: " + Nodes.Count);
                int count = 0;
                foreach (var node in Nodes)
                {
                    try
                    {
                        count += node.Players.Count();
                    }
                    catch { }
                    await node.DisconnectAsync();
                    await node.ConnectAsync();
                }

                Console.WriteLine("Players count: " + count);
            }
            catch { }
        }

        public static LavaNode GetNode(IGuild guild) => GetNode(guild.Id);
        public static LavaNode GetNode(ulong guildId)
        {
            lock (obj) { }

            if (!Nodes.Any()) return null;
            if (Nodes.Count == 1) return Nodes.First();

            LavaNode min = Nodes.First();
            int players = min.Players.Count();
            lock (obj)
            {
                foreach (var node in Nodes)
                {
                    if (node.IsConnected && (from p in node.Players select p.VoiceChannel.GuildId).Contains(guildId))
                        return node;

                    var count = node.Players.Count();
                    if (count < players) min = node;
                }
            }

            return min;
        }

        private static Task PlayerUpdated(PlayerUpdateEventArgs args)
        {
            var player = args.Player;
            var guildid = player.VoiceChannel.GuildId;
            var pos = args.Position;

            if (!Players.ContainsKey(guildid))
                Players.Add(guildid, (player.VoiceChannel, player.TextChannel, player.Track, pos, player.PlayerState == Victoria.Enums.PlayerState.Playing));
            else Players[guildid] = (player.VoiceChannel, player.TextChannel, player.Track, pos, player.PlayerState == Victoria.Enums.PlayerState.Playing);

            return Task.CompletedTask;
        }





        //internal static void SetLavaSaver()
        //{
        //    if (timer.Enabled) return;

        //    timer.Enabled = true;
        //    timer.Elapsed += (sen, e) =>
        //    {
        //        if (!Program.Client.WatchWebSocketDisconnect) return;

        //        var player = (from n in Nodes
        //                      from p in n.Players
        //                      select new { p.VoiceChannel.GuildId, Datas = (p.VoiceChannel, p.TextChannel, p.Track, p.Track.Position) }).ToList();
                
        //        lock (obj) { }

        //        Players.Clear();
        //        Players.AddRange(player);
        //        //Players = player; 
        //    };
        //}
        //private static readonly System.Timers.Timer timer = new System.Timers.Timer(5 * 1000) { Enabled = false, AutoReset = true };
}
}
