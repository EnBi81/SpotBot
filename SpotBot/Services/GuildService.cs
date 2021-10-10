using Newtonsoft.Json;
using SpotBot.Spotify;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace SpotBot.Services
{
    public class GuildService
    {
        public string Prefix { get; set; }
        public bool TrackStartedMessage { get; set; }

        public GuildService() : this(BasePrefix)
        {

        }
        public GuildService(string prefix)
        {
            Prefix = prefix;
            TrackStartedMessage = false;
        }




        #region Statik
        private const string BasePrefix = "!sb";

        private static readonly Dictionary<ulong, GuildService> _dictonary = new Dictionary<ulong, GuildService>();
        public static async Task LoadService()
            => (await DataHelper.GetGuildService()).ToList().ForEach(c => _dictonary.Add(c.GuildId, c.Service));

        public static bool GetService(ulong guildId, out GuildService service)
        {
            if(_dictonary.ContainsKey(guildId))
            {
                service = _dictonary[guildId];
                return true;
            }

            service = null;
            return false;
        }
        public static string GetPrefix(ulong guildId) => _dictonary.ContainsKey(guildId) ? _dictonary[guildId].Prefix : BasePrefix;
        public static bool GetMd() => DateTime.Now.DayOfYear % 2 == 0;
        public static bool TrackStartedMessageCheck(ulong guildId) => _dictonary.ContainsKey(guildId) ? _dictonary[guildId].TrackStartedMessage : false;
        public static bool TrackStartedMessageCheck(Discord.IGuild guild) => TrackStartedMessageCheck(guild.Id);

        public static async Task AddNew(GuildService service, ulong guildId)
        {
            _dictonary.Add(guildId, service);
            await DataHelper.AddGuildService(guildId, service);
        }
        public static async Task ChangePrefix(ulong guildId, string prefix)
        {
            if(_dictonary.ContainsKey(guildId))
            {
                _dictonary[guildId].Prefix = prefix;
                await DataHelper.SetGuildService(guildId, prefix);
            }
            else
            {
                await AddNew(new GuildService(prefix), guildId);
            }
        }
        public static GuildService GetService(ulong guildId)
        {
            if (_dictonary.ContainsKey(guildId)) return _dictonary[guildId];

            var serv = new GuildService();

            _dictonary.Add(guildId, serv);
            return serv;

        }
        public static async Task ChangeTrackStartedMessage(ulong guildId, bool tsm)
        {
            if(!_dictonary.TryGetValue(guildId, out GuildService service))
            {
                if (tsm) return;
                service = new GuildService() { TrackStartedMessage = tsm };
                
                await AddNew(service, guildId);
                return;
            }

            service.TrackStartedMessage = tsm;
            await DataHelper.SetGuildService(guildId, null, tsm);

        }
        
        #endregion
    }
}
