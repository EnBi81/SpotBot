using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using Victoria;
using Victoria.Enums;

using AGoodSpotifyAPI.Classes;
using Discord.WebSocket;
using Discord;

namespace SpotBot.Helpers
{
    public static class Extensions
    {
        public static bool CanConnect(this SocketGuildUser user, IVoiceChannel channel)
        {
            var permissions = user.GetPermissions(channel);

            return permissions.Connect && permissions.Speak;
        }

        public static bool IsUrl(this string szoveg) => Regex.IsMatch(szoveg, @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");

        public static async Task<Track> GetSpotifyTrack(this LavaTrack track)
        {
            var helper = new YoutubeConv(track.Title);
            return await helper.GetSpotifyTrack();
        }

        public static async Task<bool> MoveChannelSB(this LavaNode node, IVoiceChannel from, IVoiceChannel to, SocketGuildUser user)
        {
            try
            {
                if (!CanConnect(user, to)) return false;

                SpotBotClient.GuildIdsForMoving.Add(from.GuildId);
                SpotBotClient.MovePlayer.Remove(from.Id);
                SpotBotClient.MovePlayer.Add(from.Id, to);

                await node.LeaveAsync(from);
                return true;
            }
            catch { return false; }
        }

        public static async Task<List<LavaTrack>> SearchSongs(this LavaNode Node, string searchQuery, bool returnAllSongs = false, bool fromYt = true)
        {
            Victoria.Responses.Rest.SearchResponse searchResponse;


            if (searchQuery.IsUrl())
                searchResponse = await Node.SearchAsync(searchQuery);
            else if (fromYt)
                searchResponse = await Node.SearchYouTubeAsync(searchQuery);
            else 
                searchResponse = await Node.SearchSoundCloudAsync(searchQuery);

            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {

                return null;
            }

            var seged = new List<LavaTrack>();
            var tracks = searchResponse.Tracks;

            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                seged.AddRange(searchResponse.Tracks);
            }
            else if (returnAllSongs)
            {
                seged.AddRange(tracks);
            }
            else if (!returnAllSongs)
            {
                if (tracks.Any()) seged.Add(tracks[0]);
            }

            return seged;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var rng = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

        }

        public static float GetSimilarity(this string text, string otherTxt)
        {
            static int ComputeDistance(string s, string t)
            {
                static int Min3(int first, int sec, int third)
                    => first > sec ? (first > third ? first : third) : (sec > third ? sec : third);



                 int n = s.Length;
                int m = t.Length;
                int[,] distance = new int[n + 1, m + 1]; // matrix
                if (n == 0) return m;
                if (m == 0) return n;
                //init1
                for (int i = 0; i <= n; distance[i, 0] = i++) ;
                for (int j = 0; j <= m; distance[0, j] = j++) ;
                //find min distance
                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        int cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);
                        distance[i, j] = Min3(distance[i - 1, j] + 1,
                        distance[i, j - 1] + 1,
                        distance[i - 1, j - 1] + cost);
                    }
                }
                return distance[n, m];
            }

            static float GetSimilarityHelp(string string1, string string2)
            {
                float dis = ComputeDistance(string1, string2);
                float maxLen = string1.Length;
                if (maxLen < string2.Length)
                    maxLen = string2.Length;
                if (maxLen == 0.0F)
                    return 1.0F;
                else
                    return 1.0F - dis / maxLen;
            }


            return GetSimilarityHelp(text, otherTxt);
        }

        public static string GetGoodUrl(this string query)
        {
            try
            {
                if (!query.Contains("youtu.be") && !query.Contains("youtube.com")) return query;
                var parameters = query[(query.IndexOf("?") + 1)..];

                var coll = HttpUtility.ParseQueryString(parameters);
                if (coll.AllKeys.Contains("v")) return "https://www.youtube.com/watch?v=" + coll["v"];

            }
            catch {  }
            return query;

        }

       
    }
}
