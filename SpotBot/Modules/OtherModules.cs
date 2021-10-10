using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

using SpotBot.Configs;
using SpotBot.Helpers;
using SpotBot.Services;

namespace SpotBot.Modules
{
    public class OtherModules : ModuleBase<SocketCommandContext>
    {
        private readonly Config _config;

        

        public OtherModules(Config config)
        {
            _config = config;
        }

        [Command("TrackStarted")]
        public async Task SetTrackStarted()
        {
            var messageOn = !GuildService.TrackStartedMessageCheck(Context.Guild);
            await GuildService.ChangeTrackStartedMessage(Context.Guild.Id, messageOn);

            if (messageOn)
                await ReplyAsync("Turned on.");
            else await ReplyAsync("Turned off.");


        }

        [Command("Invite", RunMode = RunMode.Async)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Invite()
        {                                               
            var embed = await Task.Run(() => new EmbedBuilder()                                                                                                                   
                .WithTitle("Invite SpotBot to your own server!")
                .WithColor(Color.Gold)
                .WithUrl(_config.InviteLink)
                .Build());

            await ReplyAsync(embed: embed);
        }

        [Command("Ping", RunMode = RunMode.Async)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Ping()
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle(Context.Client.Latency + " ms")
                .WithColor(Color.Green)
                .Build());
            await ReplyAsync(embed: embed);
        }

        [Command("Servers", RunMode = RunMode.Async)]
        public async Task Guilds()
        {
            var e = await Task.Run(() => new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle("Server count: " + Context.Client.Guilds.Count)
                .Build());

            await ReplyAsync(embed: e);

        }

        [Command("Owner", RunMode = RunMode.Async)]
        public async Task Owner()
        {
            var info = await Context.Client.GetApplicationInfoAsync();

            var user = info.Owner;

            var e = new EmbedBuilder().WithAuthor(user).Build();
            await ReplyAsync(embed: e);
        }

        [Command("SetGame", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task SetGame([Remainder]string text)
        {
            await Context.Client.SetGameAsync(text);
        }

        [Command("SetStatus", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task SetActivity(int num = 0)
        {
            UserStatus status = num switch
            {
                1 => UserStatus.AFK,
                2 => UserStatus.DoNotDisturb,
                3 => UserStatus.Invisible,
                _ => UserStatus.Online
            };
            await Context.Client.SetStatusAsync(status);
        }

        [Command("Help", RunMode = RunMode.Async)]
        public async Task Help()
        {
            var e = await Task.Run(() => GetHelpText(Context));

            await ReplyAsync(embed: e);
        }

        public static Embed GetHelpText(SocketCommandContext context)
        {
            var prefix = GuildService.GetPrefix(context.Guild.Id);
            var e = new EmbedBuilder()
                .WithAuthor(context.Client.CurrentUser)
                .WithTitle($"SpotBot is currently under development and in alfa testing.\nErrors may occur while listening to music.\n\nGeneral prefix: !sb\nServer prefix: {prefix}")
                .WithDescription("\nFor commands, [click here](http://spotbot.nicepage.io)");

            //var commands = new List<(string Name, string Value)>
            //{
            //      ("Join", "Joins to the user's voice channel."),
            //      ("Play {query}", "Plays the track given in the parameter.\nquery can be a link, or a youtube search query.\nAlias: P"),
            //      ("Pause", "Pauses the track if any is being played."),
            //      ("Skip {count/optional}", "Skips the track\nCount parameter is optional, if not given then skips only 1 track.\nAliases: S, Next"),
            //      ("Previous {count/optional}", "The player goes jumps back and plays the previous track"),
            //      ("Disconnect", "Disconnects from voice channel if connected.\nAliases: Leave, Dc, Goodbye"),
            //      ("Shuffle", "Shuffles the current queue."),
            //      ("Loop", "Looping off / Looping queue / Looping a track"),
            //      ("NowPlaying", "Shows the track played at the moment by SpotBot."),
            //      ("Queue", "Shows the queue of the current player."),
            //      ("Player", "Really cool feature, try it while listening to music."),
            //      ("Seek {position}", "Seeks the player to the given position.\nFormats: hh:mm:ss, mm:ss"),
            //      ("Volume {newVolume/optional}", "Sets the player's volume.\nIf newVllume is not given, the current volume will be shown."),
            //      ("Clear", "Clears the queue"),
            //      ("Search {query}", "Searches for a query on youtube."),
            //      ("Remove {position}", "Removes the song from the queue."),
            //      ("Move {position} {newPos}", "Moves a track to a new position."),
            //      ("Prefix {new prefix/optional}", "Changes the server's prefix if the new prefix parameter is given\nElse returns the current prefix"),
            //      ("Invite", "Invite SpotBot to your own server"),
            //    //  ("Servers", "Shows SpotBot's server count."),
            //      ("Login", "This function isn't available at the moment."),
            //      ("Logout", "This function isn't available at the moment."),
            //      ("Playliked", "This function isn't available at the moment."),
            //      ("Getplaylists", "This function isn't available at the moment."),
            //      ("Getuser", "This function isn't available at the moment."),
            //};


            return e.Build();
        }

        [Command("GetSongJSON")]
        [RequireOwner]
        public async Task ReturnTextJson()
        {
            var p = Player.GetPlayer(Context.Guild);
            if (p is null) return;

            await ReplyAsync(JsonConvert.SerializeObject(p.CurrentTrack));
        }

        [Command("Reacting")]
        [RequireOwner]
        public async Task StartStopReacting()
        {
            SpotBotClient.React = false;
            await Context.Client.SetStatusAsync(UserStatus.DoNotDisturb);
            await Context.Message.AddReactionAsync(EmojiHelper.Done);
        }

        [Command("TrySpotify", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task TrySpotify([Remainder] string query)
        {
            try
            {
                var node = LavaNodeService.FirstNode;

                var songs = await node.SearchSongs(query);
                string text = "No Matches\n\n";
                int found = 0;
                
                foreach (var song in songs)
                {

                    var result = await song.GetSpotifyTrack();
                    if (result is null) text +=  ++found + ". " + song.Title +"\n";

                    if (text.Length >= 1900 && text.Length <= 2000)
                    {
                        await ReplyAsync($"```{text}```");
                        text = string.Empty;
                    }

                    

                }
                await ReplyAsync($"```{text}```");
                await ReplyAsync($"Found: {songs.Count - found}/{songs.Count}");
            }
            catch (Exception e) { Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
