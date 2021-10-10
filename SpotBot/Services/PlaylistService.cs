using AGoodSpotifyAPI.Classes;
using AGoodSpotifyAPI.InterFaces;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;

using SpotBot.Helpers;
using SpotBot.Modules;
using SpotBot.Spotify;

using YoutubeAPIThing;


namespace SpotBot.Services
{
    public class PlaylistService
    {
        private static Dictionary<ulong, PlaylistService> Services { get; } = new Dictionary<ulong, PlaylistService>();
        public static bool TryGetService(ulong messageId, out PlaylistService service)
        {
            if(Services.ContainsKey(messageId))
            {
                service = Services[messageId];
                return true;
            }

            service = null;
            return false;
        }

        private bool PrivatePlaylists { get; }
        private DateTime Start { get; }
        public SocketUser SocketUser { get; }
        public IGuild Guild { get; }
        private CurrentUser User { get; }
        public RestUserMessage Message { get; private set; }
        public int Position { get; private set; } = 0;
        public int FirstPos { get; private set; } = 0;
        public ISpotYTPlaylist[] Playlists { get; }

        private async Task End()
        {
            Services.Remove(Message.Id);

            try
            {
                await Message.ModifyAsync(m => m.Content = "`No content available.`");
                await Message.RemoveAllReactionsAsync();
            }
            catch { }
        }

        public PlaylistService(IEnumerable<ISpotYTPlaylist> playlists, CurrentUser user, IGuild guild, SocketUser socketUser, bool privatePlaylist)
            => (Playlists, User, Guild, SocketUser, PrivatePlaylists, Start) = (playlists.ToArray(), user, guild, socketUser, privatePlaylist, DateTime.Now);

        private static string Format(PlaylistService service, int ugras, ref int newPosition, ref int newFirstPos)
        {
            if(service.Playlists is null) return "`No playlists found!`";

            var playlists = service.Playlists;
            if (playlists.Length == 0) return "`No playlists found!`";


            var newPos = service.Position + ugras;
            if (newPos < 0) newPos = 0;
            else if (newPos >= playlists.Length) newPos = playlists.Length - 1;

            int newFirst = 0;
            if(playlists.Length > 15)
            {
                if(newPos > 7)
                {
                    newFirst = newPos - 7;
                    if (playlists.Length - newFirst < 15)
                        newFirst = playlists.Length - 15;
                }
            }

            newFirstPos = service.FirstPos = newFirst;
            newPosition = service.Position = newPos;

            int count = 15;
            string text = service.User is null ? $"Playlist results from {(playlists.First() is PlayList ? "Spotify" : "Youtube")}\n\n" : $"{service.User.DisplayName}'s Playlists.\n\n";
            static string ToString(ISpotYTPlaylist list, int i,bool current) => $"{i, 2}. {(list.Name.Length > 38 ? list.Name.Substring(0, 38) : list.Name).PadRight(current ? 38 : 40)} Tracks: {list.TrackCount, -7}{(list.Owner?.DisplayName is null ? "" : $" Owner: {list.Owner.DisplayName}")}\n";
            int vege = 0;
            for (int i = service.FirstPos; i < service.FirstPos + count && i < service.Playlists.Length; i++)
            {
                var list = service.Playlists[i];
                bool current = i == service.Position;
                var t = ToString(list, i + 1, current);

                text += (current ? "> " : "") + t;
                vege = i;
            }
            if(playlists.Length - (vege + 1) > 0)
                text += $"\nAnd {playlists.Length - (vege + 1)} more";
            text += "\nYou can select a playlist with the arrows, and enqueue the selected one with the okay emoji.";

            return $"```ml\n{text}```";
            
        }                                                                      

        public async Task SendFirstMessage(ISocketMessageChannel channel)
        {
            int p = 0, f = 0;
            var t = Format(this, 0, ref p, ref f);

            Position = p;
            FirstPos = f;
            Message = await channel.SendMessageAsync(t);
            if (Playlists.Any())
            {
                Save(true);

                await Message.AddReactionsAsync(new IEmote[] { EmojiHelper.DoubleArrowUp, EmojiHelper.ArrowUp, EmojiHelper.ArrowDown, EmojiHelper.DoubleArrowDown, EmojiHelper.Play, EmojiHelper.Shuffle, EmojiHelper.Exit });
            }
        }    

        public async Task Modify(IEmote emote, Discord.IUser user)
        {
            int ugras = 0;
            if (emote.Name == EmojiHelper.DoubleArrowUp.Name) ugras = -5;
            else if (emote.Name == EmojiHelper.ArrowUp.Name) ugras = -1;
            else if (emote.Name == EmojiHelper.ArrowDown.Name) ugras = 1;
            else if (emote.Name == EmojiHelper.DoubleArrowDown.Name) ugras = 5;
            else if (emote.Name == EmojiHelper.Play.Name || emote.Name == EmojiHelper.Shuffle.Name)
            {
                try 
                {
                    var pl = Playlists[Position];
                    if (pl is PlayList playlist)
                    {
                        List<PlaylistTrack> tracks = null;

                        try
                        {
                            if (PrivatePlaylists)
                            {
                                var u = await SBUser.GetUser(SocketUser.Id);
                                tracks = await playlist.GetTracks(await u.GetAccessToken());
                            }
                            else
                            {
                                tracks = await playlist.GetTracks(await SpotClient.GetTokenAsync());
                            }
                        }
                        catch { }

                        if (tracks is null)
                        {
                            await End();
                            return;
                        }

                        if (emote.Name == EmojiHelper.Shuffle.Name) tracks.Shuffle();

                        await SpotMusicModule.PlayTracks(tracks, SocketUser, Guild, Message.Channel);
                    }
                    else if(pl is YTPlaylist ytPlaylist)
                    {
                        await AudioModule.PlayTracksAsync(Message.Channel as ITextChannel, user as Discord.WebSocket.SocketUser, Guild, ytPlaylist.Url, true, emote.Name == EmojiHelper.Shuffle.Name);
                    }

                    
                }catch(Exception e)
                {
                    Console.WriteLine("Exception at PlaylistService playing tracks.");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                return;
            }
            else if(emote.Name == EmojiHelper.Exit.Name)
            {
                try
                {
                    await Message.RemoveAllReactionsAsync();
                }
                catch { }
                await End();
                return;
            }
            else return;                                  

            int p = 0, f = 0;
            string t = Format(this, ugras, ref p, ref f);

            Position = p;
            FirstPos = f;

            try
            {
                await Message.ModifyAsync(msg => msg.Content = t);
            }
            catch { }
        }

        public void Save(bool addIfNotFound = false)
        {
            var id = Message.Id;
            if (Services.ContainsKey(id))
            {
                Services[id] = this;
                return;
            }

            if(addIfNotFound)
                Services.Add(id, this);
        }






        #region Static Timer Part

        private static Timer Timer { get; } = new Timer(60 * 1000) { Enabled = false, AutoReset = true };
        internal static void SetTimer()
        {
            if (Timer.Enabled) return;

            Timer.Enabled = true;
            Timer.Elapsed += DeleteExpired;
        }
        private static async void DeleteExpired(object sender, ElapsedEventArgs e)
        {
            var expired = from vk in Services
                          let s = vk.Value
                          let time = DateTime.Now - s.Start
                          where time.TotalMinutes >= 15
                          select vk;

            foreach (var (k, v) in expired)
            {
                try
                {
                    Services.Remove(k);
                    await v.Message.ModifyAsync(m => m.Content = "`No content available.`");
                }
                catch
                { }
            }

        }


        #endregion

    }
}
