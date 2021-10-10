using Discord;
using Discord.Rest;
using Discord.WebSocket;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;

using Victoria;

using SpotBot.Helpers;

namespace SpotBot.Services
{
    public class QueueService
    {
        private static string EmoteFirstPage => "⏫";
        private static string EmoteNext => "🔽";
        private static string EmotePrevious => "🔼";
        private static string EmoteEnd => "⏬";


        private readonly static Dictionary<ulong, QueueService> _services = new Dictionary<ulong, QueueService>();
        public static QueueService GetQueueService(IMessage message = null, bool createNew = false)
        {
            if (_services.ContainsKey(message.Id)) 
                 return _services[message.Id];

            if (!createNew) return null;

            var guild = (message.Channel as SocketGuildChannel).Guild;
            var queue = new QueueService(guild);

            return queue;
        }
        public static async Task TryRemoveQueue(ulong guildId)
        {
            var datas = from s in _services where s.Value.Guild.Id == guildId && !s.Value.SearchResult select s;

            if (datas.Any())
                foreach (var d in datas)
                {
                    await d.Value.Message.DeleteAsync();
                    _services.Remove(d.Key);
                }
        }
        public static QueueService CreateQueueSearch(IGuild guild, IEnumerable<LavaTrack> tracks)
        {
            var q = new QueueService(guild)
            {
                Page = 0,
                SearchResult = true,
                Tracks = tracks,
            };
            

            return q;
        }

        public RestUserMessage Message { get; private set; }
        public IGuild Guild { get; }
        private static int Cutting => 20;
        private int Page { get; set; }    
        private bool First { get; set; } = true;
        public bool SearchResult { get; set; } = false;
        public IEnumerable<LavaTrack> Tracks { get; private set; }
        private DateTime Added { get; }

        private QueueService(IGuild guild)
        {
            Page = 0;
            Guild = guild;
            Added = DateTime.Now;
        }

