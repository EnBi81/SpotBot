using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using SpotBot.Configs;
using SpotBot.Services;
using SpotBot.Helpers;

using Victoria.EventArgs;

using System;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;

namespace SpotBot
{
    public class SpotBotClient
    {
        public static bool React { get; set; } = true;
        public static Config Config { get; } = Config.GetConfig();
        public static IServiceProvider Services { get; private set; }

        public bool Disconnecting { get => _client.ConnectionState == ConnectionState.Disconnected || _client.ConnectionState == ConnectionState.Disconnecting || _client.ConnectionState == ConnectionState.Connecting; }
        public bool WatchWebSocketDisconnect { get; set; } = true;

        public readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;
        private readonly CommandHandler _handler;
        public static LogService LogService { get; } = new LogService();
        private readonly Config _config;          

        public SpotBotClient()
        {
            if (Config.LogLevelDebug) Console.WriteLine("ConnectionString: " + Config.Connectionstring);

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false,
                LogLevel = Config.LogLevelDebug ? LogSeverity.Debug : LogSeverity.Info,
                
            });

            _cmdService = new CommandService(new CommandServiceConfig
            {
                LogLevel = Config.LogLevelDebug ? LogSeverity.Debug : LogSeverity.Info,
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                
            });

            _config = Config;

            

            Services = SetupServices();

            _handler = new CommandHandler(_client, _cmdService, Services);

        }

        public async Task InitializeAsync()
        {
            await GuildService.LoadService(); 
            
            _client.Ready += () => {  return Task.CompletedTask; };
            _client.Connected += () => { LavaNodeService.SetNodes(_client); LavaNodeService.ReJoin(); return Task.CompletedTask; };
            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            await _client.SetGameAsync("!sbhelp", "http://spotbot.nicepage.io/", ActivityType.Listening);
        }

        public async Task Start()
        {
            _client.Log += async msg => await LogService.LogAsync(msg);
            _client.ReactionAdded += StartReaction;
            _client.ReactionRemoved += StartReaction;
            _client.MessageDeleted += MessageDeleted;
            _client.UserVoiceStateUpdated += VoiceUpdated;
            _client.Disconnected += Disconnected;
            _client.JoinedGuild += JoinedGuild;

            await _handler.InitializeAsync();

            //await DoCsiri();
        }

        #region Csiri
#pragma warning disable IDE0051
        private async Task DoCsiri()
