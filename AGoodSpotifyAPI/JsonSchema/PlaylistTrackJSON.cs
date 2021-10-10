using AGoodSpotifyAPI.InterFaces;
using System;

namespace AGoodSpotifyAPI.JsonSchema
{
    /// <summary>
    /// Contains Timestamp
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PlaylistTrackJSON<T> where T : IPlaylistTrackable 
    {
        /// <summary>
        /// The date and time the track or episode was added.
        /// Note that some very old playlists may return null in this field.
        /// </summary>
        public DateTime? Added_at { get; set; }
        /// <summary>
        /// The Spotify user who added the track or episode.
        /// Note that some very old playlists may return null in this field.
        /// </summary>
        public UserPublicJSON Added_by { get; set; }
        /// <summary>
        /// Whether this track or episode is a local file or not.
        /// </summary>
        public bool Is_local { get; set; }
        /// <summary>
        /// Information about the track or episode.
        /// </summary>
        public T Track { get; set; }
    }
}
