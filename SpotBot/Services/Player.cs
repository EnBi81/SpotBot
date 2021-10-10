using Discord;
using System.Collections.Generic;
using System.Linq;
using Victoria;
using SpotBot.Helpers;
using System;

namespace SpotBot.Services
{
    public class Player
    {
        private class PlayerMetadata
        {
            public object Lock = new object();
            public IGuild Guild { get; }
            public LavaTrack CurrentTrack { get; set; } = null;
            public List<LavaTrack> Tracks { get; } = new List<LavaTrack>();
            public int Position { get; set; } = -1;
            public bool? Loop { get; set; } = null;
            public bool CanAddTracks { get; set; } = true;

            public ITextChannel TextChannel { get; set; }

            public IVoiceChannel VoiceChannel { get; set; }

            public PlayerMetadata(IGuild guild) => Guild = guild;

        }




        private static Dictionary<ulong, PlayerMetadata> Players { get; } = new Dictionary<ulong, PlayerMetadata>();
        public static void RemoveId(ulong id)
        {
            try
            {
                lock (Players[id].Lock)
                {
                    Players[id].Tracks.Clear();
                    Players[id].Position = -1;
                    Players[id].CurrentTrack = null;
                    Players[id].Loop = null;
                }
            }
            catch { }
        }

        public static Player GetPlayer(IGuild guild)
        {
            if (Players.ContainsKey(guild.Id)) return new Player(Players[guild.Id]);

            var p = new Player(guild);
            return p;
        }

        public IGuild Guild { get; }
        public ulong GuildId => Guild.Id;
        public LavaTrack CurrentTrack { get => Players[GuildId].CurrentTrack; set 
            {
                Players[GuildId].CurrentTrack = value; 
                Tracks[Position] = value; 
            } }
        public List<LavaTrack> Tracks => Players[GuildId].Tracks;
        public int Position { get => Players[GuildId].Position; private set => Players[GuildId].Position = value; } 
        public bool? Loop { get => Players[GuildId].Loop; set => Players[GuildId].Loop = value; }
        public ITextChannel TextChannel { get => Players[GuildId].TextChannel; set => Players[GuildId].TextChannel = value; }
        public IVoiceChannel VoiceChannel { get => Players[GuildId].VoiceChannel; set => Players[GuildId].VoiceChannel = value; }
        public bool CanAddTracks { get => Players[GuildId].CanAddTracks; set => Players[GuildId].CanAddTracks = value; }

        public LavaTrack NextTrack
        {
            get
            {
                lock (Players[GuildId].Lock)
                {
                    var loop = Loop;
                    if (Tracks.Count == 0) return null;

                    if (loop.HasValue && loop.Value)
                    {
                        if (Position == -1) Position++;

                        if (CurrentTrack is null)
                            CurrentTrack = Tracks[Position];

                        return CurrentTrack;
                    }
                    if(Position + 1 >= Tracks.Count)
                    {
                        if (loop.HasValue)
                            Position = 0;
                        else return null;
                    }
                    
                    try
                    {
                        if(!loop.HasValue)
                            Position++;
                        var t = Tracks[Position];
                        CurrentTrack = new LavaTrack(t.Hash, t.Id, t.Title, t.Author, t.Url, TimeSpan.Zero, (long)t.Duration.TotalMilliseconds, t.CanSeek, t.IsStream);
                      
                        return CurrentTrack;
                    }
                    catch (Exception e )
                    {
                        Console.WriteLine(e.Message);
                        return null;
                    }
                }
            }
        }
        public LavaTrack PreviousTrack
        {
            get
            {
                lock (Players[GuildId].Lock)
                {
                    if (Position > 0) Position--;

                    if (!Tracks.Any()) return null;

                    if (Tracks.Count <= Position) Position = Tracks.Count - 1;

                    var t = Tracks[Position];

                    CurrentTrack = new LavaTrack(t.Hash, t.Id, t.Title, t.Author, t.Url, TimeSpan.Zero, (long)t.Duration.TotalMilliseconds, t.CanSeek, t.IsStream);

                    return CurrentTrack;
                }
            }
        }