#pragma warning restore IDE0051
        {
            try
            {
                while (_client.ConnectionState != ConnectionState.Connected) ;
                await Task.Delay(3000);

                var guild = _client.GetGuild(708255164079013950);
                var node = LavaNodeService.FirstNode;
                if (node is null) throw new NotImplementedException();

                var txt = guild.GetTextChannel(708255164527542353);
                var vc = guild.GetVoiceChannel(708255164527542354);

                var player = await node.JoinAsync(vc, txt);
                var p = Player.GetPlayer(guild);

                var track = (await node.SearchAsync("https://www.youtube.com/watch?v=U06jlgpMtQs")).Tracks?[0];

                if (track is null) throw new NullReferenceException();

                p.AddTrack(track);
                p.Loop = true;
                await player.PlayAsync(track);
            }
            catch (Exception e) { Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace); }
        }
        #endregion

        private Task JoinedGuild(SocketGuild arg)
        {
            new Thread(async () =>
            {
                var e = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("Hi, thanks for inviting SpotBot.\rTo get more help, type !sbhelp to chat.")
                    .WithDescription("To change prefix, type !sbprefix {newprefix}.\nTo start playing a song, type {prefix}p {query}.");


                await arg.DefaultChannel.SendMessageAsync(embed: e.Build());

            }).Start();

            return Task.CompletedTask;
        }

        internal static List<ulong> GuildIdsForMoving { get; } = new List<ulong>();
        public Task WebSocketClosed(WebSocketClosedEventArgs arg) 
        {
            new Thread(async () =>
            {
                try
                {
                    if (Disconnecting)
                    {
                        WatchWebSocketDisconnect = false;
                    }
                    if (!WatchWebSocketDisconnect) return;

                    var id = arg.GuildId;
                    if (GuildIdsForMoving.Remove(arg.GuildId)) return;
                    if (arg.Code != 1000 && arg.Code != 4014)
                    {
                        var node = LavaNodeService.GetNode(arg.GuildId);
                        var guild = _client.GetGuild(arg.GuildId);

                        var p = Player.GetPlayer(guild);

                        await node.JoinAsync(p.VoiceChannel, p.TextChannel);
                        var player = node.GetPlayer(guild);

                        await player.PlayAsync(p.CurrentTrack);

                        return;
                    }

                    LavaNodeService.Players.Remove(arg.GuildId);
                    await Task.Run(() => Player.RemoveId(id));
                    await Task.Run(() => AudioService.WannaJoin.RemoveAll(obj => obj.Item2 == id));
                    await QueueService.TryRemoveQueue(id);
                    await PlayerService.TryRemovePlayer(id);
                }
                catch { }
            }).Start();
            return Task.CompletedTask;

        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles disconnect event.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task Disconnected(Exception arg)
        {
            Console.WriteLine("Hcucc: " + arg.HResult);
            await Task.Run(() => Console.WriteLine("Message: " + arg.Message));
            //LavaNodeService.UnSubscribe();

            //await Task.Run(() => LavaNodeService.UnSubscribe());
        }


        public static Dictionary<ulong, IVoiceChannel> MovePlayer { get; } = new Dictionary<ulong, IVoiceChannel>();
        /// <summary>
        /// Exits from VC when nobody is in there.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="fromvs"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private Task VoiceUpdated(SocketUser arg1, SocketVoiceState fromvs, SocketVoiceState to)
        {
            new Thread(async () =>
            {
                try
                {
                    if (arg1 is not SocketGuildUser user) return;
                    if (user.Id == _client.CurrentUser.Id && fromvs.VoiceChannel is not null) //Continue the track
                    {

                        if (!MovePlayer.TryGetValue(fromvs.VoiceChannel.Id, out IVoiceChannel voice)) return;
                        MovePlayer.Remove(fromvs.VoiceChannel.Id);

                        var node = LavaNodeService.GetNode(user.Guild);

                        
                        (_, var TextChannel, var Track, var Position, var IsPlaying) = LavaNodeService.Players[user.Guild.Id];
                        var p = await node.JoinAsync(voice, TextChannel);

                        if (Track is null) return;

                        await p.PlayAsync(Track);
                        if (IsPlaying) 
                            await p.PauseAsync();
                        await p.SeekAsync(Position);
                        await p.UpdateVolumeAsync(100);

                        
                        return;
                    }

                    if (user.Guild.Id == 708255164079013950) return;//Csiri szervere, hagyd békén

                    try
                    {
                        if (to.VoiceChannel?.Id == LavaNodeService.Players[user.Guild.Id].VoiceChannel.Id)
                            AutoDisconnect.Disconnect.Remove(to.VoiceChannel);
                    }
                    catch { }


                    if (fromvs.VoiceChannel is not SocketVoiceChannel vc)
                    {
                        return; 
                    }

                    var users = from u in vc.Users where !u.IsBot select u;
                    if (!users.Any() && (from u in vc.Users select u.Id).Contains(_client.CurrentUser.Id))
                    {
                        var node = LavaNodeService.GetNode(user.Guild.Id);

                        if (to.VoiceChannel is IVoiceChannel channel && channel.Id != (channel.Guild.AFKChannelId ?? 0))
                        {
                            var m = await node.MoveChannelSB(fromvs.VoiceChannel, to.VoiceChannel, user);

                            var van = LavaNodeService.Players.ContainsKey(user.Guild.Id);


                            if (van)
                            {
                                var txt = LavaNodeService.Players[user.Guild.Id].TextChannel;
                                await txt.SendMessageAsync("I don't have permission to join to the " + channel.Name);
                            }
                        }
                        else
                        {
                            try
                            {
                                AutoDisconnect.Disconnect.Add(vc, DateTime.Now);
                            }
                            catch { }

                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e.Message + "\n" + e.StackTrace); }
            }).Start();

            return Task.CompletedTask;
        }


        /// <summary>
        /// Handling both added and removed reactions.
        /// </summary>
        /// <param name="messageCache"></param>
        /// <param name="channel"></param>
        /// <param name="reaction"></param>
        /// <returns></returns>
        private Task StartReaction(Cacheable<IUserMessage, ulong> messageCache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!React) return Task.CompletedTask;
            Thread t = new Thread(async () => await Reactions(messageCache, channel, reaction));
            t.Start();
            return Task.CompletedTask;
        }
        private async Task Reactions(Cacheable<IUserMessage, ulong> messageCache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            //await channel.SendMessageAsync($"`{reaction.Emote.Name}`");
            try
            {

                var m = await messageCache.GetOrDownloadAsync();
                if (reaction.UserId == _client.CurrentUser.Id) return;

                var queue = await Task.Run(() => QueueService.GetQueueService(m, createNew: false));
                if (!(queue is null))
                {
                    await queue.Modify(reaction.Emote, m.Author);
                    return;
                }

                if (PlayerService.TryGetPlayer(m, out PlayerService service))
                {
                    await service.Update(reaction.Emote, m.Author);
                    return;
                }

                if (PlaylistService.TryGetService(m.Id, out PlaylistService plService))
                {
                    await plService.Modify(reaction.Emote, m.Author);
                }


            }
            catch (Exception e) { Console.WriteLine(e.Message + "\n" + e.StackTrace); }
            
        }

        private IServiceProvider SetupServices()
            => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_cmdService)
            .AddSingleton(LogService)
            .AddSingleton(new AudioService())
            .AddSingleton(_config)
            .BuildServiceProvider();


    }
}
