using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Dapper;
using SpotBot.Services;
using Victoria;
using System.IO;
using Newtonsoft.Json;

namespace SpotBot.Spotify
{
    public static class DataHelper
    {
        public static string ConnectionString { get; private set; } = SpotBotClient.Config.Connectionstring;
        public static string ClientId { get; } = SpotBotClient.Config.Clientid;
        private static string DataTable => "Users";
        private static string DateFormat => "yyyy-MM-ddTHH:mm:ss";

        private static MySqlConnection GetConnection => new MySqlConnection(ConnectionString);

        public static async Task<SBUser> GetUser(ulong id)
        {                           
            using var con = GetConnection;
            await con.OpenAsync();
            var res = await con.QueryAsync<SBUser>($"Select * from {DataTable} where DiscordId = {id};", new DynamicParameters());

            if (!res.Any()) return null;      
            return res.First();
        }

        public static async Task<int> AddUser(SBUser user)
        {
            using var con = GetConnection;
            await con.OpenAsync();
            var res = await con.QueryAsync<SBUser>($"Select * from {DataTable} where DiscordId = {user.DiscordId};", new DynamicParameters());
            if (res.Any()) return 0;
                                                                                                                                         //user.ExpiresOn.ToString(DateFormat)
            return await con.ExecuteAsync($"insert into {DataTable} values ({user.DiscordId},'{user.AccessToken}','{user.RefreshToken}','{user.ExpiresOn.ToString(DateFormat)}',{user.Premium});", user);

        }

        public static async Task<int> ModifyUser
            (ulong id, ulong? newId = null, string newAccess = null, string newRefresh = null, DateTime? newDate = null, bool? newPremium = null)
        {
            if (newId is null && newAccess is null && newRefresh is null && newDate is null && newPremium is null) return 0;

            var list = new List<string>(5);
            if(!(newId is null))
                list.Add("DiscordId = " + newId);
            if(!(newAccess is null))
                list.Add($"AccessToken = '{newAccess}'");
            if (!(newRefresh is null))
                list.Add($"RefreshToken = '{newRefresh}'");
            if (!(newDate is null))
                list.Add($"ExpiresOn = '{newDate.Value.ToString(DateFormat)}'");
            if (!(newPremium is null))
                list.Add($"Premium = {newPremium}");

            string cmd = $"update Users set {string.Join(',', list)} where DiscordId = {id};";
            using var con = GetConnection;
            await con.OpenAsync();

            using var com = new MySqlCommand(cmd, con);
            return await com.ExecuteNonQueryAsync();

        }

        public static async Task<int> DeleteUser(ulong userId)
        {
            string cmd = $"delete from {DataTable} where DiscordId = {userId}";

            using var con = GetConnection;
            await con.OpenAsync();

            var com = new MySqlCommand(cmd, con);
            return await com.ExecuteNonQueryAsync();
        }

        public static async Task<SongData> GetSongData(string spotUrl = null, string ytUrl = null)
        {
            if (spotUrl is null && ytUrl is null) return null;
            string command = spotUrl is null ? ytUrl : spotUrl;
            command = $"select * from YTNames where {(spotUrl is null ? "YTUrl" : "SpotUrl")} = '{command}';";

            using var con = GetConnection;
            await con.OpenAsync();

            var res = await con.QueryAsync<SongData>(command);



            if (res.Any())
            {
                var song = res.First();
                if (song is null || song.JSON is null) return null;

                song.JSON = song.JSON.Replace('`', '\'').Replace('^', '\"');
                return song;
            }
            return null;
        }

        public static async Task<int> AddSongData(SongData data, bool checkIfExist = true)
        {
            if (data.SpotId is null || data.YTUrl is null || data.JSON is null) return 0;

            if (checkIfExist)
            {
                var getting = await GetSongData(data.SpotId);
                if (!(getting is null)) return 0;
            }

            string name = await Task.Run(() => data.JSON.Replace('\'', '`').Replace('\"', '^'));

            string command = $"insert into YTNames values ('{data.SpotId}', '{data.YTUrl}', '{name}');";
            using var con = GetConnection;
            await con.OpenAsync();

            return await con.ExecuteAsync(command);
        }


        public static async Task<IEnumerable<(ulong GuildId, GuildService Service)>> GetGuildService()
        {
            using var con = GetConnection;
            await con.OpenAsync();

            var datas = con.Query<GuildServiceHelper>("select * from GuildService;");

            return from s in datas select (s.GuildId, new GuildService(s.Prefix));

        }
        public static async Task<int> SetGuildService(ulong guildId, string prefix = null, bool? TrackStarted = null)
        {
            if (prefix is null && TrackStarted is null) return 0;
            using var con = GetConnection;

            await con.OpenAsync();
            using var com = con.CreateCommand();

            var list = new List<string>();
            if (prefix is not null) list.Add($"prefix = '{prefix}'");
            if (TrackStarted is not null) list.Add($"TrackStartedMessage = {TrackStarted.Value}");

            var text = string.Join(", ", list);

            com.CommandText = $"update GuildService set {text} where GuildId = {guildId};";

            return await com.ExecuteNonQueryAsync();
        }
        public static async Task<int> AddGuildService(ulong guildId, GuildService service)
        {
            using var con = GetConnection;
            await con.OpenAsync();

            using var com = con.CreateCommand();

            com.CommandText = $"insert into GuildService values ({guildId},'{service.Prefix}');";

            return await com.ExecuteNonQueryAsync();
        }

        private class GuildServiceHelper
        {
            public ulong GuildId { get; set; }
            public string Prefix { get; set; }
        }

    }
}

