using AGoodSpotifyAPI.Exceptions.WebExceptions;
using AGoodSpotifyAPI.InterFaces;
using AGoodSpotifyAPI.JsonSchema;
using AGoodSpotifyAPI.Web;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Classes
{
    public class PlayList : ISpotYTPlaylist
    {
        /// <summary>
        /// Returns true if context is not search and the owner allows other users to modify the playlist. Otherwise returns false.
        /// </summary>
        public bool Collaborative { get; }
        /// <summary>
        /// The playlist description. Only returned for modified, verified playlists, otherwise null.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Known external URLs for this playlist.
        /// </summary>
        public string ExternalUrl { get; }
        /// <summary>
        /// Information about the followers of the playlist.
        /// </summary>
        public int Followers { get; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the playlist.
        /// </summary>
        public string Href { get; }
        /// <summary>
        /// The Spotify ID for the playlist.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Images for the playlist. The array may be empty or contain up to three images. The images are returned by size in descending order. See Working with Playlists.Note: If returned, the source URL for the image ( url ) is temporary and will expire in less than a day.
        /// </summary>
        public Image[] Images { get; }
        /// <summary>
        /// The name of the playlist.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The user who owns the playlist
        /// </summary>
        public User Owner { get; }
        /// <summary>
        /// The playlist’s public/private status: true the playlist is public, false the playlist is private, null the playlist status is not relevant.
        /// </summary>
        public bool? Public { get; }
        /// <summary>
        /// The version identifier for the current playlist. Can be supplied in other requests to target a specific playlist version
        /// </summary>
        public string SnapShotId { get; }
        /// <summary>
        /// Information about the tracks of the playlist.
        /// </summary>
        internal PagingJSON<PlaylistTrackJSON<TrackFullJSON>> Tracks { get; private set; }
        public int TrackCount { get => Tracks.Total ?? 0; }
        /// <summary>
        /// The Spotify URI for the playlist.
        /// </summary>
        public string Uri { get; }

        public async Task<List<PlaylistTrack>> GetTracks(string token)
        {
            List<PlaylistTrack> tracks = new List<PlaylistTrack>();
            var list = await Converting.GetPagingItems(Tracks, token);

            foreach (var item in list)
            {
                try
                {
                    tracks.Add(new PlaylistTrack(item));
                }
                catch (NotFoundException)
                {

                }
            }

            return tracks;
        }

        private PlayList(PlaylistSimpJSON<PlaylistTrackJSON<TrackFullJSON>> playlist, PagingJSON<PlaylistTrackJSON<TrackFullJSON>> tracks, int followers = 0)
        {
            if (playlist is PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>> pl) followers = pl.Followers?.Total ?? 0;  
            var p = playlist;

            Collaborative = p.Collaborative ?? false;
            Description = p.Description;
            ExternalUrl = p.External_urls.Spotify;
            Followers = followers;
            Href = p.Href;
            Id = p.Id;
            Images = (from i in p.Images select new Image(i)).ToArray();
            Name = p.Name;
            Owner = new User(p.Owner);
            Public = p.Public;
            SnapShotId = p.Snapshot_id;
            Uri = playlist.Uri;
            Tracks = tracks;
            
        }

        public async Task<string> AddTracksAsync(IEnumerable<string> uris, string token, int? position = null) => await PlaylistAddTracksAsync(uris, token, Id, position);
        public async Task<string> AddTrackAsync(string trackUri, string token, int? position = null) => await PlaylistAddTrackAsync(trackUri, token, Id, position);

        #region Statikus
        /// <summary>
        /// Add one or more items to a user’s playlist.
        /// </summary>
        /// <param name="uris">The track Uris which you would like to add into the playlist.</param>
        /// <param name="token"></param>
        /// <param name="playlistId"></param>
        /// <param name="position">Zero-based position.</param>
        /// <returns>The Snapshot ID for the playlist.</returns>
        public static async Task<string> PlaylistAddTracksAsync(IEnumerable<string> uris, string token, string playlistId, int? position = null)
        {
            using var res = await WebHelper.PlaylistAddTracks(token, playlistId, uris, position).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);

            return res.Result.SnapShot;
        }
        /// <summary>
        /// Add one item to a user’s playlist.
        /// </summary>
        /// <param name="uris">The track Uris which you would like to add into the playlist.</param>
        /// <param name="token"></param>
        /// <param name="playlistId"></param>
        /// <param name="position">Zero-based position.</param>
        /// <returns>The Snapshot ID for the playlist.</returns>
        public static async Task<string> PlaylistAddTrackAsync(string trackUri, string token, string playlistId, int? position = null)
            => await PlaylistAddTracksAsync(new string[] { trackUri}, token, playlistId, position);

        public static async Task<PlayList> CreatePlaylistAsync(string token, string userId, string playlistName, bool? Public = null, bool? collaborative = null, string description = null)
        {
            var res = await WebHelper.CreatePlaylist(token: token, userId, playlistName, Public, collaborative, description);

            return InitializeAsync(res);
        }

        public static async Task<PlayList> GetPlaylistAsync(string token, string playlistId, Markets? market = null)
        {
            var res = await WebHelper.GetPlaylist(token: token, playlistId: playlistId, market: market).GetResultAsync();
            if (res.IsError) throw new Exception(res.Error.Message);

            var playlist = res.Result;

            return InitializeAsync(playlist);
        }

        public static async Task<PlayList[]> GetUserPlaylistsAsync(string token, string userId)
        {
            var res = await WebHelper.GetUserPlaylist(token, userId).GetResultAsync();

            if (res.IsError) throw new Exception(res.Error.Message);

            var items = await Converting.GetPagingItems(res.Result, token);

            List<PlayList> playlists = new List<PlayList>();

            items.ForEach(p => playlists.Add(InitializeAsync(p)));

            return playlists.ToArray();
        }

        public static async Task<PlayList[]> GetCurrentUserPlaylistsAsync(string token)
        {
            var res = await WebHelper.GetCurrentUserPlaylist(token).GetResultAsync();
            if (res.IsError) throw new Exception(res.Error.Message);

            var items = await Converting.GetPagingItems(res.Result, token);

            List<PlayList> playlists = new List<PlayList>();

            foreach (var p in items)
            {
                playlists.Add(InitializeAsync(p));
            };

            return playlists.ToArray();
        }
        
        public static async Task<Image[]> GetPlaylistImagesAsync(string token, string playlistId)
        {
            var res = await WebHelper.GetPlaylistCoverImages(token, playlistId).GetResultAsync();
            if (res.IsError) throw new Exception(res.Error.Message);

            return (from i in res.Result select new Image(i)).ToArray();
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj) => obj is PlayList && (obj as PlayList).Id == Id;
        public override int GetHashCode() => HashCode.Combine(this);
        public override string ToString() => $"{Name} [{Id}]";
        #endregion

        #region Internal Helper
        internal static PlayList InitializeAsync(PlaylistSimpJSON<PlaylistTrackJSON<TrackFullJSON>> playlist)
        {
            var p = new PlayList(playlist, playlist.Tracks);
            return p;
        }
        #endregion
    }
}
