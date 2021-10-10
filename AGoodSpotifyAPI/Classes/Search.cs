using AGoodSpotifyAPI.Web;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace AGoodSpotifyAPI.Classes
{
    public static class Search
    {
        public static async Task<Track[]> SearchTrack(string query, string token)
        {
            var res = await WebHelper.Search(token, query, new[] { SearchType.Track }, limit: 1).GetResultAsync();
            if (res.IsError) throw new Exception(res.Error.Message);

            var s = res.Result;

            return await s.GetTracks(token);
        }

        public static async Task<PlayList[]> SearchPlaylist(string token, string playlistName)
        {
            var res = await WebHelper.Search(token, playlistName, new[] { SearchType.Playlist }).GetResultAsync();
            if (res.IsError) throw new Exception(res.Error.Message);

            var s = res.Result?.Playlists?.Items;
            if (s is null) return null;

            return (from p in s select PlayList.InitializeAsync(p)).ToArray();

        }

        public static async Task<Album[]> SearchAlbum(string query, string token)
        {
            var res = await WebHelper.Search(token, query, new[] { SearchType.Album }).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);

            var s = res.Result?.Albums?.Items;

            if (s is null) return null;

            return (from a in s select Album.InitializeAsync(a)).ToArray();
        }

    }
}
