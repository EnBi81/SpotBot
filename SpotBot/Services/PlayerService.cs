using Discord;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using SpotBot.Helpers;

using Victoria;
using Victoria.Enums;

using Discord.WebSocket;

using YoutubeExplode;

namespace SpotBot.Services
{
    public class PlayerService
    {

        private static readonly Dictionary<ulong, PlayerService> _services = new Dictionary<ulong, PlayerService>();
        public static bool TryGetPlayer(IMessage message, out PlayerService playerService)
        {
            if(_services.ContainsKey(message.Id))
            {
                playerService = _services[message.Id];
                return true;
            }
            playerService = null;
            return false;
        }
        public static PlayerService GetNewPlayer(IGuild guild)
        {
            //var datas = from cucc in _services
            //            let s = cucc.Value
            //            where s.Guild.Id == guild.Id
            //            select cucc;
            return new PlayerService(guild);
        }
        public static async Task TryRemovePlayer(ulong guildId)
        {
            var data = from d in _services where d.Value.Guild.Id == guildId select d;

            if(data.Any())
            {
                foreach (var d in data)
                {
                    _services.Remove(d.Key);
                    await d.Value.Message.DeleteAsync();
                }
            }
        }


        public IGuild Guild { get; }
        public IUserMessage Message { get; private set; }

        private PlayerService(IGuild guild)
            => Guild = guild;


        private Embed CreateEmbed(LavaPlayer player, string iconUrl)
        {
            var p = Player.GetPlayer(Guild);

            var embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithAuthor("Player for " + Guild.Name + ". Invite SpotBot to your own server here.", iconUrl, url: SpotBotClient.Config.InviteLink);
            if (player.Track is null) embed = embed.WithTitle("No tracks currently in the player");
            else
            {
                var t = player.Track;
               
                embed = embed.WithTitle($"Current Track ({p.Position + 1}): " + player.Track.Title)
                    .WithDescription($"Position: {(t.Position.TotalHours > 1 ? t.Position.ToString("hh\\:mm\\:ss") : t.Position.ToString("mm\\:ss"))}/{(t.Duration.TotalHours > 1 ? t.Duration.ToString("hh\\:mm\\:ss") : t.Duration.ToString("mm\\:ss"))}" +
                    $"{(p.Position < p.Tracks.Count - 1 ? $"\nNext Track: [{p.Tracks[p.Position + 1].Title}]({p.Tracks[p.Position + 1].Url})" : string.Empty)}")
                    .WithUrl(player.Track.Url);

                //try
                //{
                //    string thumb;

                //    if (!(ytUrl is null) && ytUrl == t.Url) thumb = thumbnail;
                //    else
                //    {
                //        var thumbnails = (await YoutubeClient.Videos.GetAsync(player.Track.Id)).Thumbnails;
                //        thumb = thumbnails.MediumResUrl;
                //    }

                //    embed = embed.WithThumbnailUrl(thumb);
                //}
                //catch { }

            }

            var field = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Player state")
                .WithValue(player.PlayerState);

            var loop = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Loop")
                .WithValue(p.Loop is null ? "Looping off" : !p.Loop.Value ? "Looping the queue" : "Looping the track");

            var volume = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Volume")
                .WithValue(player.Volume);

            var rem = p.Tracks.Count - p.Position - 1;

            var remaining = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"Remaining music{(rem == 1 ? "" : "s")}")
                .WithValue(rem);

            var vege = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Music by:")
                .WithValue("SpotBot");




            embed = embed.AddField(field).AddField(loop).AddField(volume);
            if (rem > 0) embed = embed.AddField(remaining);

            embed = embed.AddField(vege);

