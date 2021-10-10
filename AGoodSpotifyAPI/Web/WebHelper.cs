using AGoodSpotifyAPI.Classes;
using AGoodSpotifyAPI.JsonSchema;
using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Web
{
    internal class WebHelper<T>
    {
        public string Token { get; set; }
        public string Url { get; set; }
        public Method Method { get; set; }
        public string PlusKey { get; }
        public string PlusValue { get; }

        internal WebHelper(string token, string url, Method method, string plusKey = null, string plusValue = null) 
            => (Token, Url, Method, PlusKey, PlusValue) = (token, url, method, plusKey, plusValue);

        public async Task<WebResult<T>> GetResultAsync()
        {
            var result = await WebPart.MakeWebRequest(this);
            return result;
        }
    }

    internal static class WebHelper
    {

        #region Album - Done

        public static WebHelper<AlbumFullJSON> GetAlbumHelper(string token, string albumId)
        {
            WebHelper<AlbumFullJSON> helper =
                new WebHelper<AlbumFullJSON>(token, "https://api.spotify.com/v1/albums/" + albumId, Method.Get);

            return helper;
        }

        public static WebHelper<PagingJSON<TrackSimpJSON>> GetAlbumsTrackHelper(string token, string albumId, int? limit = null)
        {
            if(!(limit is null))
               if (limit < 1 || limit > 50) throw new ArgumentOutOfRangeException("Limit must be between 1 and 50");

            string url = $"https://api.spotify.com/v1/albums/{albumId}/tracks";

            if (!(limit is null)) url += $"?limit={limit}";

            WebHelper<PagingJSON<TrackSimpJSON>> helper =
                new WebHelper<PagingJSON<TrackSimpJSON>>(token, url, Method.Get);

            return helper;
           
        }

        public static WebHelper<AlbumMultiJSON> GetAlbumsHelper(string token, IEnumerable<string> albumIds)
        {
            if (albumIds.Count() > 50) throw new ArgumentOutOfRangeException("Can't handle more than 50 albums.");
            if (albumIds.Count() == 0) throw new ArgumentNullException("Length cannot be zero.");

            string albums = string.Join(',', albumIds);

            WebHelper<AlbumMultiJSON> helper =
                new WebHelper<AlbumMultiJSON>(token, "https://api.spotify.com/v1/albums/?ids=" + albums, Method.Get);

            return helper;
        }

        #endregion

        #region Artist - Done (albumtypes!!!)

        public static WebHelper<ArtistFullJSON> GetArtistHelper(string token, string artistId)
        {
            WebHelper<ArtistFullJSON> helper =
                new WebHelper<ArtistFullJSON>(token, "https://api.spotify.com/v1/artists/" + artistId, Method.Get);

            return helper;
        }

        public static WebHelper<PagingJSON<AlbumSimpJSON>> GetArtistsAlbumHelper(string token, string artistId, AlbumGroups[] includeGroups = null, Markets? market = null, int? limit = null, int? offset = null)
        {
            List<string> parameters = new List<string>();

            if (includeGroups.Length > 0) parameters.Add($"include_groups={string.Join(',', includeGroups)}");

            if(!(limit is null))
            {
                if (limit < 1 || limit > 50) throw new ArgumentOutOfRangeException("Limit has to be between 1 and 50");
                parameters.Add($"limit={limit}"); 
            }

            if (!(market is null)) parameters.Add("country=" + market.ToString());

            if (!(offset is null)) parameters.Add("offset=" + offset);


            string url = $"https://api.spotify.com/v1/artists/{artistId}/albums";
            if (parameters.Count > 0) url += "?" + string.Join('&', parameters);

            WebHelper<PagingJSON<AlbumSimpJSON>> helper =
                new WebHelper<PagingJSON<AlbumSimpJSON>>(token, url, Method.Get);

            return helper;
        }

        public static WebHelper<TrackMultiJSON> GetArtistsTopTracksHelper(string token, string artistId, Markets? market = null)
        {
            WebHelper<TrackMultiJSON> helper =
                new WebHelper<TrackMultiJSON>(token, $"https://api.spotify.com/v1/artists/{artistId}/top-tracks?country=" + (market is null ? "from_token" : market.ToString()), Method.Get);

            return helper;
        }

        public static WebHelper<ArtistMultiJSON> GetArtistsRelatedArtistsHelper(string token, string artistId)
        {
            WebHelper<ArtistMultiJSON> helper =
                new WebHelper<ArtistMultiJSON>(token, $"https://api.spotify.com/v1/artists/{artistId}/related-artists", Method.Get);

            return helper;
        }

        public static WebHelper<ArtistMultiJSON> GetSeveralArtists(string token, IEnumerable<string> artistIds)
        {
            if (artistIds.Count() > 50) throw new ArgumentOutOfRangeException("Can't handle more than 50 artists.");
            if (artistIds.Count() == 0) throw new ArgumentNullException("Length cannot be zero.");

            string url = "https://api.spotify.com/v1/artists?ids=" + string.Join(',', artistIds);

            WebHelper<ArtistMultiJSON> helper =
                new WebHelper<ArtistMultiJSON>(token, url, Method.Get);

            return helper;
        }

        #endregion

        #region Browse

        public static WebHelper<CategoryJSON> BrowseCategoryHelper(string token, string categoryId)
        {
            WebHelper<CategoryJSON> helper =
                new WebHelper<CategoryJSON>(token, "https://api.spotify.com/v1/browse/categories/" + categoryId, Method.Get);

            return helper;
        }

        public static WebHelper<PagingJSON<PlaylistSimp2JSON>> BrowseCategorysPlaylistHelper(string token, string categoryId, int limit = 20)
        {
            if (limit > 50) throw new ArgumentOutOfRangeException("Can't get more than 50 playlists.");
            if (limit <= 0) throw new ArgumentNullException("Limit cannot be less than one.");

            string url = $"https://api.spotify.com/v1/browse/categories/{categoryId}/playlists?limit={limit}";

            WebHelper<PagingJSON<PlaylistSimp2JSON>> helper =
                new WebHelper<PagingJSON<PlaylistSimp2JSON>>(token, url, Method.Get);

            return helper;
        }

        public static WebHelper<ListOfCategoriesJSON> BrowseListOfCategoriesHelper(string token)
        {
            WebHelper<ListOfCategoriesJSON> helper =
                new WebHelper<ListOfCategoriesJSON>(token, "https://api.spotify.com/v1/browse/categories", Method.Get);

            return helper;
        }

        public static WebHelper<ListOfFeaturedPlaylistsJSON> BrowseListOfFeaturedPlaylistsHelper(string token, int limit = 20,string country = null)
        {
            if (limit > 50) throw new ArgumentOutOfRangeException("Can't get more than 50 playlists.");
            if (limit <= 0) throw new ArgumentNullException("Limit cannot be less than one.");

            if (!(country is null)) country = $"country={country}&";
            else country = "";

            string url = $"https://api.spotify.com/v1/browse/featured-playlists?{country}timestamp={DateTime.Now:yyyy-MM-ddTHH:mm:ss}&limit={limit}";

            WebHelper<ListOfFeaturedPlaylistsJSON> helper =
                new WebHelper<ListOfFeaturedPlaylistsJSON>(token, url, Method.Get);

            return helper;
        }

        public static WebHelper<ListOfNewReleasesJSON> BrowseNewReleases(string token, int limit = 20, string country = null)
        {
            if (limit > 50) throw new ArgumentOutOfRangeException("Can't get more than 50 playlists.");
            if (limit <= 0) throw new ArgumentNullException("Limit cannot be less than one.");

            if (country is null) country = "";
            else country = $"country={country}&";

            string url = $"https://api.spotify.com/v1/browse/new-releases?{country}limit={limit}";

            WebHelper<ListOfNewReleasesJSON> helper =
                new WebHelper<ListOfNewReleasesJSON>(token, url, Method.Get);

            return helper;
        }

        #endregion

        #region Follow

        [RequireScopes(AuthScopes.User_Follow_Read)]
        public static WebHelper<bool[]> CheckArtistsFollowing(string token, params string[] artistIds)
        {
            if (artistIds.Length == 0) throw new ArgumentException("This needs at least 1 artist");
            if (artistIds.Length > 50) throw new ArgumentException("Max 50 Artists are allowed");

            string url = "https://api.spotify.com/v1/me/following/contains?type=artist&ids=" + string.Join(',', artistIds);

            WebHelper<bool[]> helper =
                new WebHelper<bool[]>(token, url, Method.Get);

            return helper;
        }

        [RequireScopes(AuthScopes.User_Follow_Read)]
        public static WebHelper<bool[]> CheckUsersFollowing(string token, params string[] usersIds)
        {
            var artistIds = usersIds;

            if (artistIds.Length == 0) throw new ArgumentException("This needs at least 1 user");
            if (artistIds.Length > 50) throw new ArgumentException("Max 50 Users are allowed");

            string url = "https://api.spotify.com/v1/me/following/contains?type=user&ids=" + string.Join(',', artistIds);

            WebHelper<bool[]> helper =
                new WebHelper<bool[]>(token, url, Method.Get);

            return helper;
        }

        [RequireScopes(AuthScopes.User_Follow_Modify)]
        public static WebHelper<object> FollowArtists(string token, string[] artistIds)
        {
            if (artistIds.Length == 0) throw new ArgumentException("You have to put at least 1 artist.");
            if (artistIds.Length > 50) throw new ArgumentException("Maximum 50 artists are permitted.");

            string artists = string.Join(",", artistIds);

            string url = "https://api.spotify.com/v1/me/following?type=artist&ids=" + artists;

            WebHelper<object> helper =
                new WebHelper<object>(token, url, Method.Put);

            return helper;
        }

        [RequireScopes(AuthScopes.User_Follow_Modify)]
        public static WebHelper<object> FollowUsers(string token, string[] userIds)
        {
            if (userIds.Length == 0) throw new ArgumentException("You have to put at least 1 user.");
            if (userIds.Length > 50) throw new ArgumentException("Maximum 50 users are permitted.");

            string users = string.Join(",", userIds);

            string url = "https://api.spotify.com/v1/me/following?type=user&ids=" + users;

            WebHelper<object> helper =
                new WebHelper<object>(token, url, Method.Put);

            return helper;
        }

        [RequireScopes()]
        public static WebHelper<object> FollowPlaylist(string token, string playlistId)
        {
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}/followers";

            WebHelper<object> helper =
                new WebHelper<object>(token, url, Method.Put);

            return helper;
        }

        #endregion

        #region Library
        [RequireScopes(AuthScopes.User_Library_Read)]
        public static async Task<bool[]> CheckSavedAlbums(IEnumerable<string> albumIds, string token)
        {
            if (albumIds.Count() <= 0 || albumIds.Count() > 50) throw new ArgumentException();

            string url = "https://api.spotify.com/v1/me/albums/contains?ids=" + string.Join(",", albumIds);

            var res = await new WebHelper<bool[]>(token, url, Method.Get).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);

            return res.Result;
        }

        [RequireScopes(AuthScopes.User_Library_Read)]
        public static async Task<bool[]> CheckSavedTracks(IEnumerable<string> trackIds, string token)
        {
            if (trackIds.Count() <= 0 || trackIds.Count() > 50) throw new ArgumentException();

            string url = "https://api.spotify.com/v1/me/tracks/contains?ids=" + string.Join(",", trackIds);

            var res = await new WebHelper<bool[]>(token, url, Method.Get).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);

            return res.Result;
        }

        [RequireScopes]
        public static WebHelper<PagingJSON<SavedAlbumJSON>> GetSavedAlbums(string token)
        {
            string url = "https://api.spotify.com/v1/me/albums?limit=50";

            var helper = new WebHelper<PagingJSON<SavedAlbumJSON>>(token, url, Method.Get);

            return helper;
        }

        [RequireScopes]
        public static WebHelper<PagingJSON<SavedTrackJSON>> GetSavedTracks(string token)
        {
            string url = "https://api.spotify.com/v1/me/tracks?limit=50";

            var helper = new WebHelper<PagingJSON<SavedTrackJSON>>(token, url, Method.Get);

            return helper;
        }

        [RequireScopes(AuthScopes.User_Library_Modify)]
        public static async Task RemoveSavedAlbums(IEnumerable<string> albumIds, string token)
        {
            if (albumIds.Count() == 0 || albumIds.Count() > 50) throw new ArgumentException();

            string url = "https://api.spotify.com/v1/me/albums?ids=" + string.Join(",", albumIds);

            var res = await new WebHelper<ErrorJSON>(token, url, Method.Delete).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);
        }

        [RequireScopes(AuthScopes.User_Library_Modify)]
        public static async Task RemoveSavedTracks(IEnumerable<string> trackIds, string token)
        {
            if (trackIds.Count() == 0 || trackIds.Count() > 50) throw new ArgumentException();

            string url = "https://api.spotify.com/v1/me/tracks?ids=" + string.Join(",", trackIds);

            var res = await new WebHelper<ErrorJSON>(token, url, Method.Delete).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);
        }

        [RequireScopes(AuthScopes.User_Library_Modify)]
        public static async Task SaveAlbums(IEnumerable<string> albumIds, string token)
        {
            if (albumIds.Count() == 0 || albumIds.Count() > 50) throw new ArgumentException();

            string url = "https://api.spotify.com/v1/me/albums?ids=" + string.Join(",", albumIds);

            var res = await new WebHelper<ErrorJSON>(token, url, Method.Put).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);
        }

        [RequireScopes(AuthScopes.User_Library_Modify)]
        public static async Task SaveTracks(IEnumerable<string> trackIds, string token)
        {
            if (trackIds.Count() == 0 || trackIds.Count() > 50) throw new ArgumentException();

            string url = "https://api.spotify.com/v1/me/tracks?ids=" + string.Join(",", trackIds);

            var res = await new WebHelper<ErrorJSON>(token, url, Method.Put).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);
        }
        #endregion

        #region Personalization - Done
        [RequireScopes(AuthScopes.User_Top_Read)]
        public static WebHelper<PagingJSON<ArtistFullJSON>> GetUsersTopArtists(string token, int limit = 20, int offset = 0, TimeRange? range = null)
        {
            string url = "https://api.spotify.com/v1/me/top/artists";

            var parameters = new List<string>(3);

            if (limit < 1 || limit > 50) throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 50");
            if (offset < 0 || offset > 100000) throw new ArgumentOutOfRangeException("offset", "offset must be between 0 and 100 000");

            if (limit != 20) parameters.Add("limit=" + limit);
            if (offset != 0) parameters.Add("offset=" + offset);
            if(!(range is null))
            {
                parameters.Add(Converting.RangeConvert(range));
            }

            if (parameters.Count > 0) url += "?" + string.Join("&", parameters);

            WebHelper<PagingJSON<ArtistFullJSON>> helper =
                new WebHelper<PagingJSON<ArtistFullJSON>>(token, url, Method.Get);

            return helper;
        }

        [RequireScopes(AuthScopes.User_Top_Read)]
        public static WebHelper<PagingJSON<TrackFullJSON>> GetUsersTopTracks(string token, int limit = 20, int offset = 0, TimeRange? range = null)
        {
            string url = "https://api.spotify.com/v1/me/top/tracks";

            var parameters = new List<string>(3);

            if (limit < 1 || limit > 50) throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 50");
            if (offset < 0 || offset > 100000) throw new ArgumentOutOfRangeException("offset", "offset must be between 0 and 100 000");

            if (limit != 20) parameters.Add("limit=" + limit);
            if (offset != 0) parameters.Add("offset=" + offset);
            if (!(range is null))
            {
                parameters.Add(Converting.RangeConvert(range));
            }

            if (parameters.Count > 0) url += "?" + string.Join("&", parameters);

            WebHelper<PagingJSON<TrackFullJSON>> helper =
                new WebHelper<PagingJSON<TrackFullJSON>>(token, url, Method.Get);

            return helper;
        }
        #endregion

        #region Playlist
        public static WebHelper<SnapShotJSON> PlaylistAddTracks(string token, string playlistId, IEnumerable<string> uris, int? position = null)
        {
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks?";
            if(!(position is null))
            {
                if (position < 0) throw new ArgumentOutOfRangeException("position", "position cannot be less than zero");

                url = "position=" + position + "&";
            }

            url += "uris=" + string.Join(",", uris);

            var helper = new WebHelper<SnapShotJSON>(token, url, Method.Post);

            return helper;
        }

        [RequireScopes(AuthScopes.Playlist_Modify_Private, AuthScopes.Playlist_Modify_Public)]
        public async static Task<PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>>> CreatePlaylist(string token, string userId, string playlistName, bool? Public = null, bool? collaborative = null, string description = null)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");

            var dic = new Dictionary<string, string>() { ["name"] = playlistName };

            if (!(Public is null)) dic.Add("public", "" + Public);
            if (!(collaborative is null)) dic.Add("collaborative", collaborative + "");
            if (!(description is null)) dic.Add("description", description);

            var res = await client.PostAsync($"https://api.spotify.com/v1/users/{userId}/playlists", new FormUrlEncodedContent(dic));

            string text = await res.Content.ReadAsStringAsync();

            PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>> playlist = JsonConvert.DeserializeObject<PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>>>(text);

            return playlist;

        }

        public static WebHelper<PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>>> GetPlaylist(string token, string playlistId, Markets? market = null)
        {
            string url = "https://api.spotify.com/v1/playlists/" + playlistId + "?additional_types=track";

            if (!(market is null)) url += "&market=" + market.ToString();

            var helper = new WebHelper<PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>>>(token, url, Method.Get);

            return helper;
        }

        public static WebHelper<PagingJSON<PlaylistSimpJSON<PlaylistTrackJSON<TrackFullJSON>>>> GetUserPlaylist(string token, string userId, int limit = 50, int offset = 0)
        {
            string url = $"https://api.spotify.com/v1/users/{userId}/playlists";

            var parameters = new List<string>(2);

            if (limit < 1 || limit > 50) throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 50");
            if (limit != 20) parameters.Add("limit=" + limit);
            if (offset < 0 || offset > 100000) throw new ArgumentOutOfRangeException("offset", "offset must be between 0 and 100 000");
            if (offset != 0) parameters.Add("offset=" + offset);

            if (parameters.Count > 0) url += "?" + string.Join("&", parameters);

            WebHelper<PagingJSON<PlaylistSimpJSON<PlaylistTrackJSON<TrackFullJSON>>>> helper =
                new WebHelper<PagingJSON<PlaylistSimpJSON<PlaylistTrackJSON<TrackFullJSON>>>>(token, url, Method.Get);

            return helper;

        }

        public static WebHelper<PagingJSON<PlaylistSimpJSON<PlaylistTrackJSON<TrackFullJSON>>>> GetCurrentUserPlaylist(string token, int limit = 50, int offset = 0)
        {
            if (limit <= 0 || limit > 50) throw new ArgumentOutOfRangeException();
            if (offset < 0 || offset > 100000) throw new ArgumentOutOfRangeException();

            string url = $"https://api.spotify.com/v1/me/playlists?limit={limit}&offset={offset}";

            WebHelper<PagingJSON<PlaylistSimpJSON<PlaylistTrackJSON<TrackFullJSON>>>> helper =
                new WebHelper<PagingJSON<PlaylistSimpJSON<PlaylistTrackJSON<TrackFullJSON>>>>(token, url, Method.Get);

            return helper;
        }

        public static WebHelper<ImageJSON[]> GetPlaylistCoverImages(string token, string playlistId)
        {
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}/images";

            WebHelper<ImageJSON[]> helper = new WebHelper<ImageJSON[]>(token, url: url, Method.Get);

            return helper;
        }

        #endregion

        #region Player
        [RequireScopes(AuthScopes.User_Read_Playback_State)]
        public static WebHelper<DeviceMultiJSON> GetUserDevices(string token)
        {
            string url = "https://api.spotify.com/v1/me/player/devices";
             
            WebHelper<DeviceMultiJSON> helper = new WebHelper<DeviceMultiJSON>(token, url, Method.Get);

            return helper;
        }

        public static async Task<(bool Success, Exception e)> PlayerPlayTracks(string token, IEnumerable<string> uris, string deviceId = null)
        {
            try
            {
                var url = "https://api.spotify.com/v1/me/player/play";
                if (!(deviceId is null)) url += "?device_id=" + deviceId;
                using var client = new HttpClient();
                client.SetBearerToken(token);

                string jsonUris = JsonConvert.SerializeObject(uris);
                //var dic = new Dictionary<string, string> { ["uris"] = jsonUris };
                //var content = new FormUrlEncodedContent(dic);


                //var resp = await client.PutAsync(url, content);
                //Console.WriteLine("Statuscode (Webhelper 553): " + resp.StatusCode);
                //Console.WriteLine(await resp.Content.ReadAsStringAsync());

                //if (resp.StatusCode == HttpStatusCode.NoContent) return (true, null);


                var message = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = new StringContent("{\"uris\": " + jsonUris + "}"),

                };

                var resp = await client.SendAsync(message).ConfigureAwait(false);

                if (resp.StatusCode == HttpStatusCode.NoContent) return (true, null);


            }
            catch (Exception e) {  return (false, e); }

            return (false, null);
            
        }

        public static async Task<(bool Success, Exception Exception)> QueueTrack(string token, string uri, string deviceId = null)
        {
            try
            {
                var url = $"https://api.spotify.com/v1/me/player/queue?uri={uri}";
                if (!(deviceId is null)) url += $"&device_id={deviceId}";

                var client = new HttpClient();

                var message = new HttpRequestMessage(HttpMethod.Post, url);
                message.SetBearerToken(token);

                var resp = await client.SendAsync(message);

                if (resp.StatusCode == HttpStatusCode.NoContent) return (true, null);
            }
            catch (Exception e) { return (false, e); }

            return (false, null);
            
        }

        #endregion

        #region Tracks - Done

        public static WebHelper<AudioAnalyzeJSON> GetAudioAnalyzeHelper(string token, string trackId)
        {
            WebHelper<AudioAnalyzeJSON> helper = 
                new WebHelper<AudioAnalyzeJSON>(token, "https://api.spotify.com/v1/audio-analysis/" + trackId, Method.Get);

            return helper;
        }

        public static WebHelper<AudioFeaturesJSON> GetAudioFeaturesHelper(string token, string trackId)
        {
            WebHelper<AudioFeaturesJSON> helper =
                new WebHelper<AudioFeaturesJSON>(token, "https://api.spotify.com/v1/audio-features/" + trackId, Method.Get);

            return helper;
        }

        public static WebHelper<AudioFeaturesMultiJSON> GetAudioFeaturesMultiHelper(string token, params string[] trackIds)
        {
            if (trackIds.Length > 50) throw new ArgumentOutOfRangeException("Can't handle more than 50 tracks.");
            if (trackIds.Length == 0) throw new ArgumentNullException("Length cannot be zero.");

            var tracks = string.Join(',', trackIds);

            WebHelper<AudioFeaturesMultiJSON> helper =
                new WebHelper<AudioFeaturesMultiJSON>(token, "https://api.spotify.com/v1/audio-features/?ids=" + tracks, Method.Get);

            return helper;
        }

        public static WebHelper<TrackFullJSON> GetTrackFullHelper(string token, string trackId)
        {
            WebHelper<TrackFullJSON> helper = 
                new WebHelper<TrackFullJSON>(token, "https://api.spotify.com/v1/tracks/" + trackId, Method.Get);

            return helper;
        }

        public static WebHelper<TrackMultiJSON> GetTrackMultiHelper(string token, IEnumerable<string> trackIds)
        {
            if (trackIds.Count() > 50) throw new ArgumentOutOfRangeException("Can't handle more than 50 tracks.");
            if (trackIds.Count() == 0) throw new ArgumentNullException("Length cannot be zero.");

            WebHelper<TrackMultiJSON> helper =
                new WebHelper<TrackMultiJSON>(token, "https://api.spotify.com/v1/tracks/?ids=" + string.Join(',', trackIds), Method.Get);

            return helper;
        }

        #endregion

        #region Search

        /// <summary>
        /// Search for literally anything
        /// </summary>
        /// <param name="token"></param>
        /// <param name="query"></param>
        /// <param name="types"></param>
        /// <param name="market"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="includeExtern"></param>
        /// <param name="matchQueryInOrder"></param>
        /// <returns></returns>
        public static WebHelper<SearchJSON> Search(string token, string query, SearchType[] types, Markets? market = null, int? limit = null, int? offset = null, bool includeExtern = false, bool matchQueryInOrder = false)
        {
            string FormatQuery()
            {
                if (string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException("query", "");


                string q = query.Trim();
                q = q.Replace(" ", "%20");
                q = q.Replace(":", "%3A");

                if (matchQueryInOrder) q = ("\"" + q + "\"").Replace("\"", "%22");

                return "q=" + q;
            }
            string FormatSearchTypes()
            {
                if (types.Length < 1) throw new Exception("There must be at least one type");
                types = types.Distinct().ToArray();

                string text = "type=";
                text += string.Join(",", types);

                return text.ToLower();
            }
            string FormatMarkets()
            {
                string text = "market=";

                if (market == Markets.FromToken) text += "from_token";
                else text += market.ToString();

                return text;
            }
            string FormatLimit()
            {
                string text = "limit=";

                if (limit is null) text += "20";
                else
                {
                    if (limit < 1 || limit > 50) throw new ArgumentOutOfRangeException();

                    text += limit + "";
                }

                return text;
            }
            string FormatOffset()
            {
                string text = "offset=";

                if (offset is null) text += "0";
                else
                {
                    if (offset < 0 || offset > 2000) throw new Exception();

                    text += offset + "";
                }
                   

                return text;
            }
            static string FormatExternal() => "include_external=audio";


            List<string> parameters = new List<string>
            {
                FormatQuery(),
                FormatSearchTypes(),
                FormatLimit(),
                FormatOffset()
            };

            if (!(market is null)) parameters.Add(FormatMarkets());
            if (includeExtern) parameters.Add(FormatExternal());

            string url = "https://api.spotify.com/v1/search?";
            url += string.Join("&", parameters);


            var helper = new WebHelper<SearchJSON>(token, url, Method.Get);

            return helper;
        }

        #endregion

        #region User - Done

        [RequireScopes(AuthScopes.User_Read_Email, AuthScopes.User_Read_Private)]
        public static WebHelper<UserPrivateJSON> GetUserPrivateHelper(string token)
        {
            WebHelper<UserPrivateJSON> helper =
                new WebHelper<UserPrivateJSON>(token, "https://api.spotify.com/v1/me", Method.Get);

            return helper;
        }

        public static WebHelper<UserPublicJSON> GetUserPublicHelper(string token, string userId)
        {
            WebHelper<UserPublicJSON> helper =
                new WebHelper<UserPublicJSON>(token, "https://api.spotify.com/v1/users/" + userId, Method.Get);

            return helper;
        }

        #endregion

        #region Attribute Region

        public class RequireScopesAttribute : Attribute
        {

#pragma warning disable IDE0052 // Remove unread private members
            private AuthScopes[] _scopes;
#pragma warning restore IDE0052 // Remove unread private members

            public RequireScopesAttribute(params AuthScopes[] scopes)
               => _scopes = scopes;

        }

        #endregion
    }

}
