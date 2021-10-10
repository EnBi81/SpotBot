using AGoodSpotifyAPI.Web;

namespace AGoodSpotifyAPI.JsonSchema
{
    internal class PlaylistSimpJSON<T>
    {
        /// <summary>
        /// Returns true if context is not search and the owner allows other users to modify the playlist. Otherwise returns false.
        /// </summary>
        public bool? Collaborative { get; set; }
        /// <summary>
        /// The playlist description. Only returned for modified, verified playlists, otherwise null .
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Known external URLs for this playlist.
        /// </summary>
        public ExternalUrlsJSON External_urls { get; set; }
        /// <summary>
        /// A link to the Web API endpoint providing full details of the playlist.
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// The Spotify ID for the playlist.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Images for the playlist. The array may be empty or contain up to three images. The images are returned by size in descending order. See Working with Playlists.
        /// Note: If returned, the source URL for the image(url ) is temporary and will expire in less than a day.
        /// </summary>
        public ImageJSON[] Images { get; set; }
        /// <summary>
        /// The name of the playlist.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The user who owns the playlist
        /// </summary>
        public UserPublicJSON Owner { get; set; }
        /// <summary>
        /// The playlist’s public/private status: true the playlist is public, false the playlist is private, null the playlist status is not relevant. For more about public/private status, see Working with Playlists.
        /// </summary>
        public bool? Public { get; set; }
        /// <summary>
        /// The version identifier for the current playlist. Can be supplied in other requests to target a specific playlist version
        /// </summary>
        public string Snapshot_id { get; set; }
        /// <summary>
        /// Information about the tracks of the playlist.
        /// </summary>
        public PagingJSON<PlaylistTrackJSON<TrackFullJSON>> Tracks { get; set; }
        /// <summary>
        /// The object type: “playlist”
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The Spotify URI for the playlist.
        /// </summary>
        public string Uri { get; set; }

        public async System.Threading.Tasks.Task<PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>>> GetFullPlaylist(string token)
        {
            if (this is PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>>) return this as PlaylistFullJSON<PlaylistTrackJSON<TrackFullJSON>>;

            return (await WebHelper.GetPlaylist(token, Id).GetResultAsync()).Result;
        }

    }
}