        private static string Format(IEnumerable<LavaTrack> tracks, int position, IGuild guild, int page, bool first = false, bool searchResult = false)
        {
            var md = GuildService.GetMd();
            string format = md ? "md" : "ml";

            int substring = 70;
            int pad = 75;

            if (!tracks.Any())
            {
                _services.Remove(guild.Id);
                return searchResult ? "Couldn't find any track." : "There are no tracks in the queue";
            }
            tracks = tracks.ToList();                                                    
            string text = $"{(md ? "#" : "")}{(searchResult ? "SearchResult" : $"Queue For {guild.Name}")}\n\n";

            var list = tracks.ToList();
            int more;
            int endpos;
            bool vege;

            if(searchResult)
            {
                double maxPage1 = Math.Ceiling(list.Count / (double)Cutting) - 1;

                if (page < 0) page = 0;
                else if (page > maxPage1) page = (int)maxPage1;

                int start1 = page * Cutting;
                if(list.Count - start1 < 20)
                {
                    if (list.Count < 20) start1 = 0;
                    else start1 = list.Count - 20;
                }


                endpos = start1;
                for (int i = start1; i < start1 + Cutting & (vege = i < list.Count); i++)
                {
                    var t = list[i];

                    text += $"{i + 1,2}. {(t.Title.Length > substring ? t.Title.Substring(0, substring) : t.Title),-70} {(t.Duration.TotalHours >= 1 ? t.Duration.ToString("hh\\:mm\\:ss") : t.Duration.ToString("mm\\:ss"))}\n";

                    endpos = i;
                }

                more = list.Count - endpos - 1;
                if (more > 0 && vege)
                    text += $"\n{(md ? "#" : "")}And {more} more tracks";

                return $"```{format}\n{text}```";
            }

            if (first)
            {

                if (position > 0)
                {
                    var prev = list[position - 1];
                    text += $"{position,2}. {(prev.Title.Length > substring ? prev.Title.Substring(0, substring) : prev.Title),-70} {(prev.Duration.TotalHours >= 1 ? prev.Duration.ToString("hh\\:mm\\:ss") : prev.Duration.ToString("mm\\:ss"))}\n";
                }

                var cur = list[position];
                text += "-----------Current-----------\n";
                text += $"{position + 1,2}. {(cur.Title.Length > substring ? cur.Title.Substring(0, substring) : cur.Title),-70} {(cur.Duration.TotalHours >= 1 ? cur.Duration.ToString("hh\\:mm\\:ss") : cur.Duration.ToString("mm\\:ss"))}\n";
                text += "-----------Current-----------\n";

                endpos = position;
               
                for (int i = position + 1; (vege = i < list.Count) && i < position + Cutting - (position > 0 ? 1 : 0); i++)
                {
                    var t = list[i];

                    text += $"{i + 1,2}. {(t.Title.Length > substring ? t.Title.Substring(0, substring) : t.Title),-70} {(t.Duration.TotalHours >= 1 ? t.Duration.ToString("hh\\:mm\\:ss") : t.Duration.ToString("mm\\:ss"))}\n";
                    endpos = i + 1;
                }
                more = list.Count - endpos;
                if (more > 0 && vege)
                    text += $"\n{(md ? "#" : "")}And {more} more tracks";

                if (!vege) text += "\nYou have reached the end of the queue. Feel free to add more tracks.";

                return $"```{format}\n{text}```";
            }

            else
            {
                double maxPage = Math.Ceiling(list.Count / (double)Cutting) - 1;

                if (page < 0) page = 0;
                else if (page > maxPage) page = (int)maxPage;

                int start = page * Cutting;
                if (list.Count - start < 20)
                {
                    if (list.Count < 20) start = 0;
                    else start = list.Count - 20;
                }

                endpos = start;
                for (int i = start; i < start + Cutting & (vege = i < list.Count); i++)
                {
                    if (i == position)
                    {
                        var cur = list[i];
                        text += "-----------Current-----------\n";
                        text += $"{position + 1,2}. {(cur.Title.Length > substring ? cur.Title.Substring(0, substring) : cur.Title).PadRight(pad - (position + 1).ToString("00").Length)} {(cur.Duration.TotalHours >= 1 ? cur.Duration.ToString("hh\\:mm\\:ss") : cur.Duration.ToString("mm\\:ss"))}\n";
                        text += "-----------Current-----------\n";
                    }
                    else
                    {
                        var t = list[i];

                        text += $"{i + 1,2}. {(t.Title.Length > substring ? t.Title.Substring(0, substring) : t.Title).PadRight(pad - (position + 1).ToString("00").Length)} {(t.Duration.TotalHours >= 1 ? t.Duration.ToString("hh\\:mm\\:ss") : t.Duration.ToString("mm\\:ss"))}\n";
                    }
                    endpos = i;
                }


                more = list.Count - endpos - 1;
                if (more > 0 && vege)
                    text += $"\n{(md ? "#" : "")}And {more} more tracks";
                if (!vege) text += "\nYou have reached the end of the queue. Feel free to add more tracks.";

                return $"```{format}\n{text}```";
            }

        }

        public void Save()
        {
            if (_services.ContainsKey(Message.Id))
                _services[Message.Id] = this;
            else _services.Add(Message.Id, this);
        }

        public async Task SendFirstText(ISocketMessageChannel channel)
        {
            var p = Player.GetPlayer(Guild);
            IEnumerable<LavaTrack> tracks;
            if(SearchResult)
            {
                string txt = Format(Tracks, 0, Guild, Page, searchResult: true);
                Message = await channel.SendMessageAsync(txt);
                if (Message.Content != "There are no tracks in the queue")
                {
                    if (Tracks.Count() > 20)
                    {
                        Save();
                        await Message.AddReactionsAsync(new IEmote[] { EmojiHelper.DoubleArrowUp, EmojiHelper.ArrowUp, EmojiHelper.ArrowDown, EmojiHelper.DoubleArrowDown });
                    }

                }
                
                return;
            }

            tracks = p.Tracks;
            var CurrentTrack = p.Position;

            string text = Format(tracks, CurrentTrack, Guild, 0, true);
            if (text.Contains('\'') || text.Contains("\"")) text = text.Replace('\'', '`').Replace('\"', '`');
            Message = await channel.SendMessageAsync(text);
            Save();
            if (Message.Content != "There are no tracks in the queue")
               await Message.AddReactionsAsync(new IEmote[] { EmojiHelper.DoubleArrowUp, EmojiHelper.ArrowUp, EmojiHelper.ArrowDown, EmojiHelper.DoubleArrowDown, EmojiHelper.Shuffle, EmojiHelper.CurrentTrack, EmojiHelper.ColorChange });

            Page = CurrentTrack / Cutting;

            var previous = from cucc in _services
                           let q = cucc.Value
                           let m = q.Message
                           where channel.Id == m.Channel.Id && m.Id != Message.Id && m.CreatedAt < Message.CreatedAt
                           select (cucc.Key, m);


            foreach (var (Key, Message) in previous)
            {
                try
                {
                    _services.Remove(Key);
                    await Message.DeleteAsync();

                }
                catch { }
            }



        }