            return embed.Build();
        }
        private async Task AddReactionsAsync()
        {
            var list = new List<Emoji>
            { 
                EmojiHelper.PausePlay,
                EmojiHelper.Previous,
                EmojiHelper.Next,
                EmojiHelper.Loop,
                EmojiHelper.Mute,
                EmojiHelper.VolumeDown,
                EmojiHelper.VolumeUp,
                EmojiHelper.ColorChange,
                EmojiHelper.Exit
            };

            await Message.AddReactionsAsync(list.ToArray());
        }
        public async Task SendFirstMessage(LavaPlayer player, ISocketMessageChannel channel)
        {
            if (!(player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Playing))
            {
                Message = await channel.SendMessageAsync(embed: new EmbedBuilder().WithTitle("First Start the player.").Build());
                return;
            }

            var embed = CreateEmbed(player, (await Guild.GetCurrentUserAsync()).GetAvatarUrl());

            Message = await channel.SendMessageAsync(embed: embed);

            _services.Add(Message.Id, this);
            await AddReactionsAsync();


            var previous = from s in _services
                           let p = s.Value
                           let channelId = p.Message.Channel.Id
                           where channelId == Message.Channel.Id && p.Message.Id != Message.Id && p.Message.CreatedAt < Message.CreatedAt
                           select s;

            foreach (var (k, v) in previous)
            {
                try
                {
                    _services.Remove(k);
                     await v.Message.DeleteAsync();
                }
                catch {  }
            }

        }

        public async Task Update(IEmote emote, IUser requestedBy)
        {
            var node = LavaNodeService.GetNode(Guild.Id);
            if (node is null) return;

            if (!node.TryGetPlayer(Guild, out LavaPlayer player)) return;



                if (requestedBy is not IVoiceState state || state.VoiceChannel?.Id != player.VoiceChannel.Id)
                    return;

            var p = Player.GetPlayer(Guild);

            if (emote.Name == EmojiHelper.Loop.Name)
            {
                if (p.Loop is null) p.Loop = false;
                else if (p.Loop == false) p.Loop = true;
                else p.Loop = null;
            }
            else if (emote.Name == EmojiHelper.Mute.Name)
            {
                if (player.Volume != 0) await player.UpdateVolumeAsync(0);
                else await player.UpdateVolumeAsync(100);
            }
            else if (emote.Name == EmojiHelper.Next.Name)
            {
                var t = p.NextTrack;
                if (t is not null)
                {
                    await player.PlayAsync(t);
                }
            }
            else if (emote.Name == EmojiHelper.Previous.Name)
            {
                if (player.Track.Position.TotalSeconds > 10)
                {
                    await player.SeekAsync(TimeSpan.Zero);
                }
                else
                {
                    var t = p.PreviousTrack;
                    if (t is not null)
                        await player.PlayAsync(t);
                }
            }
            else if (emote.Name == EmojiHelper.PausePlay.Name)
            {
                if (player.PlayerState == PlayerState.Playing)
                    await player.PauseAsync();
                else if (player.PlayerState == PlayerState.Paused)
                    await player.ResumeAsync();
            }
            else if (emote.Name == EmojiHelper.VolumeDown.Name)
            {
                if (player.Volume >= 20) await player.UpdateVolumeAsync((ushort)(player.Volume - 20));
                else if (player.Volume > 0) await player.UpdateVolumeAsync(0);
            }
            else if (emote.Name == EmojiHelper.VolumeUp.Name)
            {
                if (player.Volume <= 280) await player.UpdateVolumeAsync((ushort)(player.Volume + 20));
                else await player.UpdateVolumeAsync(300);
            }
            else if (emote.Name == EmojiHelper.ColorChange.Name)
            {

            }
            else if(emote.Name == EmojiHelper.Exit.Name)
            {
                try
                {
                    await node.LeaveAsync(player.VoiceChannel);
                }
                catch { }
                return;
            }
            else return;


            try
            {
                await Message.ModifyAsync(msg => msg.Embed = CreateEmbed(player, Message.Author.GetAvatarUrl()));
            }
            catch { }
        }


    }
}
