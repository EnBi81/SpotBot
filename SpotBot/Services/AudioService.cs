using Discord;
using Discord.WebSocket;
using SpotBot.Spotify;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace SpotBot.Services
{
    public class AudioService
    {
        /// <summary>
        /// First is user, second the guild.
        /// </summary>
        public static List<(ulong, ulong, DateTime, SocketGuildChannel)> WannaJoin { get; } = new List<(ulong, ulong, DateTime, SocketGuildChannel)>();


        public static Dictionary<IGuild, IMessage> NowPlayingMessages { get; } = new Dictionary<IGuild, IMessage>();
        
    }
}
