using System;
using System.Web;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;

using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using Victoria.Payloads;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using SpotBot.Services;
using SpotBot.Helpers;
using static SpotBot.Helpers.EmbedHelper;

using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Playlists;

using AngleSharp.Dom;

namespace SpotBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.Connect)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireBotPermission(GuildPermission.Speak)]
    [RequireBotPermission(ChannelPermission.AddReactions)]
    public sealed class AudioModule : ModuleBase<SocketCommandContext>
    {
        public const int MaxQueueCount = 600;
        private static YoutubeClient Client { get; } = new YoutubeClient();

        private readonly AudioService _audioService;

        public AudioModule(AudioService service)
        {
            _audioService = service;
        }

        private LavaNode GetNode => LavaNodeService.GetNode(Context.Guild.Id);

        public static async Task PlayTracksAsync(ITextChannel txtchannel, SocketUser user, IGuild guild, string searchQuery, bool fromYoutube = true, bool shuffle = false)
        {
            var LavaNode = LavaNodeService.GetNode(guild);
            if (LavaNode is null)
            {
                await txtchannel.SendMessageAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            Embed em;
            try
            {
                LavaPlayer player = null;
                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    em = CreateEmbed("Please provide search terms.", color: Color.Red);
                    await txtchannel.SendMessageAsync(embed: em);
                    return;
                }
                if (user is not IVoiceState voiceState || voiceState.VoiceChannel is not IVoiceChannel channel)
                {
                    var e = CreateEmbed("You must be connected to a voice channel!", color: Color.Red);
                    await txtchannel.SendMessageAsync(embed: e);
                    return;
                }
                if (channel.Id == (channel.Guild.AFKChannelId ?? 0))
                {
                    await txtchannel.SendMessageAsync("Cannot connect to Afk channel \\:/");
                    return;
                }
                if (voiceState.VoiceChannel is not SocketVoiceChannel chan || !(guild as SocketGuild).GetUser(Program.Client._client.CurrentUser.Id).CanConnect(chan))
                {
                    await txtchannel.SendMessageAsync(EmojiHelper.Exit + " I don't have a permission to connect.");
                    return;
                }
                if (!LavaNode.HasPlayer(guild))
                {
                    player = await LavaNode.JoinAsync(voiceState.VoiceChannel, txtchannel);
                }
                else if ((player = LavaNode.GetPlayer(guild)).VoiceChannel.Id != voiceState.VoiceChannel.Id)
                {
                    SpotBotClient.GuildIdsForMoving.Add(guild.Id);
                    await LavaNode.MoveChannelSB(player.VoiceChannel, voiceState.VoiceChannel, user as SocketGuildUser);
                }

                player ??= LavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync(100);

                var tracks = await LavaNode.SearchSongs(searchQuery, fromYt: fromYoutube);
                if (tracks is null)
                {
                    await txtchannel.SendMessageAsync("`Couldn't find any track.`");
                    return;
                }
                if (!tracks.Any())
                {
                    await txtchannel.SendMessageAsync("`Couldn't find any track!`");
                    return;
                }


                var p = Player.GetPlayer(guild);
                p.CanAddTracks = true;
                p.VoiceChannel = voiceState.VoiceChannel;
                p.TextChannel = txtchannel;
                try
                {
                    if (p.Tracks.Count + tracks.Count > MaxQueueCount)
                    {
                        await txtchannel.SendMessageAsync(EmojiHelper.Exit + $" Can't add more than {MaxQueueCount} tracks to the queue");
                        return;
                    }
                    if (shuffle) tracks.Shuffle();
                    p.AddTracks(tracks);
                }
                catch
                {

                }
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    em = tracks.Count > 1 ? CreateEmbed($"Enqueued {tracks.Count} tracks", color: Color.Green) : CreateEmbed(null, description: $"Queued: [{tracks.First().Title}]({tracks.First().Url})", color: Color.Green);
                    await txtchannel.SendMessageAsync(embed: em);
                    await player.ResumeAsync();
                }
                else
                {
                    var t = p.NextTrack;
                    if (t is null) return;

                    await player.PlayAsync(t);
                    var emb = new EmbedBuilder()
                        .WithTitle("Now Playing: " + t.Title)
                        .WithColor(Color.Green)
                        .WithUrl(t.Url);
                    if (tracks.Count > 1) emb = emb.WithDescription($"Enqueued {tracks.Count} tracks.");

                    em = emb.Build();

                    await txtchannel.SendMessageAsync(embed: em);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
        public static async Task PlayTracksAsync(SocketCommandContext context, string searchQuery) => await PlayTracksAsync(context.Channel as ITextChannel, context.User, context.Guild, searchQuery);

        public async Task PlayTracks(string searchQuery, bool fromYoutube = true)
            => await PlayTracksAsync(Context.Channel as ITextChannel, Context.User, Context.Guild, searchQuery, fromYoutube);

        public static Task OnTrackEnded(TrackEndedEventArgs args)
        {
            var t = new Thread(async () =>
            {
                LavaTrack track = null;
                try
                {
                    await AudioService.NowPlayingMessages[args.Player.VoiceChannel.Guild].DeleteAsync();
                    AudioService.NowPlayingMessages.Remove(args.Player.VoiceChannel.Guild);
                }
                catch { }
                try
                {
                    var r = args.Reason;
                    if (r == TrackEndReason.Replaced || r == TrackEndReason.Cleanup) return;

                    var player = args.Player;
                    var p = Player.GetPlayer(player.VoiceChannel.Guild);

                    track = p.NextTrack;
                    if (track is null)
                    {
                        if(p.CanAddTracks)
                        await player.TextChannel.SendMessageAsync($"`We've arrived to the end of the queue. Add more tracks with {GuildService.GetPrefix(player.TextChannel.GuildId)}p command!`");
                        return;
                    }

                    await args.Player.PlayAsync(track);

                }
                catch
                { }

            });

            t.Start();

            return Task.CompletedTask;
        }
        public static Task OnTrackStuck(TrackStuckEventArgs e)
        {

            var t = new Thread(async () =>
            {
                try
                {
                    var p = e.Player;
                    await p.PlayAsync(e.Track);
                }
                catch { }
            });
            t.Start();

            return Task.CompletedTask;
        }
        public static Task OnTrackException(TrackExceptionEventArgs arg)
        {
            Console.WriteLine(arg.ErrorMessage);
            var t = new Thread(async () =>
            {
                try
                {
                    var player = arg.Player;

                    await player.PlayAsync(player.Track);
                }
                catch { }
            });

            t.Start();
            return Task.CompletedTask;
        }
        public static Task OnTrackStarted(TrackStartEventArgs arg)
        {
            new Thread(async () =>
            {
                var guild = arg.Player.VoiceChannel.Guild;
                try
                {
                    await AudioService.NowPlayingMessages[guild].DeleteAsync();
                }
                catch { }
                try
                {

                    var player = arg.Player;
                    if (!GuildService.TrackStartedMessageCheck(player.VoiceChannel.Guild)) return;

                    var embed = new EmbedBuilder().WithTitle(arg.Track.Title).WithUrl(arg.Track.Url).Build();

                    var mess = await player.TextChannel.SendMessageAsync(embed: embed);
                    AudioService.NowPlayingMessages.Remove(guild);
                    AudioService.NowPlayingMessages.Add(player.VoiceChannel.Guild, mess);
                }
                catch { }
            }).Start();

            return Task.CompletedTask;
        }

        private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

        [Command("Lyrics", RunMode = RunMode.Async)]
        public async Task ShowGeniusLyrics()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
            {
                await ReplyAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
            if (string.IsNullOrWhiteSpace(lyrics) || lyrics == "No lyrics found")
            {
                lyrics = await player.Track.FetchLyricsFromOVHAsync();

                if (string.IsNullOrWhiteSpace(lyrics) || lyrics == "No lyrics found")
                {
                    await ReplyAsync($"No lyrics found for {player.Track.Title}");
                    return;
                }
            }


            var e = new EmbedBuilder().WithTitle(player.Track.Title).WithDescription(lyrics).Build();

            await ReplyAsync(embed: e);
        }

        [Command("Join", RunMode = RunMode.Async)]
        public async Task JoinAsync()
        {

            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }


            Embed e;
            if (Context.User is not IVoiceState voiceState || voiceState.VoiceChannel is not IVoiceChannel channel)
            {
                e = CreateEmbed("You must be connected to a voice channel!", color: Color.Red);
                await ReplyAsync(embed: e);
                return;
            }
            if(channel.Id == (channel.Guild.AFKChannelId ?? 0))
            {
                await ReplyAsync("Cannot connect to Afk channel \\:/");
                return;
            }
            if(voiceState.VoiceChannel is not SocketVoiceChannel chan || !Context.Guild.GetUser(Context.Client.CurrentUser.Id).CanConnect(chan))
            {
                await ReplyAsync(EmojiHelper.Exit + " I don't have a permission to connect.");
                return;
            }

            try
            {
                LavaPlayer player;
                if (!LavaNode.HasPlayer(Context.Guild))
                    await LavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                else if ((player = LavaNode.GetPlayer(Context.Guild)).VoiceChannel.Id != voiceState.VoiceChannel.Id)
                {
                    SpotBotClient.GuildIdsForMoving.Add(Context.Guild.Id);
                    await LavaNode.MoveChannelSB(player.VoiceChannel, voiceState.VoiceChannel, Context.User as SocketGuildUser);
                }

                e = CreateEmbed($"Joined {voiceState.VoiceChannel.Name}!", color: Color.Magenta);
                LavaNodeService.Players.Add(Context.Guild.Id, (voiceState.VoiceChannel, Context.Channel as ITextChannel, null, TimeSpan.Zero, false));
                await ReplyAsync(embed: e);
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }


        }

        [Command("Play", RunMode = RunMode.Async)]
        [Alias("Resume")]
        public async Task Resume()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            if (LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player1))
            {
                if (player1.PlayerState == PlayerState.Paused)
                {
                    await player1.ResumeAsync();
                    await Context.Message.AddReactionAsync(EmojiHelper.Play);
                }
            }
        }

        [Command("P", RunMode = RunMode.Async)]
        public async Task PlayOrPause()
        {

            var node = GetNode;
            
            if(node is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured");
                return;
            }

            if (!node.TryGetPlayer(Context.Guild, out LavaPlayer player)) return;

            if (player.PlayerState == PlayerState.Playing) await player.PauseAsync();
            else if (player.PlayerState == PlayerState.Paused) await player.ResumeAsync();
            else
            {
                await ReplyAsync(EmojiHelper.Exit + " No player has found in the server.");
                return;
            }

            await Context.Message.AddReactionAsync(EmojiHelper.Done);
        }

        [Command("Play", RunMode = RunMode.Async)]
        [Alias("P")]
        public async Task PlayAsync([Remainder] string searchQuery) => await PlayTracks(searchQuery.GetGoodUrl(), true);

        [Command("PlaySoundCloud", RunMode = RunMode.Async)]
        [Alias("PlaySc", "PSC", "PS", "PlayS")]
        public async Task PlaySoundCloud([Remainder] string searchQuery) => await PlayTracks(searchQuery, false);

        [Command("PlayNext")]
        [Alias("PN")]
        public async Task PlayNext([Remainder] string searchQuery)
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            Embed em;
            try
            {
                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    em = CreateEmbed("Please provide search terms.", color: Color.Red);
                    await ReplyAsync(embed: em);
                    return;
                }

                var voiceState = Context.User as IVoiceState;
                if (!LavaNode.HasPlayer(Context.Guild))
                {
                    await PlayAsync(searchQuery);
                    return;
                }

                var player = Player.GetPlayer(Context.Guild);

                var song = await LavaNode.SearchSongs(searchQuery.GetGoodUrl());

                if(song is null || !song.Any())
                {
                    em = new EmbedBuilder().WithTitle($"I couldn't find any songs for `{searchQuery}`").WithColor(Color.Red).Build();
                    await ReplyAsync(embed: em);
                    return;
                }

                player.AddNext(song);

                if (song.Count > 1)
                    em = CreateEmbed($"{song.Count} tracks have been queued.", color: Color.Green);
                else em = new EmbedBuilder().WithDescription($"[{song.First().Title}]({song.First().Url}) will be played next.").WithColor(Color.Green).Build();

                await ReplyAsync(embed: em);

            }
            catch { await ReplyAsync(EmojiHelper.Exit + " An error has occured."); }
        }

        [Command("Pause", RunMode = RunMode.Async)]
        public async Task Pause()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            if (!LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player)) return;

            if (player.PlayerState != PlayerState.Playing)
            {
                await Context.Message.AddReactionAsync(EmojiHelper.Exit);
                return;
            }
            await player.PauseAsync();
            await Context.Message.AddReactionAsync(EmojiHelper.Pause);

        }

        [Command("Skip", RunMode = RunMode.Async)]
        [Alias("Next", "N")]
        public async Task Skip(int count = 1, bool SendFeedback = true)
        {
            try
            {
                var LavaNode = GetNode;
                if (LavaNode is null)
                {
                    await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                    return;
                }

                Embed e;
                var g = Context.Guild;
                if (!LavaNode.TryGetPlayer(g, out LavaPlayer player)) return;

                Player p = Player.GetPlayer(Context.Guild);

                if (count < 1)
                {
                    e = CreateEmbed("Wrong number \\:(", color: Color.Red);
                    if (SendFeedback)
                        await ReplyAsync(embed: e);
                    return;
                }
                if (count > (p.Tracks.Count - (p.Position + 1)))
                {
                    p.JumpToEnd();
                    await player.SeekAsync(player.Track.Duration - new TimeSpan(0, 0, 2));
                    e = CreateEmbed("We have arrived to the end of the queue.");
                    if (SendFeedback)
                        await ReplyAsync(embed: e);
                    return;
                }
                LavaTrack t = null;

                t = p.Skip(count);

                if (t is null) return;
                await player.PlayAsync(t);

                
                if (count == 1) await Context.Message.AddReactionAsync(EmojiHelper.Next);
                else
                {
                    string text = $"Skipped {count} tracks";
                    e = CreateEmbed(text, color: Color.DarkMagenta);
                    if (SendFeedback)
                        await ReplyAsync(embed: e);
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message + "\n" + e.StackTrace); }
        }

        [Command("Stop", RunMode = RunMode.Async)]
        public async Task Stop()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }
            if (!LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player)) return;

            if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
            {
                await Context.Message.AddReactionAsync(EmojiHelper.Exit);
                return;
            }

            await Clear();
            await Context.Message.AddReactionAsync(EmojiHelper.Done);

        }

        [Command("Disconnect", RunMode = RunMode.Async)]
        [Alias("Dc", "Leave", "GoodBye")]
        public async Task Disconnect()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }
            if (!LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
                return;

            await LavaNode.LeaveAsync(player.VoiceChannel);
            await ReplyAsync(":wave: Bye Bye");
        }

        [Command("Shuffle", RunMode = RunMode.Async)]
        [Alias("Mix")]
        public async Task Shuffle()
        {
            var LavaNode = GetNode;

            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }
            Embed e;
            if (!LavaNode.HasPlayer(Context.Guild))
            {
                e = CreateEmbed("First add some tracks.");
                await ReplyAsync(embed: e);
                return;
            }

            var p = Player.GetPlayer(Context.Guild);
            p.Shuffle();
            await Context.Message.AddReactionAsync(EmojiHelper.Shuffle);
        }

        [Command("NowPlaying", RunMode = RunMode.Async)]
        [Alias("Np")]
        public async Task NowPlaying()
        {
            try
            {
                var LavaNode = GetNode;
                if (LavaNode is null)
                {
                    await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                    return;
                }

                Embed e;
                if (!LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
                {
                    e = CreateEmbed("There is no player in this server.", color: Color.Red);
                    await ReplyAsync(embed: e);
                    return;
                }

                if (player.Track is null)
                {
                    e = CreateEmbed("No track here. Maybe add some \\:)", color: Color.Purple);
                    await ReplyAsync(embed: e);
                    return;
                }

                string position;
                var t = player.Track;

                if (t.Duration.TotalHours >= 1)
                {
                    position = string.Format(@"{0:hh\:mm\:ss}/{1:hh\:mm\:ss}", t.Position, t.Duration);
                }
                else
                    position = string.Format(@"{0:mm\:ss}/{1:mm\:ss}", t.Position, t.Duration);

                e = new EmbedBuilder().WithTitle($"Now Playing: {t.Title}").WithUrl(t.Url).WithColor(Color.Green).WithDescription($"Position: `{position}`").Build();
                await ReplyAsync(embed: e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        [Command("Queue", RunMode = RunMode.Async)]
        [Alias("Q")]
        public async Task GetQueue()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            if (!LavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("`No player found in this server.`");
                return;
            }
            var p = Player.GetPlayer(Context.Guild);
            var queue = p.Tracks.Skip(p.Position).ToList();

            if (queue.Count == 0)
            {
                await ReplyAsync("`The Queue is empty.`");
                return;
            }

            await QueueService.GetQueueService(Context.Message, true).SendFirstText(Context.Channel);

        }

        [Command("Replay", RunMode = RunMode.Async)]
        public async Task Replay()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            Embed e;
            if (!LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                e = CreateEmbed("I'm not connected to a voice channel.", color: Color.Red);
                await ReplyAsync(embed: e);
                return;
            }

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                await player.SeekAsync(TimeSpan.Zero);
                await player.ResumeAsync();
                await Context.Message.AddReactionAsync(EmojiHelper.Previous);
            }
            else
            {
                e = CreateEmbed("First start Playing a Track.");
                await ReplyAsync(embed: e);
                return;
            }
        }

        [Command("Seek", RunMode = RunMode.Async)]
        public async Task SeekAsync(string seek)
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            Embed e;
            if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                e = CreateEmbed("I'm not connected to a voice channel.", color: Color.Red);
                await ReplyAsync(embed: e);
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                e = CreateEmbed("Woaaah there, I can't seek when nothing is playing.");
                await ReplyAsync(embed: e);
                return;
            }


            TimeSpan timeSpan;
            int hour = 0, min = 0, sec;
            if (int.TryParse(seek, out int seconds)) sec = seconds;
            else if (seek.Contains(":"))
            {
                string splitter = ":";
                string[] split = seek.Split(splitter);
                if (split.Length == 2)
                {
                    bool jo = int.TryParse(split[0], out min) & int.TryParse(split[1], out sec);
                    if (!jo) return;
                }
                else if (split.Length == 3)
                {
                    bool jo = int.TryParse(split[0], out hour) & int.TryParse(split[1], out min) & int.TryParse(split[2], out sec);
                    if (!jo) return;
                }
                else return;

            }
            else if (seek.Contains(" "))
            {
                string splitter = " ";
                string[] split = seek.Split(splitter);
                if (split.Length == 2)
                {
                    bool jo = int.TryParse(split[0], out min) & int.TryParse(split[1], out sec);
                    if (!jo) return;
                }
                else if (split.Length == 3)
                {
                    bool jo = int.TryParse(split[0], out hour) & int.TryParse(split[1], out min) & int.TryParse(split[2], out sec);
                    if (!jo) return;
                }
                else return;
            }
            else return;

            try
            {
                sec += hour * 3600 + min * 60;

                if (sec < 0) throw new Exception();

                timeSpan = TimeSpan.FromSeconds(sec);
                if (!(timeSpan > player.Track.Duration))
                {
                    await player.SeekAsync(timeSpan);
                    await Context.Message.AddReactionAsync(EmojiHelper.Done);
                }
                else throw new Exception();
            }
            catch (Exception exception)
            {
                e = CreateEmbed("That's kinda out of range.");
                await ReplyAsync(embed: e);
            }
        }

        [Command("Volume", RunMode = RunMode.Async)]
        [Alias("Vlm", "Vol")]
        public async Task VolumeAsync(ushort? volume = null)
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            Embed e;
            if (!LavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                e = CreateEmbed("I'm not connected to a voice channel.", color: Color.Red);
                await ReplyAsync(embed: e);
                return;
            }

            try
            {
                if (volume is null)
                {
                    e = CreateEmbed("Current volume: " + player.Volume);
                    await ReplyAsync(embed: e);

                    return;
                }
                if (volume < 0 || volume > 300)
                {
                    await ReplyAsync("Volume must be between 0 and 300.");
                    return;
                }
                var added = volume.Value - player.Volume;

                if (added != 0)
                   await player.UpdateVolumeAsync(volume.Value);

                Emoji emoji;

                if (added == 0) emoji = EmojiHelper.Done;
                else if (volume.Value == 0) emoji = EmojiHelper.Mute;
                else if (added > 0) emoji = EmojiHelper.VolumeUp;
                else emoji = EmojiHelper.VolumeDown;

                await Context.Message.AddReactionAsync(emoji);

            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Loop", RunMode = RunMode.Async)]
        public async Task Loop()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            Embed e;

            if (!LavaNode.HasPlayer(Context.Guild))
            {
                e = CreateEmbed("No player found.");
                await ReplyAsync(embed: e);

                return;
            }

            var player = Player.GetPlayer(Context.Guild);



            if (player.Loop is null)
            {
                e = CreateEmbed("Now looping the queue.");
                await ReplyAsync(embed: e);

                player.Loop = false;
            }
            else if (player.Loop.Value == false)
            {
                e = CreateEmbed("Now looping the current track.");
                await ReplyAsync(embed: e);

                player.Loop = true;
            }
            else
            {
                e = CreateEmbed("Looping off.");
                await ReplyAsync(embed: e);

                player.Loop = null;
            }


        }

        [Command("Previous", RunMode = RunMode.Async)]
        public async Task Previous(int count = 1)
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            Embed e;

            if (count <= 0)
            {
                e = CreateEmbed("Wrong num \\:(");

                await ReplyAsync(embed: e);
                return;
            }

            var p = Player.GetPlayer(Context.Guild);
            if (!p.Tracks.Any())
            {
                e = CreateEmbed("There are no tracks in the queue");
                await ReplyAsync(embed: e);
                return;
            }

            if (!LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player)) return;

            LavaTrack prev = null;
            for (int i = 0; i < count; i++)
                prev = p.PreviousTrack;

            if (prev is null) return;

            await player.PlayAsync(prev);
            await Context.Message.AddReactionAsync(EmojiHelper.Previous);

        }

        [Command("Player", RunMode = RunMode.Async)]
        public async Task PlayerEmbed()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            if (!LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync("`There isn't any player on this server.`");
                return;
            }
            await PlayerService.GetNewPlayer(Context.Guild).SendFirstMessage(player, Context.Channel);
        }

        [Command("Search", RunMode = RunMode.Async)]
        [Alias("S")]
        public async Task Search([Remainder] string search)
        {
            static Embed GetEmbed(Video track, Playlist pl = null)
            {
                var eng = track.Engagement;

                var e = new EmbedBuilder()
                            .WithAuthor(track.Author)
                            .WithColor(Color.Red)
                            .WithTitle(track.Title)
                            .WithUrl(track.Url)
                            .WithDescription("Duration: " + track.Duration.ToString() + (pl is null ? "" : $"\nFrom playlist: [{pl.Title}]({pl.Url})"))
                            .WithFields(
                            new EmbedFieldBuilder().WithName("Views").WithValue(eng.ViewCount).WithIsInline(true),
                            new EmbedFieldBuilder().WithName("Likes / Dislikes").WithValue(eng.LikeCount + " / " + eng.DislikeCount).WithIsInline(true),
                            new EmbedFieldBuilder().WithName("Upload date").WithValue(track.UploadDate).WithIsInline(true))
                            .Build();

                return e;
            }
            static Embed GetPlEmbed(Playlist pl, int trackCount)
            {
                var eng = pl.Engagement;

                var e = new EmbedBuilder()
                    .WithAuthor(pl.Author)
                    .WithColor(Color.Red)
                    .WithTitle(pl.Title)
                    .WithUrl(pl.Url)
                    .WithDescription(trackCount + " tracks.")
                    .WithFields(
                    new EmbedFieldBuilder().WithName("Views").WithValue(eng.ViewCount).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Likes / Dislikes").WithValue(eng.LikeCount + " / " + eng.DislikeCount).WithIsInline(true))
                    .Build();

                return e;
            }


            YoutubeClient client = Client;
           
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            Embed e;
            if (search.IsUrl())
            {
                Console.WriteLine("Regex");
                try
                {
                    var param = HttpUtility.ParseQueryString(search[(search.IndexOf("?") + 1)..]);
                    var keys = param.AllKeys;

                    if (keys.Contains("v")) //Ez egy video
                    {
                        if (keys.Contains("list")) // Listabol valo zene
                        {
                            var track = await client.Videos.GetAsync(param["v"]);
                            var pl = await client.Playlists.GetAsync("list");
                            e = GetEmbed(track, pl);

                        }
                        else
                        {
                            var track = await client.Videos.GetAsync(param["v"]);

                            e = GetEmbed(track);
                        }
                    }
                    else if (keys.Contains("list"))//Ez egy lista
                    {
                        var pl = await client.Playlists.GetAsync(param["list"]);

                        e = GetPlEmbed(pl, (await client.Playlists.GetVideosAsync(pl.Id)).Count);

                    }
                    else if (search.Contains("youtu.be"))
                    {
                        var id = search[search.IndexOf(".be/" + 5)..];
                        var video = await client.Videos.GetAsync(id);

                        e = GetEmbed(video);

                    }
                    else if (search.Contains("www.youtube.com/channel/"))
                    {
                        int indexofchannel = search.IndexOf("channel/");
                        var id = search[(indexofchannel + "channel/".Length)..];
                        id = id.Substring(0, id.IndexOf("/"));

                        var user = await client.Channels.GetAsync(id);

                        e = new EmbedBuilder()
                            .WithAuthor(user.Title, user.LogoUrl, user.Url)
                            .WithColor(Color.Orange)
                            .Build();
                    }
                    else
                    {
                        var cucc = await LavaNode.SearchAsync(search);
                        if (cucc.Tracks?.Count == 0) e = CreateEmbed("Not tracks found.", color: Color.Magenta);
                        else
                        {
                            var track = cucc.Tracks[0];
                            e = new EmbedBuilder()
                                .WithTitle(track.Title)
                                .WithUrl(track.Url)
                                .WithDescription("Duration: " + track.Duration.ToString())
                                .WithAuthor(track.Author)
                                .Build();

                        }
                    }

                    await ReplyAsync(embed: e);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
            else
            {
                var resp = await LavaNode.SearchYouTubeAsync(search);
                if (!resp.Tracks.Any())
                {
                    e = CreateEmbed("No tracks found.", color: Color.Magenta);
                    await ReplyAsync(embed: e);
                    return;
                }
                if (string.IsNullOrWhiteSpace(resp.Playlist.Name))
                    await QueueService.CreateQueueSearch(Context.Guild, resp.Tracks).SendFirstText(Context.Channel);
            }
        }

        [Command("Clear", RunMode = RunMode.Async)]
        public async Task Clear()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }


            var p = Player.GetPlayer(Context.Guild);

            if (!p.Tracks.Any())
            {
                var e = CreateEmbed("There are no tracks in the queue");

                await ReplyAsync(embed: e);
                return;
            }

            p.ClearPlaylist();
            if (LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await player.SeekAsync(player.Track.Duration.Subtract(TimeSpan.FromSeconds(1)));
            }


            await Context.Message.AddReactionAsync(EmojiHelper.Done);
        }

        [Command("Band")]
        [RequireOwner]
        public async Task Equalizer(int band, double gain = 0)
        {
            if (band < 0 || band > 14) return;
            if (gain < -0.25 || gain > 1) return;

            var node = GetNode;
            if (!node.TryGetPlayer(Context.Guild, out LavaPlayer player)) return;

            await player.EqualizerAsync(new EqualizerBand(band, gain));

        }

        [Command("Forward", RunMode = RunMode.Async)]
        public async Task Forward(int num = 10)
        {
            if(num == 0)
            {
                await ReplyAsync(EmojiHelper.Exit + " parameter cannot be zero.");
            }

            var node = GetNode;
            if(node is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            if(!node.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync(EmojiHelper.Exit + " No player has found on this server.");
                return;
            }

            var track = player.Track;

            var pos = track.Position;
            TimeSpan toPos;

            if(num < 0)
            {
                var rewind = num * -1;
                if (pos.TotalSeconds <= rewind) toPos = TimeSpan.Zero;
                else toPos = TimeSpan.FromSeconds((int)pos.TotalSeconds - rewind); 
            }
            else
            {
                var dur = track.Duration;
                if (dur.TotalSeconds < (pos.TotalSeconds + num)) toPos = dur - new TimeSpan(0, 0, 1);
                else toPos = pos.Add(TimeSpan.FromSeconds(num));
            }

            try
            {
                await player.SeekAsync(toPos);
                await Context.Message.AddReactionAsync(num < 0 ? EmojiHelper.Previous : EmojiHelper.Next);
            }
            catch { await Context.Message.AddReactionAsync(EmojiHelper.Exit); }

        }

        [Command("Rewind", RunMode = RunMode.Async)]
        public async Task Rewind(int num = 10) => await Forward(num * -1);

        [Command("Remove", RunMode = RunMode.Async)]
        [Alias("Delete")]
        public async Task Remove(params int[] counts)
        {
            if (counts.Length == 0) return;

            var player = Player.GetPlayer(Context.Guild);

            if (player.Tracks.Count == 0)
            {
                await ReplyAsync("`No tracks in the queue.`");
                return;
            }

            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] -= 1;
            }


            if (counts.Length > 1)
            {
                int co = 0;
                foreach (var count in (from c in counts orderby c descending select c).Distinct())
                {
                    try
                    {
                        if (count < 0 && count >= player.Tracks.Count) continue;
                        if (count == player.Position) await Skip(0, false);
                        player.RemoveTrack(count);
                        co++;
                    }
                    catch { }
                }
                await ReplyAsync($"Removed {co} tracks from the queue.");
            }
            else
            {
                var count = counts[0];
                if (count == player.Position) await Skip(1, false);
                try
                {
                    var name = player.RemoveTrack(count);
                    await ReplyAsync($"Removed `{name}` from the queue.");
                }
                catch (ArgumentOutOfRangeException)
                {
                    await ReplyAsync($"`Wrong number: {count + 1}`");
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }





        }

        [Command("Move")]
        public async Task Move(int which, int where)
        {
            var player = Player.GetPlayer(Context.Guild);

            if (player.Tracks.Count == 0)
            {
                await ReplyAsync("`No tracks in the queue to move.`");
                return;
            }

            if (which == player.Position)
            {
                await ReplyAsync(EmojiHelper.Exit + " Can't move the current track!");
            }
            which--;
            where--;

            try
            {
                await Task.Run(() => player.MoveTrack(which, where));
                await Context.Message.AddReactionAsync(EmojiHelper.Done);
            }
            catch (ArgumentNullException)
            {
                await ReplyAsync(EmojiHelper.Exit + " The 2 parameters must be different!");
            }
            catch (ArgumentOutOfRangeException)
            {
                await ReplyAsync(EmojiHelper.Exit + " One of the parameter was out of range.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        [Command("Try")]
        [RequireOwner]
        public async Task Get()
        {
            var LavaNode = GetNode;
            if (LavaNode is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                return;
            }

            foreach (var a in Context.Message.Attachments)
            {
                var res = await LavaNode.SearchAsync(a.Url);
                var t = res.Tracks?[0];

                if(t is null)
                {
                    await ReplyAsync("Couldn't find any track.");
                    continue;
                }

                await ReplyAsync("Found: " + t.Title + " " + t.Duration);
            }
        }

        [Command("RickRoll")]
        public async Task RickRoll(SocketGuildUser user = null)
        {
            try
            {
                if (user is null) user = Context.User as SocketGuildUser;

                var vc = user.VoiceChannel;
                if (vc is null) return;

                var node = GetNode;
                LavaPlayer player;

                if (node.HasPlayer(Context.Guild))
                {
                    player = node.GetPlayer(Context.Guild);
                }
                else player = await node.JoinAsync(vc, Context.Channel as ITextChannel);

                var rick = new LavaTrack("QAAAmwIALVJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKFZpZGVvKQAUT2ZmaWNpYWwgUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQAHeW91dHViZQAAAAAAAAAA",
                    "dQw4w9WgXcQ", "Rick Astley - Never Gonna Give You Up (Video)",
                    "Official Rick Astley", "https://www.youtube.com/watch?v=dQw4w9WgXcQ%22%7D", TimeSpan.Zero, 213000, true, false);

                await player.PlayAsync(rick);


                var pl = Player.GetPlayer(Context.Guild);
                var t = pl.PreviousTrack;

            }
            catch { }

        }

        [Command("Lets", ignoreExtraArgs: true)]
        public async Task Lets()
        {
            var user = Context.User as SocketGuildUser;
            var vc = user.VoiceChannel;
            if (vc is null) return;

            var node = GetNode;

            var g = Context.Guild;

            LavaPlayer player = node.HasPlayer(g) ? node.GetPlayer(g) : await node.JoinAsync(vc, Context.Channel as ITextChannel);

            var res = await node.SearchAsync("https://youtu.be/nMJYfDRWVJo");
            var t = res.Tracks?[0];

            if(player.Track is not null)
            {
                var p = Player.GetPlayer(g);
                p.Tracks[p.Position].Position.Add(player.Track.Position);
                p.JumpBack();

            }
            await player.PlayAsync(t);

        }

        [Command("ReJoin")]
        public async Task ReJoin()
        {
            try
            {
                var node = GetNode;
                if (node is null)
                {
                    await ReplyAsync(EmojiHelper.Exit + " An error occured.");
                    return;
                }

                if (!node.TryGetPlayer(Context.Guild, out LavaPlayer player))
                {
                    await ReplyAsync("No player has found in this server.");
                    return;
                }

                var volume = player.Volume;
                var (VoiceChannel, TextChannel, Track, Position, IsPlaying) = LavaNodeService.Players[Context.Guild.Id];


                SpotBotClient.GuildIdsForMoving.Add(Context.Guild.Id);
                await node.LeaveAsync(VoiceChannel);
                player = await node.JoinAsync(VoiceChannel, TextChannel);
                await player.PlayAsync(Track);
                await player.SeekAsync(Position);
                await player.UpdateVolumeAsync((ushort)volume);
                await Context.Message.AddReactionAsync(EmojiHelper.Done);
            }
            catch { await Context.Message.AddReactionAsync(EmojiHelper.Exit); }
        }

        [Command("JumpTo")]
        public async Task JumpTo(int pos)
        {
            var p = Player.GetPlayer(Context.Guild);

            try
            {
                var t = p.JumpTo(pos - 1);
                if(!GetNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
                {
                    await ReplyAsync(EmojiHelper.Exit + " No player has found in this server.");
                    return;
                }

                await player.PlayAsync(t);
                await Context.Message.AddReactionAsync(EmojiHelper.Done);
            
            }
            catch (ArgumentOutOfRangeException)
            {
                await ReplyAsync(EmojiHelper.Exit + " Position out of range.");
            }
            catch(ArgumentNullException)
            {
                await ReplyAsync(EmojiHelper.Exit + "Cannot jump to same position!");
            }
            catch
            {
                await ReplyAsync(EmojiHelper.Exit + " An error occured.");
            }
        }

    }
}