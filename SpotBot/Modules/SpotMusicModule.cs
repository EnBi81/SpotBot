using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;

using AGoodSpotifyAPI.Classes;
using AGoodSpotifyAPI.InterFaces;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Victoria;
using Victoria.Enums;

using SpotBot.Services;
using SpotBot.Helpers;
using SpotBot.Spotify;
using static SpotBot.Helpers.EmbedHelper;

using YoutubeAPIThing;

namespace SpotBot.Modules
{

    [RequireContext(ContextType.Guild)]
    public class SpotMusicModule : ModuleBase<SocketCommandContext>
    {
        public SpotMusicModule() { }


        public static async Task PlayTracks(IEnumerable<ITrack> tracks, SocketUser user, IGuild guild, IMessageChannel channel, string message = null)
        {
            try { 
                if (tracks is null || user is null || guild is null || channel is null) throw new ArgumentNullException(nameof(tracks));

                if(!tracks.Any())
                {
                    await channel.SendMessageAsync(EmojiHelper.Exit + " `Playlists is empty.`");
                    return;
                }

                var node = LavaNodeService.GetNode(guild.Id);
                if(node is null)
                {
                    await channel.SendMessageAsync(EmojiHelper.Exit + " An error occured.");
                    return;
                }


                LavaPlayer player = null;
                var sbuser = await SBUser.GetUser(user.Id);
                bool join = true;
                if (user is not IVoiceState voiceState || voiceState.VoiceChannel is not IVoiceChannel vc)
                {
                    var e = CreateEmbed("You must be connected to a voice channel!", color: Color.Red);
                    await channel.SendMessageAsync(embed: e);
                    return;
                }
                if (channel.Id == (vc.Guild.AFKChannelId ?? 0))
                {
                    await channel.SendMessageAsync("Cannot connect to Afk channel \\:/");
                    return;
                }
                if (voiceState.VoiceChannel is not SocketVoiceChannel chan || !(guild as SocketGuild).GetUser(Program.Client._client.CurrentUser.Id).CanConnect(chan))
                {
                    await channel.SendMessageAsync(EmojiHelper.Exit + " I don't have a permission to connect.");
                    return;
                }
                if (!node.HasPlayer(guild))
                {
                    if (join)
                    {
                        player = await node.JoinAsync(voiceState.VoiceChannel, channel as ITextChannel);
                    }
                }
                else if ((player = node.GetPlayer(guild)).VoiceChannel.Id != voiceState.VoiceChannel.Id)
                {
                    SpotBotClient.GuildIdsForMoving.Add(guild.Id);
                    await node.MoveChannelSB(player.VoiceChannel, voiceState.VoiceChannel, user as SocketGuildUser);
                }

                if (join)
                {
                    player ??= node.GetPlayer(guild);
                    await player.UpdateVolumeAsync(100);
                }
                Player p = Player.GetPlayer(guild);
                p.CanAddTracks = true;
                p.TextChannel = channel as ITextChannel;
                p.VoiceChannel = voiceState.VoiceChannel;
                bool first = true;
                var stopIf = new[] { PlayerState.Disconnected, PlayerState.Stopped, PlayerState.Connected};

                int nullCount = 0; 

                foreach (var t in tracks)
                {
                    try
                    {
                        var query = t.ArtistNames.First() + " " + t.Name;

                        LavaTrack track = null;


                        var songData = await SongData.GetSongData(t.Id);
                        if (songData is null)
                        {
                            var foundTracks = (await node.SearchYouTubeAsync(query)).Tracks;
                            
                            if(!foundTracks.Any())
                            {
                                int countTracks = 1;
                                do
                                {
                                    if(countTracks != 2)
                                    foundTracks = (await node.SearchYouTubeAsync(t.Name)).Tracks;

                                    if (countTracks == 2 && t.Name.Length > 17)
                                        foundTracks = (await node.SearchYouTubeAsync(t.Name.Substring(0, t.Name.IndexOf(" ", 13)))).Tracks;

                                    if (foundTracks.Any()) break;

                                } while (countTracks++ > 3);

                                if(!foundTracks.Any())
                                {
                                    Console.WriteLine("Hasn't found track: " + t.Name);
                                    continue;
                                }

                            }

                            track = foundTracks[0];

                            await SongData.AddSongData(new SongData { SpotId = t.Id, YTUrl = track.Url, Track = track }, false);
                        }
                        else track = songData.Track;

                        if (track is null) continue;

                        if (!p.CanAddTracks) return;
                        if (!join)
                        {
                            p.AddTrack(track);
                            continue;
                        }
                        if (p.Tracks.Count > AudioModule.MaxQueueCount)
                        {
                            await channel.SendMessageAsync(EmojiHelper.Exit + $" Can't add more than {AudioModule.MaxQueueCount} tracks to the queue.");
                            return;
                        }
                        if (first)
                        {
                            if (!(player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused))
                            {
                                p.AddTrack(track);
                                await player.PlayAsync(p.NextTrack);
                                var title = message ?? "Now Playing: " + player.Track.Title;

                                var e = CreateEmbed(title, description: $"Loading {tracks.Count()} track{(tracks.Count() > 1 ? "s" : "")}", color: Color.Purple);

                                await channel.SendMessageAsync(embed: e);
                            }
                            else
                            {
                                await channel.SendMessageAsync(embed: CreateEmbed($"Loading {tracks.Count()} track{(tracks.Count() > 1 ? "s" : "")}", color: Color.Green));
                                p.AddTrack(track);
                            }
                            first = false;
                        }
                        else
                        {
                            if (stopIf.Contains(player.PlayerState)) return;
                            p.AddTrack(track);
                        }
                    }
                    catch(ArgumentNullException e)
                    {
                        if (nullCount++ > 5) return;
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                    catch (Exception e){
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
        public static async Task PlayTracks(IEnumerable<ITrack> tracks, SocketCommandContext context, string message = null)
            => await PlayTracks(tracks, context.User, context.Guild, context.Channel, message);

        private static async Task<SBUser> GetUser(ulong id) => await SBUser.GetUser(id);

        [Command("PlayLiked", RunMode = RunMode.Async)]
        [Alias("PL")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.Speak)]
        [RequireBotPermission(GuildPermission.Connect)]
        public async Task PlayLiked(params string[] parameters) //Orderby
        {

            bool shuffle = parameters.Contains("-s");
            bool first = true;
            int count = -1;
            bool? asc = null;
            bool? order = null;
            bool? beExplicit = null;

            try
            {
                var nums = parameters.Where(p => { var s = p.StartsWith("-c") || p.StartsWith("-f"); return s && p.Length > 2 && int.TryParse(p[2..], out int _); }).Select(str => Convert.ToInt32(str[2..]));
                if (!nums.Any())
                {
                    nums = parameters.Where(p => { var s = p.StartsWith("-l"); return s && p.Length > 2 && int.TryParse(p[2..], out int _); }).Select(str => Convert.ToInt32(str[2..]));
                    if (nums.Any())
                    {
                        count = nums.First();
                        first = false;
                    }

                }
                else count = nums.First();

                if (count < -1 || count == 0)
                {
                    await ReplyAsync("Incorrect parameter value: count.");
                    return;
                }

                //Orderby author, title, saved at
                var orderbyA = parameters.Where(p => p.StartsWith("-o") && p.Length > 2).Select(p => p[2..]);
                var orderbyD = parameters.Where(p => p.StartsWith("-od") && p.Length > 2).Select(p => p[2..]);


                if (orderbyA.Any())
                {
                    asc = true;
                    order = orderbyA.First() switch { "artist" => null, "title" => false, "savedat" => true, _ => asc = null };
                }
                else if (orderbyD.Any())
                {
                    asc = false;
                    order = orderbyD.First() switch { "artist" => null, "title" => false, "savedat" => true, _ => asc = null };
                }

                //filter: Explicit

                if (parameters.Contains("-e")) beExplicit = true;
                else if (parameters.Contains("-ne")) beExplicit = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }

            try
            {
                SBUser user;
                try
                {
                    user = await GetUser(Context.User.Id);
                    if (user is null) throw new NullReferenceException();
                }
                catch (NullReferenceException)
                {
                    await ReplyAsync("You are not logged in.");
                    return;
                }

                string token;

                try
                {
                    token = await user.GetAccessToken();
                }
                catch
                {
                    await ReplyAsync("Some error has been occured. Please log in again.");
                    

                    return;
                }
                var tracks = (await SavedTrack.GetSavedTracks(token)).ToList();

                if (tracks is null)
                {
                    await ReplyAsync("There are no tracks");
                    return;
                }
                if(tracks.Count == 0)
                {
                    await ReplyAsync("There are no tracks");
                    return;
                }

                if(asc is bool Ascending)
                {
                    if(Ascending)
                    {
                        if (order is null) tracks = tracks.OrderBy(t => t.ArtistNames.First()).ToList();
                        else if (order.Value) tracks = tracks.OrderBy(t => t.SavedAt).ToList();
                        else tracks = tracks.OrderBy(t => t.Name).ToList();
                    }
                    else
                    {
                        if (order is null) tracks = tracks.OrderByDescending(t => t.ArtistNames.First()).ToList();
                        else if (order.Value) tracks = tracks.OrderByDescending(t => t.SavedAt).ToList();
                        else tracks = tracks.OrderByDescending(t => t.Name).ToList();
                    }
                }

                if(count > 0)
                {
                    if (count > tracks.Count)
                    {
                        count = tracks.Count;
                    }
                    else if (first)
                        tracks = tracks.Take(count).ToList();
                    else if (!first)
                        tracks = tracks.TakeLast(count).ToList();
                }

                if (shuffle)
                    tracks.Shuffle();

                if (beExplicit.HasValue) tracks = beExplicit.Value ? tracks.Where(t => t.Explicit ?? false).ToList() : tracks.Where(t => !t.Explicit ?? false).ToList();
                    
                
                await PlayTracks(tracks, Context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        [Command("GetPlaylists", RunMode = RunMode.Async)]
        [Alias("GPL")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.Speak)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        public async Task GetPlaylists()
        {
            SBUser user;

            try
            {
                user = await GetUser(Context.User.Id);
                if (user is null) throw new NullReferenceException();
            }
            catch
            {
                await ReplyAsync(EmojiHelper.Exit + " You are not logged in.");
                return;
            }

            try
            {
                var spotUser = await CurrentUser.GetCurrentUser(await user.GetAccessToken());

                var playlists = await spotUser.GetPlaylists(await user.GetAccessToken());

                Console.WriteLine(playlists.Length);
                await new PlaylistService(playlists, spotUser, Context.Guild, Context.User, true).SendFirstMessage(Context.Channel);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        [Command("GetPlaylists", RunMode = RunMode.Async)]
        [Alias("GPL")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.Speak)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [RequireOwner]
        public async Task GetPlaylists(string u)
        {
            var list = from coll in await Context.Channel.GetUsersAsync(CacheMode.AllowDownload).ToListAsync()
                       from User in coll
                       where !User.IsBot && (User.Username == u || (User + "") == u || (User is SocketGuildUser guildUser && guildUser.Nickname == u))
                       select User;

            if(!list.Any())
            {
                await ReplyAsync($"Couldn't find `{u}`. Check if the user has permission to this channel, and try again.");
                return;
            }


            var dcUser = list.First();
            SBUser user;

            try
            {
                user = await GetUser(dcUser.Id);
                if (user is null) throw new NullReferenceException();
            }
            catch
            {
                await ReplyAsync(EmojiHelper.Exit + " " + u + " is not logged in.");
                return;
            }

            try
            {
                var spotUser = await CurrentUser.GetCurrentUser(await user.GetAccessToken());

                var playlists = from p in await PlayList.GetUserPlaylistsAsync(await user.GetAccessToken(), spotUser.Id)
                                where p.Public.HasValue && p.Public.Value == true 
                                select p;

                await new PlaylistService(playlists, spotUser, Context.Guild, Context.User, false).SendFirstMessage(Context.Channel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        [Command("GetUser", RunMode = RunMode.Async)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task SelfUser()
        {
            try
            {
                var user = await GetUser(Context.User.Id);
                if(user is null)
                {
                    await ReplyAsync("You are not signed in.");
                    return;
                }

                var u = await CurrentUser.GetCurrentUser(await user.GetAccessToken());

                var embed = SpotUser(u);

                await ReplyAsync(embed: embed);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }

        [Command("GetUser")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireOwner]
        public async Task GetUser(string u)
        {
            try
            {
                var list = from User in Context.Guild.Users
                           where !User.IsBot && (User.Username == u || (User + "") == u || (User is SocketGuildUser guildUser && guildUser.Nickname == u))
                           select User;

                if (!list.Any())
                {
                    await ReplyAsync($"Couldn't find `{u}`. Check if the user has permission to this channel, and try again.");
                    return;
                }


                var dcUser = list.First();
                var user = await GetUser(dcUser.Id);
                if (user is null)
                {
                    await ReplyAsync(dcUser+ " is not signed in.");
                    return;
                }

                var currentUser = await CurrentUser.GetCurrentUser(await user.GetAccessToken());

                var embed = SpotUser(currentUser);

                await ReplyAsync(embed: embed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
        
        [Command("SearchPlaylist", RunMode = RunMode.Async)]
        [Alias("SPL")]
        public async Task SearchPlaylist(string param, [Remainder]string query = null)
        {
            try
            {
                IEnumerable<ISpotYTPlaylist> playlists;

                if((param == "-s" || param == "-y") && query is null)
                {
                    await ReplyAsync("Wrong parameters!");
                    return;
                }

                var lower = param.ToLower();
                if (lower == "-s") playlists = await Search.SearchPlaylist(await SpotClient.GetTokenAsync(), query);
                else if (lower == "-y") playlists = await YTPlaylist.SearchPlaylist(query);
                else playlists = await YTPlaylist.SearchPlaylist(param + " " + query);


                if (playlists is null) await ReplyAsync("An error occured.");
                if (!playlists.Any()) await ReplyAsync("No playlist has been found.");

                await new PlaylistService(playlists, null, Context.Guild, Context.User, false).SendFirstMessage(Context.Channel);


            }
            catch { await ReplyAsync("An error occured"); }
            
        }

        [Command("PlayPlaylist", RunMode = RunMode.Async)]
        [Alias("PP")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.Speak)]
        [RequireBotPermission(GuildPermission.Connect)]
        public async Task PlayPlaylist(string param, [Remainder] string query = "")
        {
            if(param.IsUrl())
            {
                if (param.Contains("spotify")) (param, query) = ("-s", param);
            }


            if (param.ToLower() == "-s")
            {
                string token;
                var user = await SBUser.GetUser(Context.User.Id);
                try
                {
                    token = await user.GetAccessToken();
                }
                catch
                {
                    token = await SpotClient.GetTokenAsync();
                }

                PlayList playlist = null;
                query = FormatToId(query);
                try
                {
                    try
                    {
                        if(query.Length == 22)
                            playlist = await PlayList.GetPlaylistAsync(token, query);
                    }
                    catch { }

                    if (playlist is null)
                    {
                        var res = await Search.SearchPlaylist(token, query);

                        if (res is null || res.Length == 0)
                        {
                            throw new Exception();
                        }

                        var search = (from s in res
                                      let percent = s.Name.ToLower() == query.ToLower() ? 1 : s.Name.GetSimilarity(query)
                                      orderby percent descending
                                      select s);

                        if (search.Any())
                            playlist = search.First();
                        else throw new Exception();
                    }
                }
                catch
                {
                    await ReplyAsync(EmojiHelper.Exit + " Couldn't find the playlist.");
                    return;
                }

                try
                {
                    var tracks = await playlist.GetTracks(token);
                    await PlayTracks(tracks, Context, "Now Playing: " + playlist.Name);
                }
                catch
                {
                    await ReplyAsync(EmojiHelper.Exit + " An error occured while getting the tracks of the playlist.");
                }
                return;
            }
            else if (param.ToLower() != "-y")
                query = param + (string.IsNullOrEmpty(query) ? "" : " " + query);

            
            if(param.IsUrl())
            {
                await AudioModule.PlayTracksAsync(Context, query);
                return;
            }

            var playlists = await YTPlaylist.SearchPlaylist(query);
            if (!playlists.Any())
            {
                await ReplyAsync("Couldn't find any playlist\\:/");
                return;
            }

            YTPlaylist ytplaylist = playlists.First();
            float egyezes = 0;
            foreach (var pl in playlists)
            {
                float sim;
                if (pl.Name.ToLower() == pl.Name.ToLower()) ytplaylist = pl;
                else if ((sim = pl.Name.GetSimilarity(query)) > egyezes) (ytplaylist, egyezes) = (pl, sim);
            }

            await AudioModule.PlayTracksAsync(Context.Channel as ITextChannel, Context.User, Context.Guild, ytplaylist.Url);

            static string FormatToId(string text)
            {
                if (text.IsUrl())
                {
                    string plUrl = "playlist/";
                    text = text[(text.IndexOf(plUrl) + plUrl.Length)..];

                    if (text.Contains("/")) text = text.Substring(0, text.IndexOf("/"));
                    if (text.Contains("?")) text = text.Substring(0, text.IndexOf("?"));
                }

                if (text.StartsWith("spotify:playlist:")) text = text[(text.LastIndexOf(":") + 1)..];

                return text;

            }
        }

        [Command("PlayOnSpotify", RunMode = RunMode.Async)]
        [Alias("POS")]
        public async Task PlayOnSpotify(string deviceType = null)
        {
            var timer = new System.Timers.Timer(60 * 1000) { Enabled = true, AutoReset = false };
            
            var method = typeof(SpotMusicModule).GetMethod(nameof(PlayOnSpotifyHelper));
            var task = (Task)method.Invoke(this, new[] { deviceType });
            timer.Elapsed += (obj, sen) => {
                if (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    Console.WriteLine("Ending task");
                    task.Dispose();
                    Context.Message.AddReactionAsync(EmojiHelper.Exit);
                }
            };
            Console.WriteLine("Starting task");
            await task;
            Console.WriteLine("Task ended");
            
            
        }
        private async Task PlayOnSpotifyHelper(string deviceType = null)
        {
            try
            {
                await ReplyAsync("Note that this this function is currently in beta testing on spotify, so it might not work (as expected).");
                var songs1 = Player.GetPlayer(Context.Guild).Tracks;
                var songs = songs1;

                var user = await SBUser.GetUser(Context.User.Id);
                if (user is null)
                {
                    await ReplyAsync(EmojiHelper.Exit + " You are not logged in!");
                    return;
                }


                var token = await user.GetAccessToken();
                var spotU = await CurrentUser.GetCurrentUser(token);

                if (spotU.Product != "premium")
                {
                    await ReplyAsync(EmojiHelper.Exit + " This function is only for **Spotify Premium** users.");
                    return;
                }

                if (!songs.Any())
                {
                    await ReplyAsync("There are no songs currently in the queue.");
                    return;
                }

                var devices = from d in await Device.GetUserDevices(token) ?? Array.Empty<Device>() where !d.IsRestricted select d;
                if (devices is null || !devices.Any())
                {
                    await ReplyAsync("No device has found to play on.");
                    return;
                }

                if (!(deviceType is null))
                {
                    var type = deviceType.ToLower() switch
                    {
                        "phone" => AGoodSpotifyAPI.DeviceType.Smartphone,
                        "mobile" => AGoodSpotifyAPI.DeviceType.Smartphone,
                        "m" => AGoodSpotifyAPI.DeviceType.Smartphone,
                        "tv" => AGoodSpotifyAPI.DeviceType.TV,
                        "pc" => AGoodSpotifyAPI.DeviceType.Computer,
                        "computer" => AGoodSpotifyAPI.DeviceType.Computer,
                        "tablet" => AGoodSpotifyAPI.DeviceType.Tablet,
                        "speaker" => AGoodSpotifyAPI.DeviceType.Speaker,
                        "console" => AGoodSpotifyAPI.DeviceType.GameConsole,
                        _ => AGoodSpotifyAPI.DeviceType.Unknown
                    };

                    if (type != AGoodSpotifyAPI.DeviceType.Unknown)
                    {
                        devices = from d in devices where type == d.Type select d;
                        if (!devices.Any())
                        {
                            await ReplyAsync($"No {type} has found.");
                            return;
                        }
                    }
                }

                Device playon = (from d in devices orderby d.IsActive descending select d).First();

                await Context.Message.AddReactionAsync(EmojiHelper.Cool);
                var uris = new List<string>();

                int count = 0;

                foreach (var song in songs)
                {
                    new Thread(async () =>
                    {
                        try
                        {
                            var data = await SongData.GetSongData(ytUrl: song.Url);
                            if (!string.IsNullOrEmpty(data?.SpotId))
                                uris.Add("spotify:track:" + data.SpotId);  //spotify:track:2igwJvVOqwwvOJNxpdh0ME
                            else
                            {
                                var track = await song.GetSpotifyTrack();
                                uris.Add(track.Uri);
                            }

                        }
                        catch { }
                        count++;
                    }).Start();
                }

                while (count != songs.Count) ;

                token = user.Expired ? await (await SBUser.GetUser(Context.User.Id)).GetAccessToken() : token;

                bool ok;
                if (uris.Count <= 100)
                    ok = await SpotPlayer.Play(token, uris, playon.Id);
                else
                {
                    ok = await SpotPlayer.Play(token, uris.Take(100));
                    foreach (var t in uris.Skip(100))
                    {
                        new Thread(async () => { try { await SpotPlayer.QueueTrack(token, t); } catch { } }).Start();
                    }
                }

                if (ok)
                    await Context.Message.AddReactionAsync(EmojiHelper.Done);
                else await Context.Message.AddReactionAsync(EmojiHelper.Exit);
            }
            catch (Exception e) { Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace); await Context.Message.AddReactionAsync(EmojiHelper.Exit); }
        }


        [Command("CreatePlaylist", RunMode = RunMode.Async)]
        [Alias("CP")]
        public async Task CreatePlaylist()
        {
            await ReplyAsync("Not implemented yet.");
        }

        [Command("PlayArtist")]
        [Alias("PAr")]
        public async Task PlayArtist([Remainder]string query)
        {
            await ReplyAsync("Not implemented yet." + query);
        }

        [Command("PlayAlbum")]
        [Alias("PAl")]
        public async Task PlayAlbum([Remainder] string query)
        {
            if(query.IsUrl())
            {
                query = query[(query.IndexOf("album/") + "album/".Length)..];
                if(query.Contains("/"))
                    query = query.Substring(0, query.IndexOf("/"));
                if (query.Contains("?"))
                    query = query.Substring(0, query.IndexOf("?"));
            }

            if (query.StartsWith("spotify:album:"))
                query = query[(query.LastIndexOf(":") + 1)..];

            IEnumerable<Track> tracks = null;

            try
            {
                if(query.Length == 22)
                tracks = (await Album.GetAlbumTracksAsync(query, await SpotClient.GetTokenAsync())).ToList();
            }
            catch { }
            
            try
            {
                if (tracks is null) {
                    Console.WriteLine(query);
                    var a = (await Search.SearchAlbum(query, await SpotClient.GetTokenAsync())).First();
                    tracks = await a.GetTracksAsync(await SpotClient.GetTokenAsync());
                }

            }
            catch { }

            if(tracks is null)
            {
                await ReplyAsync(EmojiHelper.Exit + " Couldn't find any album :/\nTry entering the album's url, URI, id, or just its name.");
                return;
            }


            await PlayTracks(tracks, Context);
        }

        [Command("Like")]
        public async Task Like([Remainder] string track = null)
        {
            try
            {
                LavaTrack lavaTrack;
                Track spotTrack;

                var user = await SBUser.GetUser(Context.User.Id);
                if (user is null)
                {
                    await ReplyAsync("You are not logged in!");
                    return;
                }

                #region lavaTrack feltoltese
                if (track is not null)
                {
                    var res = await LavaNodeService.FirstNode.SearchSongs(track);
                    if (!res.Any())
                    {
                        await ReplyAsync("Haven't found any track :(");
                        return;
                    }
                    if (res.Count > 1)
                    {
                        await ReplyAsync("Cannot save a whole playlist (yet).");
                        return;
                    }

                    lavaTrack = res.First();
                }
                else
                {
                    lavaTrack = Player.GetPlayer(Context.Guild).CurrentTrack;
                    if (lavaTrack is null)
                    {
                        await ReplyAsync("No track is currently playing.");
                        return;
                    }
                }
                #endregion


                #region spotTrack megkeresese

                var songdata = await SongData.GetSongData(ytUrl: lavaTrack.Url);
                if (songdata is null)
                {
                    spotTrack = await new YoutubeConv(lavaTrack.Title).GetSpotifyTrack();
                    if (spotTrack is null)
                    {
                        await ReplyAsync("Couldn't find the track on spotify.");
                        return;
                    }
                }
                else
                {
                    try
                    {
                        spotTrack = await Track.GetTrackAsync(await SpotClient.GetTokenAsync(), songdata.SpotId);
                        if (spotTrack is null) throw new Exception();
                    }
                    catch
                    {
                        await ReplyAsync("An error occured while finding the track on spotify.");
                        return;
                    }
                }
                #endregion

                var token = user.Expired ? await (await SBUser.GetUser(Context.User.Id)).GetAccessToken() : await user.GetAccessToken();

                await spotTrack.SaveTrack(token);
                await ReplyAsync("Liked track: " + spotTrack.Name);

            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                await ReplyAsync("An error occured");
            }


        }

    }
}