        private Player(IGuild guild)
        {
                Guild = guild;
            try
            {
                Players.Add(guild.Id, new PlayerMetadata(guild));
            }
            catch { }
        }
        private Player(PlayerMetadata data)
        {
            Guild = data.Guild;
            
        }


        public void ClearPlaylist()
        {
            lock (Players[GuildId].Lock)
            {
                Players[GuildId].Tracks.Clear();
                Position = -1;
                CanAddTracks = false;
            }
        }

        public void JumpToEnd()
        {
            lock (Players[GuildId].Lock)
            {
                Position = Tracks.Count;
            }
        }

        public void Shuffle()
        {
            lock (Players[GuildId].Lock)
            {
                Players[GuildId].Tracks.Shuffle();
                Players[GuildId].Tracks.Remove(CurrentTrack);
                Players[GuildId].Tracks.Insert(0, CurrentTrack);
                Position = 0;
            }
        }

        public string RemoveTrack(int count)
        {
            lock (Players[GuildId].Lock)
            {
                if (count < 0 || count >= Tracks.Count) throw new ArgumentOutOfRangeException(nameof(count));

                if (count <= Position) Position--;

                var name = Tracks[count];

                Players[GuildId].Tracks.RemoveAt(count);
                return name.Title;
            }
        }

        public void MoveTrack(int which, int where)
        {
            lock (Players[GuildId].Lock)
            {
                if (which < 0 || which >= Tracks.Count || where < 0 || where >= Tracks.Count) throw new ArgumentOutOfRangeException(nameof(where));
                if (which == where) throw new ArgumentNullException(nameof(which));

                var t = Tracks[which];
                Players[GuildId].Tracks.RemoveAt(which);
                Players[GuildId].Tracks.Insert(where > which ? where - 1 : where, t);
                int plusz = 0;
                if (which < Position) plusz--;
                if (where <= Position) plusz++;

                Position += plusz;
            }
        }

        public void AddTracks(IEnumerable<LavaTrack> tracks)
        {
            lock (Players[GuildId].Lock)
            {
                if (tracks is null) return;

                if (tracks.Count() + Tracks.Count > 600)
                {
                    if (tracks.Count() + Tracks.Count - Position > 600) throw new Exception();

                    for (int i = 0; i < Position - 1; i++)
                    {
                        try
                        {
                            Players[GuildId].Tracks.RemoveAt(0);
                        }
                        catch { }
                    }
                }
                else Players[GuildId].Tracks.AddRange(tracks);
            }
        }
        public void AddTrack(LavaTrack track) => AddTracks(new[] { track });

        public LavaTrack Skip(int count = 1)
        {
            lock (Players[GuildId].Lock)
            {
                if (Position + count > Tracks.Count)
                {
                    Position = Tracks.Count;
                    CurrentTrack = null;
                }
                else
                {
                    Position += count;
                    CurrentTrack = Tracks[Position];
                }

                return CurrentTrack;
            }

        }

        public void AddNext(IEnumerable<LavaTrack> tracks)
        {
            lock (Players[GuildId].Lock)
            {
                Players[GuildId].Tracks.InsertRange(Position + 1, tracks);
            }
        }

        public void JumpBack()
        {
            Position = Position >= 0 ? Position - 1 : -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"
        /// <exception cref="ArgumentNullException"
        public LavaTrack JumpTo(int where)
        {
            if (where < 0 || where >= Tracks.Count)
                throw new ArgumentOutOfRangeException(nameof(where));
            if (where == Position)
                throw new ArgumentNullException(nameof(where));

            lock(Players[GuildId].Lock)
            {
                CurrentTrack = Tracks[Position = where];
                return CurrentTrack;
            }
        }
    }
}