        public async Task Modify(IEmote emote, IUser requestedBy) => await ModifyAsync(this, emote, requestedBy);

        public static async Task ModifyAsync(QueueService service, IEmote emote, IUser requestedBy)
        {
            var Message = service.Message;
            var Guild = service.Guild;
            var Page = service.Page;
            var First = service.First;
            var searchResult = service.SearchResult;

            string text = string.Empty;
            var p = Player.GetPlayer(Guild);

            var Tracks = searchResult ? service.Tracks : p.Tracks;

            if (!Tracks.Any())
            {
                await Message.ModifyAsync(msg => msg.Content = "`There arent any tracks currently in the queue`");
                return;
            }

            if (emote.Name == EmoteFirstPage)
            {
                if (First)
                    First = false;
                text = Format(Tracks, p.Position, Guild, Page = 0, searchResult: searchResult);
            }
            else if (emote.Name == EmoteEnd)
            {
                if (First)
                    First = false;
                text = Format(Tracks, p.Position, Guild, Page = Tracks.Count() / Cutting, searchResult: searchResult);
            }
            else if (emote.Name == EmoteNext)
            {
                if (Page == Tracks.Count() / Cutting) return;
                if (First)
                    First = false;
                text = Format(Tracks, p.Position, Guild, ++Page, searchResult: searchResult);
            }
            else if (emote.Name == EmotePrevious)
            {
                if (Page == 0) return;
                if (First && p.Position % 10 != 2)
                    First = false;
                else Page--;
                text = Format(Tracks, p.Position, Guild, Page, searchResult: searchResult);
            }
            else if (emote.Name == EmojiHelper.Shuffle.Name && !searchResult)
            {
                if (requestedBy is SocketGuildUser user)
                {
                    var node = LavaNodeService.GetNode(p.Guild);
                    if (node is null) return;

                    if (!node.TryGetPlayer(p.Guild, out LavaPlayer player)) return;

                    if (user.VoiceChannel is null && user.VoiceChannel != (player.VoiceChannel)) return;
                }

                p.Shuffle();
                if (First)
                    First = false;
                text = Format(Tracks, p.Position, Guild, Page = 0,searchResult: searchResult);
            }
            else if(emote.Name == EmojiHelper.CurrentTrack.Name)
            {
                First = true;
                text = Format(Tracks, p.Position, Guild, Page, First);
            }
            else if (emote.Name == EmojiHelper.ColorChange.Name)
            {
                text = Format(Tracks, p.Position, Guild, Page, First,searchResult: searchResult);
            }
            else return;
            if (text != "There are no tracks in the queue")
            {
                service.Page = Page;
                service.First = First;
                service.Save();
            }

            if (text.Contains('\'') || text.Contains("\"")) text = text.Replace('\'', '`').Replace('\"', '`');

            await Message.ModifyAsync(msg => msg.Content = text);

        }





        #region Statik TimerPart
        public static void SetTimer()
        {
            if (_timer.Enabled) return;
            _timer.Start();
            _timer.Elapsed += async (sen, e) =>
            {
                var lejart = from s in _services
                             let v = s.Value
                             let ido = DateTime.Now - v.Added
                             where v.SearchResult && ido.TotalMinutes > 15
                             select s;

                foreach (var (k, v) in lejart)
                {
                    try
                    {
                        _services.Remove(k);
                        await v.Message.ModifyAsync(m => { m.Content = "`No content available`"; m.Embed = null; });
                    }
                    catch { }
                }
            };
        }
        private static readonly System.Timers.Timer _timer = new Timer(1000 * 60) { Enabled = false, AutoReset = true };
        #endregion

    }
}
